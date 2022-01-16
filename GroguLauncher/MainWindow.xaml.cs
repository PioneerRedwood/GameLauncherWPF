﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using GroguLauncher.Managers;
using GroguLauncher.Handlers;
using GroguLauncher.Components;
using GroguLauncher.Social;


namespace GroguLauncher
{
	public partial class MainWindow : Window
	{
		public GameLaunchManager LaunchManager { get; private set; }
		public SocialHandler SocialHandler { get; private set; }

		public ObservableCollection<Friend> FriendList { get; private set; }
		public ObservableCollection<Friend> FriendRequestList { get; private set; }

		public ObservableCollection<GameComponent> GameList { get; private set; }

		// TODO: set for test
		private readonly List<string> gameList;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;

			LaunchManager = new GameLaunchManager(this);
			SocialHandler = new SocialHandler();
			GameList = new ObservableCollection<GameComponent>();
			gameList = new List<string> { "BrawlMasters", "TowerDefenseGame" };

			UserNameText.Text = App.UserInfo["USER_NAME"];

			foreach (string gameName in gameList)
			{
				GameList.Add(GameLaunchManager.AvailableGameList[gameName]);
			}

			GameListBox.ItemsSource = GameList;

			Loaded += Window_Loaded;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SocialSectorUpdate();

			// TODO: first selected item
			GameListBox.SelectedIndex = 0;
			GameSectorUpdate();
		}

		private void GameSectorUpdate()
		{
			LaunchManager.NotifySelectedGameChanged(gameList[GameListBox.SelectedIndex]);
			LaunchManager.CheckForUpdates();
		}

		private async void SocialSectorUpdate()
		{
			// ref https://stackoverflow.com/questions/17237034/wpf-chat-list-box-with-user-image-display
			FriendListBox.ItemsSource = await SocialHandler.GetFriendList();

			FriendRequestList = await SocialHandler.GetFriendRequestList();
			if (FriendRequestList.Count > 0)
			{
				// TODO: dynamically create component named FriendRequestListGrid

				FriendRequestListGrid.IsEnabled = true;
				FriendRequestCountText.Content = FriendRequestList.Count;
			}
			else
			{
				FriendRequestListGrid.IsEnabled = false;
				FriendRequestCountText.Content = 0;
			}
		}

		private void GamePatchButton_Click(object sender, RoutedEventArgs e)
		{
			if (((GameComponent)GameListBox.SelectedItem).Status == GamePatchStatus.Ready)
			{
				LaunchManager.ExecuteGame();
			}
		}

		private void ProfileGrid_MouseEnter(object sender, MouseEventArgs e)
		{
			//ProfileGrid.Background = new SolidColorBrush(Color.FromArgb(0, 0x7a, 0x7a, 0x78));
			ProfileGrid.Background = Brushes.Red;
		}

		private void ProfileGrid_MouseLeave(object sender, MouseEventArgs e)
		{
			ProfileGrid.Background = null;
		}

		private void OpenSearchFriendWindowButton_Click(object sender, RoutedEventArgs e)
		{
			SearchFriendWindow window = new SearchFriendWindow(SocialHandler);
			window.ShowDialog();
		}

		private void FriendRequestListGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			FriendRequestListWindow window = new FriendRequestListWindow(FriendRequestList);
			window.ShowDialog();

			SocialSectorUpdate();
		}

		private void LogoutButton_Click(object sender, RoutedEventArgs e)
		{
			App.UserInfo.Clear();

			LoginWindow window = new LoginWindow();
			window.Show();

			Close();
		}

		private void GameListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			GameSectorUpdate();
		}
	}
}
