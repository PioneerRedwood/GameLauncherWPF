using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;

namespace GroguLauncher
{
	public partial class App : Application
	{
		public static Dictionary<string, string> UserInfo = new Dictionary<string, string>();

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			// TODO: load the registry key of "Remember me" in LoginWindow

		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			// TODO: Register the key of "Remember me"

		}

		protected override void OnLoadCompleted(NavigationEventArgs e)
		{
			base.OnLoadCompleted(e);
			
		}

	}
}
