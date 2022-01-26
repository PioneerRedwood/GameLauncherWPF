using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace GroguLauncher.Models
{
	public class MessageDataModel : INotifyPropertyChanged
	{
		private UserModel _sender;
		private UserModel _receiver;
		private DateTime _messageDate;
		private string _message;

		public UserModel Sender 
		{
			get => _sender;
			set
			{
				if(_sender == value) 
				{
					return;
				}

				_sender = value;
				NotifyChanged("sender");
			} 
		}
		public UserModel Receiver
		{
			get => _receiver;
			set
			{
				if (_receiver == value)
				{
					return;
				}

				_receiver = value;
				NotifyChanged("receiver");
			}
		}
		public DateTime MessageDate
		{
			get => _messageDate;
			set
			{
				if (_messageDate == value)
				{
					return;
				}

				_messageDate = value;
				NotifyChanged("messageDate");
			}
		}
		public string Message
		{
			get => _message;
			set
			{
				if (_message == value)
				{
					return;
				}

				_message = value;
				NotifyChanged("message");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
