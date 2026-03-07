namespace VirtualSteward.ACNetwork.Weather;

public interface IWeatherTypeProvider
{
  public WeatherType GetWeatherType( WeatherFxType id );
}
