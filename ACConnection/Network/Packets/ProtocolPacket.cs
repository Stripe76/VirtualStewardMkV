using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct ProtocolPacket( ACServerProtocol id ) : INetworkPacket
{
  public ACServerProtocol ProtocolID = id;

  public ACServerProtocol GetID( ) { return ProtocolID; }

  public override string ToString( )
  {
    return $"""
            ProtocolID: {ProtocolID}
            """;
  }

  public void FromReader( PacketReader reader )
  {
  }
  public void ToWriter( ref PacketWriter writer )
  {
  }
}