using ACConnection.Model;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct SessionRequest : INetworkPacket
{
  public SessionType SessionType;

  public ACServerProtocol GetID( ) { return ACServerProtocol.SessionRequest; }

  public override string ToString( )
  {
    return $"""
            SessionType: {SessionType}

            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionType = reader.Read<SessionType>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.SessionRequest );
    writer.Write( (byte)SessionType );
  }
}