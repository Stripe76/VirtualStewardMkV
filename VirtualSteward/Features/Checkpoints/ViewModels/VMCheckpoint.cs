using System;
using System.Windows.Input;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Bindables;
using Framework.UI;
using VirtualSteward.Features.TrackMap.ViewModels;

namespace VirtualSteward.Features.Checkpoints.ViewModels;

public partial class VMCheckpoint : UIBase,IComparable<VMCheckpoint>
{
    private uint _frame;
    private string _title = "";
    private Point _position;
    private double _direction;

    public uint Frame
    {
        get => _frame;
        set => SetProperty( ref _frame,value );
    }
    public string Title
    {
        get => _title;
        set => SetProperty( ref _title,value );
    }
    public Point Position
    {
        get => _position;
        set => SetProperty( ref _position,value );
    }
    public double Direction
    {
        get => _direction;
        set => SetProperty( ref _direction,value );
    }
    public ICommand? PointerPressed { get; init; }

    [ObservableProperty] private VMMapItem _mapItem;

    public VMCheckpoint( CheckpointSave save,Point position )
    {
        _frame = save.Frame;
        _title = save.Name;
        _position = position;
        _direction = save.Direction;

        _mapItem = new VMMapItem( this );
    }
    public VMCheckpoint( uint frame,double direction )
    {
        _frame = frame;
        _direction = direction;

        _mapItem = new VMMapItem( this );
    }

    public void UpdateFrame( )
    {
        OnPropertyChanged( nameof( Frame ) );
    }

    public int CompareTo( VMCheckpoint? other )
    {
        if( other != null )
            return (int)Frame - (int)other.Frame;
        return -1;
    }
}

[Serializable]
public class CheckpointSave
{
    public string Name { get; set; } = "";
    public uint Frame { get; set; }
    public double Direction { get; set; }

    public CheckpointSave( ) { }
    public CheckpointSave( VMCheckpoint checkPoint )
    {
        Name = checkPoint.Title;
        Frame = checkPoint.Frame;
        Direction = checkPoint.Direction;
    }
}

public class VMCheckpointList : ObservableCollectionEx<VMCheckpoint>
{

}