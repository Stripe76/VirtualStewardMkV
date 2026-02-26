using System;
using System.Numerics;
using ACLibrary.Data;
using Framework.Bindables;

namespace VirtualSteward.Datasources.ViewModels;

public class VMCarPosition
{
    public Vector3 Position;
    public Vector3 Rotation;

    public uint LapTime;
    public uint LastLapTime;

    public int BrakePedal;
}

public class VMCarData( ) : BindableBase
{
    public Vector3 Position;
    public Vector3 Rotation;

    public float SteeringWheel;

    public float GasPedal;
    public float BrakePedal;
    public float ClutchPedal;

    public float RPMs;
    public int Gear;
    public int Fuel;
}

public class VMServerData( ) : BindableBase
{
    public uint TimeStamp;

    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Velocity;

    public float WheelsAngle;
    public float SteeringWheel;

    public float FLAngular;
    public float FRAngular;
    public float RLAngular;
    public float RRAngular;

    public byte GasPedal;
    public byte BrakePedal;

    public ushort RPMs;
    public byte Gear;
    public int Fuel;

    public StatusFlags Flags;

#if DEBUG
    public Vector3h BodyOrientation;
    public Vector3hArray4 SusOrientation;
    public Vector3hArray4 TyreOrientation;
#endif

    [Flags]
    public enum StatusFlags
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
}