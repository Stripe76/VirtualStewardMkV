using VirtualSteward.ACNetwork.Weather;

namespace VirtualSteward.ACNetwork.Shared;

public class ACServerSettings
{
  public string TrackID = string.Empty;
  public string VariantID = string.Empty;
  public string WelcomeMessage = "Virtual Steward Server";

  public string ServerName = "Virtual Steward";
  public string ServerAddress = "127.0.0.1";
#if DEBUG
  public int HttpPort = 8080;
#else
  public int HttpPort = 8081;
#endif
  public int TcpPort = 9600;
  public int UdpPort = 9601;
  public int ServerFrequency = 18;

  public float TrackGrip = 1.0f;
  public float FuelRate = 0;
  public float TiresWear = 0;
  public float TimeOfDay = 0;

  public ACServerWeather Weather = new( );

  public bool TiresBlanket = true;
  public bool ExtendedCarPhysic = false;
  public bool ExtendedTrackPhysic = false;
  public bool RecalcVelocities = true;
  public bool BatchedUpdates = true;
  public bool AllowWrongWay = true;
  public bool DisableCollisions = false;
  public bool EnableRain = false;
  public bool? HeadlightsOnOff = null;

  public string? CSPSettingsFile = null;
}

public class ACServerWeather
{
  public string Graphics = "3_clear";
  public short WindSpeed = 0;
  public short WindDirection = 0;

  public WeatherData WeatherData = new ( );
}

public class ACServerSlot( int sessionID,string carModel,string carSkin,string playerName,string playerTeam,string playerNation )
{
  public int SessionID { get; set; } = sessionID;

  public string CarModel { get; set; } = carModel;
  public string CarSkin { get; set; } = carSkin;

  public string PlayerName { get; set; } = playerName;
  public string PlayerTeam { get; set; } = playerTeam;
  public string PlayerNation { get; set; } = playerNation;
}

public class ACServerCar( string name,int available,int connected )
{
  public string Name { get; set; } = name;
  public string Model { get; set; } = name;
  public string ServerAvailability { get => $"{Model} ({Available - Connected}\\{Available})"; }

  public int Available { get; set; } = available;
  public int Connected { get; set; } = connected;

  public override string ToString( )
  {
    return Model;
  }
}

