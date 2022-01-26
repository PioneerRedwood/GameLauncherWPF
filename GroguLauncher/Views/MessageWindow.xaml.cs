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
			foreach(UserModel model in _contacts)
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

		private void MessageWindow_OnClosed(object sender, EventArgs e)
		{
			_mainWindow.OnMessageWindowClosed();
		}

		#region Control Title Actions
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

		private async void GetContactList()
		{
			_contacts = await _socialHandler.GetFriendList();

			SpeakToListBox.ItemsSource = _contacts;
		}

		private async void GetSelectedUserMessage()
		{
			// change UI
			_selectedUser = (UserModel)SpeakToListBox.SelectedItem;

			SpeakToNameLabel.Content = _selectedUser.Name;

			Console.WriteLine("Get Selected User Message");
			MessageListView.ItemsSource = await _socialHandler.GetMessageData(_selectedUser.Id);
		}

		private void SpeakToListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			GetSelectedUserMessage();

			if(MessageListView.Items.Count > 0)
			{
				Console.WriteLine("Set auto scroll to the end");
				// auto scroll to the end of list
				MessageListView.SelectedItem = MessageListView.Items.Count - 1;
				MessageListView.ScrollIntoView(MessageListView.SelectedItem);

				MessageListView.SelectedItem = MessageListView.Items.GetItemAt(MessageListView.Items.Count - 1);
				MessageListView.ScrollIntoView(MessageListView.SelectedItem);
			}
		}

		private async void MessageText_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter && MessageText.Text.Length > 0)
			{
				if(await _socialHandler.SendMessage(_selectedUser, MessageText.Text))
				{
					MessageText.Text = "";
				}
				else
				{
					// ERROR? 
				}
			}
		}
	}
}
