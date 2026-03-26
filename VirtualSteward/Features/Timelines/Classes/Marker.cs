using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Utilities;

namespace VirtualSteward.Features.Timelines.Classes;

[TemplatePart("PART_Thumb",typeof(Border))]
[PseudoClasses(":vertical", ":horizontal")]
public class Marker : TemplatedControl
{
    private Border? _thumb;
    
    public static readonly StyledProperty<double> MinimumProperty =
        RangeBase.MinimumProperty.AddOwner<Marker>();

    public static readonly StyledProperty<double> MaximumProperty =
        RangeBase.MaximumProperty.AddOwner<Marker>();

    public static readonly StyledProperty<double> ValueProperty =
        RangeBase.ValueProperty.AddOwner<Marker>();

    public static readonly StyledProperty<double> ViewportSizeProperty =
        ScrollBar.ViewportSizeProperty.AddOwner<Marker>();

    public static readonly StyledProperty<Orientation> OrientationProperty =
        ScrollBar.OrientationProperty.AddOwner<Marker>();

    public static readonly StyledProperty<bool> IsDirectionReversedProperty =
        AvaloniaProperty.Register<Marker, bool>(nameof(IsDirectionReversed));

    static Marker()
    {
        AffectsArrange<Marker>(IsDirectionReversedProperty, MinimumProperty, MaximumProperty, ValueProperty,
            OrientationProperty);
    }

    public Marker()
    {
        UpdatePseudoClasses(Orientation);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private double ThumbValue => Value;

    public double ViewportSize
    {
        get => GetValue(ViewportSizeProperty);
        set => SetValue(ViewportSizeProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool IsDirectionReversed
    {
        get => GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    private double ThumbCenterOffset { get; set; }
    private double Density { get; set; }

    /// <summary>
    /// Calculates the distance along the <see cref="Thumb"/> of a specified point along the
    /// track.
    /// </summary>
    /// <param name="point">The specified point.</param>
    /// <returns>
    /// The distance between the Thumb and the specified pt value.
    /// </returns>
    public virtual double ValueFromPoint(Point point)
    {
        double val;

        // Find distance from center of thumb to given point.
        if (Orientation == Orientation.Horizontal)
        {
            val = ThumbValue + ValueFromDistance(point.X - ThumbCenterOffset, point.Y - (Bounds.Height * 0.5));
        }
        else
        {
            val = ThumbValue + ValueFromDistance(point.X - (Bounds.Width * 0.5), point.Y - ThumbCenterOffset);
        }

        return Math.Max(Minimum, Math.Min(Maximum, val));
    }

    /// <summary>
    /// Calculates the change in the <see cref="Value"/> of the <see cref="Track"/> when the
    /// <see cref="Thumb"/> moves.
    /// </summary>
    /// <param name="horizontal">The horizontal displacement of the thumb.</param>
    /// <param name="vertical">The vertical displacement of the thumb.</param>        
    public virtual double ValueFromDistance(double horizontal, double vertical)
    {
        double scale = IsDirectionReversed ? -1 : 1;

        if (Orientation == Orientation.Horizontal)
        {
            return scale * horizontal * Density;
        }
        else
        {
            // Increases in y cause decreases in Sliders value
            return -1 * scale * vertical * Density;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Size desiredSize = new Size(0.0, 0.0);

        // Only measure thumb.
        // Repeat buttons will be sized based on thumb
        if (_thumb != null)
        {
            _thumb.Measure(availableSize);
            desiredSize = _thumb.DesiredSize;
        }

        if (!double.IsNaN(ViewportSize))
        {
            // ScrollBar can shrink to 0 in the direction of scrolling
            if (Orientation == Orientation.Vertical)
                desiredSize = desiredSize.WithHeight(0.0);
            else
                desiredSize = desiredSize.WithWidth(0.0);
        }

        return desiredSize;
    }
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        double decreaseButtonLength, thumbLength, increaseButtonLength;
        var isVertical = Orientation == Orientation.Vertical;
        var viewportSize = Math.Max(0.0, ViewportSize);

        // If viewport is NaN, compute thumb's size based on its desired size,
        // otherwise compute the thumb base on the viewport and extent properties
        if (double.IsNaN(ViewportSize))
        {
            ComputeSliderLengths(arrangeSize, isVertical, out decreaseButtonLength, out thumbLength,
                out increaseButtonLength);
        }
        else
        {
            // Don't arrange if there's not enough content or the track is too small
            if (!ComputeScrollBarLengths(arrangeSize, viewportSize, isVertical, out decreaseButtonLength,
                    out thumbLength, out increaseButtonLength))
            {
                return arrangeSize;
            }
        }

        // Layout the pieces of track
        var offset = new Point();
        var pieceSize = arrangeSize;
        var isDirectionReversed = IsDirectionReversed;

        if (isVertical)
        {
            CoerceLength(ref thumbLength, arrangeSize.Height);

            offset = offset.WithY(isDirectionReversed ? decreaseButtonLength : increaseButtonLength);
            pieceSize = pieceSize.WithHeight(thumbLength);

            if (_thumb != null)
            {
                var bounds = new Rect(offset, pieceSize);
                var adjust = CalculateThumbAdjustment(_thumb, bounds);
                _thumb.Arrange(bounds);
                //Thumb.AdjustDrag(adjust);
            }

            ThumbCenterOffset = offset.Y + (thumbLength * 0.5);
        }
        else
        {
            CoerceLength(ref decreaseButtonLength, arrangeSize.Width);
            CoerceLength(ref increaseButtonLength, arrangeSize.Width);
            CoerceLength(ref thumbLength, arrangeSize.Width);

            offset = offset.WithX(isDirectionReversed ? increaseButtonLength : decreaseButtonLength);
            pieceSize = pieceSize.WithWidth(thumbLength);

            if (_thumb != null)
            {
                var bounds = new Rect(offset, pieceSize);
                var adjust = CalculateThumbAdjustment(_thumb, bounds);
                _thumb.Arrange(bounds);
                //Thumb.AdjustDrag(adjust);
            }

            ThumbCenterOffset = offset.X + (thumbLength * 0.5);
        }
        return arrangeSize;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // if we had a control template before, we need to unsubscribe any event listeners
        if(_thumb is not null)
        {
            //_porco.PointerReleased-= StarsPresenter_PointerReleased;
        }

        // try to find the control with the given name
        _thumb = e.NameScope.Find("PART_Thumb") as Border;

        // listen to pointer-released events on the stars presenter.
        if(_thumb != null)
        {
            //_starsPresenter.PointerReleased += StarsPresenter_PointerReleased;
        }
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<Orientation>());
        }
    }

    private Vector CalculateThumbAdjustment(Border thumb, Rect newThumbBounds)
    {
        return newThumbBounds.Position - thumb.Bounds.Position;
    }

    private static void CoerceLength(ref double componentLength, double trackLength)
    {
        if (componentLength < 0)
        {
            componentLength = 0.0;
        }
        else if (componentLength > trackLength || double.IsNaN(componentLength))
        {
            componentLength = trackLength;
        }
    }

    private void ComputeSliderLengths(Size arrangeSize, bool isVertical, out double decreaseButtonLength,
        out double thumbLength, out double increaseButtonLength)
    {
        double min = Minimum;
        double range = Math.Max(0.0, Maximum - min);
        double offset = Math.Min(range, ThumbValue - min);

        double trackLength;

        // Compute thumb size
        if (isVertical)
        {
            trackLength = arrangeSize.Height;
            thumbLength = _thumb == null ? 0 : _thumb.DesiredSize.Height;
        }
        else
        {
            trackLength = arrangeSize.Width;
            thumbLength = _thumb == null ? 0 : _thumb.DesiredSize.Width;
        }

        CoerceLength(ref thumbLength, trackLength);

        double remainingTrackLength = trackLength - thumbLength;

        decreaseButtonLength = remainingTrackLength * offset / range;
        CoerceLength(ref decreaseButtonLength, remainingTrackLength);

        increaseButtonLength = remainingTrackLength - decreaseButtonLength;
        CoerceLength(ref increaseButtonLength, remainingTrackLength);

        Density = range / remainingTrackLength;
    }

    private bool ComputeScrollBarLengths(Size arrangeSize, double viewportSize, bool isVertical,
        out double decreaseButtonLength, out double thumbLength, out double increaseButtonLength)
    {
        var min = Minimum;
        var range = Math.Max(0.0, Maximum - min);
        var offset = Math.Min(range, ThumbValue - min);
        var extent = Math.Max(0.0, range) + viewportSize;
        var trackLength = isVertical ? arrangeSize.Height : arrangeSize.Width;
        double thumbMinLength = 10;

        StyledProperty<double> minLengthProperty = isVertical ? MinHeightProperty : MinWidthProperty;

        var thumb = _thumb;

        if (thumb != null && thumb.IsSet(minLengthProperty))
        {
            thumbMinLength = thumb.GetValue(minLengthProperty);
        }

        thumbLength = trackLength * viewportSize / extent;
        CoerceLength(ref thumbLength, trackLength);
        thumbLength = Math.Max(thumbMinLength, thumbLength);

        // If we don't have enough content to scroll, disable the track.
        var notEnoughContentToScroll = MathUtilities.LessThanOrClose(range, 0.0);
        var thumbLongerThanTrack = thumbLength > trackLength;

        // if there's not enough content or the thumb is longer than the track, 
        // hide the track and don't arrange the pieces
        if (notEnoughContentToScroll || thumbLongerThanTrack)
        {
            ShowChildren(false);
            ThumbCenterOffset = Double.NaN;
            Density = Double.NaN;
            decreaseButtonLength = 0.0;
            increaseButtonLength = 0.0;
            return false; // don't arrange
        }
        else
        {
            ShowChildren(true);
        }

        // Compute lengths of increase and decrease button
        double remainingTrackLength = trackLength - thumbLength;
        decreaseButtonLength = remainingTrackLength * offset / range;
        CoerceLength(ref decreaseButtonLength, remainingTrackLength);

        increaseButtonLength = remainingTrackLength - decreaseButtonLength;
        CoerceLength(ref increaseButtonLength, remainingTrackLength);

        Density = range / remainingTrackLength;

        return true;
    }

    private void ShowChildren(bool visible)
    {
        // WPF sets Visible = Hidden here but we don't have that, and setting IsVisible = false
        // will cause us to stop being laid out. Instead show/hide the child controls.
        if (_thumb != null)
        {
            _thumb.IsVisible = visible;
        }
    }

    private void UpdatePseudoClasses(Orientation o)
    {
        PseudoClasses.Set(":vertical", o == Orientation.Vertical);
        PseudoClasses.Set(":horizontal", o == Orientation.Horizontal);
    }
}
