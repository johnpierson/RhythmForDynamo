// The 3d spatial tags dialog is Revit 2025 and up, with the node it belongs to.
#if R25_OR_GREATER
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Rhythm.SpatialTagsUi
{
    /// <summary>
    /// The two pieces of MVVM this dialog needs, written out rather than taken from a package.
    ///
    /// The add-in this is ported from used CommunityToolkit.Mvvm. Rhythm deploys into a flat bin
    /// folder shared with everything else Dynamo has loaded, where every additional assembly is a
    /// version-conflict waiting to happen, and this is two small classes. Nothing else in the
    /// toolkit was being used.
    /// </summary>
    internal abstract class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// A command backed by a delegate, with an optional guard.
    ///
    /// <see cref="RaiseCanExecuteChanged"/> is routed through <see cref="CommandManager"/>, so the
    /// buttons re-ask their guard whenever WPF next re-evaluates commands. That is what keeps
    /// "Create / Update Tags" disabled until there is something to tag without the view model
    /// having to know which buttons exist.
    /// </summary>
    internal sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(Cast(parameter));
        }

        public void Execute(object parameter)
        {
            _execute(Cast(parameter));
        }

        /// <summary>
        /// A null parameter, or one of the wrong type, becomes the default rather than throwing.
        /// WPF passes null before a binding has resolved, which is not a programming error.
        /// </summary>
        private static T Cast(object parameter)
        {
            return parameter is T ? (T)parameter : default(T);
        }
    }
}
#endif
