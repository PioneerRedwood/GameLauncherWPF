using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroguLauncher.Cef
{
	public class CefLocalSchemeHandlerFactory : ISchemeHandlerFactory
	{
		public static string SchemeName = "local";

		public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
		{
			return schemeName == SchemeName ? new CefLocalSchemeHandler() : null;
		}
	}
}
