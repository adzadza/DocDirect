using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DocDirect.Commands
{
    public class RelayCommand<T> : ICommand
    {
        #region Fields
        private readonly Action<T> m_Execute = null;
        private readonly Predicate<object> m_CanExecute = null;
        #endregion

        #region Constructors
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }
        
        public RelayCommand(Action<T> execute, Predicate<object> canExecute)
        {
            if (execute == null)
	        {
                throw new ArgumentNullException("Execute");
	        }

            m_Execute = execute;
            m_CanExecute = canExecute;
        }

        #endregion

        #region ICommand Implementation

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return m_CanExecute == null ? true : m_CanExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (parameter is T)
            {
                var typedParameter = (T)parameter;
                m_Execute(typedParameter);
            }
        }

        #endregion
    }
}
