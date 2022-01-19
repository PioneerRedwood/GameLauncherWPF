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

namespace GroguLauncher
{
	public enum GamePatchStatus
	{
		Play,
		Failed,
		Updating,
		Uninitialized,
		Update,
		Initializing,
	}
	public struct GameVersion
	{
		public static GameVersion zero = new GameVersion(0, 0, 0);

		private readonly short major;
		private readonly short minor;
		private readonly short subMinor;

		public GameVersion(short major, short minor, short subMinor)
		{
			this.major = major;
			this.minor = minor;
			this.subMinor = subMinor;
		}

		public GameVersion(string version)
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

		public bool Equals(GameVersion otherVersion)
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
}

namespace GroguLauncher.Managers
{
	public class GameLaunchManager
	{
		private readonly MainWindow window;
		public List<string> GameList { get; private set; }
		public Dictionary<int, Components.GameComponent> AvailableGames { get; private set; }

		private Components.GameComponent selectedGame = null;

		public GameLaunchManager(MainWindow _mainWindow)
		{
			window = _mainWindow;

			GameList = new List<string> { "BrawlMasters", "TowerDefenseGame" };

			AvailableGames = new Dictionary<int, Components.GameComponent>
			{
				{0,
					new Components.GameComponent ("BrawlMasters", 0,
					"https://drive.google.com/uc?export=download&id=12JtzLaz57Y3_vuSEq3LQRq7RBqy_aHyA",
					"https://drive.google.com/uc?export=download&id=1GB3B0K_84nXBsQyR27JBbiGu04Y3E-m2",
					"https://github.com/PioneerRedwood/BrawlMasters")
				},
				{1,
				new Components.GameComponent("TowerDefenseGame", 1,
				"https://drive.google.com/uc?export=download&id=1iIjTEanlGKVz6pwcWF_UUqw3Npa1-zSh",
				"https://drive.google.com/uc?export=download&id=1e1AOC6ssD5jV2e0LgcoCck60APZga0KO",
				"https://github.com/PioneerRedwood/Unity_2D_DefenseGame")
				},
			};

			// TODO: async check for all avable game list
			InitializeGames();
		}

		public void InitializeGames()
		{
			for (int index = 0; index < AvailableGames.Count; ++index)
			{
				InitializeGame(AvailableGames[index]);
			}
		}

		public void InitializeGame(Components.GameComponent game)
		{
			if (game.IsInitialized)
			{
				return;
			}

			// Check the version
			if (File.Exists(game.VersionFile))
			{
				GameVersion version = new GameVersion(File.ReadAllText(game.VersionFile));

				if (!version.Equals(game.Version))
				{
					DownloadVersionFile(game);
				}
			}
			else
			{
				DownloadVersionFile(game);
			}
		}

		private void DownloadVersionFile(Components.GameComponent game)
		{
			try
			{
				WebClient client = new WebClient();

				client.DownloadStringCompleted +=
					new DownloadStringCompletedEventHandler(
						(object sender, DownloadStringCompletedEventArgs token) =>
						{
							Components.GameComponent temp = (Components.GameComponent)token.UserState;
							GameVersion newVersion = new GameVersion(token.Result);

							temp.IsInitialized = true;
							temp.Version = newVersion;
							temp.Status = GamePatchStatus.Update;

							File.WriteAllText(temp.VersionFile, token.Result);

							int index = GameList.IndexOf(temp.Name);
							if(index == window.GameListBox.SelectedIndex)
							{
								NotifySelectedGameChanged(index);
							}

						});

				client.DownloadStringAsync(new Uri(game.VersionUri), game);
			}
			catch (Exception ex)
			{
				OnGamePatchStatusChanged(GamePatchStatus.Failed, ex);
			}
		}

		public void NotifySelectedGameChanged(int index)
		{
			// TODO: should be conducted after GameComponent initiailzed end
			Console.WriteLine("NotifySelectedGameChanged " + index);

			selectedGame = AvailableGames[index];

			window.CurrentGameLabel.Content = selectedGame.Name;
			window.GamePatchButton.Content = selectedGame.Status.ToString();

			window.MainBrowser.Address = selectedGame.PageUrl;
			window.VersionLabel.Content = selectedGame.Version.ToString();
		}

		public void ExecuteGame()
		{
			if (File.Exists(selectedGame.ExeFile) && selectedGame.Status == GamePatchStatus.Play)
			{
				ProcessStartInfo startInfo = new ProcessStartInfo(selectedGame.ExeFile);
				startInfo.WorkingDirectory = Path.Combine(selectedGame.RootPath, selectedGame.Name);

				window.Hide();

				Process.Start(startInfo).WaitForExit();

				window.Show();
			}
		}

		public void InstallGame()
		{
			try
			{
				WebClient webClient = new WebClient();
				OnGamePatchStatusChanged(GamePatchStatus.Updating, null);

				// TODO: delete all the game files exist
				if (File.Exists(selectedGame.ExeFile))
				{
					ClearOldGameFiles();
				}

				webClient.DownloadFileCompleted +=
					new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs token) =>
					{
						try
						{
							ZipFile.ExtractToDirectory(selectedGame.ZipFile, selectedGame.RootPath);
							File.Delete(selectedGame.ZipFile);

							OnGamePatchStatusChanged(GamePatchStatus.Play, null);
						}
						catch (Exception ex)
						{
							OnGamePatchStatusChanged(GamePatchStatus.Failed, ex);
						}
					});

				webClient.DownloadFileAsync(new Uri(selectedGame.ZipUri), selectedGame.ZipFile);
			}
			catch (Exception ex)
			{
				OnGamePatchStatusChanged(GamePatchStatus.Failed, ex);
			}
		}

		// ref https://stackoverflow.com/questions/5519888/delete-everything-in-a-directory-except-a-file-in-c-sharp
		private void ClearOldGameFiles()
		{
			// TODO: delete every files and directories exclude Version.txt
			string[] filePaths = Directory.GetFiles(selectedGame.RootPath);
			foreach(string filePath in filePaths)
			{
				FileInfo info = new FileInfo(filePath);
				string lowerName = info.Name.ToLower();
				if (lowerName != "version.txt")
				{
					File.Delete(filePath);
				}
			}

			foreach(string dirPath in Directory.GetDirectories(selectedGame.RootPath))
			{
				Directory.Delete(dirPath, true);
			}
		}

		private void OnGamePatchStatusChanged(GamePatchStatus status, Exception ex)
		{
			selectedGame.Status = status;
			window.GamePatchButton.Content = status.ToString();

			switch (status)
			{
				case GamePatchStatus.Play:
					break;
				case GamePatchStatus.Failed:
					MessageBox.Show($"Error installing game files: {ex}");
					break;
				case GamePatchStatus.Updating:
					break;
				case GamePatchStatus.Update:
					break;
				case GamePatchStatus.Initializing:
					break;
				case GamePatchStatus.Uninitialized:
					break;
				default:
					break;
			}
		}


	}
}
