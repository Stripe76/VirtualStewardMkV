using System.Text;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.UdpPlugin;

// TODO: this is currently unused
public readonly record struct EndSession : INetworkPacket
{
  public string? ReportJsonFilename { get; init; }

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
    writer.Write( (byte)UdpPluginProtocol.EndSession );
    writer.WriteString( ReportJsonFilename,Encoding.UTF8 );
  }
}