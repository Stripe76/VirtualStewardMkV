using System.Text;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class CSPKickBanMessageOverride : INetworkPacket
{
  public string? Message;

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
    writer.Write( (byte)ACServerProtocol.Extended );
    writer.Write( (byte)CSPMessageTypeTcp.KickBanMessage );
    writer.WriteString( Message,Encoding.UTF8,4 );
  }
}