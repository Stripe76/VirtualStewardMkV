using System;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VirtualSteward.Features.TrackMap.ViewModels;

[ObservableObject]
public partial class VMMapItem( object item )
{
    private readonly ScaleTransform _Scale = new ScaleTransform( );
    private readonly RotateTransform _Rotation = new RotateTransform( );

    public object Item => item;

    [ObservableProperty] private Point _position = new Point( 0,0 );
    public double Scale
    {
        get => _Scale.ScaleX;
        set => _Scale.ScaleX = _Scale.ScaleY = Math.Max( value,5.0 ) / 50.0f;
    }
    public double Rotation
    {
        get => _Rotation.Angle;
        set => _Rotation.Angle = value;
    }
    public TransformGroup Transforms { get; } = new TransformGroup();
}