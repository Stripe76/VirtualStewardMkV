using ACConnection.Model;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

public class CurrentSessionUpdate : INetworkPacket
{
  public Session? CurrentSession;
  public float TrackGrip;
  public List<byte> Grid = [];
  public long StartTime;

  public int Cars;

  public CurrentSessionUpdate( )
  {

  }

  public CurrentSessionUpdate( int cars )
  {
    Cars = cars;
  }

  public ACServerProtocol GetID( ) { return ACServerProtocol.CurrentSessionUpdate; }

  public override string ToString( )
  {
    return $"""
            CurrentSession: {CurrentSession?.ToString( )}
            TrackGrip: {TrackGrip}
            StartTime: {StartTime}
            Grid: {Grid}
            """;
  }
  public void FromReader( PacketReader reader )
  {
    CurrentSession = new( );

    CurrentSession.Name = reader.ReadUTF8String( );
    CurrentSession.Id = reader.Read<byte>( );
    CurrentSession.Type = (SessionType)reader.Read<byte>( );
    CurrentSession.Time = reader.Read<ushort>( );
    CurrentSession.Laps = reader.Read<ushort>( );
    TrackGrip = reader.Read<float>( );

    for( int i = 0; i < Cars; i++ )
      Grid.Add( reader.Read<byte>( ) );

    StartTime = reader.Read<long>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    ArgumentNullException.ThrowIfNull( CurrentSession );
    ArgumentNullException.ThrowIfNull( Grid );

    writer.Write( (byte)ACServerProtocol.CurrentSessionUpdate );
    writer.WriteUTF8String( CurrentSession.Name );
    writer.Write( (byte)CurrentSession.Id );
    writer.Write( (byte)CurrentSession.Type );
    writer.Write( (ushort)CurrentSession.Time );
    writer.Write( (ushort)CurrentSession.Laps );
    writer.Write( TrackGrip );

    foreach( var car in Grid )
      writer.Write( car );

    writer.Write( StartTime );
  }
}