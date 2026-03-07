using CommunityToolkit.Mvvm.ComponentModel;

using Framework.Bindables;

namespace Framework.UI;

public partial class UIItem : UIBase,IMultiListItem
{
    [ObservableProperty] private bool _isActive;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isHighlighted;
}

public class UIItemList : MultiList<UIItem>
{
    
}