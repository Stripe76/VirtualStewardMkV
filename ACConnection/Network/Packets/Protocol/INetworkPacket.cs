namespace ACConnection.Network.Packets.Protocol;

public interface INetworkPacket
{
  ACServerProtocol GetID( );

  void FromReader( PacketReader reader );
  void ToWriter( ref PacketWriter writer );
}