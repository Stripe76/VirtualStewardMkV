using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct CarListRequest : INetworkPacket
{
  public int PageIndex;

  public ACServerProtocol GetID( ) { return ACServerProtocol.CarListRequest; }

  public override string ToString( )
  {
    return $"""
            PageIndex: {PageIndex}

            """;
  }

  public void FromReader( PacketReader reader )
  {
    PageIndex = reader.Read<byte>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.CarListRequest );
    writer.Write( (byte)PageIndex );
  }
}