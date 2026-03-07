using ACConnection.Model;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class DamageUpdateServer : INetworkPacket
{
  public byte SessionId;
  public DamageZoneLevel DamageZoneLevel;

  public ACServerProtocol GetID( ) { return ACServerProtocol.DamageUpdate; }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    DamageZoneLevel = reader.Read<DamageZoneLevel>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( ACServerProtocol.DamageUpdate );
    writer.Write( SessionId );
    writer.Write( DamageZoneLevel );
  }
}