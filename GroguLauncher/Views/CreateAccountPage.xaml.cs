using System;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GroguLauncher
{
	public partial class CreateAccountPage : Page
	{
		private readonly LoginWindow _loginWindow;

		public CreateAccountPage(LoginWindow _window)
		{
			InitializeComponent();
			_loginWindow = _window;

			// TODO: if you're coming via OAuth, the MailTextBox should be filled out and can't be edited.
			if (_loginWindow.IsOAuthSucceed)
			{
				MailText.Text = _loginWindow.GoogleAuthHandler.UserInfo["email"];
				MailText.IsReadOnly = true;
			}
			else
			{
				MailText.Text = "";
				MailText.IsReadOnly = false;
			}
		}

		// Sign up
		public async void SubmitButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: Submit the form
			// check vaildation

			// TODO: ID Mail regex check
			if (!(MailText.Text.Length > 0 && IsEmailAllowed(MailText.Text)))
			{
				ResultLabel.Content = "Email";
				MailText.Focus();
				return;
			}

			// TODO: Name regex check
			if (!(NameText.Text.Length > 0))
			{
				ResultLabel.Content = "Name";
				NameText.Focus();
				return;
			}

			// TODO: Pwd regex check
			if (!(PwdText.Password.Length> 0))
			{
				ResultLabel.Content = "Password";
				PwdText.Focus();
				return;
			}

			// TODO: Pwd and PwdValid same check
			if (!(PwdVaildText.Password.Length > 0 && PwdText.Password == PwdVaildText.Password))
			{
				ResultLabel.Content = "Confirm";
				PwdVaildText.Focus();
				return;
			}

			// TODO: Valid with User DB
			Task<bool> checkTask = _loginWindow.AccountHandler.CheckAccountExsists(MailText.Text, NameText.Text);
			bool result = await checkTask;
			if (result)
			{
				if (MessageBox.Show("You can create account with this.\nDo you want to create?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					// TODO: Insert user data
					checkTask = _loginWindow.AccountHandler.CreateAccount(MailText.Text, NameText.Text, PwdText.Password);
					result = await checkTask;

					// TODO: Go to the LoginWindow
					if (result)
					{
						_loginWindow.LoadPreviousContent();
					}
					else
					{
						return;
					}
				}
				else
				{
					MailText.Focus();
					return;
				}
			}
			else
			{
				ResultLabel.Content = "The account already exsists.";
				return;
			}
		}

		public void GoToLoginButton_Click(object sender, RoutedEventArgs e)
		{
			_loginWindow.LoadPreviousContent();
		}

		// ref https://stackoverflow.com/questions/201323/how-can-i-validate-an-email-address-using-a-regular-expression
		private bool IsEmailAllowed(string target)
		{
			try
			{
				MailAddress address = new MailAddress(target);

				return (address.Address == target);
			}
			catch (FormatException)
			{
				// TODO: handle the exception
				return false;
			}
		}

		private void Border_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				_loginWindow.DragMove();
			}
		}
		private void MiniMizeButton_Click(object sender, RoutedEventArgs e)
		{
			_loginWindow.WindowState = WindowState.Minimized;
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			_loginWindow.Close();
		}
	}
}
