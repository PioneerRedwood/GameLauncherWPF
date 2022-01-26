using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using GroguLauncher.Managers;
using GroguLauncher.Handlers;
using System.Windows.Controls;
using System.Data.Linq;

using GroguLauncher.Models;

namespace GroguLauncher
{
	public partial class MainWindow : Window
	{
		public GameLaunchManager LaunchManager { get; private set; }
		public SocialHandler SocialHandler { get; private set; }
		public ObservableCollection<UserModel> FriendRequestList { get; private set; }
		public ObservableCollection<GameModel> GameList { get; private set; }

		private Views.MessageWindow _messageWindow;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;

			// WARNING! Set non-UI elements first
			UserNameLabel.Content = App.UserInfo["USER_NAME"];

			LaunchManager = new GameLaunchManager(this);
			SocialHandler = new SocialHandler();
			GameList = new ObservableCollection<GameModel>();

			for (int index = 0; index < LaunchManager.AvailableGames.Count; ++index)
			{
				GameList.Add(LaunchManager.AvailableGames[index]);
			}

			GameListBox.SelectedIndex = 0;
			GameListBox.ItemsSource = GameList;

			Loaded += Window_Loaded;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SocialSectorUpdate();
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
			GamePatchStatus status = ((GameModel)GameListBox.SelectedItem).Status;
			switch (status)
			{
				case GamePatchStatus.Play:
					LaunchManager.ExecuteGame();
					break;
				case GamePatchStatus.Update:
					LaunchManager.InstallGame();
					break;
				case GamePatchStatus.Uninitialized:
					LaunchManager.InitializeGame((GameModel)GameListBox.SelectedItem);
					break;
				default:
					break;
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
			LaunchManager.NotifySelectedGameChanged(GameListBox.SelectedIndex);
		}

		private void Border_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				DragMove();
			}
		}

		private void MiniMizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}


		#region When FriendListBox Double Clciked, Show MessageWindow
		// ref https://docs.microsoft.com/ko-kr/dotnet/desktop/wpf/graphics-multimedia/hit-testing-in-the-visual-layer?view=netframeworkdesktop-4.8
		private void FriendListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			Point point = e.GetPosition(FriendListBox);

			VisualTreeHelper.HitTest(FriendListBox,
				new HitTestFilterCallback(FriendListHitTestFilter),
				new HitTestResultCallback(FriendListHitTestResult),
				new PointHitTestParameters(point));
		}

		private HitTestResultBehavior FriendListHitTestResult(HitTestResult result)
		{
			return HitTestResultBehavior.Continue;
		}

		private HitTestFilterBehavior FriendListHitTestFilter(DependencyObject @object)
		{
			UserModel model = new UserModel();

			if (@object.GetType() == typeof(Image))
			{
				Image wrapper = (Image)@object;
				object context = wrapper.DataContext;

				if (model.GetType().IsAssignableFrom(context.GetType()))
				{
					model = (UserModel)context;
					ShowMessageWindow(model);

					return HitTestFilterBehavior.Stop;
				}
			}
			else if (@object.GetType() == typeof(Border))
			{
				Border wrapper = (Border)@object;
				object context = wrapper.DataContext;

				if (model.GetType().IsAssignableFrom(context.GetType()))
				{
					model = (UserModel)context;
					ShowMessageWindow(model);

					return HitTestFilterBehavior.Stop;
				}
			}

			return HitTestFilterBehavior.Continue;
		}

		private void ShowMessageWindow(UserModel model)
		{
			if(_messageWindow != null)
			{
				// TODO: message window set this model on top
				if(_messageWindow.WindowState == WindowState.Minimized)
				{
					_messageWindow.WindowState = WindowState.Normal;
				}
				_messageWindow.Focus();
			}
			else
			{
				_messageWindow = new Views.MessageWindow(this, model);
				_messageWindow.Show();
				_messageWindow.Focus();
			}
		}

		public void OnMessageWindowClosed()
		{
			_messageWindow = null;
			Focus();
		}

		//public void SetMessageWindow
		#endregion
	}
}
