using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Framework.Bindables;

public interface IMultiListItem : INotifyPropertyChanged
{
  bool IsEnabled { get; set;  }
  bool IsActive { get; set;  }
  bool IsVisible { get; set;  }
  bool IsSelected { get; set;  }
  bool IsHighlighted { get; set;  }
  bool DeleteItem { get; set;  }
}

public class MultiList<T> : ObservableCollectionEx<T> where T : class,IMultiListItem
{
  //private BindingList<T> _itemsList;
  private ObservableCollection<T> _activeList = [];
  private ObservableCollection<T> _visibleList = [];
  private ObservableCollection<T> _selectedList = [];

  private T? _lastActiveItem = null;

  public ObservableCollection<T> ActiveItems
  {
    get => _activeList;
    set => SetProperty( ref _activeList,value );
  }
  public ObservableCollection<T> VisibleItems
  {
    get => _visibleList;
    set => SetProperty( ref _visibleList,value );
  }
  public ObservableCollection<T> SelectedItems
  {
    get => _selectedList;
    set => SetProperty( ref _selectedList,value );
  }

  public T? ActiveItem
  {
    get => _lastActiveItem;
    set
    {
      _lastActiveItem = value;

      if( value != null )
      {
        if( !_activeList.Contains( value ) && Contains( value ) )
          value.IsActive = true;
      }
      else
      {
        foreach( T item in this )
        {
          if( item.IsActive )
            item.IsActive = false;
        }
        OnPropertyChanged( nameof( ActiveItem ) );

        ActiveItemChanged?.Invoke( this,value );
      }
    }
  }
  public T? SelectedItem
  {
    get
    {
      if( _selectedList.Count > 0 )
        return _selectedList[0];
      return null;
    }
    set
    {
      if( value != null )
      {
        if( !_selectedList.Contains( value ) && Contains( value ) )
          value.IsSelected = true;
      }
      else
      {
        foreach( T item in this )
        {
          if( item.IsSelected )
            item.IsSelected = false;
        }
        OnPropertyChanged( nameof( SelectedItem ) );

        SelectedItemChanged?.Invoke( this,value );
      }
    }
  }

  public bool MultiActiveEnabled = false;
  public bool MultiSelectedEnabled = false;
  public bool MultiActiveWithCtrlEnabled = false;

  public bool FirstAlwaysActive = false;

  public event EventHandler<T?>? ActiveItemChanged;
  public event EventHandler<T?>? SelectedItemChanged;

  public MultiList( bool multiSelect = false,bool multiActive = false,bool multiActiveWithCtrl = false )
  {
    MultiActiveEnabled = multiActive;
    MultiSelectedEnabled = multiSelect;
    MultiActiveWithCtrlEnabled = multiActiveWithCtrl;
    
    CollectionChanged += OnCollectionChanged; 
  }

  public void Add( T item,bool select = false,bool active = false,bool visible = true )
  {
    base.Add( item );
    
    item.PropertyChanged += ItemOnPropertyChanged; 

    if( select )
      item.IsSelected = true;
    if( visible )
      item.IsVisible = true;
    if( active )
      item.IsActive = true;
  }
  public bool AddIfNotContains( T item,bool select = false,bool active = false,bool visible = true )
  {
    if( !Contains( item ) )
    {
      Add( item,select,active,visible );

      return true;
    }
    return false;
  }

  public new void Remove( T item )
  {
    item.IsActive = false;
    item.IsSelected = false;

    item.PropertyChanged -= ItemOnPropertyChanged; 

    base.Remove( item );
    
    _activeList.Remove( item );
    _selectedList.Remove( item );
  }
  public new  void RemoveAt( int index )
  {
    T item = this[index];

    item.IsActive = false;
    item.IsSelected = false;

    Remove( item );
    
    _activeList.Remove( item );
    _selectedList.Remove( item );
  }

  public void AddRange( IEnumerable<T> list )
  {
    foreach( T item in list )
      Add( item );
  }

  public new void Clear( )
  {
    base.Clear( );
    
    _activeList.Clear( );
    _selectedList.Clear( );
  }

  private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (sender is not null and T item)
    {
      if( e.PropertyName == nameof( IMultiListItem.IsActive ) )
      {
        if( item.IsActive )
        {
          if( !MultiActiveEnabled /*&&
              (!MultiActiveWithCtrlEnabled /*|| (!Keyboard.IsKeyDown( Key.LeftCtrl ) && !Keyboard.IsKeyDown( Key.RightCtrl )))*/
            )
          {
            foreach( T activeItem in this )
            {
              if( activeItem != item && activeItem.IsActive )
                activeItem.IsActive = false;
            }
          }
          if( !_activeList.Contains( item ) )
            _activeList.Add( item );

          _lastActiveItem = item;
        }
        else
        {
          _activeList.Remove( item );

          if( _activeList.Count > 0 )
            _lastActiveItem = _activeList[0];
          else
          {
            if( FirstAlwaysActive && Count > 0 )
              _lastActiveItem = Items[0];
            else
              _lastActiveItem = null;
          }
        }
        OnPropertyChanged( nameof( ActiveItem ) );

        ActiveItemChanged?.Invoke( this,ActiveItem );
      }
      else if (e.PropertyName == nameof(IMultiListItem.IsSelected))
      {
        if (item.IsSelected)
        {
          if (!MultiSelectedEnabled)
          {
            foreach (T selectedItem in this)
            {
              if (selectedItem != item && selectedItem.IsSelected)
                selectedItem.IsSelected = false;
            }
          }
          if (!_selectedList.Contains(item))
            _selectedList.Add(item);

          OnPropertyChanged(nameof(SelectedItem));

          SelectedItemChanged?.Invoke(this, SelectedItem);
        }
        else
        {
          _selectedList.Remove(item);

          SelectedItemChanged?.Invoke(this, SelectedItem);
        }
      }
      else if (e.PropertyName == nameof(IMultiListItem.IsVisible))
      {
        if (item.IsVisible)
        {
          if (!_visibleList.Contains(item))
            _visibleList.Add(item);
        }
        else
        {
          _visibleList.Remove(item);
        }
      }
      else if (e.PropertyName == nameof(IMultiListItem.DeleteItem))
      {
        Remove( item );
      }
    }
  }
  private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.Action == NotifyCollectionChangedAction.Reset )
    {
      _selectedList.Clear();
      _activeList.Clear();
      _visibleList.Clear();

      OnPropertyChanged(nameof(ActiveItem));
      OnPropertyChanged(nameof(SelectedItem));
    }
  }

  #region Notify propery changed
  protected bool SetProperty<P>( ref P storage,P value,[CallerMemberName] string? propertyName = null )
  {
    if( Equals( storage,value ) )
    {
      return false;
    }
    storage = value;

    return true;
  }

  protected void OnPropertyChanged( [CallerMemberName] string? propertyName = null )
  {
    OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
  }
  #endregion
}