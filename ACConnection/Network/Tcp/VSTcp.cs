using System.Net.Sockets;
using System.Threading.Channels;
using Steamworks;
using ACConnection.Network.Packets.Protocol;
using ACConnection.Network.Packets;
using ACConnection.Network.Packets.Handshake;
using ACConnection.Utils;

namespace ACConnection.Network.Tcp;

public abstract class VSTcpBase
{
  private NetworkStream TcpStream { get; }
  private byte[] TcpSendBuffer { get; }

  private CancellationTokenSource DisconnectTokenSource { get; }
  private Task SendLoopTask { get; set; } = null!;
  private int _disconnectRequested = 0;

  private Channel<INetworkPacket> OutgoingPacketChannel { get; }
  private Channel<INetworkPacket> IncomingPacketChannel { get; }

  public bool IsDisconnectRequested => _disconnectRequested == 1;
  public bool IsConnected { get; set; }
  public TcpClient TcpClient { get; }
  public int MaxClients;

  public Channel<INetworkPacket> IncomingPackets { get => IncomingPacketChannel; }

  public VSTcpBase( TcpClient tcpClient )
  {
    TcpClient = tcpClient;

    tcpClient.ReceiveTimeout = (int)TimeSpan.FromMinutes( 5 ).TotalMilliseconds;
    tcpClient.SendTimeout = (int)TimeSpan.FromSeconds( 30 ).TotalMilliseconds;
    tcpClient.LingerState = new LingerOption( true,2 );
    tcpClient.NoDelay = true;

    TcpStream = tcpClient.GetStream( );

    TcpSendBuffer = GC.AllocateArray<byte>( ushort.MaxValue + 2,true );
    DisconnectTokenSource = new CancellationTokenSource( );

    IncomingPacketChannel = Channel.CreateBounded<INetworkPacket>( 256 );
    OutgoingPacketChannel = Channel.CreateBounded<INetworkPacket>( 256 );
  }

  public bool HasPackets( )
  {
    return IncomingPacketChannel.Reader.Count > 0;
  }

  public INetworkPacket? GetNextPacket( )
  {
    INetworkPacket? packet;

    IncomingPacketChannel.Reader.TryRead( out packet );

    return packet;
  }

  public Task StartAsync( )
  {
    SendLoopTask = Task.Run( SendLoopAsync );

    _ = Task.Run( ReceiveLoopAsync );

    return Task.CompletedTask;
  }
  public Task StopAsync( )
  {
    Task.WhenAny( Task.Delay( 2000 ),SendLoopTask ).Wait( );

    DisconnectTokenSource.Cancel( );
    DisconnectTokenSource.Dispose( );

    return Task.CompletedTask;
  }

  public void SendPacket<TPacket>( TPacket packet ) where TPacket : INetworkPacket
  {
    try
    {
      if( !OutgoingPacketChannel.Writer.TryWrite( packet ) && !IsDisconnectRequested )
      {
        _ = DisconnectAsync( );
      }
    }
    catch( Exception )
    {
#if DEBUG
      throw;
#endif
    }
  }

  public static byte[] GetSessionTicket( )
  {
    try
    {
      if( !SteamClient.IsValid )
        SteamClient.Init( 244210 );

      AuthTicket ticket = SteamUser.GetAuthSessionTicket( );

      return ticket.Data;
    }
    catch( Exception e )
    {
#if DEBUG
      throw;
#endif
    }
    return [0];
  }

  public void SendChecksum( HandshakeResponse hr,string sFolder )
  {
    ChecksumManager _checksumManager = new ChecksumManager( sFolder,hr.TrackName,hr.TrackConfig,hr.CarModel );

    _checksumManager.Initialize( );

    if( hr.ChecksumPaths is not null )
    {
      List<byte> checksums = new ( );
      foreach( string sCheck in hr.ChecksumPaths )
      {
        string sPath = sCheck;
        bool surfaceFix = false;

        if( sCheck.Contains( "/csp/" ) )
        {
          // "content/tracks/csp/1937/../magione/data/surfaces.ini"
          sPath = sCheck.Substring( 0,sCheck.IndexOf( "/csp/" ) );
          sPath += sCheck.Substring( sCheck.IndexOf( "/../" ) + 3 );

          surfaceFix = true;
        }
        if( ChecksumManager.TryCreateChecksum( $"{sFolder}/{sPath}",out var arCheck,surfaceFix ) )
        {
          checksums.AddRange( arCheck );
        }
        else
        {
          throw new Exception( );
        }
      }
      if( _checksumManager.CarChecksums.ContainsKey( hr.CarModel ) )
      {
        Dictionary<string,byte[]>? arCar = _checksumManager.CarChecksums.GetValueOrDefault( hr.CarModel );
        if( arCar != null )
        {
          foreach( var ar in arCar.Values )
            checksums.AddRange( ar );
        }
      }
      ChecksumResponse cr = new ChecksumResponse( checksums.ToArray( ) );

      SendPacket( cr );
    }
  }

  public abstract INetworkPacket? OnReceived( ACServerProtocol id,PacketReader reader );

  private async Task SendLoopAsync( )
  {
    try
    {
      await foreach( var packet in OutgoingPacketChannel.Reader.ReadAllAsync( DisconnectTokenSource.Token ) )
      {
        if( packet is BatchedPacket batched )
        {
          const int streamLength = 30000;
          var streamOffset = TcpSendBuffer.Length - streamLength;
          using var tempStream = new MemoryStream(TcpSendBuffer, streamOffset, streamLength);
          var tempBuffer = TcpSendBuffer.AsMemory(0, streamOffset);
          foreach( var inner in batched.Packets )
          {
            var writer = new PacketWriter(tempStream, tempBuffer);
            writer.WritePacket( inner );
            await writer.SendAsync( DisconnectTokenSource.Token );
          }

          await TcpStream.WriteAsync( TcpSendBuffer.AsMemory( streamOffset,(int)tempStream.Position ),DisconnectTokenSource.Token );
        }
        else
        {
          var writer = new PacketWriter(TcpStream, TcpSendBuffer);
          writer.WritePacket( packet );
          await writer.SendAsync( DisconnectTokenSource.Token );
        }
      }
    }
    catch( ChannelClosedException ) { }
    catch( ObjectDisposedException ) { }
    catch( OperationCanceledException ) { }
    catch( Exception )
    {
      _ = DisconnectAsync( );

#if DEBUG
      throw;
#endif
    }
  }
  private async Task ReceiveLoopAsync( )
  {
    byte[] buffer = new byte[2046];
    NetworkStream stream = TcpStream;

    try
    {
      while( !DisconnectTokenSource.IsCancellationRequested )
      {
        PacketReader reader = new PacketReader(stream, buffer);
        reader.SliceBuffer( await reader.ReadPacketAsync( ) );

        if( reader.Buffer.Length == 0 )
          continue;

        ACServerProtocol id = (ACServerProtocol)reader.Read<byte>();
        INetworkPacket? packet = null;
        try
        {
          packet = OnReceived( id,reader );
        }
        catch( Exception ex )
        {
          packet ??= new DebugPacket( id,ex.Message );
        }
#if DEBUG
        packet ??= new DebugPacket( id );
#endif
        if( packet is not null )
          IncomingPacketChannel.Writer.TryWrite( packet );
      }
    }
    catch( ObjectDisposedException ) { }
    catch( IOException ) { }
    catch( Exception ex )
    {
#if DEBUG
      throw;
#endif
    }
    /*
    finally
    {
      await DisconnectAsync( );
    }
    */
  }

  internal async Task DisconnectAsync( )
  {
    try
    {
      if( Interlocked.CompareExchange( ref _disconnectRequested,1,0 ) == 1 )
        return;

      await Task.Yield( );

      OutgoingPacketChannel.Writer.TryComplete( );
      _ = await Task.WhenAny( Task.Delay( 2000 ),SendLoopTask );

      try
      {
        DisconnectTokenSource.Cancel( );
        DisconnectTokenSource.Dispose( );
      }
      catch( ObjectDisposedException ) { }

      TcpClient.Dispose( );
    }
    catch( Exception ex )
    {
#if DEBUG
      throw;
#endif
    }
  }
}

public class VSTcpServer : VSTcpBase
{
  public VSTcpServer( TcpClient tcpClient ) : base( tcpClient )
  {
  }

  public override INetworkPacket? OnReceived( ACServerProtocol id,PacketReader reader )
  {
    INetworkPacket? packet = null;
    switch( id )
    {
      case ACServerProtocol.P2PUpdate:
        packet = reader.ReadPacket<P2PUpdateRequest>( );
        break;
      case ACServerProtocol.MandatoryPitUpdate:
        packet = reader.ReadPacket<MandatoryPitUpdate>( );
        break;
      case ACServerProtocol.Handshake:
        //packet = reader.ReadPacket<BlacklistedResponse>( );
        break;
      case ACServerProtocol.WrongPassword:
        packet = reader.ReadPacket<WrongPasswordResponse>( );
        break;
      case ACServerProtocol.RequestNewConnection:
        packet = reader.ReadPacket<HandshakeRequest>( );
        break;
      case ACServerProtocol.NewCarConnection:
        packet = reader.ReadPacket<HandshakeResponse>( );
        break;
      case ACServerProtocol.CarListRequest:
        packet = reader.ReadPacket<CarListRequest>( );
        break;
      case ACServerProtocol.CarList:
        packet = reader.ReadPacket<CarListResponse>( );
        break;
      case ACServerProtocol.ServerRunning:
        break;
      case ACServerProtocol.UnsupportedProtocol:
        packet = reader.ReadPacket<UnsupportedProtocolResponse>( );
        break;
      case ACServerProtocol.CleanExitDrive:
        packet = new ProtocolPacket( ACServerProtocol.CleanExitDrive );
        break;
      case ACServerProtocol.Checksum:
        packet = reader.ReadPacket<ChecksumResponse>( );
        break;
      case ACServerProtocol.NoSlotsAvailable:
        packet = reader.Read<NoSlotsAvailableResponse>( );
        break;
      case ACServerProtocol.PositionUpdate:
        break;
      case ACServerProtocol.Chat:
        packet = reader.ReadPacket<ChatMessage>( );
        break;
      case ACServerProtocol.MegaPacket:
        packet = reader.ReadPacket<BatchedPositionUpdate>( );
        break;
      case ACServerProtocol.LapCompleted:
        packet = reader.ReadPacket<LapCompletedServer>( );
        break;
      case ACServerProtocol.CurrentSessionUpdate:
        packet = new CurrentSessionUpdate( MaxClients );
        packet.FromReader( reader );
        break;
      case ACServerProtocol.RaceOver:
        break;
      case ACServerProtocol.Pulse:
        break;
      case ACServerProtocol.CarDisconnected:
        packet = reader.ReadPacket<CarDisconnected>( );
        break;
      case ACServerProtocol.CarConnect:
        break;
      case ACServerProtocol.SessionRequest:
        packet = reader.ReadPacket<SessionRequest>( );
        break;
      case ACServerProtocol.TyreCompoundChange:
        packet = reader.ReadPacket<TyreCompoundUpdate>( );
        break;
      case ACServerProtocol.WelcomeMessage:
        packet = reader.ReadPacket<WelcomeMessage>( );
        break;
      case ACServerProtocol.CarSetup:
        break;
      case ACServerProtocol.DrsZonesUpdate:
        break;
      case ACServerProtocol.SunAngleUpdate:
        packet = reader.ReadPacket<SunAngleUpdate>( );
        break;
      case ACServerProtocol.DamageUpdate:
        packet = reader.ReadPacket<DamageUpdateServer>( );
        break;
      case ACServerProtocol.RaceStart:
        break;
      case ACServerProtocol.SectorSplit:
        break;
      case ACServerProtocol.CarConnected:
        packet = reader.ReadPacket<CarConnectedServer>( );
        break;
      case ACServerProtocol.DriverInfoUpdate:
        packet = reader.ReadPacket<DriverInfoUpdate>( );
        break;
      case ACServerProtocol.VoteNextSession:
        break;
      case ACServerProtocol.VoteRestartSession:
        break;
      case ACServerProtocol.VoteKickUser:
        break;
      case ACServerProtocol.VoteQuorumNotReached:
        break;
      case ACServerProtocol.KickCar:
        packet = reader.ReadPacket<KickCar>( );
        break;
      case ACServerProtocol.SessionClosed:
        break;
      case ACServerProtocol.AuthFailed:
        packet = reader.ReadPacket<AuthFailedResponse>( );
        break;
      case ACServerProtocol.BoPUpdate:
        packet = reader.ReadPacket<BallastUpdate>( );
        break;
      case ACServerProtocol.WeatherUpdate:
        packet = reader.ReadPacket<WeatherUpdate>( );
        break;
      case ACServerProtocol.ClientEvent:
        break;
      case ACServerProtocol.Extended:
        byte type = reader.Read<byte>( );

        if( type == (byte)CSPMessageTypeTcp.ClientMessage )
        {
          byte sessionID = reader.Read<byte>( );
          ushort msgType = reader.Read<ushort>( );

          if( msgType == (ushort)CSPClientMessageType.HandshakeOut )
          {
            var cspPacket = reader.ReadPacket<CSPHandshakeOut>();

          }
          else
          {

          }
        }
        break;
      case ACServerProtocol.LobbyCheck:
        break;
      case ACServerProtocol.PingPong:
        break;
      case ACServerProtocol.PingUpdate:
        break;
    }
    return packet;
  }
}

public class VSTcpClient : VSTcpBase
{
  public VSTcpClient( TcpClient tcpClient ) : base( tcpClient )
  {
  }

  public override INetworkPacket? OnReceived( ACServerProtocol id,PacketReader reader )
  {
    INetworkPacket? packet = null;
    switch( id )
    {
      case ACServerProtocol.P2PUpdate:
        packet = reader.ReadPacket<P2PUpdateRequest>( );
        break;
      case ACServerProtocol.MandatoryPitUpdate:
        packet = reader.ReadPacket<MandatoryPitUpdate>( );
        break;
      case ACServerProtocol.Handshake:
        packet = reader.ReadPacket<BlacklistedResponse>( );
        break;
      case ACServerProtocol.WrongPassword:
        packet = reader.ReadPacket<WrongPasswordResponse>( );
        break;
      case ACServerProtocol.RequestNewConnection:
        packet = reader.ReadPacket<HandshakeRequest>( );
        break;
      case ACServerProtocol.NewCarConnection:
        packet = reader.ReadPacket<HandshakeResponse>( );
        break;
      case ACServerProtocol.CarListRequest:
        packet = reader.ReadPacket<CarListRequest>( );
        break;
      case ACServerProtocol.CarList:
        packet = reader.ReadPacket<CarListResponse>( );
        break;
      case ACServerProtocol.ServerRunning:
        break;
      case ACServerProtocol.UnsupportedProtocol:
        packet = reader.ReadPacket<UnsupportedProtocolResponse>( );
        break;
      case ACServerProtocol.CleanExitDrive:
        break;
      case ACServerProtocol.Checksum:
        packet = reader.ReadPacket<ChecksumResponse>( );
        break;
      case ACServerProtocol.NoSlotsAvailable:
        packet = reader.Read<NoSlotsAvailableResponse>( );
        break;
      case ACServerProtocol.PositionUpdate:
        break;
      case ACServerProtocol.Chat:
        packet = reader.ReadPacket<ChatMessage>( );
        break;
      case ACServerProtocol.MegaPacket:
        packet = reader.ReadPacket<BatchedPositionUpdate>( );
        break;
      case ACServerProtocol.LapCompleted:
        packet = reader.ReadPacket<LapCompletedServer>( );
        break;
      case ACServerProtocol.CurrentSessionUpdate:
        packet = new CurrentSessionUpdate( MaxClients );
        packet.FromReader( reader );
        break;
      case ACServerProtocol.RaceOver:
        break;
      case ACServerProtocol.Pulse:
        break;
      case ACServerProtocol.CarDisconnected:
        packet = reader.ReadPacket<CarDisconnected>( );
        break;
      case ACServerProtocol.CarConnect:
        packet = reader.ReadPacket<CarConnect>( );
        break;
      case ACServerProtocol.SessionRequest:
        packet = reader.ReadPacket<SessionRequest>( );
        break;
      case ACServerProtocol.TyreCompoundChange:
        packet = reader.ReadPacket<TyreCompoundUpdate>( );
        break;
      case ACServerProtocol.WelcomeMessage:
        packet = reader.ReadPacket<WelcomeMessage>( );
        break;
      case ACServerProtocol.CarSetup:
        packet = reader.ReadPacket<CarSetup>( );
        break;
      case ACServerProtocol.DrsZonesUpdate:
        break;
      case ACServerProtocol.SunAngleUpdate:
        packet = reader.ReadPacket<SunAngleUpdate>( );
        break;
      case ACServerProtocol.DamageUpdate:
        packet = reader.ReadPacket<DamageUpdateServer>( );
        break;
      case ACServerProtocol.RaceStart:
        packet = reader.ReadPacket<RaceStart>( );
        break;
      case ACServerProtocol.SectorSplit:
        packet = reader.ReadPacket<SectorSplitOutgoing>( );
        break;
      case ACServerProtocol.CarConnected:
        packet = reader.ReadPacket<CarConnectedServer>( );
        break;
      case ACServerProtocol.DriverInfoUpdate:
        packet = reader.ReadPacket<DriverInfoUpdate>( );
        break;
      case ACServerProtocol.VoteNextSession:
        break;
      case ACServerProtocol.VoteRestartSession:
        break;
      case ACServerProtocol.VoteKickUser:
        break;
      case ACServerProtocol.VoteQuorumNotReached:
        break;
      case ACServerProtocol.KickCar:
        packet = reader.ReadPacket<KickCar>( );
        break;
      case ACServerProtocol.SessionClosed:
        break;
      case ACServerProtocol.AuthFailed:
        packet = reader.ReadPacket<AuthFailedResponse>( );
        break;
      case ACServerProtocol.BoPUpdate:
        packet = reader.ReadPacket<BallastUpdate>( );
        break;
      case ACServerProtocol.WeatherUpdate:
        packet = reader.ReadPacket<WeatherUpdate>( );
        break;
      case ACServerProtocol.ClientEvent:
        packet = reader.ReadPacket<ClientEvent>( );
        break;
      case ACServerProtocol.Extended:
        byte type = reader.Read<byte>( );

        if( type == (byte)CSPMessageTypeTcp.ClientMessage )
        {
          byte sessionID = reader.Read<byte>( );
          ushort msgType = reader.Read<ushort>( );

          if( msgType == (ushort)CSPClientMessageType.HandshakeIn )
          {
            uint minVersion = reader.Read<uint>( );
            bool requireWeatherFx = reader.Read<bool>( );
          }
          else
          {

          }
        }
        break;
      case ACServerProtocol.LobbyCheck:
        packet = reader.ReadPacket<LobbyCheck>( );
        break;
      case ACServerProtocol.PingPong:
        // UDP
        break;
      case ACServerProtocol.PingUpdate:
        // UDP
        break;
    }
    return packet;
  }
}
