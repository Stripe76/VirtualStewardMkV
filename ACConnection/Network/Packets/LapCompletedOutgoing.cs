using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class LapCompletedServer : INetworkPacket
{
  public byte SessionId;
  public uint LapTime;
  public byte Cuts;
  public CompletedLap[]? Laps;
  public float TrackGrip;

  public class CompletedLap( )
  {
    public byte SessionId;
    public uint LapTime;
    public ushort NumLaps;
    public byte HasCompletedLastLap;
  }

  public ACServerProtocol GetID( ) { return ACServerProtocol.LapCompleted; }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            LapTime: {LapTime}
            Cuts: {Cuts}
            Laps: {Laps}
            TrackGrip: {TrackGrip}
            """;
  }

  public void FromReader( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    LapTime = reader.Read<ushort>( );
    Cuts = reader.Read<byte>( );

    int nCount = reader.Read<byte>( );

    Laps = new CompletedLap[nCount];
    for( int i = 0; i < nCount; i++ )
    {
      Laps[i] = new CompletedLap( )
      {
        SessionId = reader.Read<byte>( ),
        LapTime = reader.Read<uint>( ),
        NumLaps = reader.Read<ushort>( ),
        HasCompletedLastLap = reader.Read<byte>( )
      };
    }
    TrackGrip = reader.Read<float>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    if( Laps == null )
      throw new ArgumentNullException( nameof( Laps ) );

    writer.Write( (byte)ACServerProtocol.LapCompleted );
    writer.Write( SessionId );
    writer.Write( LapTime );
    writer.Write( Cuts );
    writer.Write( (byte)Laps.Length );
    foreach( var lap in Laps )
    {
      writer.Write( lap.SessionId );
      writer.Write( lap.LapTime );
      writer.Write( lap.NumLaps );
      writer.Write( lap.HasCompletedLastLap );
    }
    writer.Write( TrackGrip );
  }
}
