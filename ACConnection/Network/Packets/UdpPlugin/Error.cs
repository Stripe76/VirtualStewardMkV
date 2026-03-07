using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.UdpPlugin;

public readonly record struct Error : INetworkPacket
{
  public string? Message { get; init; }

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.UnsupportedProtocol;
  }

  public void FromReader( PacketReader reader )
  {
    throw new NotImplementedException( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)UdpPluginProtocol.Error );
    writer.WriteUTF32String( Message );
  }
}