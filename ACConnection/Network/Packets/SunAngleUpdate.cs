using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class SunAngleUpdate : INetworkPacket
{
  public float SunAngle;

  public ACServerProtocol GetID( ) { return ACServerProtocol.SunAngleUpdate; }

  public override string ToString( )
  {
    return $"""
            SunAngle: {SunAngle:0.00}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    SunAngle = reader.Read<float>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.SunAngleUpdate );
    writer.Write( SunAngle );
  }
}