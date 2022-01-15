using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GroguLauncher
{
	public partial class MainWindow : Window
	{
		public Managers.LaunchManager LaunchManager { get; private set; }
		public Handlers.SocialHandler SocialHandler { get; private set; }

		public ObservableCollection<Social.Friend> FriendList { get; private set; }
		public ObservableCollection<Social.Friend> FriendRequestList { get; private set; }

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;

			LaunchManager = new Managers.LaunchManager(this);
			SocialHandler = new Handlers.SocialHandler();

			UserNameText.Text = App.UserInfo["USER_NAME"];

			Loaded += Window_Loaded;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SocialSectorUpdate();
		}

		private void SocialSectorUpdate()
		{
			GetFriendList();
			GetFriendRequestList();
		}

		// ref https://stackoverflow.com/questions/17237034/wpf-chat-list-box-with-user-image-display
		private async void GetFriendList()
		{
			FriendListBox.ItemsSource = await SocialHandler.GetFriendList();
		}

		private async void GetFriendRequestList()
		{
			FriendRequestList = await SocialHandler.GetFriendRequestList();
			if(FriendRequestList.Count > 0)
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

			LaunchManager.CheckForUpdates();
			LaunchManager.ExecuteGame();
		}

		private void ProfileImage_MouseEnter(object sender, MouseEventArgs e)
		{
			ProfileImage.ToolTip = "ProfileImage";
		}

		private void ProfileImage_MouseLeave(object sender, MouseEventArgs e)
		{

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

		// TODO: Search friend
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
	}
}
