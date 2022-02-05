using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GroguLauncher.Handlers;

namespace GroguLauncher
{
	public partial class LoginWindow : Window
	{
		public AccountHandler AccountHandler { get; private set; }
		public GoogleAuthHandler GoogleAuthHandler { get; private set; }
		public bool IsOAuthSucceed { get; private set; }

		private object _prevContent;
		private MainWindow _mainWindow = null;

		public LoginWindow()
		{
			InitializeComponent();

			// TODO: First Debugging
			MailText.Text = "A@mail.com";
			PwdBox.Password = "123";

			AccountHandler = new AccountHandler();

			_prevContent = Content;
		}

		// ref https://social.msdn.microsoft.com/Forums/en-US/85faaad1-a092-4bfe-b934-2d0ae917b654/reopen-a-window-in-wpf?forum=wpf
		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			Dispatcher dispatcher = Application.Current.Dispatcher;
			dispatcher.InvokeAsync(new Action(() => { Hide(); }));
		}

		public void NotifyAuthDone()
		{
			// TODO: OAuth done.. what you gonna do?
			StoreCurrentContent();
			IsOAuthSucceed = true;

			CreateAccountPage page = new CreateAccountPage(this);
			Content = page;
		}

		public void StoreCurrentContent()
		{
			_prevContent = Content;
		}

		public void LoadPreviousContent()
		{
			Content = _prevContent;
			if (IsOAuthSucceed)
			{
				MailText.Text = GoogleAuthHandler.UserInfo["email"];
			}
			else
			{
				MailText.Text = "";
			}
		}

		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			TryLogin();
		}

		private async void TryLogin()
		{
			if (MailText.Text.Length > 0 && PwdBox.Password.Length > 0)
			{
				// TODO: Login result type as enum
				App.UserInfo = await AccountHandler.Login(MailText.Text, PwdBox.Password);

				if (App.UserInfo.ContainsKey("USER_ID"))
				{
					if (_mainWindow == null)
					{
						_mainWindow = new MainWindow(this);

						_mainWindow.Show();
					}
					else
					{
						_mainWindow.Show();
					}

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

		#region OAuth click
		public void BattlenetAuthButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: 배틀넷 로그인

		}

		public void GoogleAuthButton_Click(object sender, RoutedEventArgs e)
		{
			// ref https://github.com/googlesamples/oauth-apps-for-windows/blob/master/OAuthDesktopApp/

			GoogleAuthHandler = new GoogleAuthHandler(this);
			GoogleAuthHandler.Authenticate();
		}

		public void KakaoAuthButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: Kakao 로그인

		}
		#endregion

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
					TryLogin();
					break;
				default:
					break;
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
