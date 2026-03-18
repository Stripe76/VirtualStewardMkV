using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
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
        set => _Scale.ScaleX = _Scale.ScaleY = Math.Max( value,5.0 ) / 50.0f;
    }
    public double Rotation
    {
        get => _Rotation.Angle;
        set => _Rotation.Angle = value;
    }
    public TransformGroup Transforms { get; } = new TransformGroup();

    public IImage Image { get; }
    public ICommand? PointerPressed { get; set; }

    public VMMapImage( IImage image )
    {
        Image = image;

        Transforms.Children.Add(_Scale);    
        Transforms.Children.Add(_Rotation);
    }

    public void BindIsVisible( UIBase visibleBinding )
    {
        visibleBinding.PropertyChanged += VisibleBinding_PropertyChanged;
    }

    private void VisibleBinding_PropertyChanged( object? sender,PropertyChangedEventArgs e )
    {
        if( e.PropertyName is nameof( UIBase.IsVisible ) && sender is not null and UIBase binding )
            IsVisible = binding.IsVisible;
    }
}

public class VMMapImageList : ObservableCollection<VMMapImage>
{
    
}