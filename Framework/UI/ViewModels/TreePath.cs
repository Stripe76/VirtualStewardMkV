using System.ComponentModel;
using System.Collections.Specialized;

using Framework.Bindables;

namespace Framework.UI.ViewModels;

public class TreePath<T> where T : IMultiListItem
{
  private readonly List<string> _paths;
  private readonly ObservableCollectionEx<T> _items;

  private readonly TreeNode _rootNode = new ( ) { Title = "root" };

  public bool ShowExpander { get; set; } = false;
  public bool ShowCheckbox { get; set; } = false;
  public bool ShowRadiobutton { get; set; } = false;
  
  public bool ExpandAll { get; set; }

  public List<TreeNode> Nodes => _rootNode.Children.Values.ToList( );

  public TreePath( ObservableCollectionEx<T> items,List<string> paths )
  {
    _paths = paths;
    _items = items;
    _items.CollectionChanged += Items_CollectionChanged;
    
    _items.Refresh(  );
  }

  private TreeNode BuildTree( TreeNode root,ObservableCollectionEx<T> items,List<string> paths )
  {
    int id = 0;
    foreach( var path in paths )
    {
      foreach( var obj in items )
      {
        //if( !obj.IsEnabled )
          //continue;

        var currentNode = root;
        var properties = (path+"/^").Split( '/' );

        foreach( var prop in properties )
        {
          string nodeTitle = "";
          if( prop[0] == '{' )
          {
            nodeTitle = prop[1..^1];
          }
          else if( prop[0] == '^' )
          {
            nodeTitle = id++.ToString( );

            TreeLeaf leafNode;
            if( ShowCheckbox )
            {
              leafNode = new TreeLeafCheckbox( obj )
              {
                Title = nodeTitle,
                IsExpanded = ExpandAll,
              };
            }
            else
            {
              leafNode = new TreeLeaf( obj )
              {
                Title = nodeTitle,
                IsExpanded = ExpandAll,
              };
            }
            currentNode?.Children.Add( nodeTitle,leafNode );
            
            continue;
          }
          else
          {
            var propValue = obj.GetType( )?.GetProperty( prop )?.GetValue( obj )?.ToString( );
            if( propValue != null )
              nodeTitle = propValue;
          }
          TreeNode? childNode = null;
          currentNode?.Children.TryGetValue( nodeTitle,out childNode );
          if( childNode == null )
          {
            childNode = new TreeNode( obj )
            {
              Title = nodeTitle,
              IsExpanded = ExpandAll,
            };
            currentNode?.Children.Add( nodeTitle,childNode );
          }
          currentNode = childNode;
        }
        if( currentNode != null )
        {
          //currentNode.Objects.Add( obj );
          //currentNode.IsSelected = obj.IsSelected;

          /*
          currentNode.ShowCheckbox = ShowCheckbox;
          currentNode.ShowRadiobutton = ShowRadiobutton;
          currentNode.RadiobuttonGroup = RadiobuttonGroup;
          currentNode.ShowExpander = ShowExpander;
          */

          //if( obj.ShowControl )
            //currentNode.Controls.Add( obj );
            
          /*
          UIElement? detailsControl = obj.GetDetailsControl( );
          if( detailsControl != null )
            currentNode.Controls.Add( detailsControl );
          */
        }
      }
    }
    //RaisePropertyChanged( root );

    return root;
  }

  private void Items_CollectionChanged( object? sender,NotifyCollectionChangedEventArgs e )
  {
    if( e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset )
    {
      BuildTree( _rootNode,_items,_paths );
    }
  }
}

public class TreeNode : UIItem
{
  private string _title = "";

  public string Title
  {
    get => _title;
    set => SetProperty( ref _title,value );
  }
  public bool IsLeaf
  {
    get => Objects.Count > 0;
  }

  public TreeNode( IMultiListItem? item = null,string title = "" )
  {
    if( item != null )
      //item.PropertyChanged += Item_OnSelectedChanged;
      item.PropertyChanged += Item_PropertyChanged;
  }

  public List<IMultiListItem> Objects { get; set; } = [];
  public SortedList<string,TreeNode> Children { get; set; } = [];

  public IList<TreeNode> ChildrenItems => Children.Values;

  private void Item_PropertyChanged( object? sender,PropertyChangedEventArgs e )
  {
    if( sender != null && e.PropertyName == nameof( IMultiListItem.IsSelected ) )
    {
      IsSelected = ((IMultiListItem)sender).IsSelected;

      OnPropertyChanged( nameof( IsSelected ) );
    }
  }
}

public class TreeLeaf( object item ) : TreeNode
{
  public object Item { get; } = item;
}

public class TreeLeafCheckbox( object item ) : TreeLeaf( item )
{
}