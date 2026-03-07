using System.Numerics;
using System.Runtime.InteropServices;
using ACConnection.Network.Packets;
using ACConnection.Network.Packets.Protocol;
using ACConnection.Utils;

[StructLayout( LayoutKind.Sequential,Pack = 1 )]
public struct PositionUpdateIn : INetworkPacket
{
  public byte PakSequenceId;
  public uint LastRemoteTimestamp;
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
  public float NormalizedPosition;

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.PositionUpdate;
  }

  public override string ToString( )
  {
    return $"""
            PakSequenceId: {PakSequenceId}
            LastRemoteTimestamp: {LastRemoteTimestamp}
            Position: {Position}
            Rotation: {Rotation}
            Velocity: {Velocity}
            TyreAngularSpeedFL: {TyreAngularSpeedFL}
            TyreAngularSpeedFR: {TyreAngularSpeedFR}
            TyreAngularSpeedRL: {TyreAngularSpeedRL}
            TyreAngularSpeedRR: {TyreAngularSpeedRR}
            SteerAngle: {SteerAngle}
            WheelAngle: {WheelAngle}
            EngineRpm: {EngineRpm}
            Gear: {Gear}
            StatusFlag: {StatusFlag}
            PerformanceDelta: {PerformanceDelta}
            Gas: {Gas}
            NormalizedPosition: {NormalizedPosition}
            """;
  }

  // Packets like this can crash the physics thread of other players
  public bool IsValid( )
  {
    return !Position.ContainsNaN( ) && !Rotation.ContainsNaN( ) && !Velocity.ContainsNaN( )
           && !Position.ContainsAbsLargerThan( 100_000.0f ) && !Velocity.ContainsAbsLargerThan( 500.0f );
  }

  public void FromReader( PacketReader reader )
  {
    PakSequenceId = reader.Read<byte>( );
    LastRemoteTimestamp = reader.Read<uint>( );
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
    StatusFlag = reader.Read<CarStatusFlags>( );
    PerformanceDelta = reader.Read<short>( );
    Gas = reader.Read<byte>( );
    NormalizedPosition = reader.Read<float>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)ACServerProtocol.PositionUpdate );
    writer.Write( PakSequenceId );
    writer.Write( LastRemoteTimestamp );
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
    writer.Write( PerformanceDelta );
    writer.Write( Gas );
  }
}

[StructLayout( LayoutKind.Sequential,Pack = 1 )]
public struct FirstPositionUpdateIn : INetworkPacket
{
  public byte PakSequenceId;
  public uint LastRemoteTimestamp;
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
  public float NormalizedPosition;

  public ACServerProtocol GetID( )
  {
    return ACServerProtocol.ClientFirstUpdate;
  }

  public override string ToString( )
  {
    return $"""
            PakSequenceId: {PakSequenceId}
            LastRemoteTimestamp: {LastRemoteTimestamp}
            Position: {Position}
            Rotation: {Rotation}
            Velocity: {Velocity}
            TyreAngularSpeedFL: {TyreAngularSpeedFL}
            TyreAngularSpeedFR: {TyreAngularSpeedFR}
            TyreAngularSpeedRL: {TyreAngularSpeedRL}
            TyreAngularSpeedRR: {TyreAngularSpeedRR}
            SteerAngle: {SteerAngle}
            WheelAngle: {WheelAngle}
            EngineRpm: {EngineRpm}
            Gear: {Gear}
            StatusFlag: {StatusFlag}
            PerformanceDelta: {PerformanceDelta}
            Gas: {Gas}
            NormalizedPosition: {NormalizedPosition}
            """;
  }

  // Packets like this can crash the physics thread of other players
  public bool IsValid( )
  {
    return !Position.ContainsNaN( ) && !Rotation.ContainsNaN( ) && !Velocity.ContainsNaN( )
           && !Position.ContainsAbsLargerThan( 100_000.0f ) && !Velocity.ContainsAbsLargerThan( 500.0f );
  }

  public void FromReader( PacketReader reader )
  {
    PakSequenceId = reader.Read<byte>( );
    LastRemoteTimestamp = reader.Read<uint>( );
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
    StatusFlag = reader.Read<CarStatusFlags>( );
    PerformanceDelta = reader.Read<short>( );
    Gas = reader.Read<byte>( );
    NormalizedPosition = reader.Read<float>( );
  }
  public void ToWriter( ref PacketWriter writer )
  {
    writer.Write( (byte)UdpPluginProtocol.ClientFirstUpdate );
    writer.Write( PakSequenceId );
    writer.Write( LastRemoteTimestamp );
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
    writer.Write( PerformanceDelta );
    writer.Write( Gas );
  }
}