using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct DebugPacket( ACServerProtocol id,string message = null ) : INetworkPacket
{
  public ACServerProtocol ProtocolID = id;
  public string Message = message;

  public ACServerProtocol GetID( ) { return ProtocolID; }

  public override string ToString( )
  {
    return $"""
            DEBUG PACKET
            Message: {Message}
            """;
  }

  public void FromReader( PacketReader reader )
  {
  }
  public void ToWriter( ref PacketWriter writer )
  {
  }
}