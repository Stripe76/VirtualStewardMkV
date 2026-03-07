namespace VirtualSteward.ACNetwork.Weather;

public class WeatherData
{
  public WeatherFxType Type { get; set; }
  public WeatherFxType UpcomingType { get; set; }
  public ushort TransitionValue { get; set; }
  public double TransitionValueInternal { get; set; }
  public double TransitionDuration { get; set; }
  public float TemperatureAmbient { get; set; }
  public float TemperatureRoad { get; set; }
  public int Pressure { get; set; }
  public float Humidity { get; set; }
  public float WindSpeed { get; set; }
  public int WindDirection { get; set; }
  public float RainIntensity { get; set; }
  public float RainWetness { get; set; }
  public float RainWater { get; set; }
  public float TrackGrip { get; set; }

  public WeatherData( WeatherFxType type = WeatherFxType.None,WeatherFxType upcomingType = WeatherFxType.None )
  {
    Type = type;
    UpcomingType = upcomingType;

    TemperatureAmbient = 21;
    TemperatureRoad = 28;
    TrackGrip = 1;
  }
}