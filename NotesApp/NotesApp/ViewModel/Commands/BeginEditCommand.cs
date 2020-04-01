using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NotesApp.ViewModel.Commands
{
    public class BeginEditCommand : ICommand
    {
        public NotesVM VM { get; set; }
        public event EventHandler CanExecuteChanged;

        public BeginEditCommand(NotesVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            VM.StartEditing();
        }
    }
}
