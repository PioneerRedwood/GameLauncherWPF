using System;
using System.Collections.Generic;
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

using GroguLauncher.Managers;
using GroguLauncher.Handlers;


namespace GroguLauncher
{
	public partial class LoginWindow : Window
	{
		public AccountHandler accountHandler { get; private set; }
		public GoogleAuthHandler googleAuthHandler { get; private set; }
		public object prevContent { get; private set; }
		public bool isOAuthSucceed { get; private set; }

		public LoginWindow()
		{
			InitializeComponent();

			MailText.Text = "A@mail.com";
			PwdTextBox.Text = "123";

			accountHandler = new AccountHandler();

			prevContent = Content;
		}

		public void NotifyAuthDone()
		{
			// TODO: OAuth done.. what you gonna do?
			StoreCurrentContent();
			isOAuthSucceed = true;

			CreateAccountPage page = new CreateAccountPage(this);
			Content = page;
		}

		public void StoreCurrentContent()
		{
			prevContent = Content;
		}

		public void LoadPreviousContent()
		{
			Content = prevContent;
			if (isOAuthSucceed)
			{
				MailText.Text = googleAuthHandler.UserInfo["email"];
			}
			else
			{
				MailText.Text = "";
			}
		}

		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			Login();
		}

		private async void Login()
		{
			if (MailText.Text.Length > 0 && PwdTextBox.Text.Length > 0)
			{
				App.UserInfo = await accountHandler.Login(MailText.Text, PwdTextBox.Text);

				if (App.UserInfo.Count > 0)
				{
					MainWindow window = new MainWindow();

					window.Show();
					Close();
				}
				else
				{
					ResultLabel.Content = "Failed to login";
				}
			}
			else
			{
				ResultLabel.Content = "Fill out ID, Password";
			}
		}

		public void BattlenetAuthButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: 배틀넷 로그인

		}

		public void GoogleAuthButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: 구글 로그인
			// https://github.com/googlesamples/oauth-apps-for-windows/blob/master/OAuthDesktopApp/

			googleAuthHandler = new GoogleAuthHandler(this);
			googleAuthHandler.Authenticate();
		}

		public void KakaoAuthButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: Kakao 로그인

		}

		private void SignupText_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			StoreCurrentContent();

			CreateAccountPage page = new CreateAccountPage(this);
			Content = page;
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					Login();
					break;
				default:
					break;
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

		private void SignupLabel_MouseEnter(object sender, MouseEventArgs e)
		{
			SignupLabel.Background = Brushes.White;
			SignupLabel.Foreground = Brushes.Black;
		}

		private void SignupLabel_MouseLeave(object sender, MouseEventArgs e)
		{
			SignupLabel.Background = Brushes.Transparent;
			SignupLabel.Foreground = Brushes.White;
		}
	}
}
