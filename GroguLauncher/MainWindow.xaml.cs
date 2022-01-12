using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GroguLauncher
{
	enum LauncherStatus
	{
		ready,
		failed,
		downloadingGame,
		downloadingUpdate
	}

	public partial class MainWindow : Window
	{
		public Dictionary<string, string> userInfo { get; private set; }

		private Managers.LaunchManager launchManager;

		public MainWindow()
		{
			InitializeComponent();

			launchManager = new Managers.LaunchManager(this);

			UserNameText.Text = App.userInfo["USER_NAME"];
		}


		private void Window_ContentRendered(object sender, EventArgs e)
		{
			launchManager.CheckForUpdates();

		}

		private void PlayButton_Click(object sender, RoutedEventArgs e)
		{
			launchManager.ExecuteGame();
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


	}
}
