using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.Handshake;

public readonly struct NoSlotsAvailableResponse : INetworkPacket
{
  public ACServerProtocol GetID( ) { return ACServerProtocol.NoSlotsAvailable; }

  public void FromReader( PacketReader reader )
  {
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.NoSlotsAvailable );
  }
}