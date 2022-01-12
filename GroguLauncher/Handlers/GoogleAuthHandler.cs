// Copyright 2016 Google Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GroguLauncher.Handlers
{
	public class GoogleAuthHandler : OAuthHandler
	{
		private LoginWindow window;

		// access_token, expires_in, refresh_token, scope, token_type, id_token
		public Dictionary<string, string> authToken { get; private set; }
		// sub, name, given_name, family_name, picture_uri, locale
		// 
		public Dictionary<string, string> userInfo { get; private set; }

		public GoogleAuthHandler(LoginWindow _window)
		{
			window = _window;
		}

		// client config
		private const string clientID = "58613277-67o5b6avkmnk7t3sa2qob64serpt9p7n.apps.googleusercontent.com";
		private const string clientSecret = "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";
		private const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";

		// execution auth
		public async void Authenticate()
		{
			// Set Window blurred .. or Wait state

			// Generates state the PKCE values
			string state = RandomDataBase64Url(32);
			string code_verifier = RandomDataBase64Url(32);
			string code_challenge = Base64UrlEncodeNoPadding(Sha256(code_verifier));
			const string code_challenge_method = "S256";

			// Creates a redirect URI using an available port on the loopback address
			string redirectURI =
				string.Format("http://{0}:{1}/", IPAddress.Loopback, FindUnusedTcpPort());
			PrintOutput(redirectURI);

			// Creates an HttpListener to listen for requests on that redirect URI
			HttpListener listener = new HttpListener();
			try
			{
				listener.Prefixes.Add(redirectURI);

				listener.Start();
			}
			catch (HttpListenerException e)
			{
				PrintOutput($"HttpListenerException! {e.Message}");
				return;
			}
			PrintOutput("Listening..");


			// Create the OAuth 2.0 Authorization request
			string request =
				string.Format(
					"{0}?response_type=code" +
					"&scope=openid%20email" +
					"&redirect_uri={1}" +
					"&client_id={2}" +
					"&state={3}" +
					"&code_challenge={4}" +
					"&code_challenge_method={5}",

					authorizationEndpoint,
					Uri.EscapeDataString(redirectURI),
					clientID,
					state,
					code_challenge,
					code_challenge_method);

			// Opens request in the browser
			System.Diagnostics.Process.Start(request);

			// Waits for the OAuth authorization response
			HttpListenerContext context = await listener.GetContextAsync();

			// Brings this app back to the foreground
			//window.Activate();

			// Sends an HTTP response to the browser
			HttpListenerResponse response = context.Response;
			string responseString =
				string.Format(
					"<html>" +
					"<head><meta http-equiv='refresh' content=10;url=https://google.com'></head>" +
					"<body>Please return to the app.</body>" +
					"</html>");
			byte[] buffer = Encoding.UTF8.GetBytes(responseString);
			response.ContentLength64 = buffer.Length;
			Stream responseOutput = response.OutputStream;
			Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
			{
				responseOutput.Close();
				listener.Stop();
				Console.WriteLine("Http server stopped");
			});

			// Checks for errors
			if (context.Request.QueryString.Get("error") != null)
			{
				PrintOutput(string.Format("OAuth authorization error: {0}",
					context.Request.QueryString.Get("error")));
			}

			if (context.Request.QueryString.Get("code") == null
				|| context.Request.QueryString.Get("state") == null)
			{
				PrintOutput("Malformed authorization response. " + context.Request.QueryString);
				return;
			}

			// Extracts the code
			string code = context.Request.QueryString.Get("code");
			string incoming_state = context.Request.QueryString.Get("state");

			// Compares the received state to the expected value, to ensure that
			// this app made the request which resulted in authorization
			if (incoming_state != state)
			{
				PrintOutput(string.Format("Received request with invalid state ({0})", incoming_state));
				return;
			}
			PrintOutput("Authorization code: " + code);

			// Starts the code exchange at the Token Endpoint
			PerformCodeExchage(code, code_verifier, redirectURI);
		}

		public async void PerformCodeExchage(string code, string code_verifier, string redirectURI)
		{
			PrintOutput("Exchange code for tokens");

			// builds the request
			string tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
			string tokenRequestBody =
				string.Format(
					"code={0}" +
					"&redirect_uri={1}" +
					"&client_id={2}" +
					"&code_verifier={3}" +
					"&client_secret={4}" +
					"&scope=" +
					"&grant_type=authorization_code",

				code,
				Uri.EscapeDataString(redirectURI),
				clientID,
				code_verifier,
				clientSecret
				);

			// sends the request
			HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
			tokenRequest.Method = "POST";
			tokenRequest.ContentType = "application/x-www-form-urlencoded";
			tokenRequest.Accept =
				"Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

			byte[] byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
			tokenRequest.ContentLength = byteVersion.Length;
			Stream stream = tokenRequest.GetRequestStream();
			await stream.WriteAsync(byteVersion, 0, byteVersion.Length);
			stream.Close();

			try
			{
				// gets the response
				WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
				using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
				{
					// reads response body
					string responseText = await reader.ReadToEndAsync();
					PrintOutput(responseText);

					// converts to dictionary
					authToken =
						JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

					string access_token = authToken["access_token"];
					RequestUserInfo(access_token);
				}
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.ProtocolError)
				{
					if (e.Response is HttpWebResponse response)
					{
						PrintOutput("Http: " + response.StatusCode);
						using (StreamReader reader = new StreamReader(response.GetResponseStream()))
						{
							// reads response body
							string responseText = await reader.ReadToEndAsync();
							PrintOutput(responseText);
						}
					}
				}
			}
		}

		// request user info
		private async void RequestUserInfo(string access_token)
		{
			PrintOutput("Making API call to UserInfo ... ");

			// builds the request
			string requestURI = "https://www.googleapis.com/oauth2/v3/userinfo";

			// sends the request
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestURI);
			request.Method = "GET";
			request.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
			request.ContentType = "application/x-www-form-urlencoded";
			request.Accept =
				"Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*,q=0.8";

			// gets the response
			WebResponse response = await request.GetResponseAsync();
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				// reads response body
				string responseText = await reader.ReadToEndAsync();
				userInfo =
					JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

				window.NotifyAuthDone();

				PrintOutput(responseText);
			}
		}

		// for Debugging 
		public void PrintOutput(string content)
		{
			// fill TextBox with the give content
			//textBoxOutput.Text = textBoxOutput.Text + content + Environment.NewLine;
			Console.WriteLine(content);
		}

	}
}
