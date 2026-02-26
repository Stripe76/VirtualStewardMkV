namespace Framework.UI.ViewModels;

public class SideBar : UIBase
{
    public UIItemList Items
    {
        get;
    } 
    
    public SideBar(UIItemList items)
    {
        Items = items;
    }
}