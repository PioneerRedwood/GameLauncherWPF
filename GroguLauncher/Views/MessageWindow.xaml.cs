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
		private readonly ContactModel onTopContact;
		private ObservableCollection<ContactModel> contacts;
		public MessageWindow(ContactModel contact)
		{
			InitializeComponent();

			// TODO: set top contact
			onTopContact = contact;

			// TODO: get Messages from DB
			contacts = new ObservableCollection<ContactModel>();
			for(int i= 0; i < 5; ++i)
			{
				contacts.Add(onTopContact);
			}


		}

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
	}
}
