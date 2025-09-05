// Corrected RelayCommand and ViewModel example to resolve compilation errors.

using System;
using System.ComponentModel;
using System.Windows.Input;

/// <summary>
/// A command that can be bound to UI elements and encapsulates an action and a "can execute" predicate.
/// This implementation fixes the ambiguity issues seen in the provided error messages.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Occurs when changes occur that affect whether the command should execute.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>True if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object parameter)
    {
        // Check if the command can be executed.
        // If _canExecute is null, it means the command can always be executed.
        return _canExecute == null || _canExecute(parameter);
    }

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    public void Execute(object parameter)
    {
        // Execute the command's action.
        _execute(parameter);
    }
}

/// <summary>
/// An example ViewModel demonstrating the use of the corrected RelayCommand.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    private double _value;
    private double _discount;
    private double _finalValue;

    public ICommand CalculateCommand { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public double Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    public double Discount
    {
        get => _discount;
        set
        {
            if (_discount != value)
            {
                _discount = value;
                OnPropertyChanged(nameof(Discount));
            }
        }
    }

    public double FinalValue
    {
        get => _finalValue;
        private set
        {
            if (_finalValue != value)
            {
                _finalValue = value;
                OnPropertyChanged(nameof(FinalValue));
            }
        }
    }

    public MainViewModel()
    {
        // Initialize values
        Value = 1000;
        Discount = 10;

        // Use a new command name to reflect its purpose
        CalculateCommand = new RelayCommand(
            execute: _ => ExecuteMyAction(),
            canExecute: _ => CanExecuteMyAction()
        );

        // Perform the initial calculation
        ExecuteMyAction();
    }

    private void ExecuteMyAction()
    {
        // Calculate the final value using the provided formula
        // Use 100.0 to ensure floating-point division
        FinalValue = Value - (Value * (Discount / 100.0));
        Console.WriteLine($"Original Value: {Value}");
        Console.WriteLine($"Discount Percentage: {Discount}%");
        Console.WriteLine($"Final Value: {FinalValue}");
    }

    private bool CanExecuteMyAction()
    {
        // Ensure that Value and Discount are valid numbers and Discount is a valid percentage
        return Value >= 0 && Discount >= 0 && Discount <= 100;
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
