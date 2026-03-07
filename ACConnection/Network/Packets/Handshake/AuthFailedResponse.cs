using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.Handshake;

public struct AuthFailedResponse : INetworkPacket
{
  public string Reason;

  public AuthFailedResponse( string reason )
  {
    Reason = reason;
  }

  public ACServerProtocol GetID( ) { return ACServerProtocol.AuthFailed; }

  public override string ToString( )
  {
    return $"""
            Reason: {Reason}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    Reason = reader.ReadUTF32String( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.AuthFailed );
    writer.WriteUTF32String( Reason );
  }
}