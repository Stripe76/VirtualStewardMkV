using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class CSPHandshakeIn : INetworkPacket
{
  public uint MinVersion { get; set; }
  public bool RequiresWeatherFx { get; set; }

  public ACServerProtocol GetID( ) { return ACServerProtocol.Handshake; }

  public void FromReader( PacketReader reader )
  {
    reader.Read<byte>( );
    reader.Read<byte>( );
    reader.Read<ushort>( );
    MinVersion = reader.Read<uint>( );
    RequiresWeatherFx = reader.Read<bool>( );
  }

  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.Extended );
    writer.Write( (byte)CSPMessageTypeTcp.ClientMessage );
    writer.Write( (byte)255 );
    writer.Write( (ushort)CSPClientMessageType.HandshakeIn );
    writer.Write( MinVersion );
    writer.Write( RequiresWeatherFx );
  }
}