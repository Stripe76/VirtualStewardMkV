using Framework.UI.Values;

namespace VirtualSteward.Features.Server.Values;

public enum ServerHeadligths
{
    AlwaysOn,
    AlwaysOff,
    FromReplay
}

public class HeadlightsValue( string name,string title ) : BaseValue<ServerHeadligths>( ServerHeadligths.FromReplay,name,title )
{
    public ServerHeadligths[] Items { get; } = [ServerHeadligths.FromReplay,ServerHeadligths.AlwaysOn,ServerHeadligths.AlwaysOff];
}