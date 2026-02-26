using Framework.Bindables;

namespace Framework.UI.Values;

public class MultilistValue<T> : BaseValue<T> where T : class,IMultiListItem
{
    public MultiList<T> Items { get; }
    
    public MultilistValue(string name,string title,MultiList<T> items) : base(null, name, title)
    {
        Items = items;
        Items.SelectedItemChanged += SelectedItemChanged;
        
        ValueChanged += SelectedValueChanged;
    }

    private void SelectedValueChanged(T? obj)
    {
        obj?.IsSelected = true;
    }
    private void SelectedItemChanged(object? sender, T? e)
    {
        Value = e;
    }
}