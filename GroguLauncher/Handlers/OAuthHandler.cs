using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace GroguLauncher.Handlers
{
	public abstract class OAuthHandler
	{
		// for find unused tcp port http://stackoverflow.com/a/3978040
		public static int FindUnusedTcpPort()
		{
			TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			int port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop();
			return port;
		}

		// Returns URI-safe data with a given input length.
		public static string RandomDataBase64Url(uint length)
		{
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			byte[] bytes = new byte[length];
			rng.GetBytes(bytes);
			return Base64UrlEncodeNoPadding(bytes);
		}

		// Returns the SHA256 hash of the input string.
		public static byte[] Sha256(string input)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(input);
			SHA256Managed sha256 = new SHA256Managed();
			return sha256.ComputeHash(bytes);
		}

		// Base64url no-padding encodes the given input buffer.
		public static string Base64UrlEncodeNoPadding(byte[] buffer)
		{
			string base64 = Convert.ToBase64String(buffer);

			base64 = base64.Replace("+", "-");
			base64 = base64.Replace("/", "_");

			base64 = base64.Replace("=", "");

			return base64;
		}
	}
}
