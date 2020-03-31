using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesApp.Model
{
    public class User : INotifyPropertyChanged
    {
		private int id;
		[PrimaryKey, AutoIncrement]
		public int Id
		{
			get { return id; }
			set 
			{
				id = value;
				OnPropertyChanged("Id");
			}
		}

		private string username;
		[MaxLength(50)]
		public string Username
		{
			get { return username; }
			set
			{
				username = value;
				OnPropertyChanged("Username");
			}
		}

		private string name;
		[MaxLength(50)]
		public string Name
		{
			get { return name; }
			set 
			{
				name = value;
				OnPropertyChanged("Name");
			}
		}

		private string lastName;
		[MaxLength(50)]
		public string LastName
		{
			get { return lastName; }
			set 
			{ 
				lastName = value;
				OnPropertyChanged("LastName");
			}
		}

		private string email;

		public string Email
		{
			get { return email; }
			set 
			{ 
				email = value;
				OnPropertyChanged("Email");
			}
		}

		private string password;

		public string Password
		{
			get { return password; }
			set 
			{ 
				password = value;
				OnPropertyChanged("Password");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
