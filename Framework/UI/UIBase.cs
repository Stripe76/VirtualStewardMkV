using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Bindables;

namespace Framework.UI;

public partial class UIBase : ObservableObject
{
    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private bool _isExpanded = false;

    public bool DeleteItem
    {
        get => false;
        set => OnPropertyChanged(nameof(DeleteItem));
    }
}

public class UIBaseList : ObservableCollectionEx<UIBase>
{
    
}