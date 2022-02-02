using CefSharp;
using CefSharp.Callback;
using GroguLauncher.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroguLauncher.Cef
{
	// ref https://bbonczek.github.io/jekyll/update/2018/04/24/serve-content-in-cef-without-http-server.html
	// ref https://stackoverflow.com/questions/35965912/cefsharp-custom-schemehandler
	public class CefLocalSchemeHandler : ResourceHandler
	{
		private string rootPath = Directory.GetCurrentDirectory() + "/Resources/Htmls";

		public CefLocalSchemeHandler() { }

		public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
		{
			Uri uri = new Uri(request.Url);
			string fileName = uri.AbsolutePath;

			string requestedFilePath = rootPath + fileName;

			bool isAccessToFilePermitted = IsRequestedPathInsideFolder
				(new DirectoryInfo(requestedFilePath), new DirectoryInfo(rootPath));

			if(isAccessToFilePermitted && File.Exists(requestedFilePath))
			{
				byte[] bytes = File.ReadAllBytes(requestedFilePath);
				Stream = new MemoryStream(bytes);

				string fileExtension = Path.GetExtension(fileName);
				MimeType = GetMimeType(fileExtension);

				callback.Continue();
				return CefReturnValue.Continue;
			}

			callback.Dispose();
			return CefReturnValue.Cancel;
		}

		// for security
		public bool IsRequestedPathInsideFolder(DirectoryInfo path, DirectoryInfo folder)
		{
			if(path.Parent == null)
			{
				return false;
			}

			if(string.Equals(path.Parent.FullName, folder.FullName, StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}

			return IsRequestedPathInsideFolder(path.Parent, folder);
		}
	}
}
