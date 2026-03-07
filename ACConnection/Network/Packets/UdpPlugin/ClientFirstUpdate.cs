using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.UdpPlugin;

public readonly record struct ClientFirstUpdate : INetworkPacket
{
  public byte SessionId { get; init; }

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.UdpProtocol;
  }

  public void FromReader( PacketReader reader )
  {
    throw new NotImplementedException( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)UdpPluginProtocol.ClientFirstUpdate );
    writer.Write( SessionId );
  }
}