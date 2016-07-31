using System;
using System.Windows.Input;

namespace ActorExtractor.Core
{
    public class RelayCommand : ICommand
    {
        public Func<object, bool> CanExecute { get; set; }
        public Action<object> Execute { get; set; }

        public RelayCommand(Action execute) : this((o) => execute(), null)
        { }

        public RelayCommand(Action<object> execute) : this(execute, null)
        { }

        public RelayCommand(Action execute, Func<bool> canExecute) : this((o) => execute(), (o) => canExecute())
        { }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            Execute = execute;
            CanExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute?.Invoke(parameter) ?? true;
        }

        void ICommand.Execute(object parameter)
        {
            Execute?.Invoke(parameter);
        }
    }
}
