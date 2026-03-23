using System;
using System.Collections.Generic;
using System.Linq;
using Framework.UI.Values;
using VirtualSteward.ACNetwork.Weather;

namespace VirtualSteward.Features.Server.Values;

public class WeatherTypeValue : BaseValue<string>
{
    public static readonly DefaultWeatherTypeProvider WeatherTypeProvider = new ( );

    public List<string> Items { get; } = [];
    
    public WeatherTypeValue( string name,string title ) : base( "Clear",name,title )
    {
        foreach( var s in Enum.GetValues<WeatherFxType>( ) )
        {
            Items.Add( s.ToString( ) );
        }
        Items.Sort( );
    }
}