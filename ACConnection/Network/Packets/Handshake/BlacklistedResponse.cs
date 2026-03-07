using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.Handshake;

public readonly struct BlacklistedResponse : INetworkPacket
{

  public ACServerProtocol GetID( ) { return ACServerProtocol.AuthFailed; }

  public void FromReader( PacketReader reader )
  {
  }

  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.Handshake );
  }
}