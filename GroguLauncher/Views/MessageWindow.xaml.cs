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

		private ObservableCollection<UserModel> _contacts;

		private UserModel _selectedUser;

		public MessageWindow(MainWindow mainWindow, UserModel selectedUser)
		{
			InitializeComponent();

			_mainWindow = mainWindow;
			_selectedUser = selectedUser;

			_socialHandler = new SocialHandler();

			GetContactList();
			foreach (UserModel model in _contacts)
			{
				if (_selectedUser.Equals(model))
				{
					SpeakToListBox.SelectedItem = model;
					break;
				}
			}

			GetSelectedUserMessage();

			Closed += MessageWindow_OnClosed;
		}

		private bool IsInContacts(UserModel model)
		{
			bool result = false;
			foreach (UserModel user in _contacts)
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
				_selectedUser = model;
				GetSelectedUserMessage();

				AutoScrollToEnd();
			}
		}

		private void MessageWindow_OnClosed(object sender, EventArgs e)
		{
			_mainWindow.OnMessageWindowClosed();
		}

		private async void GetContactList()
		{
			_contacts = await _socialHandler.GetFriendList();

			SpeakToListBox.ItemsSource = _contacts;
		}

		private async void GetSelectedUserMessage()
		{
			_selectedUser = (UserModel)SpeakToListBox.SelectedItem;

			SpeakToNameLabel.Content = _selectedUser.Name;

			MessageListView.ItemsSource = await _socialHandler.GetMessageData(_selectedUser.Id);
		}

		private void SpeakToListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			GetSelectedUserMessage();

			AutoScrollToEnd();
		}

		private void AutoScrollToEnd()
		{
			if (MessageListView.Items.Count > 0)
			{
				MessageListView.SelectedItem = MessageListView.Items.GetItemAt(MessageListView.Items.Count - 1);
				MessageListView.ScrollIntoView(MessageListView.SelectedItem);
			}
		}

		private async void MessageText_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && MessageText.Text.Length > 0)
			{
				if (await _socialHandler.SendMessage(_selectedUser, MessageText.Text))
				{
					MessageText.Text = "";
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
