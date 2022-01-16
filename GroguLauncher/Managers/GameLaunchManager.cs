// ref https://github.com/tom-weiland/csharp-game-launcher

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace GroguLauncher.Managers
{
	public enum GamePatchStatus
	{
		Ready,
		Failed,
		Updating,
	}
	internal struct GameVersion
	{
		internal static GameVersion zero = new GameVersion(0, 0, 0);

		private short major;
		private short minor;
		private short subMinor;

		public GameVersion(short major, short minor, short subMinor)
		{
			this.major = major;
			this.minor = minor;
			this.subMinor = subMinor;
		}

		internal GameVersion(string version)
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

		internal bool Equals(GameVersion otherVersion)
		{
			return (major == otherVersion.major)
				& (minor == otherVersion.minor)
				& (subMinor == otherVersion.subMinor);
		}

		public override string ToString()
		{
			return $"{major}.{minor}.{subMinor}";
		}
	}

	public class GameLaunchManager
	{
		private readonly MainWindow window;
		public static Dictionary<string, Components.GameComponent> AvailableGameList;
		private Components.GameComponent selectedGame;
		

		public GameLaunchManager(MainWindow _mainWindow)
		{
			window = _mainWindow;

			AvailableGameList = new Dictionary<string, Components.GameComponent>
			{
				{"BrawlMasters",
					new Components.GameComponent ("BrawlMasters", 0,
					"https://drive.google.com/uc?export=download&id=12JtzLaz57Y3_vuSEq3LQRq7RBqy_aHyA",
					"https://drive.google.com/uc?export=download&id=1GB3B0K_84nXBsQyR27JBbiGu04Y3E-m2",
					"https://github.com/PioneerRedwood/BrawlMasters")
				},
				{"TowerDefenseGame",
				new Components.GameComponent("TowerDefenseGame", 1,
				"https://drive.google.com/uc?export=download&id=1iIjTEanlGKVz6pwcWF_UUqw3Npa1-zSh",
				"https://drive.google.com/uc?export=download&id=1e1AOC6ssD5jV2e0LgcoCck60APZga0KO",
				"https://github.com/PioneerRedwood/Unity_2D_DefenseGame")
				},
			};
		}

		public void NotifySelectedGameChanged(string name)
		{
			selectedGame = AvailableGameList[name];

			// TODO: change view
			window.CurrentGameText.Text = name;
			window.GamePatchButton.Content = selectedGame.Status.ToString();
			window.MainBrowser.Address = selectedGame.PageUrl;
			
		}

		// TODO: !OPTIMIZATION!
		public void CheckForUpdates()
		{
			if (selectedGame.Status == GamePatchStatus.Ready)
			{
				return;
			}

			if (File.Exists(selectedGame.VersionFile))
			{
				GameVersion localVersion = new GameVersion(File.ReadAllText(selectedGame.VersionFile));
				window.VersionText.Text = localVersion.ToString();

				try
				{
					WebClient client = new WebClient();

					GameVersion newVersion =
						new GameVersion(client.DownloadString(selectedGame.VersionUri));

					if (newVersion.Equals(localVersion))
					{
						OnGamePatchStatusChanged(GamePatchStatus.Ready, null);
					}
					else
					{
						// need to update
						InstallGameFiles(true, newVersion);
					}
				}
				catch (Exception ex)
				{
					OnGamePatchStatusChanged(GamePatchStatus.Failed, ex);
				}
			}
			else
			{
				InstallGameFiles(false, GameVersion.zero);
			}
		}

		public void ExecuteGame()
		{
			if (File.Exists(selectedGame.ExeFile) && selectedGame.Status == GamePatchStatus.Ready)
			{
				ProcessStartInfo startInfo = new ProcessStartInfo(selectedGame.ExeFile);
				startInfo.WorkingDirectory = Path.Combine(selectedGame.RootPath, selectedGame.Name);

				window.Hide();

				Process.Start(startInfo).WaitForExit();

				window.Show();
			}
			else if (selectedGame.Status == GamePatchStatus.Failed)
			{
				CheckForUpdates();
			}
		}

		private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs token)
		{
			try
			{
				string newVersion = ((GameVersion)token.UserState).ToString();
				ZipFile.ExtractToDirectory(selectedGame.ZipFile, selectedGame.RootPath);
				File.Delete(selectedGame.ZipFile);

				File.WriteAllText(selectedGame.VersionFile, newVersion);
				OnGamePatchStatusChanged(GamePatchStatus.Ready, null);

				window.VersionText.Text = newVersion;
			}
			catch (Exception ex)
			{
				OnGamePatchStatusChanged(GamePatchStatus.Failed, ex);
			}
		}

		private void InstallGameFiles(bool isUpdate, GameVersion version)
		{
			try
			{
				WebClient webClient = new WebClient();
				OnGamePatchStatusChanged(GamePatchStatus.Updating, null);

				if (!isUpdate)
				{
					version = new GameVersion(webClient.DownloadString(selectedGame.VersionUri));
				}

				webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
				webClient.DownloadFileAsync(new Uri(selectedGame.ZipUri), selectedGame.ZipFile, version);
			}
			catch (Exception ex)
			{
				OnGamePatchStatusChanged(GamePatchStatus.Failed, ex);
			}
		}

		private void OnGamePatchStatusChanged(GamePatchStatus status, Exception ex)
		{
			selectedGame.Status = status;
			switch (status)
			{
				case GamePatchStatus.Ready:
					selectedGame.Status = GamePatchStatus.Ready;
					window.GamePatchButton.Content = "Play";
					break;
				case GamePatchStatus.Failed:
					window.GamePatchButton.Content = "Update Failed - Retry";
					MessageBox.Show($"Error installing game files: {ex}");
					break;
				case GamePatchStatus.Updating:
					selectedGame.Status = GamePatchStatus.Updating;
					window.GamePatchButton.Content = "Updating";
					break;
			}
		}
	}
}
