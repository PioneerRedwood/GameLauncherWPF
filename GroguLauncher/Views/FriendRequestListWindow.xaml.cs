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

namespace GroguLauncher
{
	public partial class FriendRequestListWindow : Window
	{
		private readonly ObservableCollection<ContactModel> FriendRequestList;
		private Handlers.SocialHandler socialHandler;

		public FriendRequestListWindow(ObservableCollection<ContactModel> requestList)
		{
			InitializeComponent();

			socialHandler = new Handlers.SocialHandler();

			// TODO: Get a request list
			FriendRequestList = requestList;
			FriendshipRequestListBox.ItemsSource = FriendRequestList;
		}

		private async void AcceptButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: PostRequest Accepted

			ContactModel friend = (sender as Button).DataContext as ContactModel;
			if (await socialHandler.PostRequestFriendRelation(int.Parse(App.UserInfo["USER_ID"]), friend.Id, Handlers.SocialHandler.FriendshipStatusCode.Accepted))
			{
				FriendRequestList.Remove(friend);
			}
		}

		private void DenyButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: PostRequest Denied
			ContactModel friend = (sender as Button).DataContext as ContactModel;
		}

		private void CompleteButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

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
	}
}
