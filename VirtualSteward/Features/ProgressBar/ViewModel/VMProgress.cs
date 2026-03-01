using System;
using Framework.UI;

namespace VirtualSteward.Features.ProgressBar.ViewModel;

public class VMProgress : UIBase,IProgress<float>
{
    private double _progress;

    public double Progress
    {
        get => _progress;
        set => SetProperty( ref _progress,value );
    }

    public void Report( float x )
    {
        if( x >= 0 )
        {
            IsVisible = true;
        }
        else
        {
            IsVisible = false;
            
            OnPropertyChanged( "DismissToast" );
        }
        Progress = x;
    }
}