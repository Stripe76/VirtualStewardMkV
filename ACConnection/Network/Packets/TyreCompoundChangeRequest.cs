using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct TyreCompoundChangeRequest : INetworkPacket
{
  public string CompoundName;

  public ACServerProtocol GetID( ) { return ACServerProtocol.TyreCompoundChange; }

  public override string ToString( )
  {
    return $"""
            CompoundName: {CompoundName}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    CompoundName = reader.ReadUTF8String( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    throw new NotImplementedException( );
  }
}