using Framework.UI.Values;
using Framework.UI.Configurations;
using VirtualSteward.Features.Timelines.ViewModels;

namespace VirtualSteward.Features.Server.Configurations;

public class CMServerStartOptions( Pages.Server.Server server,VMTimeline timeline ) : Configuration( "SERVER_START" )
{
    public BaseBool StartReplayProperty => StartReplay;  
    public BaseBool LaunchCMProperty => LaunchCM;  
    
    public BaseBool StartReplay = new BaseBool( nameof( StartReplay ) )
    {
        //ValueChanged = ( value ) => { server.LaunchReplay = value; }
    };
    public BaseBool LaunchCM = new BaseBool(  nameof( LaunchCM ) )
    {
        //ValueChanged = ( value ) => { server.LaunchCM = value; }
    };
    public BaseBool LoopReplay = new BaseBool( nameof( LoopReplay ) )
    {
        ValueChanged = ( value ) => { server.FrameValidation.LoopReplay = value; }
    };
    public BaseBool LoopScrubs = new BaseBool( "LoopSegment" )
    {
        ValueChanged = ( value ) => { server.FrameValidation.LoopScrubs = value; }
    };

    public readonly BaseInt LoopStart = new BaseInt( nameof( LoopStart ) )
    {
        ValueChanged = ( value ) => timeline.ScrubA = (uint)value
    };
    public readonly BaseInt LoopEnd = new BaseInt( nameof( LoopEnd ) )    
    {
        ValueChanged = ( value ) => timeline.ScrubB = (uint)value
    };

}