using Framework.UI.Values;
using Framework.UI.Configurations;

using VirtualSteward.ACNetwork.Shared;

namespace VirtualSteward.Features.Server.Configurations;

public class CMServerPorts( ACServerSettings settings ) : Configuration( "SERVER_PORTS","Server ports" )
{
    public BaseInt HttpPort = new( nameof( HttpPort ),"HTTP Port" )
    {
        ValueChanged = ( value ) => { settings.HttpPort = value; },
        Value = 8080,
    };
    public BaseInt TcpPort = new( nameof( TcpPort ),"TCP Port" )
    {
        ValueChanged = ( value ) => { settings.TcpPort = value; },
        Value = 9600,
    };
    public BaseInt UdpPort = new( nameof( UdpPort ),"UDP Port" )
    {
        ValueChanged = ( value ) => { settings.UdpPort = value; },
        Value = 9600,
    };
}    
