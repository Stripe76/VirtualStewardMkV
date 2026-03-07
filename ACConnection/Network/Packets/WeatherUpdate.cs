using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class WeatherUpdate : INetworkPacket
{
  public byte Ambient;
  public byte Road;
  public string? Graphics;
  public short WindSpeed;
  public short WindDirection;

  public ACServerProtocol GetID( ) { return ACServerProtocol.WeatherUpdate; }

  public void FromReader( PacketReader reader )
  {
    Ambient = reader.Read<byte>( );
    Road = reader.Read<byte>( );
    Graphics = reader.ReadUTF32String( );
    WindSpeed = reader.Read<short>( );
    WindDirection = reader.Read<short>( );
  }

  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.WeatherUpdate );
    writer.Write( Ambient );
    writer.Write( Road );
    writer.WriteUTF32String( Graphics );
    writer.Write( WindSpeed );
    writer.Write( WindDirection );
  }

  public override string ToString( )
  {
    return $"{nameof( Ambient )}: {Ambient}, {nameof( Road )}: {Road}, {nameof( Graphics )}: {Graphics}, {nameof( WindSpeed )}: {WindSpeed}, {nameof( WindDirection )}: {WindDirection}";
  }
}