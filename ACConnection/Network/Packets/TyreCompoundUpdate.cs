using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class TyreCompoundUpdate : INetworkPacket
{
  public byte SessionId;
  public string? CompoundName;

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.TyreCompoundChange;
  }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            CompoundName: {CompoundName}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    CompoundName = reader.ReadUTF8String( );
  }

  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.TyreCompoundChange );
    writer.Write( SessionId );
    writer.WriteUTF8String( CompoundName );
  }
}