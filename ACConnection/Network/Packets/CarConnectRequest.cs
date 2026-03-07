using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class CarConnect : INetworkPacket
{
  public byte SessionId;

  public ACServerProtocol GetID( ) { return ACServerProtocol.CarConnect; }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.CarConnect );
    writer.Write( SessionId );
  }
}
