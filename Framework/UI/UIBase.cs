using CommunityToolkit.Mvvm.ComponentModel;
using Framework.Bindables;

namespace Framework.UI;

public partial class UIBase : ObservableObject
{
    [ObservableProperty] private bool _isVisible;
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private bool _isExpanded;
}

public class UIBaseList : ObservableCollectionEx<UIBase>
{
    
}