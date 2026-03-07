using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class ChecksumResponse : INetworkPacket
{
  public byte[] Checksums;

  public ChecksumResponse( )
  {
    Checksums = [];
  }

  public ChecksumResponse( byte[] checksums )
  {
    Checksums = checksums;
  }

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.Checksum;
  }

  public override string ToString( )
  {
    return $"""
            Checksums: {Checksums}
            """;
  }

  public void FromReader( PacketReader reader )
  {
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.Checksum );
    writer.WriteBytes( Checksums );
  }
}
