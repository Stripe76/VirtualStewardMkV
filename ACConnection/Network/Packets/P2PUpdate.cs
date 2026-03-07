using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class P2PUpdate : INetworkPacket
{
  public byte SessionId;
  public short P2PCount;
  public bool Active;

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.P2PUpdate;
  }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            P2PCount: {P2PCount}
            Active: {Active}

            """;
  }

  public void FromReader( PacketReader reader )
  {
    throw new NotImplementedException( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.P2PUpdate );
    writer.Write( SessionId );
    writer.Write( P2PCount );
    writer.Write( Active );
  }
}