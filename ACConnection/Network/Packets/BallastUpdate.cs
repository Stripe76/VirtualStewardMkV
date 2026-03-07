using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class BallastUpdate : INetworkPacket
{
  public byte SessionId;
  public float BallastKg;
  public float Restrictor;

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.BoPUpdate;
  }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            BallastKg: {BallastKg}
            Restrictor: {Restrictor}

            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    BallastKg = reader.Read<float>( );
    Restrictor = reader.Read<float>( );
  }

  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.BoPUpdate );
    writer.Write<byte>( 1 );
    writer.Write( SessionId );
    writer.Write( BallastKg );
    writer.Write( Restrictor );
  }
}