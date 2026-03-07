using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.UdpPlugin;

public readonly record struct Chat : INetworkPacket
{
  public byte SessionId { get; init; }
  public string? Message { get; init; }

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.Chat;
  }

  public void FromReader( PacketReader reader )
  {
    throw new NotImplementedException( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)UdpPluginProtocol.Chat );
    writer.Write( SessionId );
    writer.WriteUTF32String( Message );
  }
}