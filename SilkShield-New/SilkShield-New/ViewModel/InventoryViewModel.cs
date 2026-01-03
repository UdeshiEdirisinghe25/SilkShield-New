using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SilkShield_New.Data;
using SilkShield_New.View;

namespace SilkShield_New.ViewModel
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _databaseHelper;
        private List<InventoryItem> _fullInventoryItems;
        private ObservableCollection<InventoryItem> _inventoryItems;
        private InventoryItem _selectedItem;
        private string _filterText;
        private bool _isDataGridReadOnly = true;

        public bool IsDataGridReadOnly
        {
            get => _isDataGridReadOnly;
            set
            {
                if (_isDataGridReadOnly != value)
                {
                    _isDataGridReadOnly = value;
                    OnPropertyChanged(nameof(IsDataGridReadOnly));
                }
            }
        }

        public InventoryViewModel()
        {
            _databaseHelper = new DatabaseHelper();
            InventoryItems = new ObservableCollection<InventoryItem>();

            // Initialize commands
            EditCommand = new CustomizeCommand<InventoryItem>(ExecuteEdit);
            DeleteCommand = new CustomizeCommand<InventoryItem>(ExecuteDelete);

            // Load all data once at startup
            LoadAllInventoryItems();
            ApplyFilter();
        }

        #region Properties

        public ObservableCollection<InventoryItem> InventoryItems
        {
            get => _inventoryItems;
            set
            {
                _inventoryItems = value;
                OnPropertyChanged(nameof(InventoryItems));
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (_filterText != value)
                {
                    _filterText = value;
                    OnPropertyChanged(nameof(FilterText));
                    ApplyFilter();
                }
            }
        }

        public InventoryItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        #endregion

        #region Commands

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        #endregion

        #region Methods

        private void LoadAllInventoryItems()
        {
            try
            {
                // Use OrderByDescending to put the highest ItemID (newest) at the top
                _fullInventoryItems = _databaseHelper.GetAllInventoryItems()
                                                     .OrderByDescending(item => item.ItemID)
                                                     .ToList();

                // If you are using a filtered collection for the UI, update it here too
                // FilteredInventoryItems = new ObservableCollection<InventoryItem>(_fullInventoryItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory items: {ex.Message}", "Error");
            }
        }

        private void ApplyFilter()
        {
            InventoryItems.Clear();

            if (string.IsNullOrWhiteSpace(FilterText))
            {
                foreach (var item in _fullInventoryItems)
                {
                    InventoryItems.Add(item);
                }
            }
            else
            {
                var filteredItems = _fullInventoryItems
                                     .Where(item => item.ItemName.ToLower().Contains(FilterText.ToLower()))
                                     .ToList();

                foreach (var item in filteredItems)
                {
                    InventoryItems.Add(item);
                }
            }
        }

        // Public refresh method used by MainWindow.RefreshInventoryIfActive()
        public void LoadInventoryData()
        {
            try
            {
                LoadAllInventoryItems();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing inventory: {ex.Message}", "Error");
            }
        }

        // *UPDATED METHOD*
        public void UpdateItem(InventoryItem item)
        {
            if (item == null) return;

            try
            {
                bool updated = _databaseHelper.UpdateInventoryItem(item);
                if (updated)
                {
                    // Find the item in the full list and update its properties
                    var existingItem = _fullInventoryItems.FirstOrDefault(i => i.ItemID == item.ItemID);
                    if (existingItem != null)
                    {
                        // Update the properties of the existing item
                        existingItem.ItemName = item.ItemName;
                        existingItem.Category = item.Category;
                        existingItem.Material = item.Material;
                        existingItem.MeasuringUnit = item.MeasuringUnit;
                        existingItem.UnitPrice = item.UnitPrice;
                        existingItem.StockStatus = item.StockStatus;
                    }

                    // Refresh the DataGrid to show the changes
                    ApplyFilter();

                    MessageBox.Show($"Item '{item.ItemName}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to update item.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsDataGridReadOnly = true;
            }
        }

        private void ExecuteEdit(InventoryItem item)
        {
            if (item == null) return;

            if (Application.Current.MainWindow is SilkShield_New.View.MainWindow mainWin)
            {
                // 1. Create the UserControl (the Edit view)
                var editView = new SilkShield_New.View.EditInventory();

                // 2. Create and assign the ViewModel with the selected item data
                var editVm = new EditInventoryViewModel(item);
                editView.DataContext = editVm;

                // 3. FIX: Set Content instead of calling Navigate
                mainWin.MainContentArea.Content = editView;
            }
        }

        private void ExecuteDelete(InventoryItem item)
        {
            if (item == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete '{item.ItemName}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool deleted = _databaseHelper.DeleteInventoryItem(item.ItemID);
                    if (deleted)
                    {
                        InventoryItems.Remove(item);
                        _fullInventoryItems.Remove(item);

                        MessageBox.Show("Item deleted successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete item.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting item: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class CustomizeCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public CustomizeCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

    public class CustomizeCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public CustomizeCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}