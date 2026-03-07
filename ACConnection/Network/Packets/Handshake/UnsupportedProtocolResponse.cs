using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.Handshake;

public struct UnsupportedProtocolResponse : INetworkPacket
{
  public ushort Protocol;

  public ACServerProtocol GetID( ) { return ACServerProtocol.UnsupportedProtocol; }

  public void FromReader( PacketReader reader )
  {
    Protocol = reader.Read<ushort>( );
  }

  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.UnsupportedProtocol );
    writer.Write( (ushort)202 );
  }
}