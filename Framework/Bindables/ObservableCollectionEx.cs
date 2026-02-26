using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Framework.Bindables;

public class ObservableCollectionEx<T> : ObservableCollection<T>
{
  private bool _notificationSupressed = false;
  private bool _supressNotification = false;

  public bool SupressNotification
  {
    get => _supressNotification;
    set
    {
      _supressNotification = value;
      if( !_supressNotification && _notificationSupressed )
      {
        this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        _notificationSupressed = false;
      }
    }
  }

  public void Refresh( )
  {
    //CollectionViewSource.GetDefaultView( this ).Refresh( );
  }

  protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
  {
    if( SupressNotification )
    {
      _notificationSupressed = true;
      return;
    }
    base.OnCollectionChanged( e );
  }
}
