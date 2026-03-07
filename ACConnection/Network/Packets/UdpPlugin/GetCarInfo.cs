using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.UdpPlugin;

public struct GetCarInfo : INetworkPacket
{
  public byte SessionId;

  public ACServerProtocol GetID( ) { return ACServerProtocol.DriverInfoUpdate; }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    throw new NotImplementedException( );
  }
}