using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class CarConnectedServer : INetworkPacket
{
  public byte SessionId;
  public string? Name;
  public string? Nation;

  public ACServerProtocol GetID( ) { return ACServerProtocol.CarConnected; }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            Name: {Name}
            Nation: {Nation}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    Name = reader.ReadUTF8String( );
    Nation = reader.ReadUTF8String( );
  }

  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.CarConnected );
    writer.Write( SessionId );
    writer.WriteUTF8String( Name );
    writer.WriteUTF8String( Nation );
  }
}