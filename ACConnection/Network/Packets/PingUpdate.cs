using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct PingUpdate : INetworkPacket
{
  public uint Time;
  public ushort CurrentPing;

  public PingUpdate( uint time,ushort currentPing )
  {
    Time = time;
    CurrentPing = currentPing;
  }

  public ACServerProtocol GetID( ) { return ACServerProtocol.PingUpdate; }

  public override string ToString( )
  {
    return $"""
            Time: {Time}
            CurrentPing: {CurrentPing}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    Time = reader.Read<uint>( );
    CurrentPing = reader.Read<ushort>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.PingUpdate );
    writer.Write( Time );
    writer.Write( CurrentPing );
  }
}

public struct PingPong : INetworkPacket
{
  public int Time;
  public int TimeOffset;

  public PingPong( int time,int timeOffset )
  {
    Time = time;
    TimeOffset = timeOffset;
  }

  public ACServerProtocol GetID( ) { return ACServerProtocol.PingPong; }

  public override string ToString( )
  {
    return $"""
            Time: {Time}
            TimeOffset: {TimeOffset}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    Time = reader.Read<int>( );
    TimeOffset = reader.Read<int>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.PingPong );
    writer.Write( Time );
    writer.Write( TimeOffset );
  }
}

public struct LobbyCheck : INetworkPacket
{
  public ushort HttpPort;

  public LobbyCheck( ushort port )
  {
    HttpPort = port;
  }

  public ACServerProtocol GetID( ) { return ACServerProtocol.LobbyCheck; }

  public override string ToString( )
  {
    return $"""
            HttpPort: {HttpPort}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    HttpPort = reader.Read<ushort>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.LobbyCheck );
    writer.Write( HttpPort );
  }
}