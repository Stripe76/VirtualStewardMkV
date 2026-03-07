using System.Numerics;
using System.Diagnostics.CodeAnalysis;
using ACConnection.Network.Packets.Protocol;

namespace ACConnection.Network.Packets;

[Flags]
public enum CarStatusFlags
{
  BrakeLightsOn = 0x10,
  LightsOn = 0x20,
  Horn = 0x40,
  IndicateLeft = 0x800,
  IndicateRight = 0x1000,
  HazardsOn = 0x2000,
  HighBeamsOff = 0x4000,
  WiperLevel1 = 0x200000,
  WiperLevel2 = 0x400000,
  WiperLevel3 = WiperLevel1 | WiperLevel2,
  AllFlags = 0b01111111111111111111111111111111,
}

[SuppressMessage( "ReSharper","InconsistentNaming" )]
public struct PositionUpdateOut : INetworkPacket
{
  public byte SessionId;
  public byte PakSequenceId;
  public uint Timestamp;
  public ushort Ping;
  public Vector3 Position;
  public Vector3 Rotation;
  public Vector3 Velocity;
  public byte TyreAngularSpeedFL;
  public byte TyreAngularSpeedFR;
  public byte TyreAngularSpeedRL;
  public byte TyreAngularSpeedRR;
  public byte SteerAngle;
  public byte WheelAngle;
  public ushort EngineRpm;
  public byte Gear;
  public CarStatusFlags StatusFlag;
  public short PerformanceDelta;
  public byte Gas;

  public PositionUpdateOut( byte sessionId,
      byte pakSequenceId,
      uint timestamp,
      ushort ping,
      Vector3 position,
      Vector3 rotation,
      Vector3 velocity,
      byte tyreAngularSpeedFl,
      byte tyreAngularSpeedFr,
      byte tyreAngularSpeedRl,
      byte tyreAngularSpeedRr,
      byte steerAngle,
      byte wheelAngle,
      ushort engineRpm,
      byte gear,
      CarStatusFlags statusFlag,
      short performanceDelta,
      byte gas )
  {
    SessionId = sessionId;
    PakSequenceId = pakSequenceId;
    Timestamp = timestamp;
    Ping = ping;
    Position = position;
    Rotation = rotation;
    Velocity = velocity;
    TyreAngularSpeedFL = tyreAngularSpeedFl;
    TyreAngularSpeedFR = tyreAngularSpeedFr;
    TyreAngularSpeedRL = tyreAngularSpeedRl;
    TyreAngularSpeedRR = tyreAngularSpeedRr;
    SteerAngle = steerAngle;
    WheelAngle = wheelAngle;
    EngineRpm = engineRpm;
    Gear = gear;
    StatusFlag = statusFlag;
    PerformanceDelta = performanceDelta;
    Gas = gas;
  }

  public ACServerProtocol GetID( ) { return ACServerProtocol.PositionUpdate; }

  public override string ToString( )
  {
    return $"""
            SessionId: {SessionId}
            pakSequenceId: {PakSequenceId}
            timestamp: {Timestamp}
            ping: {Ping}
            position: {Position}
            rotation: {Rotation}
            velocity: {Velocity}
            tyreAngularSpeedFl: {TyreAngularSpeedFL}
            tyreAngularSpeedFr: {TyreAngularSpeedFR}
            tyreAngularSpeedRl: {TyreAngularSpeedRL}
            tyreAngularSpeedRr: {TyreAngularSpeedRR}
            steerAngle: {SteerAngle}
            wheelAngle: {WheelAngle}
            engineRpm: {EngineRpm}
            gear: {Gear}
            statusFlag: {StatusFlag}
            performanceDelta: {PerformanceDelta}
            gas: {Gas}
            """;
  }

  public void FromReader( PacketReader reader ) => FromReader( reader,false );
  public void FromReader( PacketReader reader,bool batched )
  {
    SessionId = reader.Read<byte>( );
    PakSequenceId = reader.Read<byte>( );
    Timestamp = reader.Read<uint>( );
    Ping = reader.Read<ushort>( );
    Position = reader.Read<Vector3>( );
    Rotation = reader.Read<Vector3>( );
    Velocity = reader.Read<Vector3>( );
    TyreAngularSpeedFL = reader.Read<byte>( );
    TyreAngularSpeedFR = reader.Read<byte>( );
    TyreAngularSpeedRL = reader.Read<byte>( );
    TyreAngularSpeedRR = reader.Read<byte>( );
    SteerAngle = reader.Read<byte>( );
    WheelAngle = reader.Read<byte>( );
    EngineRpm = reader.Read<ushort>( );
    Gear = reader.Read<byte>( );
    StatusFlag = (CarStatusFlags)reader.Read<uint>( );

    if( !batched )
    {
      PerformanceDelta = reader.Read<short>( );
      Gas = reader.Read<byte>( );
    }
  }
  public void FromReaderCustom( PacketReader reader )
  {
    SessionId = reader.Read<byte>( );
    PakSequenceId = reader.Read<byte>( );
    Timestamp = reader.Read<uint>( );
    Ping = reader.Read<ushort>( );
    Position = reader.Read<Vector3>( );
    Rotation = reader.Read<Vector3>( );
    Velocity = reader.Read<Vector3>( );
    TyreAngularSpeedFL = reader.Read<byte>( );
    TyreAngularSpeedFR = reader.Read<byte>( );
    TyreAngularSpeedRL = reader.Read<byte>( );
    TyreAngularSpeedRR = reader.Read<byte>( );
    SteerAngle = reader.Read<byte>( );
    WheelAngle = reader.Read<byte>( );
    EngineRpm = reader.Read<ushort>( );
    Gear = reader.Read<byte>( );
    StatusFlag = (CarStatusFlags)reader.Read<uint>( );
    Gas = reader.Read<byte>( );
    PerformanceDelta = reader.Read<short>( );
  }

  public void ToWriter( ref PacketWriter writer ) => ToWriter( ref writer,false );
  public void ToWriter( ref PacketWriter writer,bool batched )
  {
    if( !batched )
      writer.Write( (byte)ACServerProtocol.PositionUpdate );
    writer.Write( SessionId );
    writer.Write( PakSequenceId );
    writer.Write( Timestamp );
    writer.Write( Ping );
    writer.Write( Position );
    writer.Write( Rotation );
    writer.Write( Velocity );
    writer.Write( TyreAngularSpeedFL );
    writer.Write( TyreAngularSpeedFR );
    writer.Write( TyreAngularSpeedRL );
    writer.Write( TyreAngularSpeedRR );
    writer.Write( SteerAngle );
    writer.Write( WheelAngle );
    writer.Write( EngineRpm );
    writer.Write( Gear );
    writer.Write( (uint)StatusFlag );
    if( !batched )
    {
      writer.Write( PerformanceDelta );
      writer.Write( Gas );
    }
  }
  public void ToWriterCustom( ref PacketWriter writer )
  {
    writer.Write( SessionId );
    writer.Write( PakSequenceId );
    writer.Write( Timestamp );
    writer.Write( Ping );
    writer.Write( Position );
    writer.Write( Rotation );
    writer.Write( Velocity );
    writer.Write( TyreAngularSpeedFL );
    writer.Write( TyreAngularSpeedFR );
    writer.Write( TyreAngularSpeedRL );
    writer.Write( TyreAngularSpeedRR );
    writer.Write( SteerAngle );
    writer.Write( WheelAngle );
    writer.Write( EngineRpm );
    writer.Write( Gear );
    writer.Write( (uint)StatusFlag );
    writer.Write( Gas );
    writer.Write( PerformanceDelta );
  }
}