// ref https://github.com/tom-weiland/csharp-game-launcher

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
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
		public static GameVersion Zero = new GameVersion(0, 0, 0);

		private readonly short _major;
		private readonly short _minor;
		private readonly short _subMinor;

		public GameVersion(short major, short minor, short subMinor)
		{
			_major = major;
			_minor = minor;
			_subMinor = subMinor;
		}

		public GameVersion(string version)
		{
			string[] versionStrings = version.Split('.');
			if (versionStrings.Length != 3)
			{
				_major = 0;
				_minor = 0;
				_subMinor = 0;
				return;
			}

			_major = short.Parse(versionStrings[0]);
			_minor = short.Parse(versionStrings[1]);
			_subMinor = short.Parse(versionStrings[2]);
		}

		public bool Equals(GameVersion otherVersion)
		{
			return (_major == otherVersion._major)
				& (_minor == otherVersion._minor)
				& (_subMinor == otherVersion._subMinor);
		}

		public override string ToString()
		{
			return $"{_major}.{_minor}.{_subMinor}";
		}
	}
}

namespace GroguLauncher.Handlers
{
	public class GameLaunchHandler
	{
		public Dictionary<int, GameModel> AvailableGames { get; private set; }

		private readonly List<string> _gameList;
		private readonly MainWindow _mainWindow;
		private GameModel _selectedGame = null;

		public GameLaunchHandler(MainWindow _mainWindow)
		{
			this._mainWindow = _mainWindow;

			_gameList = new List<string> { "BrawlMasters", "TowerDefenseGame" };

			AvailableGames = new Dictionary<int, GameModel>
			{
				{0,
					new GameModel ("BrawlMasters", 0,
					"https://drive.google.com/uc?export=download&id=12JtzLaz57Y3_vuSEq3LQRq7RBqy_aHyA",
					"https://drive.google.com/uc?export=download&id=1GB3B0K_84nXBsQyR27JBbiGu04Y3E-m2",
					"local://game.gg/BrawlMasters.html")
				},

				{1,
				new GameModel("TowerDefenseGame", 1,
				"https://drive.google.com/uc?export=download&id=1iIjTEanlGKVz6pwcWF_UUqw3Npa1-zSh",
				"https://drive.google.com/uc?export=download&id=1e1AOC6ssD5jV2e0LgcoCck60APZga0KO",
				"local://game.gg/TowerDefenseGame.html")
				},
			};

			InitializeGames();
		}

		private void InitializeGames()
		{
			for (int index = 0; index < AvailableGames.Count; ++index)
			{
				InitializeGame(AvailableGames[index]);
			}
		}

		public void InitializeGame(GameModel game)
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

		private void DownloadVersionFile(GameModel game)
		{
			try
			{
				WebClient client = new WebClient();

				client.DownloadStringCompleted +=
					new DownloadStringCompletedEventHandler(
						(object sender, DownloadStringCompletedEventArgs token) =>
						{
							GameModel temp = (GameModel)token.UserState;
							GameVersion newVersion = new GameVersion(token.Result);

							temp.IsInitialized = true;
							temp.Version = newVersion;
							temp.Status = GamePatchStatus.Update;

							File.WriteAllText(temp.VersionFile, token.Result);

							int index = _gameList.IndexOf(temp.Name);
							if (index == _mainWindow.GameListBox.SelectedIndex)
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
			if(_selectedGame == null || _selectedGame != AvailableGames[index]) 
			{
				_selectedGame = AvailableGames[index];

				_mainWindow.CurrentGameLabel.Content = _selectedGame.Name;
				_mainWindow.GamePatchButton.Content = _selectedGame.Status.ToString();

				_mainWindow.CefBrowser_UrlChanged(_selectedGame.PageUrl);
				_mainWindow.VersionLabel.Content = _selectedGame.Version.ToString();
			}
		}

		public void ExecuteGame()
		{
			if (File.Exists(_selectedGame.ExeFile) && _selectedGame.Status == GamePatchStatus.Play)
			{
				ProcessStartInfo startInfo = new ProcessStartInfo(_selectedGame.ExeFile);
				startInfo.WorkingDirectory = Path.Combine(_selectedGame.RootPath, _selectedGame.Name);

				_mainWindow.Hide();

				Process.Start(startInfo).WaitForExit();

				_mainWindow.Show();
			}
		}

		public void InstallGame()
		{
			try
			{
				WebClient webClient = new WebClient();
				OnGamePatchStatusChanged(GamePatchStatus.Updating, null);

				// TODO: delete all the game files exist
				if (File.Exists(_selectedGame.ExeFile))
				{
					ClearOldGameFiles();
				}

				webClient.DownloadFileCompleted +=
					new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs token) =>
					{
						try
						{
							ZipFile.ExtractToDirectory(_selectedGame.ZipFile, _selectedGame.RootPath);
							File.Delete(_selectedGame.ZipFile);

							OnGamePatchStatusChanged(GamePatchStatus.Play, null);
						}
						catch (Exception ex)
						{
							OnGamePatchStatusChanged(GamePatchStatus.Failed, ex);
						}
					});

				webClient.DownloadFileAsync(new Uri(_selectedGame.ZipUri), _selectedGame.ZipFile);
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
			string[] filePaths = Directory.GetFiles(_selectedGame.RootPath);
			foreach (string filePath in filePaths)
			{
				FileInfo info = new FileInfo(filePath);
				string lowerName = info.Name.ToLower();
				if (lowerName != "version.txt")
				{
					File.Delete(filePath);
				}
			}

			foreach (string dirPath in Directory.GetDirectories(_selectedGame.RootPath))
			{
				Directory.Delete(dirPath, true);
			}
		}

		private void OnGamePatchStatusChanged(GamePatchStatus status, Exception ex)
		{
			_selectedGame.Status = status;
			_mainWindow.GamePatchButton.Content = status.ToString();

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
