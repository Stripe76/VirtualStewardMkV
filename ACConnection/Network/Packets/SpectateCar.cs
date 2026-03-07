using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct SpectateCar : INetworkPacket
{
  public byte SessionId;
  public byte CameraMode;

  public ACServerProtocol GetID( ) { return ACServerProtocol.Handshake; }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    CameraMode = reader.Read<byte>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    throw new NotImplementedException( );
  }
}