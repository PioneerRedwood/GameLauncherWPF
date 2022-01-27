using GroguLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace GroguLauncher.Views
{
	/// <summary>
	/// LicenseWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class LicenseWindow : Window
	{
		private List<LicenseModel> _models;

		private readonly string _licenseDirPath = Directory.GetCurrentDirectory() + "/Resources/Licenses";

		public LicenseWindow()
		{
			InitializeComponent();

			_models = new List<LicenseModel>();

			// TODO: write "LICENSE INFO" into the Label
			ReadLicenses();

			LicenseList.ItemsSource = _models;
		}

		private void ReadLicenses()
		{
			string[] licensesFilePaths = Directory.GetFiles(_licenseDirPath);
			foreach (string path in licensesFilePaths)
			{
				if (path.Contains(".license"))
				{
					FileInfo file = new FileInfo(path);
					string content = File.ReadAllText(file.FullName);

					_models.Add(new LicenseModel(System.IO.Path.GetFileNameWithoutExtension(file.FullName), content));
				}
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
	}
}
