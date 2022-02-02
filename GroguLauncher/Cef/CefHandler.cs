using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Enums;

namespace GroguLauncher.Cef
{
	public class CefHandler
	{
		public CefSettings Settings { get; private set; }

		public CefHandler()
		{
			Settings = new CefSettings
			{
				CookieableSchemesList = "local"
			};

			Settings.RegisterScheme(new CefCustomScheme
			{
				SchemeName = CefLocalSchemeHandlerFactory.SchemeName,
				SchemeHandlerFactory = new CefLocalSchemeHandlerFactory()
			});

		}
	}
}
