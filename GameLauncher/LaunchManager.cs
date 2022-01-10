using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace GameLauncher
{
	struct Version
	{
		internal static Version zero = new(0, 0, 0);

		private short major;
		private short minor;
		private short subMinor;

		public Version(short major, short minor, short subMinor)
		{
			this.major = major;
			this.minor = minor;
			this.subMinor = subMinor;
		}

		internal Version(string version)
		{
			string[] versionStrings = version.Split('.');
			if (versionStrings.Length != 3)
			{
				major = 0;
				minor = 0;
				subMinor = 0;
				return;
			}

			major = short.Parse(versionStrings[0]);
			minor = short.Parse(versionStrings[1]);
			subMinor = short.Parse(versionStrings[2]);
		}

		internal bool IsDifferentThan(Version otherVersion)
		{
			if (major != otherVersion.major)
			{
				return true;
			}
			else
			{
				if (minor != otherVersion.minor)
				{
					return true;
				}
				else
				{
					if (subMinor != otherVersion.subMinor)
					{
						return true;
					}
				}
			}

			return false;
		}

		public override string ToString()
		{
			return $"{major}.{minor}.{subMinor}";
		}
	}

	internal class LaunchManager
	{
		private MainWindow mainWindow;
		private string rootPath;
		private string versionFile;
		private string gameZip;
		private string gameExe;
		private const string versionCheckLink = "https://drive.google.com/uc?export=download&id=1jAf3-xhgDoIajv0n5PnUXUyIjgb4GtiD";
		private const string gameZipUri = "https://drive.google.com/uc?export=download&id=128Cpdx-SQ2Dj9b9T-ZhOSEKXP4U1w3oX";

		private LauncherStatus _status;
		internal LauncherStatus Status
		{
			get => _status;
			set
			{
				_status = value;
				
				switch (_status)
				{
					case LauncherStatus.ready:
						mainWindow.PlayButton.Content = "Play";
						break;
					case LauncherStatus.failed:
						mainWindow.PlayButton.Content = "Update Failed - Retry";
						break;
					case LauncherStatus.downloadingGame:
						mainWindow.PlayButton.Content = "Downloading Game";
						break;
					case LauncherStatus.downloadingUpdate:
						mainWindow.PlayButton.Content = "Downloding Update";
						break;
					default:
						break;
				}
			}
		}

		public LaunchManager(MainWindow _mainWindow)
		{
			mainWindow = _mainWindow;

			rootPath = Directory.GetCurrentDirectory();
			versionFile = Path.Combine(rootPath, "Version.txt");
			gameZip = Path.Combine(rootPath, "Build.zip");
			gameExe = Path.Combine(rootPath, "Build", "BrawlMasters_01.exe");
		}

		public void CheckForUpdates()
		{
			if (File.Exists(versionFile))
			{
				Version localVersion = new Version(File.ReadAllText(versionFile));
				mainWindow.VersionText.Text = localVersion.ToString();

				try
				{
					WebClient client = new();
					Version onlineVersion = new(client.DownloadString(versionCheckLink));

					if (onlineVersion.IsDifferentThan(localVersion))
					{
						InstallGameFiles(true, onlineVersion);
					}
					else
					{
						Status = LauncherStatus.ready;
					}
				}
				catch (Exception ex)
				{
					Status = LauncherStatus.failed;
					MessageBox.Show($"Error checking for game updates {ex.Message}");
				}
			}
			else
			{
				InstallGameFiles(false, Version.zero);
			}
		}

		public void ExecuteGame()
		{
			if (File.Exists(gameExe) && Status == LauncherStatus.ready)
			{
				ProcessStartInfo startInfo = new(gameExe);
				startInfo.WorkingDirectory = Path.Combine(rootPath, "Build");
				//Process.Start(startInfo);

				//Close();
			}
			else if (Status == LauncherStatus.failed)
			{
				CheckForUpdates();
			}
		}

		private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
		{
			try
			{
				string onlineVersion = ((Version)e.UserState).ToString();
				ZipFile.ExtractToDirectory(gameZip, rootPath + "/build", true);
				File.Delete(gameZip);

				File.WriteAllText(versionFile, onlineVersion);

				mainWindow.VersionText.Text = onlineVersion;
				Status = LauncherStatus.ready;
			}
			catch (Exception ex)
			{
				Status = LauncherStatus.failed;
				MessageBox.Show($"Error finishing download: {ex}");
			}
		}

		private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
		{
			try
			{
				WebClient webClient = new();
				if (_isUpdate)
				{
					Status = LauncherStatus.downloadingUpdate;
				}
				else
				{
					Status = LauncherStatus.downloadingGame;
					_onlineVersion = new(webClient.DownloadString(versionCheckLink));
				}

				webClient.DownloadFileCompleted += new(DownloadGameCompletedCallback);
				webClient.DownloadFileAsync(new(gameZipUri), gameZip, _onlineVersion);
			}
			catch (Exception ex)
			{
				Status = LauncherStatus.failed;
				MessageBox.Show($"Error installing game files: {ex}");
			}
		}
	}
}
