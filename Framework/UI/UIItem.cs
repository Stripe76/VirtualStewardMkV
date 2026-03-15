using CommunityToolkit.Mvvm.ComponentModel;

using Framework.Bindables;

namespace Framework.UI;

public partial class UIItem : UIBase,IMultiListItem
{
    private bool _isActive;
    private bool _isMultiActive;
    
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isHighlighted;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isMultiActive = false;

            SetProperty( ref _isActive,value );
        }
    }
    public bool IsMultiActive
    {
        get => _isMultiActive;
        set
        {
            _isMultiActive = _isActive = value;

            OnPropertyChanged( );
            OnPropertyChanged( nameof( IsActive ) );
        }
    }
}

public class UIItemList : MultiList<UIItem>
{
    
}