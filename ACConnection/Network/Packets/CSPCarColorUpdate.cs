using System.Drawing;

namespace ACConnection.Network.Packets;

public class CSPCarColorUpdate : CSPClientMessageOutgoing
{
  public Color Color { get; init; }

  public CSPCarColorUpdate( )
  {
    Type = CSPClientMessageType.CarColorChange;
  }

  protected override void ToWriter( BinaryWriter writer )
  {
    writer.Write( Color.R );
    writer.Write( Color.G );
    writer.Write( Color.B );
    writer.Write( Color.A );
  }
}
