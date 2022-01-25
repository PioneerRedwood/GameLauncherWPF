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
using GroguLauncher.Models;

namespace GroguLauncher.Views
{
	public partial class MessageWindow : Window
	{
		private readonly MainWindow mainWindow;

		private readonly ContactModel onTopContact;
		
		private ObservableCollection<ContactModel> contacts;
		
		public MessageWindow(MainWindow _mainWindow, ContactModel _onTopContact)
		{
			InitializeComponent();

			mainWindow = _mainWindow;

			// TODO: set top contact
			onTopContact = _onTopContact;

			// TODO: get Messages from DB
			contacts = new ObservableCollection<ContactModel>();
			
			SpeakToListBox.ItemsSource = contacts;
			
			for (int i = 0; i < 5; ++i)
			{
				contacts.Add(onTopContact);
			}

			Closed += MessageWindow_OnClosed;
		}

		private void MessageWindow_OnClosed(object sender, EventArgs e)
		{
			mainWindow.OnMessageWindowClosed();
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
	}
}
