using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Channels;
using ACConnection.Network.Packets;
using ACConnection.Network.Packets.Protocol;
using Serilog;

namespace ACConnection.Network.Udp;

public abstract class VSUdpBase
{
  protected readonly ushort _port;
  protected readonly Socket _socket;

  protected Stopwatch _timeSource;
  protected SocketAddress _address;

  protected Channel<INetworkPacket> IncomingPacketChannel { get; }

  public SocketAddress Address
  {
    get => _address;
  }

  public Channel<INetworkPacket> IncomingPackets { get => IncomingPacketChannel; }

  private static ThreadLocal<byte[]> UdpSendBuffer { get; } = new( ( ) => GC.AllocateArray<byte>( 4096,true ) );

  public VSUdpBase( ushort port,Stopwatch timeSource )
  {
    _port = port;
    _socket = new Socket( AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp );
    _timeSource = timeSource;

    IncomingPacketChannel = Channel.CreateBounded<INetworkPacket>( 256 );
  }
  public VSUdpBase( Socket s,Stopwatch timeSource )
  {
    _socket = s;
    _timeSource = timeSource;

    IncomingPacketChannel = Channel.CreateBounded<INetworkPacket>( 256 );
  }

  public abstract void OnStarted( );
  public abstract void OnReceived( SocketAddress address,byte[] buffer,int size );

  public async Task StartAsync( CancellationToken stoppingToken )
  {
    Log.Information( "Starting UDP server on port {Port}",_port );

    OnStarted( );

    await Task.Factory.StartNew( ( ) => ReceiveLoop( stoppingToken ),TaskCreationOptions.LongRunning );
  }

  private void ReceiveLoop( CancellationToken stoppingToken )
  {
    byte[] buffer = new byte[3000];
    var address = new SocketAddress( AddressFamily.InterNetwork );

    while( !stoppingToken.IsCancellationRequested )
    {
      try
      {
        var bytesRead = _socket.ReceiveFrom( buffer,SocketFlags.None,address );

        OnReceived( address,buffer,bytesRead );
      }
      catch( SocketException ex ) when( ex.SocketErrorCode == SocketError.TimedOut )
      {
        // This is a workaround because on Linux, the SocketAddress Size will be set to 0 for some reason
        address.Size = address.Buffer.Length;
      }
      catch( Exception ex )
      {
        Log.Error( ex,"Error in UDP receive loop" );
      }
    }
    _socket.Dispose( );
  }

  public void SendProtocol( SocketAddress address,ACServerProtocol protocol )
  {
    byte[] buffer = { (byte)protocol };

    Send( address,buffer,0,buffer.Length );
  }
  public void SendPacket<TPacket>( SocketAddress address,in TPacket packet ) where TPacket : INetworkPacket
  {
    try
    {
      byte[] buffer = UdpSendBuffer.Value!;
      PacketWriter writer = new PacketWriter( buffer );
      int bytesWritten = writer.WritePacket( in packet );

      Send( address,buffer,0,bytesWritten );
    }
    catch( Exception )
    {
#if DEBUG
      throw;
#endif
    }
  }
  public void Send( SocketAddress address,byte[] buffer,int offset,int size )
  {
    if( address != null )
      _socket.SendTo( buffer.AsSpan( offset,size ),SocketFlags.None,address );
  }
}

public class VSUdpServer : VSUdpBase
{
  public int nPing = 0;
  public int nClientOffset = 0;

  public VSUdpServer( ushort port,Stopwatch timeSource ) : base( port,timeSource )
  {
  }
  public VSUdpServer( Socket s,Stopwatch timeSource ) : base( s,timeSource )
  {
  }

  public override void OnStarted( )
  {
    if( OperatingSystem.IsWindows( ))
    {
      _socket.IOControl( -1744830452,new byte[] { 0,0,0,0 },null );
    }
    _socket.ReceiveTimeout = 1000;
    _socket.Bind( new IPEndPoint( IPAddress.Any,_port ) );
  }
  public override void OnReceived( SocketAddress address,byte[] buffer,int size )
  {
    try
    {
      var reader = new PacketReader(null, buffer.AsMemory()[..size]);
      var id = (ACServerProtocol)reader.Read<byte>();

      INetworkPacket? packet = null;
      switch( id )
      {
        case ACServerProtocol.CarConnect:
          _address = address;

          packet = new CarConnect( );

          break;
        case ACServerProtocol.LobbyCheck:
          LobbyCheck lobby = new( 8081 );

          SendPacket<LobbyCheck>( address,lobby );
          break;
        case ACServerProtocol.SessionRequest:
          packet = reader.ReadPacket<SessionRequest>( );
          break;

        case ACServerProtocol.PingUpdate:
          packet = reader.ReadPacket<PingUpdate>( );

          break;
        case ACServerProtocol.PingPong:
        {
          long currentTime = _timeSource.ElapsedMilliseconds;

          packet = reader.ReadPacket<PingPong>( );

          nPing = (ushort)(currentTime - ((PingPong)packet).Time );
          //nClientOffset = (int)(currentTime - ( (nPing / 2) + ((PingPong)packet).TimeOffset));
          nClientOffset = (int)(currentTime - ((PingPong)packet).TimeOffset);

          /*
          packet = reader.ReadPacket<PingPong>( );

          int nTime = (int)((PingPong)packet).TimeOffset;
          int nOffset = (int)_timeSource.ElapsedMilliseconds;

          nClientOffset = nOffset - nTime;
          nPing = (int)(nOffset - ((PingPong)packet).Time);
          */
        }
        break;

        case ACServerProtocol.PositionUpdate:
          packet = reader.ReadPacket<PositionUpdateIn>( );
          break;
        case ACServerProtocol.MegaPacket:
          packet = reader.ReadPacket<BatchedPositionUpdate>( );
          break;
        case ACServerProtocol.Extended:
          byte type = reader.Read<byte>( );

          if( type == (byte)CSPMessageTypeUdp.CustomUpdate )
          {
            packet = reader.ReadPacket<CSPPositionUpdate>( );
          }
          break;
      }
#if DEBUG
      packet ??= new DebugPacket( id );
#endif
      if( packet is not null && _address != null )
        IncomingPacketChannel.Writer.TryWrite( packet );
    }
    catch( Exception )
    {
#if DEBUG
      throw;
#endif
    }
  }
}

public class VSUdpClient : VSUdpBase
{
  public int nPing = 0;
  public int nServerOffset = 0;

  public VSUdpClient( ushort port,Stopwatch timeSource ) : base( port,timeSource )
  {
  }
  public VSUdpClient( Socket s,Stopwatch timeSource ) : base( s,timeSource )
  {
  }

  public override void OnStarted( )
  {
  }
  public override void OnReceived( SocketAddress address,byte[] buffer,int size )
  {
    try
    {
      var reader = new PacketReader(null, buffer.AsMemory()[..size]);
      var id = (ACServerProtocol)reader.Read<byte>();

      INetworkPacket? packet = null;
      switch( id )
      {
        case ACServerProtocol.CarConnect:
          _address = address;

          packet = new CarConnect( );

          break;
        case ACServerProtocol.LobbyCheck:
          break;
        case ACServerProtocol.SessionRequest:
          break;

        case ACServerProtocol.PingUpdate:
          packet = reader.ReadPacket<PingUpdate>( );

          int nTime = (int)((PingUpdate)packet).Time;
          int nOffset = (int)(_timeSource.ElapsedMilliseconds);

          PingPong pong = new( nTime,nOffset );

          SendPacket<PingPong>( address,pong );
          break;
        case ACServerProtocol.PingPong:
          packet = reader.ReadPacket<PingPong>( );
          break;

        case ACServerProtocol.PositionUpdate:
          packet = reader.ReadPacket<PositionUpdateOut>( );
          break;
        case ACServerProtocol.MegaPacket:
          packet = reader.ReadPacket<BatchedPositionUpdate>( );
          break;
        case ACServerProtocol.CarSetup:
          packet = reader.ReadPacket<CarSetup>( );
          break;
        case ACServerProtocol.Extended:
          byte type = reader.Read<byte>( );

          if( type == (byte)CSPMessageTypeUdp.CustomUpdate )
          {
            //packet = reader.ReadPacket<CSPPositionUpdate>( );
          }
          else if( type == (byte)CSPMessageTypeUdp.WeatherUpdate )
          {
            int c = 0;
          }
          else if( type == (byte)CSPMessageTypeUdp.ClientMessage )
          {
            var packetType = reader.Read<CSPClientMessageType>();

            packet ??= new DebugPacket( id,packetType.ToString( ) );
          }
          break;
      }
#if DEBUG
      packet ??= new DebugPacket( id );
#endif
      if( packet is not null )
        IncomingPacketChannel.Writer.TryWrite( packet );
    }
    catch( Exception )
    {
#if DEBUG
      throw;
#endif
    }
  }
}