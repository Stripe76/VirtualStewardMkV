using System;
using Framework.UI;

namespace VirtualSteward.Features.ProgressBar.ViewModel;

public class VMProgress : UIBase,IProgress<float>
{
    private double _progress;
    //private SolidColorBrush _color = Brushes.ForestGreen;

    public double Progress
    {
        get => _progress;
        set => SetProperty( ref _progress,value );
    }
    /*public SolidColorBrush Color
    {
      get => _color;
      set => SetProperty( ref _color,value );
    }
    */

    public void Report( float x )
    {
        if( x >= 0 || x <= -2 )
        {
            IsVisible = true;

            if( x <= -2 )
            {
                x = 1;

//        Color = Brushes.Firebrick;
            }
            else
            {
//        Color = Brushes.ForestGreen;
            }
        }
        else
        {
            IsVisible = false;

//      CommandManager.InvalidateRequerySuggested( );
        }
        Progress = x;
    }
}