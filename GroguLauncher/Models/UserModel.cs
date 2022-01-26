using System;
using System.ComponentModel;
using System.Windows;

namespace GroguLauncher.Models
{
	public enum CurrentConnectionState
	{
		Online = 0,
		Offline = 1,
		Sleeping = 2,
	}

	// ref https://docs.microsoft.com/ko-kr/dotnet/api/system.collections.specialized.inotifycollectionchanged?redirectedfrom=MSDN&view=net-6.0
	public class UserModel : INotifyPropertyChanged
	{
		private string _name;
		private int _id;
		private bool _isLoggedIn;
		private CurrentConnectionState _state;

		public string Name
		{
			get => _name;
			set
			{
				if (_name == value)
				{
					return;
				}
				_name = value;
				NotifyChanged("name");
			}
		}
		public int Id
		{
			get => _id;
			set
			{
				if (_id == value)
				{
					return;
				}
				_id = value;
				NotifyChanged("id");
			}
		}

		public bool IsLoggedIn
		{
			get => _isLoggedIn;
			set
			{
				if (_isLoggedIn == value)
				{
					return;
				}
				_isLoggedIn = value;
				NotifyChanged("isLoggedIn");
			}
		}

		public CurrentConnectionState State
		{
			get => _state;
			set
			{
				if (_state == value)
				{
					return;
				}
				_state = value;
				NotifyChanged("state");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public override string ToString()
		{
			return _name + " " + Id + " " + _state.ToString();
		}

		public bool Equals(UserModel model)
		{
			return _id == model.Id;
		}
	}
}
