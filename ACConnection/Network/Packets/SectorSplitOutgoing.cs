using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class SectorSplitOutgoing : INetworkPacket
{
  public byte SessionId;
  public byte SplitIndex;
  public uint SplitTime;
  public byte Cuts;

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.SectorSplit;
  }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            SplitIndex: {SplitIndex}
            SplitTime: {SplitTime}
            Cuts: {Cuts}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    SplitIndex = reader.Read<byte>( );
    SplitTime = reader.Read<uint>( );
    Cuts = reader.Read<byte>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.SectorSplit );
    writer.Write( SessionId );
    writer.Write( SplitIndex );
    writer.Write( SplitTime );
    writer.Write( Cuts );
  }
}