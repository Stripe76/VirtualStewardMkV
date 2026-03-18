using System;
using System.Windows.Input;
using Avalonia;
using VirtualSteward.Features.Checkpoints.ViewModels;
using VirtualSteward.Features.Tracklines.ViewModels;
using VirtualSteward.Features.TrackMap.EditingTools;

namespace VirtualSteward.Features.Checkpoints.EditingTools;

public class CheckpointEdit( VMCheckpointList checkpoints,Func<uint,VMCheckpoint> checkpointFactory ) : EditingTool
{
    public VMTrackline? Trackline { get; set; }
    public VMCheckpoint? Checkpoint { get; set; }
    public Func<uint,VMCheckpoint> CheckpointFactory { get; } = checkpointFactory;

    public override bool LeftMouseDown( Point screenPos,Point trackPos )
    {
        return false;
    }
    public override bool LeftMouseMove( Point screenPos,Point trackPos )
    {
        if( Checkpoint != null && Trackline != null )
        {
            uint frame = Trackline.FindNearestPoint( trackPos,(uint)Checkpoint.Frame,500,500 );

            Checkpoint.Frame = frame;

            return true;
        }
        return false;
    }
    public override bool LeftMouseUp( Point screenPos,Point trackPos )
    {
        Checkpoint = null;
        
        return false;
    }

    public override bool RightMouseDown( Point screenPos,Point trackPos )
    {
        if( Checkpoint == null && Trackline != null )
        {
            uint frame = Trackline.FindNearestPoint( trackPos,0,-1,-1 );

            checkpoints.Add( Checkpoint = CheckpointFactory( frame ) );
            
            Checkpoint.UpdateFrame(  );
        }
        else if( Checkpoint != null )
        {
            checkpoints.Remove( Checkpoint );

            Checkpoint = null;
        }
        return true;
    }
    public override bool RightMouseMove( Point screenPos,Point trackPos )
    {
        if( Checkpoint != null && Trackline != null )
        {
            uint frame = Trackline.FindNearestPoint( trackPos,(uint)Checkpoint.Frame,500,500 );

            Checkpoint.Frame = frame;
        }
        return true;
    }
    public override bool RightMouseUp( Point screenPos,Point trackPos )
    {
        Checkpoint = null;
        
        return false;
    }
}