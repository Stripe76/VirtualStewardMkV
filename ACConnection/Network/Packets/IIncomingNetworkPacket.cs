using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public interface IIncomingNetworkPacketold
{
  ACServerProtocol GetID( );

  void FromReader( PacketReader reader );
}
