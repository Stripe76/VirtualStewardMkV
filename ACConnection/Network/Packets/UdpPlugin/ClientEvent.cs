using System.Numerics;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets.UdpPlugin;

public readonly record struct ClientEvent : INetworkPacket
{
  public byte EventType { get; init; }
  public byte SessionId { get; init; }
  public byte? TargetSessionId { get; init; }
  public float Speed { get; init; }
  public Vector3 WorldPosition { get; init; }
  public Vector3 RelPosition { get; init; }

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.ClientEvent;
  }

  public void FromReader( PacketReader reader )
  {
    throw new NotImplementedException( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)UdpPluginProtocol.ClientEvent );
    writer.Write( EventType );
    writer.Write( SessionId );
    if( EventType == (byte)ClientEventType.CollisionWithCar )
    {
      if( !TargetSessionId.HasValue )
        throw new ArgumentException( "ClientEvent PlayerCollision had TargetSessionId null" );
      writer.Write( TargetSessionId.Value );
    }
    writer.Write( Speed );
    writer.Write( WorldPosition );
    writer.Write( RelPosition );
  }
}