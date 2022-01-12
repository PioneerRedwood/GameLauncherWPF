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

			MailText.Text = "test@mail.com";
			PwdTextBox.Text = "1234";

			accountHandler = new AccountHandler();

			prevContent = Content;
		}

		public void NotifyAuthDone()
		{
			// TODO: OAuth done.. what you gonna do?

			// 1. check is there account of email
			// 2. if it isn't? make a new account
			// 3. it is? then login

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
				MailText.Text = googleAuthHandler.userInfo["email"];
			}
		}

		public async void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			if (MailText.Text.Length > 0 && PwdTextBox.Text.Length > 0)
			{
				Task<Dictionary<string, string>> loginTask = accountHandler.LoginSync(MailText.Text, PwdTextBox.Text);
				App.userInfo = await loginTask;
				if(App.userInfo.Count > 0)
				{
					//MessageBox.Show("Successfully logged in");
					// TODO: Update user data

					MainWindow window = new MainWindow();

					window.Show();
					Close();
				}
				else
				{
					ResultText.Text = "Failed to login";
				}
			}
			else
			{
				ResultText.Text = "Fill out ID, Password";
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
	}
}
