using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GroguLauncher.Handlers;
using GroguLauncher.Models;

namespace GroguLauncher
{
	public partial class SearchFriendWindow : Window
	{
		private readonly ObservableCollection<UserModel> _requestList;

		private readonly SocialHandler _socialHandler;

		public SearchFriendWindow(SocialHandler socialHandler)
		{
			InitializeComponent();

			_socialHandler = socialHandler;

			_requestList = new ObservableCollection<UserModel>();

			FriendshipRequestListBox.ItemsSource = _requestList;
		}

		private async void SearchButton_Click(object sender, RoutedEventArgs e)
		{
			if ((SearchText.Text.Length > 0) && (SearchText.Text != App.UserInfo["USER_NAME"]))
			{
				if (await _socialHandler.AddFriendWithName(int.Parse(App.UserInfo["USER_ID"]), SearchText.Text, SocialHandler.FriendshipStatusCode.Requested))
				{
					UserModel friend = new UserModel();
					friend.Name = SearchText.Text;

					_requestList.Add(friend);
				}

				SearchText.Text = "";
			}
			else
			{
				MessageBox.Show("You can't send the message to yourself!");
				return;
			}
		}

		private void CompleteButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		#region Control bar
		private void Border_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				DragMove();
			}
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion
	}
}
