using System;
using System.ComponentModel;

namespace GroguLauncher
{
	public enum CurrentConnectionState
	{
		Online = 0,
		Offline = 1,
		Sleeping = 2,

	}

	// ref https://docs.microsoft.com/ko-kr/dotnet/api/system.collections.specialized.inotifycollectionchanged?redirectedfrom=MSDN&view=net-6.0
	public class ContactModel : INotifyPropertyChanged
	{
		private string name;
		private int id;
		private DateTime date;
		private bool isLoggedIn;
		private CurrentConnectionState state;

		public string Name
		{
			get => name;
			set
			{
				if (name == value)
				{
					return;
				}
				name = value;
				NotifyChanged("name");
			}
		}
		public int Id
		{
			get => id;
			set
			{
				if (id == value)
				{
					return;
				}
				id = value;
				NotifyChanged("id");
			}
		}

		public DateTime Date
		{
			get => date;
			set
			{
				if (date == value)
				{
					return;
				}
				date = value;
				NotifyChanged("date");
			}
		}

		public bool IsLoggedIn
		{
			get => isLoggedIn;
			set
			{
				if (isLoggedIn == value)
				{
					return;
				}
				isLoggedIn = value;
				NotifyChanged("isLoggedIn");
			}
		}

		public CurrentConnectionState State
		{
			get => state;
			set
			{
				if (state == value)
				{
					return;
				}
				state = value;
				NotifyChanged("state");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
