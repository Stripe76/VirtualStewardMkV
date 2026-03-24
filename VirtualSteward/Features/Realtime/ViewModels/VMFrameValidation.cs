using Framework.UI;
using VirtualSteward.Features.Timelines.ViewModels;

namespace VirtualSteward.Features.Realtime.ViewModels;

public abstract class VMFrameValidation : UIBase
{
    public abstract uint ValidateFrame( int frame );
}

public class VMFrameValidationTimeline : VMFrameValidation
{
    private readonly VMTimeline _timeline;
    
    private int _loopStart = 0,_loopEnd = 0,_nextFrame = -1,_lastFrame = -1;
    private bool _loopReplay = false,_loopScrubs = false;
    
    public bool LoopReplay
    {
        get => _loopReplay;
        set
        {
            if( SetProperty( ref _loopReplay,value ) )
                UpdateLoopFrames( );
        }  
    }
    public bool LoopScrubs
    {
        get => _loopScrubs;
        set
        {
            if( SetProperty( ref _loopScrubs,value ) )
                UpdateLoopFrames( );
        }  
    }

    public uint ScrubA => _timeline.ScrubA;
    public uint ScrubB => _timeline.ScrubB;

    public VMFrameValidationTimeline( VMTimeline timeline )
    {
        _timeline = timeline;
        _timeline.PropertyChanged += Timeline_PropertyChanged;

    }
    
    public override uint ValidateFrame( int frame )
    {
        if( _nextFrame >= 0 )
        {
            frame = _nextFrame;

            _nextFrame = -1;
        }
        int resultFrame = frame;
        if( _loopStart == _loopEnd )
        {
            if( resultFrame < 0 )
                resultFrame = 0;
            else if( resultFrame >= _loopEnd )
                resultFrame = _loopEnd;
        }
        else
        {
            if( resultFrame < _loopStart )
                resultFrame = _loopEnd;
            else if( resultFrame >= _loopEnd )
                resultFrame = _loopStart;
        }
        return (uint)(_lastFrame = resultFrame);
    }

    private void UpdateLoopFrames( )
    {
        _loopEnd = (int) (_loopScrubs ? (_loopReplay ? _timeline.ScrubB : _timeline.End) : _timeline.End);
        _loopStart = (_loopScrubs ? (_loopReplay ? (int)_timeline.ScrubA : _loopEnd) : (_loopReplay ? 0 : _loopEnd));
    }
    
    private void Timeline_PropertyChanged( object? sender,System.ComponentModel.PropertyChangedEventArgs e )
    {
        if( e.PropertyName is nameof( VMTimeline.CurrentFrame ) )
        {
            if( _lastFrame != _timeline.CurrentFrame )
                _nextFrame = (int)_timeline.CurrentFrame;
        }
        else if( e.PropertyName is nameof( VMTimeline.ScrubA ) or nameof( VMTimeline.ScrubB ) )
        {
            UpdateLoopFrames( );
        }
    }
}