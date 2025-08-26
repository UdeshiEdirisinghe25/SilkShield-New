using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilkShield_New.ViewModel.ViewModel
{
    internal class NewInventoryViewModel
    {
    }
}
public class NewInventoryViewModel : INotifyPropertyChanged
{
    private string _itemName = "";
    public string ItemName
    {
        get => _itemName;
        set
        {
            {
                if (_itemName != value)
                    _itemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }
    }

    private string _category= string.Empty;
    public string Category
    {
        get => _category;
        set
        {
            _category = value;
            OnPropertyChanged(nameof(Category));
        }
    }

    private string _material;
    public string Material
    {
        get => _material;
        set
        {
            _material = value;
            OnPropertyChanged(nameof(Material));
        }
    }

    
   

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string ItemName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ItemName));
    }
}
