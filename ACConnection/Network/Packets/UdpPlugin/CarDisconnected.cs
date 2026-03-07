using System.Text;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.UdpPlugin;

public readonly record struct CarDisconnected : INetworkPacket
{
  public string? DriverName { get; init; }
  public string? DriverGuid { get; init; }
  public byte SessionId { get; init; }
  public string? CarModel { get; init; }
  public string? CarSkin { get; init; }

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.CarDisconnected;
  }

  public void FromReader( PacketReader reader )
  {
    throw new NotImplementedException( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)UdpPluginProtocol.ClosedConnection );
    writer.WriteUTF32String( DriverName );
    writer.WriteUTF32String( DriverGuid );
    writer.Write( SessionId );
    writer.WriteString( CarModel,Encoding.UTF8 );
    writer.WriteString( CarSkin,Encoding.UTF8 );
  }
}