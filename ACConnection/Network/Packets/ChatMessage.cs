using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct ChatMessage : INetworkPacket
{
  public byte SessionId;
  public string Message;

  public readonly ACServerProtocol GetID( ) { return ACServerProtocol.Chat; }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            Message: {Message}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    Message = reader.ReadUTF32String( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.Chat );
    writer.Write( SessionId );
    writer.WriteUTF32String( Message );
  }
}