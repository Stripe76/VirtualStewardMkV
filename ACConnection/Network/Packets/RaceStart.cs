using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class RaceStart : INetworkPacket
{
  public int StartTime;
  public uint TimeOffset;
  public ushort Ping;

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.RaceStart;
  }

  public override string ToString( )
  {
    return $"""
            StartTime: {StartTime}
            TimeOffset: {TimeOffset}
            Ping: {Ping}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    StartTime = reader.Read<int>( );
    TimeOffset = reader.Read<uint>( );
    Ping = reader.Read<ushort>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.RaceStart );
    writer.Write( StartTime );
    writer.Write( TimeOffset );
    writer.Write( Ping );
  }
}