using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.UI;

namespace VirtualSteward.Features.TrackMap.ViewModels;

public partial class VMMapImage : UIBase
{
    private readonly ScaleTransform _Scale = new ScaleTransform( );
    private readonly RotateTransform _Rotation = new RotateTransform( );

    [ObservableProperty] private Point _position = new Point( );
    public double Scale
    {
        get => _Scale.ScaleX;
        set => _Scale.ScaleX = _Scale.ScaleY = value / 50.0f;
    }
    public double Rotation
    {
        get => _Rotation.Angle;
        set => _Rotation.Angle = value;
    }
    public TransformGroup Transforms { get; } = new TransformGroup();

    public IImage Image { get; }

    public void UpdateImage(double zoom, Point offset, Rect clipping)
    {
        
    }

    public VMMapImage( IImage image )
    {
        Image = image;

        Transforms.Children.Add(_Scale);    
        Transforms.Children.Add(_Rotation);
    }
}

public class VMMapImageList : ObservableCollection<VMMapImage>
{
    
}