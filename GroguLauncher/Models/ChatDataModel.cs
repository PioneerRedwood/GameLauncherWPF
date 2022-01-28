using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroguLauncher.Models
{
	public class ChatDataModel : INotifyPropertyChanged
	{
		public ChatDataModel(string content)
		{
			_content = content;
		}

		private string _content;
		public string Content
		{
			get => _content;
			set
			{
				if(_content == null)
				{
					_content = value;
				}

				_content = value;
				NotifyChanged("content");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
