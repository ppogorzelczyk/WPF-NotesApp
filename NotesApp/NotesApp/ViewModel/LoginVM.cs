using NotesApp.Model;
using NotesApp.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesApp.ViewModel
{
    public class LoginVM
    {
		private Users user;

		public Users User
		{
			get { return user; }
			set { user = value; }
		}

		public RegisterCommand RegisterCommand { get; set; }
		public LoginCommand LoginCommand { get; set; }

		public event EventHandler HasLoggedIn;

		public LoginVM()
		{
			RegisterCommand = new RegisterCommand(this);
			LoginCommand = new LoginCommand(this);
			User = new Users();
		}

		public async void Login()
		{
			//using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(DatabaseHelper.GetFileLocation()))
			//{
			//	conn.CreateTable<Users>();

			//	var user = conn.Table<Users>().Where(u => u.Username == User.Username).FirstOrDefault();

			//	if (user.Password == User.Password)
			//	{
			//		App.UserId = user.Id.ToString();
			//		HasLoggedIn(this, new EventArgs());
			//	}
			//}

			try
			{
				var user = (await App.MobileServiceClient.GetTable<Users>().Where(u => u.Username == User.Username).ToListAsync()).FirstOrDefault();
				if (user != null && user.Password == User.Password)
				{
					App.UserId = user.Id;
					HasLoggedIn(this, new EventArgs());
				}
			}
			catch (Exception ex)
			{

				throw ex;
			}
			
		}

		public async void Register()
		{
			//using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(DatabaseHelper.GetFileLocation()))
			//{
			//	conn.CreateTable<User>();

			//	var result = DatabaseHelper.Insert(User);
			//	if (result)
			//	{
			//		App.UserId = User.Id.ToString();
			//		HasLoggedIn(this, new EventArgs());
			//	}
			//}

			User = await DatabaseHelper.Insert(User);
			if (!string.IsNullOrEmpty(User.Id))
			{
				App.UserId = User.Id;
				HasLoggedIn(this, new EventArgs());
			}
		}
	}
}
