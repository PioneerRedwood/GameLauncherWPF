using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GroguLauncher.Handlers;
using GroguLauncher.Models;

namespace GroguLauncher.Views
{
	public partial class MessageWindow : Window
	{
		private readonly MainWindow _mainWindow;

		private readonly SocialHandler _socialHandler;

		private ObservableCollection<UserModel> _users;

		private UserModel _selectedUser;

		public MessageWindow(MainWindow mainWindow, UserModel selectedUser)
		{
			InitializeComponent();

			_mainWindow = mainWindow;
			_selectedUser = selectedUser;

			_socialHandler = new SocialHandler();

			GetContactList();

			GetSelectedUserMessage(selectedUser);

			Closed += (object sender, EventArgs e) =>
			{
				_mainWindow.OnMessageWindowClosed();
			};
		}

		private bool IsInContacts(UserModel model)
		{
			bool result = false;
			foreach (UserModel user in _users)
			{
				if (model.Equals(user))
				{
					result = true;
				}
			}

			return result;
		}

		public void SetCurrentSelectedUser(UserModel model)
		{
			if (model != null && IsInContacts(model))
			{
				GetSelectedUserMessage(model);

				foreach (UserModel user in _users)
				{
					if (_selectedUser.Equals(user))
					{
						SpeakToListBox.SelectedItem = user;
						break;
					}
				}

				AutoScrollToEnd();
			}
		}

		private async void GetContactList()
		{
			_users = await _socialHandler.GetFriendList();

			SpeakToListBox.ItemsSource = _users;
		}

		private async void GetSelectedUserMessage(UserModel model)
		{
			_selectedUser = model;

			SpeakToNameLabel.Content = _selectedUser.Name;

			MessageListView.ItemsSource = await _socialHandler.GetMessageData(_selectedUser.Id);
		}

		private void SpeakToListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			GetSelectedUserMessage((UserModel)SpeakToListBox.SelectedItem);

			AutoScrollToEnd();
		}

		private void AutoScrollToEnd()
		{
			if (MessageListView.Items.Count > 0)
			{
				object item = MessageListView.Items.GetItemAt(MessageListView.Items.Count - 1);
				MessageListView.ScrollIntoView(item);
			}
		}

		private async void MessageText_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && MessageText.Text.Length > 0)
			{
				if (await _socialHandler.SendMessage(_selectedUser, MessageText.Text))
				{
					MessageText.Text = "";
					MessageListView.ItemsSource = await _socialHandler.GetMessageData(_selectedUser.Id);
				}
				else
				{
					// ERROR? 
				}
			}
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
			Close();
		}

		#endregion
	}
}
