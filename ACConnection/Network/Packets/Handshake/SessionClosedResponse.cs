using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.Handshake;

public readonly struct SessionClosedResponse : INetworkPacket
{
  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.SessionClosed;
  }

  public void FromReader( PacketReader reader )
  {
    throw new NotImplementedException( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.SessionClosed );
  }
}