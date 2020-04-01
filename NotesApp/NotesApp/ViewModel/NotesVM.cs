using NotesApp.Model;
using NotesApp.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesApp.ViewModel
{
    public class NotesVM : INotifyPropertyChanged
    {
		private bool isEditing;

		public bool IsEditing
		{
			get { return isEditing; }
			set 
			{ 
				isEditing = value;
				OnPropertyChanged("IsEditing");
			}
		}

		public ObservableCollection<Notebook> Notebooks { get; set; }
		
		private Notebook selectedNotebook;
		public Notebook SelectedNotebook
		{
			get { return selectedNotebook; }
			set 
			{ 
				selectedNotebook = value;
				ReadNotes();
			}
		}

		private Note selectedNote;

		public Note SelectedNote
		{
			get { return selectedNote; }
			set 
			{ 
				selectedNote = value;
				SelectedNoteChanged(this, new EventArgs());
			}
		}


		public ObservableCollection<Note> Notes { get; set; }

		public NewNotebookCommand NewNotebookCommand { get; set; }
		public NewNoteCommand NewNoteCommand { get; set; }
		public BeginEditCommand BeginEditCommand { get; set; }
		public HasEditedCommand HasEditedCommand { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler SelectedNoteChanged;

		public NotesVM()
		{
			IsEditing = false;

			NewNotebookCommand = new NewNotebookCommand(this);
			NewNoteCommand = new NewNoteCommand(this);
			BeginEditCommand = new BeginEditCommand(this);
			HasEditedCommand = new HasEditedCommand(this);

			Notebooks = new ObservableCollection<Notebook>();
			Notes = new ObservableCollection<Note>();

			ReadNotebooks();
			ReadNotes();
		}

		private void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void CreateNote(int notebookId)
		{
			Note newNote = new Note()
			{
				NotebookId = notebookId,
				CreatedTime = DateTime.Now,
				UpdatedTime = DateTime.Now,
				Title = "New note"
			};

			DatabaseHelper.Insert(newNote);
			ReadNotes();
		}

		public void CreateNotebook()
		{
			Notebook newNotebook = new Notebook()
			{
				Name = "New notebook",
				UserId = int.Parse(App.UserId)
			};

			DatabaseHelper.Insert(newNotebook);
			ReadNotebooks();
		}

		public void ReadNotebooks()
		{
			using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(DatabaseHelper.GetFileLocation()))
			{
				//int userId;
				//bool isLogged = int.TryParse(App.UserId, out userId);
				var notebooks = conn.Table<Notebook>()/*.Where(n => isLogged && n.UserId == userId)*/.ToList();
				
				Notebooks.Clear();
				foreach (var notebook in notebooks)
				{
					Notebooks.Add(notebook);
				}
			}
		}

		public void ReadNotes()
		{
			if (SelectedNotebook != null)
			{
				using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(DatabaseHelper.GetFileLocation()))
				{
					var notes = conn.Table<Note>().Where(n => n.NotebookId == SelectedNotebook.Id).ToList();
					Notes.Clear();
					
					foreach (var note in notes)
					{
						Notes.Add(note);
					}
				}
			}
		}

		public void StartEditing()
		{
			IsEditing = true;
		}

		public void HasRenamed(Notebook notebook)
		{
			if (notebook != null)
			{
				DatabaseHelper.Update(notebook);
				IsEditing = false;
				ReadNotebooks();
			}
		}

		public void UpdateSelectedNote()
		{
			DatabaseHelper.Update(SelectedNote);
		}
	}
}
