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
		private AccountHandler accountHandler;
		private GoogleAuthHandler googleAuthHandler;
		public object prevContent { get; private set; }

		public LoginWindow()
		{
			InitializeComponent();

			MailTextBox.Text = "test@mail.com";
			PwdTextBox.Text = "1234";

			accountHandler = new AccountHandler();

			prevContent = Content;
		}

		public void NotifyAuthDone()
		{
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
		}

		public async void LoginButton_Click(object sender, RoutedEventArgs e)
		{
			if (MailTextBox.Text.Length > 0 && PwdTextBox.Text.Length > 0)
			{
				// TODO: 동기 함수 실행 성공하면, MainWindow로
				Task<bool> loginTask = accountHandler.SyncLogin(MailTextBox.Text, PwdTextBox.Text);
				bool result = await loginTask;
				if(result)
				{
					MessageBox.Show("Successfully logged in");
					// TODO: Update user data

					// TODO: Go or Show to MainWindow
					//MainWindow window = new MainWindow();
					//window.Show();

					//Close();
				}
				else
				{
					MessageBox.Show("Login failed");
				}
			}
			else
			{
				MessageBox.Show("Please fill out ID and Password");
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
		
		
	}
}
