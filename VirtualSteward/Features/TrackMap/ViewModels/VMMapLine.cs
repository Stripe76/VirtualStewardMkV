using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Security;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls.Shapes;

using Framework.UI;
using Framework.Bindables;

namespace VirtualSteward.Features.TrackMap.ViewModels;

public class VMMapLine( PointCollection line,VMMapLineStyle style ) : UIBase
{
    public VMMapLineStyle Style = style;

    public PolylineList Polylines { get; } = [];

    public VMMapLine UpdatePolylines( double zoom,Point offset,Rect clipping )
    {
        var polys = VMMap.GetPolylinePointsClipped( line,zoom,offset,clipping,false );

        Polylines.Clear();
        foreach (var poly in polys)
        {
            Polylines.Add(
                new Polyline()
                {
                    Points = poly,
                    Stroke = Style.Color,
                    StrokeThickness = Style.Thickness,
                });
        }
        return this;
    }
}

public class VMMapLineList : ObservableCollectionEx<VMMapLine>
{
    
}

public class VMMapLineStyle(double thickness,IImmutableSolidColorBrush color)
{
    public double Thickness { get; } = thickness;
    public IImmutableSolidColorBrush Color { get; } = color;
    
    #region Colors
    public static IList<IImmutableSolidColorBrush> LineColors
    {
        get => [.. _lineBrushes];
    }

    private static readonly IImmutableSolidColorBrush[] _lineBrushes =
    {
        Brushes.Red,
        Brushes.Green,
        Brushes.Blue,
        Brushes.BlueViolet,
        Brushes.MediumTurquoise,
        Brushes.Brown,
        Brushes.Orange,
        Brushes.LimeGreen,
        Brushes.Olive,
        Brushes.Bisque,
        Brushes.Plum,
        Brushes.PowderBlue,
        Brushes.Purple,
        Brushes.Salmon,
        Brushes.SteelBlue,
        Brushes.Goldenrod,
        Brushes.DarkSlateBlue,
        Brushes.Tan,
        Brushes.Khaki,
        Brushes.DeepPink,
    };
    #endregion
}

public class PolylineList : ObservableCollection<Polyline>
{
    
}
