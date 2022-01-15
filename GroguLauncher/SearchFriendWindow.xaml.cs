﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GroguLauncher
{
	public partial class SearchFriendWindow : Window
	{
		private readonly Handlers.SocialHandler handler;
		public ObservableCollection<Social.Friend> FriendshipRequestList { get; private set; }

		public SearchFriendWindow(Handlers.SocialHandler socialHandler)
		{
			InitializeComponent();

			handler = socialHandler;

			FriendshipRequestList = new ObservableCollection<Social.Friend>();

			FriendshipRequestListBox.ItemsSource = FriendshipRequestList;
		}

		private async void SearchButton_Click(object sender, RoutedEventArgs e)
		{
			if ((SearchText.Text.Length > 0) && (SearchText.Text != App.UserInfo["USER_NAME"]))
			{
				if (await handler.AddFriendWithName(int.Parse(App.UserInfo["USER_ID"]), SearchText.Text, Handlers.SocialHandler.FriendshipStatusCode.Requested))
				{
					Social.Friend friend = new Social.Friend();
					friend.Name = SearchText.Text;

					FriendshipRequestList.Add(friend);
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
	}
}
