using System.Collections.ObjectModel;
using System.Collections.Specialized;

using Framework.Bindables;
using Framework.UI.Values;

using VirtualSteward.Features.PlayersList.ViewModels;

namespace VirtualSteward.Features.ReplayExport.Values;

public class PlayersExportValue : BaseValue<PlayersExportItem>
{
  private readonly VMPlayerList _players;

  public PlayersExportItemList Items
  {
    get;
  } = [];

  public PlayersExportValue( VMPlayerList players ) : base( null,"Players","" )
  {
    _players = players;
    _players.CollectionChanged += Players_CollectionChanged;

    UpdateItems( );
  }

  private void UpdateItems( )
  {
    Items.Add( new( _players,"All players" ) );
    Items.Add( new( _players.ActiveItems,"Active players" ) );
    Items.Add( new( _players.VisibleItems,"Visible players" ) );
    Items.Add( new( _players.SelectedItems,"Selected players" ) );

    Value = Items[0];
  }

  private void Players_CollectionChanged( object? sender,NotifyCollectionChangedEventArgs e )
  {
    if( sender != null && sender == _players )
    {
      if( e.Action is NotifyCollectionChangedAction.Add or 
                      NotifyCollectionChangedAction.Remove or 
                      NotifyCollectionChangedAction.Reset )
      {
        Items.Clear( );

        UpdateItems( );
      }
    }
  }
}

public class PlayersExportItem : BindableBase
{
  private readonly string _title;

  public string Title
  {
    get => $"{_title}{GetSuffix( )}";
  }
  public ObservableCollection<VMPlayer> Players { get; }

  public PlayersExportItem( ObservableCollection<VMPlayer> players,string title )
  {
    _title = title;

    Players = players;
    Players.CollectionChanged += Players_CollectionChanged;
  }

  private void Players_CollectionChanged( object? sender,NotifyCollectionChangedEventArgs e )
  {
    if( e.Action is NotifyCollectionChangedAction.Add or 
                    NotifyCollectionChangedAction.Remove or 
                    NotifyCollectionChangedAction.Reset )
    {
      OnPropertyChanged( nameof( Title ) );
    }
  }

  private string GetSuffix( )
  {
    return $" ({Players.Count})";
  }

  public override string ToString( )
  {
    return Title;
  }
}

public class PlayersExportItemList : ObservableCollection<PlayersExportItem>
{
}