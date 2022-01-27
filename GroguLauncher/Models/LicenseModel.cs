using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroguLauncher.Models
{
	public class LicenseModel
	{
		public LicenseModel(string title, string content)
		{
			Title = title;
			Content = content;
		}

		public string Title { get; private set; }
		public string Content { get; private set; }
	}
}
