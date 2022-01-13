using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroguLauncher.Social
{
	public enum FriendState
	{
		Online = 0,
		Offline = 1,
		Sleeping = 2,

	}

	// ref https://docs.microsoft.com/ko-kr/dotnet/api/system.collections.specialized.inotifycollectionchanged?redirectedfrom=MSDN&view=net-6.0
	public class Friend : INotifyPropertyChanged
	{
		private string name_;
		private int id_;
		private DateTime date_;
		private bool isLoggedIn_;
		private FriendState state_;

		public string name
		{
			get => name_;
			set
			{
				if (name_ == value)
				{
					return;
				}
				name_ = value;
				NotifyChanged("name");
			}
		}
		public int id
		{
			get => id_;
			set
			{
				if (id_ == value)
				{
					return;
				}
				id_ = value;
				NotifyChanged("id");
			}
		}

		public DateTime date
		{
			get => date_;
			set
			{
				if (date_ == value)
				{
					return;
				}
				date_ = value;
				NotifyChanged("date");
			}
		}

		public bool isLoggedIn
		{
			get => isLoggedIn_;
			set
			{
				if (isLoggedIn_ == value)
				{
					return;
				}
				isLoggedIn_ = value;
				NotifyChanged("isLoggedIn");
			}
		}

		public FriendState state
		{
			get => state_;
			set
			{
				if (state_ == value)
				{
					return;
				}
				state_ = value;
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
