using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GroguLauncher.Handlers
{
	public class LobbyHandler
	{
		internal struct LobbyMessageHeader
		{
			public LobbyHeader Header;
			public int Size;
		}

		public enum LobbyHeader
		{
			Heartbeat,

			AcceptConnect,
			SessionDisconnect,
			ConnectionUserInfo,

			GeneralChannelMessage,

			JoinLobby,
			JoinLobbyOK,
			JoinLobbyFailed,
		}

		private const uint NetworkID = uint.MaxValue;
		private const int BufferSize = 2048;
		private const string ServerAddress = "127.0.0.1";
		private const int ServerPort = 25600;

		private Socket _socket = null;
		private ConcurrentQueue<string> _queue = null;
		private byte[] _writeBuffer = null;
		private byte[] _readBuffer = null;
		private bool _isStarted = false;

		private bool _isInLobby = false;
		private Thread _heartbeatThread;

		public LobbyHandler(ConcurrentQueue<string> queue)
		{
			_queue = queue;

			// init settings
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_writeBuffer = new byte[BufferSize];
			_readBuffer = new byte[BufferSize];


		}

		public void Start()
		{
			//connect to server
			Connect();
		}

		public bool Connected()
		{
			return _socket != null && _socket.Connected;
		}

		private bool Connect()
		{
			try
			{
				bool result = false;
				if (_socket != null)
				{
					_socket.BeginConnect(IPAddress.Parse(ServerAddress), ServerPort, ConnectCallback, _socket);
				}
				return result;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
		}

		private void ConnectCallback(IAsyncResult ar)
		{
			Socket socket = (Socket)ar.AsyncState;
			socket.EndConnect(ar);
			if (socket.Connected)
			{
				Console.WriteLine("Connected, start receiving");
				_isStarted = true;

				ReceiveHeader();

				_heartbeatThread = new Thread(
					() =>
					{
						while (true)
						{
							if (Connected())
							{
								int size = 0;
								Buffer.BlockCopy(BitConverter.GetBytes((uint)LobbyHeader.Heartbeat), 0, _writeBuffer, 0, sizeof(uint));
								size += sizeof(uint);

								_socket.BeginSend(_writeBuffer, 0, size, SocketFlags.None, SendCallback, _socket);
							}
							else
							{
								Console.WriteLine("[Send] Socket it not vaild");
								break;
							}

							Thread.Sleep(500);
						}
					});
				_heartbeatThread.Start();
			}
			else
			{
				Console.WriteLine("Failed to Connect");
			}
		}

		private void ReceiveHeader()
		{
			try
			{
				if (Connected())
				{
					_socket.BeginReceive(_readBuffer, 0, 8, SocketFlags.None, ReceiveBody, _socket);
				}
				else
				{
					Console.WriteLine("[ReceiveHeader] Socket is not valid, do nothing");
					return;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return;
			}
		}

		private void ReceiveBody(IAsyncResult ar)
		{
			try
			{
				if (Connected())
				{
					int bytes = _socket.EndReceive(ar);

					LobbyMessageHeader msg = new LobbyMessageHeader();
					msg.Header = (LobbyHeader)BitConverter.ToUInt32(_readBuffer, 0);
					msg.Size = BitConverter.ToInt32(_readBuffer, 4);

					switch (msg.Header)
					{
						case LobbyHeader.Heartbeat:
							{
								// for now just time
								if (msg.Size > 0)
								{
									_socket.BeginReceive(_readBuffer, 0, msg.Size, SocketFlags.None, new AsyncCallback(
										(IAsyncResult bodyReadResult) =>
										{
											// from 0 ~ Size
											_queue.Enqueue(Encoding.Default.GetString(_readBuffer, 0, msg.Size));
										}), _socket);
								}

								break;
							}
						case LobbyHeader.AcceptConnect:
							{

								break;
							}
						case LobbyHeader.SessionDisconnect:
							{

								break;
							}
						case LobbyHeader.ConnectionUserInfo:
							{
								break;
							}
						case LobbyHeader.GeneralChannelMessage:
							{
								break;
							}
						case LobbyHeader.JoinLobby:
							{
								break;
							}
						case LobbyHeader.JoinLobbyOK:
							{
								break;
							}
						case LobbyHeader.JoinLobbyFailed:
							{
								break;
							}
						default:
							{
								break;
							}
					}

					ReceiveHeader();

					ReceiveCallback();
				}
				else
				{
					Console.WriteLine("[ReceiveBody] Socket is not valid, do nothing");
					return;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return;
			}
		}

		private void ReceiveCallback()
		{
			// notify to window
			// have received the message from the server
			Console.WriteLine("[ReceiveCallback] received");
		}

		public void Stop()
		{
			_socket.Close();
			_socket = null;

			_heartbeatThread.Join();
		}

		public void Send(string content)
		{
			if (Connected())
			{
				int size = 0;
				Buffer.BlockCopy(BitConverter.GetBytes((uint)LobbyHeader.GeneralChannelMessage), 0, _writeBuffer, 0, sizeof(uint));
				size += sizeof(uint);

				Buffer.BlockCopy(Encoding.Default.GetBytes(content), 0, _writeBuffer, size, content.Length);
				size += content.Length;

				_socket.BeginSend(_writeBuffer, 0, size, SocketFlags.None, SendCallback, _socket);
			}
			else
			{
				Console.WriteLine("[Send] Socket it not vaild");
				return;
			}
		}

		private void SendCallback(IAsyncResult ar)
		{
			int bytes = _socket.EndSend(ar);
			if (bytes > 0)
			{
				Console.WriteLine("Send success");
			}
		}
	}
}
