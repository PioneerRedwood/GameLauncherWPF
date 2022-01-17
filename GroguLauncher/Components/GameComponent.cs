using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GroguLauncher.Components
{
	public class GameComponent : INotifyPropertyChanged
	{
		private string name;
		private int id;

		public bool IsInitialized = false;
		public GamePatchStatus Status = GamePatchStatus.Uninitialized;
		public GameVersion Version;
		public string RootPath { get; private set; }
		public string VersionFile { get; private set; }
		public string ZipFile { get; private set; }
		public string ExeFile { get; private set; }
		public string VersionUri { get; private set; }
		public string ZipUri { get; private set; }
		public string PageUrl { get; private set; }

		public GameComponent(string _name, int _id, string _versionUri, string _zipUri, string _pageUrl)
		{
			name = _name;
			id = _id;
			VersionUri = _versionUri;
			ZipUri = _zipUri;
			PageUrl = _pageUrl;

			RootPath = Directory.GetCurrentDirectory() + $"/{name}";

			if (!Directory.Exists(RootPath))
			{
				Directory.CreateDirectory(RootPath);
			}

			VersionFile = Path.Combine(RootPath, "Version.txt");
			ZipFile = Path.Combine(RootPath, name + ".zip");
			ExeFile = Path.Combine(RootPath, name + ".exe");
		}

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
