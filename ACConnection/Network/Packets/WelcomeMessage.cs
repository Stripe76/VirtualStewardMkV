using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class WelcomeMessage : INetworkPacket
{
  public string? Message;

  public ACServerProtocol GetID( ) { return ACServerProtocol.WelcomeMessage; }

  public override string ToString( )
  {
    return $"""
            Message: {Message}
            """;
  }
  public void FromReader( PacketReader reader )
  {
    _ = reader.Read<byte>( );
    Message = reader.ReadUTF32String( true );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.WelcomeMessage );
    writer.Write<byte>( 0 );
    writer.WriteUTF32String( Message,true );
  }
}
