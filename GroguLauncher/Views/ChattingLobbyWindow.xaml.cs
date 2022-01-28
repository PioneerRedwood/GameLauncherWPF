﻿using GroguLauncher.Handlers;
using GroguLauncher.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GroguLauncher.Views
{
	/// <summary>
	/// ChattingLobbyWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ChattingLobbyWindow : Window
	{
		private readonly MainWindow _mainWindow;
		private LobbyHandler _lobbyHandler;
		private ConcurrentQueue<string> _queue;

		private ObservableCollection<ChatDataModel> _chatModels;
		private Thread _messagePumpThread;

		public ChattingLobbyWindow(MainWindow mainWindow)
		{
			InitializeComponent();
			_mainWindow = mainWindow;
			_queue = new ConcurrentQueue<string>();
			_chatModels = new ObservableCollection<ChatDataModel>();
			_messagePumpThread = new Thread(
				() =>
				{
					while (true)
					{
						_queue.TryDequeue(out string result);
						if(result != null && result.Length > 0)
						{
							Dispatcher dispatcher = Application.Current.Dispatcher;
							dispatcher.InvokeAsync(new Action(() => NotifyNewMessageComing(result)));
						}

						Thread.Sleep(100);
					}
				});

			ChatListView.ItemsSource = _chatModels;

			Loaded += (object sender, RoutedEventArgs e) =>
			{
				_lobbyHandler = new LobbyHandler(_queue);
				StartConnectToServer();
			};

			Closed += (object sender, EventArgs e) =>
			{
				_mainWindow.OnChattingLobbyWindowClosed();
				Disconnect();
			};
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

		private void SendTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			if (SendTextBox.Text.Length > 0)
			{
				SendTextBox.CaretIndex = SendTextBox.Text.Length - 1;
			}
			else
			{
				SendTextBox.CaretIndex = 0;
			}
		}

		#region Network wrapper
		private void StartConnectToServer()
		{
			_lobbyHandler.Start();
			_messagePumpThread.Start();
		}

		private void Disconnect()
		{
			_lobbyHandler.Stop();
			_messagePumpThread.Abort();
		}

		private void NotifyNewMessageComing(string message)
		{
			_chatModels.Add(new ChatDataModel(message));
		}
		#endregion

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && SendTextBox.Text.Length > 0)
			{
				_lobbyHandler.Send(SendTextBox.Text);
			}
		}
	}
}
