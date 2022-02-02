using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

using CefSharp;

using GroguLauncher.Handlers;
using GroguLauncher.Models;
using System;

namespace GroguLauncher
{
	public partial class MainWindow : Window
	{
		public GameLaunchHandler LaunchManager { get; private set; }
		public SocialHandler SocialHandler { get; private set; }
		public ObservableCollection<UserModel> FriendRequestList { get; private set; }
		public ObservableCollection<GameModel> GameList { get; private set; }

		private Views.MessageWindow _messageWindow;

		private Views.ChattingLobbyWindow _chattingLobbyWindow;

		private Cef.CefHandler _cefHandler;

		public MainWindow()
		{
			// TODO: CefSharp.Cef.Initialize()
			if (!InitCefSharp())
			{
				Close();
				return;
			}

			InitializeComponent();
			DataContext = this;

			UserNameLabel.Content = App.UserInfo["USER_NAME"];

			LaunchManager = new GameLaunchHandler(this);
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
		
		#region CefSharp 
		private bool InitCefSharp()
		{
			bool result = false;

			if (_cefHandler == null)
			{
				_cefHandler = new Cef.CefHandler();
			}

			if (!CefSharp.Cef.IsInitialized)
			{
				if (CefSharp.Cef.Initialize(_cefHandler.Settings))
				{
					result = true;
				}
			}

			return result;
		}

		public void CefBrowser_UrlChanged(string url)
		{
			CefBrowser.Load(url);
		}
		#endregion

		#region Profile mouse event
		private void ProfileGrid_MouseEnter(object sender, MouseEventArgs e)
		{
			//ProfileGrid.Background = new SolidColorBrush(Color.FromArgb(0, 0x7a, 0x7a, 0x78));
			ProfileGrid.Background = Brushes.Red;
		}

		private void ProfileGrid_MouseLeave(object sender, MouseEventArgs e)
		{
			ProfileGrid.Background = null;
		}
		#endregion

		private void OpenSearchFriendWindowButton_Click(object sender, RoutedEventArgs e)
		{
			SearchFriendWindow searchFriendWindow = new SearchFriendWindow(SocialHandler);

			searchFriendWindow.ShowDialog();
		}

		private void FriendRequestListGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			FriendRequestListWindow window = new FriendRequestListWindow(FriendRequestList, SocialHandler);
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

		#region Control bar
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
		#endregion

		#region When FriendListBox Double Clicked, Show MessageWindow
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
			if (_messageWindow != null)
			{
				// TODO: message window set this model on top
				if (_messageWindow.WindowState == WindowState.Minimized)
				{
					_messageWindow.WindowState = WindowState.Normal;
				}
				_messageWindow.SetCurrentSelectedUser(model);
				_messageWindow.Focus();
			}
			else
			{
				_messageWindow = new Views.MessageWindow(this, model);
				_messageWindow.SetCurrentSelectedUser(model);

				_messageWindow.Show();
				//_messageWindow.Focus();
			}
		}

		public void OnMessageWindowClosed()
		{
			_messageWindow = null;
			Focus();
		}

		#endregion

		private void LicenseApp_Click(object sender, RoutedEventArgs e)
		{
			// open the license window
			Views.LicenseWindow licenseWindow = new Views.LicenseWindow();
			licenseWindow.ShowDialog();
		}

		private void OpenChattingLobbyWindowButton_Click(object sender, RoutedEventArgs e)
		{
			if (_chattingLobbyWindow != null)
			{
				if (_chattingLobbyWindow.WindowState == WindowState.Minimized)
				{
					_chattingLobbyWindow.WindowState = WindowState.Normal;
				}

				//_chattingLobbyWindow.Focus();
				_chattingLobbyWindow.Activate();
			}
			else
			{
				_chattingLobbyWindow = new Views.ChattingLobbyWindow(this);

				_chattingLobbyWindow.Show();
			}
		}

		public void OnChattingLobbyWindowClosed()
		{
			_chattingLobbyWindow = null;
			Focus();
		}
	}
}
