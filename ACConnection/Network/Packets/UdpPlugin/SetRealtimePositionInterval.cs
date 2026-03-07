using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.UdpPlugin;

public struct SetRealtimePositionInterval : INetworkPacket
{
  public ushort Interval;

  public ACServerProtocol GetID( ) { return ACServerProtocol.UdpProtocol; }

  public void FromReader( PacketReader reader )
  {
    Interval = reader.Read<ushort>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    throw new NotImplementedException( );
  }
}