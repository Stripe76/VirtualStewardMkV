using System;
using System.Collections.Generic;
using System.Linq;
using Framework.UI.Values;
using VirtualSteward.ACNetwork.Weather;

namespace VirtualSteward.Features.Server.Values;

public class WeatherTypeValue( ) : BaseValue<WeatherFxType>( WeatherFxType.Clear,"WeatherType","Weather:" )
{
    public static readonly DefaultWeatherTypeProvider WeatherTypeProvider = new ( );

    public IEnumerable<object> Items { get; } = Enum.GetValues( typeof( WeatherFxType ) ).Cast<object>( );
}