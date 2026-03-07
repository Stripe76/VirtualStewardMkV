using Framework.UI.Values;
using Framework.UI.Configurations;

using VirtualSteward.ACNetwork.Shared;

namespace VirtualSteward.Features.Server.Configurations;

public class CMServerPorts( ACServerSettings settings ) : Configuration( "SERVER_PORTS","Server ports" )
{
    public BaseInt HttpPort = new( "HttpPort","HTTP Port" )
    {
        //Description = "Same as replay file or multiple is suggested",
        ValueChanged = ( value ) => { settings.HttpPort = value; },
        Value = 8080,
    };
}    
