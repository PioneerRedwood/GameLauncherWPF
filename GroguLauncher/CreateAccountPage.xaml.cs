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

namespace GroguLauncher
{
	public partial class CreateAccountPage : Page
	{
		enum GoToWhere
		{
			LoginWindow,
			Back
		};

		private GoToWhere goToWhere = GoToWhere.Back;
		private LoginWindow window;

		public CreateAccountPage(LoginWindow _window)
		{
			InitializeComponent();
			window = _window;

			if(NavigationService == null)
			{
				goToWhere = GoToWhere.LoginWindow;
				BackButton.Content = "Login";
			}

			// TODO: if you're coming via OAuth, the MailTextBox should be filled out and can't be edited.
		}

		public void SubmitButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: Submit the form
			// check vaildation
			//if(MailTextBox )

			// TODO: Go to the LoginWindow
		}

		public void BackButton_Click(object sender, RoutedEventArgs e)
		{
			// TODO: Go back or go to the LoginWindow
			switch (goToWhere)
			{
				case GoToWhere.LoginWindow:
					window.LoadPreviousContent();
					break;
				case GoToWhere.Back:
					if (NavigationService.CanGoBack)
					{
						NavigationService.GoBack();
					}
					break;
			}
		}
	}
}
