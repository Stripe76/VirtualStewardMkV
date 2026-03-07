using ACConnection.Model;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class CSPHandshakeOut : INetworkPacket
{
  public uint Version;
  public bool IsWeatherFxActive;
  public InputMethod InputMethod;
  public bool IsRainFxActive;
  public ulong UniqueKey;

  public ACServerProtocol GetID( ) { return ACServerProtocol.Handshake; }

  public void FromReader( PacketReader reader )
  {
    Version = reader.Read<uint>( );
    IsWeatherFxActive = reader.Read<bool>( );
    InputMethod = reader.Read<InputMethod>( );
    IsRainFxActive = reader.Read<bool>( );
    reader.Read<byte>( ); // Padding
    UniqueKey = reader.Read<ulong>( );
  }

  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( ACServerProtocol.Extended );
    writer.Write( CSPMessageTypeTcp.ClientMessage );
    writer.Write( CSPClientMessageType.HandshakeOut );

    writer.Write( Version );
    writer.Write( IsWeatherFxActive );
    writer.Write( InputMethod );
    writer.Write( IsRainFxActive );
    writer.Write( (byte)0 ); // Padding
    writer.Write( UniqueKey );
  }
}