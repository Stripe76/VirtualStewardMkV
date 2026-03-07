using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct P2PUpdateRequest : INetworkPacket
{
  public short P2PCount;
  public bool Active;

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.P2PUpdate;
  }

  public override string ToString( )
  {
    return $"""
            P2PCount: {P2PCount}
            Active: {Active}

            """;
  }

  public void FromReader( PacketReader reader )
  {
    P2PCount = reader.Read<short>( );
    Active = reader.Read<bool>( );
  }

  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.P2PUpdate );
    writer.Write( P2PCount );
    writer.Write( Active );
  }
}