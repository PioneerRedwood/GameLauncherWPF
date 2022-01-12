using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
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

namespace GroguLauncher
{
	public partial class CreateAccountPage : Page
	{
		private LoginWindow window;

		public CreateAccountPage(LoginWindow _window)
		{
			InitializeComponent();
			window = _window;

			// TODO: if you're coming via OAuth, the MailTextBox should be filled out and can't be edited.
			if (window.isOAuthSucceed)
			{
				MailText.Text = window.googleAuthHandler.userInfo["email"];
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
				ResultText.Text = "Email";
				MailText.Focus();
				return;
			}

			// TODO: Name regex check
			if (!(NameText.Text.Length > 0))
			{
				ResultText.Text = "Name";
				NameText.Focus();
				return;
			}

			// TODO: Pwd regex check
			if (!(PwdText.Text.Length > 0))
			{
				ResultText.Text = "Password";
				PwdText.Focus();
				return;
			}

			// TODO: Pwd and PwdValid same check
			if (!(PwdVaildText.Text.Length > 0 && PwdText.Text == PwdVaildText.Text))
			{
				ResultText.Text = "Confirm";
				PwdVaildText.Focus();
				return;
			}

			// TODO: Valid with User DB
			Task<bool> checkTask = window.accountHandler.CheckAccountExsists(MailText.Text, NameText.Text);
			bool result = await checkTask;
			if (result)
			{
				if (MessageBox.Show("You can create account with this.\nDo you want to create?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					// TODO: Insert user data
					checkTask = window.accountHandler.CreateAccountSync(MailText.Text, NameText.Text, PwdText.Text);
					result = await checkTask;

					// TODO: Go to the LoginWindow
					if (result)
					{
						window.LoadPreviousContent();
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
				ResultText.Text = "The account already exsists.";
				return;
			}
		}

		public void GoToLoginButton_Click(object sender, RoutedEventArgs e)
		{
			window.LoadPreviousContent();
		}

		// ref https://stackoverflow.com/questions/201323/how-can-i-validate-an-email-address-using-a-regular-expression
		private bool IsEmailAllowed(string target)
		{
			try
			{
				MailAddress address = new MailAddress(target);

				return (address.Address == target);
			}
			catch (FormatException e)
			{
				// TODO: handle the exception
				return false;
			}
		}

	}
}
