using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GroguLauncher
{
	public class GameModel : INotifyPropertyChanged
	{
		private string _name;
		private int _id;
		private GamePatchStatus _status = GamePatchStatus.Uninitialized;

		public bool IsInitialized = false;
		public GameVersion Version;
		public string RootPath { get; private set; }
		public string VersionFile { get; private set; }
		public string ZipFile { get; private set; }
		public string ExeFile { get; private set; }
		public string VersionUri { get; private set; }
		public string ZipUri { get; private set; }
		public string PageUrl { get; private set; }

		public GameModel(string _name, int _id, string _versionUri, string _zipUri, string _pageUrl)
		{
			this._name = _name;
			this._id = _id;
			VersionUri = _versionUri;
			ZipUri = _zipUri;
			PageUrl = _pageUrl;

			RootPath = Directory.GetCurrentDirectory() + $"/{this._name}";

			if (!Directory.Exists(RootPath))
			{
				Directory.CreateDirectory(RootPath);
			}

			VersionFile = Path.Combine(RootPath, "Version.txt");
			ZipFile = Path.Combine(RootPath, this._name + ".zip");
			ExeFile = Path.Combine(RootPath, this._name + ".exe");
		}

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

		public GamePatchStatus Status
		{
			get => _status;
			set 
			{
				if(_status == value)
				{
					return;
				}
				_status = value;
				NotifyChanged("status");
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
