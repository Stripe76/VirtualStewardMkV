using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class MandatoryPitUpdate : INetworkPacket
{
  public byte SessionId;
  public bool MandatoryPit;

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.MandatoryPitUpdate;
  }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            MandatoryPit: {MandatoryPit}

            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    MandatoryPit = reader.Read<bool>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.MandatoryPitUpdate );
    writer.Write( SessionId );
    writer.Write( MandatoryPit );
  }
}