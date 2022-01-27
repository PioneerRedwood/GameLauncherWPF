using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GroguLauncher.Handlers;
using GroguLauncher.Models;

namespace GroguLauncher
{
	public partial class FriendRequestListWindow : Window
	{
		private readonly ObservableCollection<UserModel> FriendRequestList;
		private readonly SocialHandler _socialHandler;

		public FriendRequestListWindow(ObservableCollection<UserModel> requestList, SocialHandler socialHandler)
		{
			InitializeComponent();

			_socialHandler = socialHandler;

			FriendRequestList = requestList;
			FriendshipRequestListBox.ItemsSource = FriendRequestList;
		}

		private async void AcceptButton_Click(object sender, RoutedEventArgs e)
		{
			UserModel friend = (sender as Button).DataContext as UserModel;
			if (await _socialHandler.PostRequestFriendRelation(
						int.Parse(App.UserInfo["USER_ID"]),
						friend.Id,
						SocialHandler.FriendshipStatusCode.Accepted))
			{
				FriendRequestList.Remove(friend);
			}
		}

		private async void DenyButton_Click(object sender, RoutedEventArgs e)
		{
			UserModel friend = (sender as Button).DataContext as UserModel;
			if (await _socialHandler.PostRequestFriendRelation(
						int.Parse(App.UserInfo["USER_ID"]),
						friend.Id,
						SocialHandler.FriendshipStatusCode.Denied))
			{
				FriendRequestList.Remove(friend);
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
