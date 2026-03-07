using ACConnection.Model;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.UdpPlugin;

public struct SetSessionInfo : INetworkPacket
{
  public byte SessionIndex;
  public string SessionName;
  public SessionType SessionType;
  public int Laps;
  public int Time;
  public uint WaitTime;

  public ACServerProtocol GetID( ) { return ACServerProtocol.UdpProtocol; }

  public void FromReader( PacketReader reader )
  {
    SessionIndex = reader.Read<byte>( );
    SessionName = reader.ReadUTF32String( );
    SessionType = reader.Read<SessionType>( );
    Laps = reader.Read<int>( );
    Time = reader.Read<int>( );
    WaitTime = reader.Read<uint>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    throw new NotImplementedException( );
  }
}