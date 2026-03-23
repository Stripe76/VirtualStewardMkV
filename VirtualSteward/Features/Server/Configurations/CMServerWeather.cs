using Framework.UI.Configurations;
using Framework.UI.Values;
using VirtualSteward.ACNetwork.Shared;
using VirtualSteward.Features.Server.Values;

namespace VirtualSteward.Features.Server.Configurations;

public class CMServerWeather( ACServerSettings settings,Pages.Server.Server server ) : Configuration( "SERVER_WEATHER","Weather" )
{
    public RangedFloat TimeOfDay = new( 0,1,nameof( TimeOfDay ),"Time of day" )
    {
        Format = "0",
        FormatValue = ( value ) =>
        {
            int m = (int)(value * (24 * 60));
            return $"{m / 60:00}:{m % 60:00}";
        },
        ValueChanged = ( value ) => { settings.TimeOfDay = value; server.UpdateWeather(  ); },
        Value = .5f,
    };

    public HeadlightsValue Headlights = new HeadlightsValue( nameof( Headlights ),"Headlights" )
    {
        ValueChanged = ( value ) =>
        {
            settings.HeadlightsOnOff = value switch
            {
                ServerHeadligths.FromReplay => null,
                ServerHeadligths.AlwaysOn => true,
                ServerHeadligths.AlwaysOff => false,
                _ => settings.HeadlightsOnOff
            };
            server.UpdateWeather(  );
        }

    };

    public WeatherTypeValue WeatherType = new WeatherTypeValue( nameof( WeatherType ),"Weather" );

    public RangedFloat AmbientTemperature = new( 0,50,nameof( AmbientTemperature ),"Air temp." )
    {
        Unit = "°C",
        Format = "0.0",
        ValueChanged = ( value ) => { settings.Weather.WeatherData.TemperatureAmbient = value; server.UpdateWeather(  );},
        Value = 21f,
    };

    public RangedFloat TrackTemperature = new( 0,100,nameof( TrackTemperature ),"Track temp." )
    {
        Unit = "°C",
        Format = "0.0",
        ValueChanged = ( value ) => { settings.Weather.WeatherData.TemperatureRoad = value; server.UpdateWeather(  ); },
        Value = 30f,
    };

    public RangedFloat WindSpeed = new( 0,40,nameof( WindSpeed ),"Wind speed" )
    {
        Unit = " km/h",
        Format = "0.0",
        ValueChanged = ( value ) => { settings.Weather.WeatherData.WindSpeed = value; server.UpdateWeather(  ); },
        Value = 0f,
    };

    public RangedInt WindDirection = new( 0,360,nameof( WindSpeed ),"Wind direction" )
    {
        Unit = "°",
        Format = "0",
        ValueChanged = ( value ) => { settings.Weather.WeatherData.WindDirection = value; server.UpdateWeather(  ); },
        Value = 0,
    };

    public RangedFloat RainIntensity = new( 0,1,nameof( RainIntensity ),"Rain intensity" )
    {
        Format = "0.0",
        ValueChanged = ( value ) => { settings.Weather.WeatherData.RainIntensity = value; server.UpdateWeather(  ); },
        Value = 0f,
    };

    public RangedFloat RainWetness = new( 0,1,nameof( RainWetness ),"Rain wetness" )
    {
        Format = "0.0",
        ValueChanged = ( value ) => { settings.Weather.WeatherData.RainWetness = value; server.UpdateWeather(  ); },
        Value = 0f,
    };

    public RangedFloat RainWater = new( 0,1,nameof( RainWater ),"Rain water" )
    {
        Format = "0.0",
        ValueChanged = ( value ) => { settings.Weather.WeatherData.RainWater = value; server.UpdateWeather(  ); },
        Value = 0f,
    };
}