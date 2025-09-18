using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SilkShield_New.Data;

namespace SilkShield_New.ViewModel
{
    public class InventoryViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper _databaseHelper;
        private List<InventoryItem> _fullInventoryItems;
        private ObservableCollection<InventoryItem> _inventoryItems;
        private InventoryItem _selectedItem;
        private string _filterText;

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
                    ApplyFilter(); // Apply the filter every time the text changes
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
        // The SearchCommand is no longer needed for real-time filtering

        #endregion

        #region Methods

        private void LoadAllInventoryItems()
        {
            try
            {
                // Retrieve all items from the database and store them in a full list
                _fullInventoryItems = _databaseHelper.GetAllInventoryItems().ToList();
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
                // If the filter text is empty or only whitespace, add all items back
                foreach (var item in _fullInventoryItems)
                {
                    InventoryItems.Add(item);
                }
            }
            else
            {
                // Filter the full list based on the search text
                var filteredItems = _fullInventoryItems
                                      .Where(item => item.ItemName.ToLower().Contains(FilterText.ToLower()))
                                      .ToList();

                // Populate the ObservableCollection with the filtered results
                foreach (var item in filteredItems)
                {
                    InventoryItems.Add(item);
                }
            }
        }

        private void ExecuteEdit(InventoryItem item)
        {
            if (item == null) return;

            // TODO: Open edit window/dialog
            MessageBox.Show($"Edit functionality for {item.ItemName} will be implemented here.",
                "Edit Item", MessageBoxButton.OK, MessageBoxImage.Information);
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
                        // Remove from both the filtered list and the full list
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

    // Generic RelayCommand implementation (unchanged)
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

    // Non-generic RelayCommand for commands with no parameters (unchanged)
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