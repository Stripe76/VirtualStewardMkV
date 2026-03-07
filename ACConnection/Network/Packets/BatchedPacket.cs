using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class BatchedPacket : INetworkPacket
{
  public List<INetworkPacket> Packets { get; } = [];

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.MegaPacket;
  }

  public void FromReader( PacketReader reader )
  {
    throw new NotImplementedException( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    throw new InvalidOperationException( "BatchedPacket can only be sent via TCP" );
  }
}