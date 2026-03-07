using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class RawPacket : INetworkPacket
{
  public byte[]? Content;

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
    if( Content == null )
      throw new ArgumentNullException( nameof( Content ) );

    writer.WriteBytes( Content );
  }
}