using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public enum CSPCarVisibility
{
  Visible = 0,
  Invisible = 1
}

public class CSPCarVisibilityUpdate : INetworkPacket
{
  public byte SessionId;
  public CSPCarVisibility Visible;

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
    writer.Write( (byte)CSPMessageTypeTcp.CarVisibilityUpdate );
    writer.Write( SessionId );
    writer.Write( (byte)Visible );
  }
}