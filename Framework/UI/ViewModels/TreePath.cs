using System.ComponentModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Framework.Bindables;

namespace Framework.UI.ViewModels;

public class TreePath<T,TLastNode> : INotifyPropertyChanged where T : IMultiListItem where TLastNode : TreeNode,new()
{
  private readonly List<string> _paths;
  private readonly ObservableCollectionEx<T> _items;

  private readonly TreeNode _rootNode = new ( ) { Title = "root" };

  public bool ShowExpander { get; set; } = false;
  public bool ShowCheckbox { get; set; } = false;
  public bool ShowRadiobutton { get; set; } = false;
  
  public bool ExpandAll { get; set; }

  public MultiList<TLastNode> LastNodes { get; } = [];

  public List<TreeNode> Nodes => _rootNode.Children.Values.ToList( );

  public TreePath( ObservableCollectionEx<T> items,List<string> paths )
  {
    _paths = paths;
    _items = items;
    _items.CollectionChanged += Items_CollectionChanged;
    
    BuildTree( _rootNode,_items,_paths );
  }

  private TreeNode BuildTree( TreeNode root,ObservableCollectionEx<T> items,List<string> paths )
  {
    root.Children.Clear(  );
    
    LastNodes.Clear(  );
    
    int id = 0;
    foreach( var path in paths )
    {
      foreach( var obj in items )
      {
        if( !obj.IsEnabled )
          continue;

        var currentNode = root;
        var properties = (path+" ^/^").Split( '/' );

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

            TreeLeaf? leafNode;
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
            //if( leafNode != null )
              currentNode?.Children.Add( nodeTitle,leafNode );
            
            continue;
          }
          else if( prop.EndsWith( " ^" ) )
          {
            var propValue = obj.GetType( )?.GetProperty( prop[0..^2] )?.GetValue( obj )?.ToString( );
            if( propValue != null )
              nodeTitle = propValue;
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
            if( prop.EndsWith( " ^" ) )
            {
              childNode = new TLastNode( )
              {
                Title = nodeTitle,
                IsExpanded = ExpandAll,
              };
              LastNodes.Add( (TLastNode)childNode );
            }
            else
            {
              childNode = new TreeNode( )
              {
                Title = nodeTitle,
                IsExpanded = ExpandAll,
              };
            }
            currentNode?.Children.Add( nodeTitle,childNode );
          }
          currentNode = childNode;
        }
      }
    }
    OnPropertyChanged( nameof( Nodes ) );
    
    RaisePropertyChanged( root );

    return root;
  }

  private static void RaisePropertyChanged( TreeNode node )
  {
    node.RaisePropertyChanged( );

    foreach( TreeNode child in node.Children.Values ) 
      RaisePropertyChanged( child );
  }

  private void Items_CollectionChanged( object? sender,NotifyCollectionChangedEventArgs e )
  {
    if( e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset )
    {
      BuildTree( _rootNode,_items,_paths );
    }
  }
  
  public event PropertyChangedEventHandler? PropertyChanged;

  protected void OnPropertyChanged( [CallerMemberName] string? propertyName = null )
  {
    PropertyChanged?.Invoke( this,new PropertyChangedEventArgs( propertyName ) );
  }
}

public class TreePath<T>( ObservableCollectionEx<T> items,List<string> paths ) : TreePath<T,TreeNode>( items,paths ) where T : IMultiListItem
{
  
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

  public TreeNode( )
  {
  }

  public List<IMultiListItem> Objects { get; set; } = [];
  public SortedList<string,TreeNode> Children { get; set; } = [];

  public IList<TreeNode> ChildrenItems => Children.Values;
  
  public void RaisePropertyChanged( )
  {
    OnPropertyChanged( nameof( ChildrenItems ) );
  }
}

public class TreeLeaf( object item ) : TreeNode
{
  public object Item { get; } = item;
}

public class TreeLeafCheckbox( object item ) : TreeLeaf( item )
{
}