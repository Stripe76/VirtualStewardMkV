using ACConnection.Model;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct DamageUpdateIncoming : INetworkPacket
{
  public DamageZoneLevel DamageZoneLevel;

  public ACServerProtocol GetID( ) { return ACServerProtocol.DamageUpdate; }

  public override string ToString( )
  {
    return $"""
            DamageZoneLevel: {DamageZoneLevel}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    DamageZoneLevel = reader.Read<DamageZoneLevel>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    throw new NotImplementedException( );
  }
}