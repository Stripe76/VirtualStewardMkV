using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public struct BatchedPositionUpdate : INetworkPacket
{
  public uint Timestamp;
  public ushort Ping;
  public ArraySegment<PositionUpdateOut> Updates;
  public List<PositionUpdateOut>? PosUpdates;

  public ACServerProtocol GetID( ) { return ACServerProtocol.PositionUpdate; }

  public override string ToString( )
  {
    return $"""
            Timestamp: {Timestamp}
            Ping: {Ping}
            PosUpdates: {PosUpdates?.ToString( )}
            """;
  }

  public BatchedPositionUpdate( uint timestamp,ushort ping,ArraySegment<PositionUpdateOut> updates )
  {
    Timestamp = timestamp;
    Ping = ping;
    Updates = updates;
  }

  public void FromReader( PacketReader reader )
  {
    Timestamp = reader.Read<uint>( );
    Ping = reader.Read<ushort>( );

    int nCount = reader.Read<byte>( );

    PosUpdates = new List<PositionUpdateOut>( nCount );
    for( int i = 0; i < nCount; i++ )
    {
      PositionUpdateOut pu = new ( );
      pu.FromReader( reader,true );

      PosUpdates.Add( pu );
    }
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.MegaPacket );
    writer.Write( Timestamp );
    writer.Write( Ping );
    writer.Write( (byte)Updates.Count );
    for( int i = 0; i < Updates.Count; i++ )
    {
      Updates[i].ToWriter( ref writer,true );
    }
  }
}