using System;
using System.Windows.Input;

namespace Project2FA.Core.Utils
{
    public class RelayCommand : ICommand, IDisposable
    {
        public event EventHandler CanExecuteChanged;

        private readonly Func<bool> _canExecuteFunc;
        private readonly Action<object> _executeAction;

        public RelayCommand(Action<object> action) : this(action, () => true)
        {
        }

        public RelayCommand(Action<object> action, Func<bool> canExecuteFunc)
        {
            _executeAction = action;
            _canExecuteFunc = canExecuteFunc;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteFunc();
        }

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }

        public void RemoveAllEvents()
        {
            CanExecuteChanged = null;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region IDisposable

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveAllEvents();
            }
        }

        #endregion IDisposable
    }
}
