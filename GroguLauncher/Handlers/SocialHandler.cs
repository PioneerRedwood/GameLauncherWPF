using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using GroguLauncher.Models;

namespace GroguLauncher.Handlers
{
	public class SocialHandler
	{
		/** Tables 
		* FRIENDSHIP - requester_id, addressee_id, created_time
		* MY_STATUS - request status table: 'R': "Requested", 'A': "Accepted", 'D': "Denied", 'B': "Blocked"
		* FRIEND_RELATION - requester_id, addresssee_id, status_code, specifier_id, specified_datetime
		* MESSENGER - sender_id, receiver_id, sent_datetime, contents
		*/

		public enum FriendshipStatusCode
		{
			Requested = 'R',
			Accepted = 'A',
			Denied = 'D',
			Blocked = 'B',
		};

		private const string _userTable = "RED_USER";
		private const string _friendshipTable = "FRIENDSHIP";
		private const string _friendRelationTable = "FRIEND_RELATION";
		private const string _messengerTable = "MESSENGER";

		public SocialHandler()
		{
			MySQLHandler.Initialize();
		}

		#region private - used from only inside of this class

		private async Task<bool> PostAcceptFriendship(int self, int friend)
		{
			bool result = false;
			if (MySQLHandler.OpenConnection())
			{
				bool isFriend = await IsFriend(self, friend);
				if (isFriend)
				{
					return result;
				}

				string query =
					"INSERT INTO " + _friendshipTable +
					" (REQUESTER_ID, ADDRESSEE_ID, CREATED_TIME)" +
					$" VALUES({friend}, {self}, NOW())";

				if (MySQLHandler.ExecuteNonQuery(query) == 1)
				{
					result = true;
				}

				MySQLHandler.CloseConnection();
			}

			return result;
		}

		private Task<UserModel> GetUserByID(int id)
		{
			UserModel friend = new UserModel();

			if (MySQLHandler.OpenConnection())
			{
				string query =
					"SELECT USER_NAME, IS_LOGGED_IN FROM " + _userTable +
					$" WHERE USER_ID = {id}";

				int result = 0;
				DataSet ds = MySQLHandler.ExecuteDataSet("friends", query, ref result);

				if (result == 1)
				{
					// TODO: !OPTIMIZATION!
					if (ds.Tables[0].Rows.Count > 0)
					{
						foreach (DataRow row in ds.Tables[0].Rows)
						{
							friend.Name = row["USER_NAME"].ToString();
							friend.IsLoggedIn = bool.Parse(row["IS_LOGGED_IN"].ToString());
						}
					}
				}
			}

			return Task.FromResult(friend);
		}

		private Task<UserModel> GetUserByName(string name)
		{
			UserModel friend = new UserModel();

			if (MySQLHandler.OpenConnection())
			{
				string query =
					"SELECT USER_ID, IS_LOGGED_IN FROM " + _userTable +
					$" WHERE USER_NAME = '{name}'";
				int result = 0;
				DataSet ds = MySQLHandler.ExecuteDataSet("friend info", query, ref result);

				// TODO: !OPTIMIZATION!
				if (ds.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow row in ds.Tables[0].Rows)
					{
						friend.Id = int.Parse(row["USER_ID"].ToString());
						friend.Name = name;
						friend.IsLoggedIn = bool.Parse(row["IS_LOGGED_IN"].ToString());
					}
				}

				MySQLHandler.CloseConnection();
			}

			return Task.FromResult(friend);
		}

		private Task<bool> IsFriend(int self, int other)
		{
			bool isFriend = false;

			if (MySQLHandler.OpenConnection())
			{
				string query =
					"SELECT REQUESTER_ID FROM " + _friendshipTable +
					$" WHERE (REQUESTER_ID = {self} AND ADDRESSEE_ID = {other}) " +
					$"OR (REQUESTER_ID = {other} AND ADDRESSEE_ID = {self})";

				int result = 0;
				DataSet _ = MySQLHandler.ExecuteDataSet("results", query, ref result);
				if (result != 0)
				{
					isFriend = true;
				}

				MySQLHandler.CloseConnection();
			}

			return Task.FromResult(isFriend);
		}

		private Task<bool> HasDenied(int targetID)
		{
			bool hasDenied = false;

			if (MySQLHandler.OpenConnection())
			{
				string query =
					"SELECT REQUESTER_ID FROM " + _friendRelationTable +
					$" WHERE ADDRESSEE_ID = {targetID}" +
					$" AND REQUESTER_ID = {int.Parse(App.UserInfo["USER_ID"])} AND STATUS_CODE = 'D' ";

				int result = 0;
				DataSet _ = MySQLHandler.ExecuteDataSet("results", query, ref result);
				if(result != 0)
				{
					hasDenied = true;
				}

				MySQLHandler.CloseConnection();
			}

			return Task.FromResult(hasDenied);
		}

		#endregion

		#region public - can be used from outside

		public async Task<ObservableCollection<UserModel>> GetFriendList()
		{
			ObservableCollection<UserModel> friends = new ObservableCollection<UserModel>();

			if (MySQLHandler.OpenConnection())
			{
				// TODO: 
				string query =
					"SELECT REQUESTER_ID, ADDRESSEE_ID FROM " + _friendshipTable +
					$" WHERE REQUESTER_ID = {int.Parse(App.UserInfo["USER_ID"])} OR ADDRESSEE_ID = {int.Parse(App.UserInfo["USER_ID"])}";

				int result = 0;
				DataSet ds = MySQLHandler.ExecuteDataSet("friends", query, ref result);

				if (result > 0 && ds.Tables[0].Rows.Count > 0)
				{
					// TODO: !OPTIMIZATION!
					foreach (DataRow row in ds.Tables[0].Rows)
					{
						UserModel friend;
						// Get a friend id
						if (int.Parse(row["ADDRESSEE_ID"].ToString()) != int.Parse(App.UserInfo["USER_ID"]))
						{
							int target = int.Parse(row["ADDRESSEE_ID"].ToString());
							friend = await GetUserByID(target);
							friend.Id = target;
						}
						else
						{
							int target = int.Parse(row["REQUESTER_ID"].ToString());
							friend = await GetUserByID(target);
							friend.Id = target;
						}

						friends.Add(friend);
					}
				}

				MySQLHandler.CloseConnection();
			}

			return friends;
		}

		public async Task<bool> AddFriendWithName(int self, string friendName, FriendshipStatusCode code)
		{
			bool result = false;

			if (MySQLHandler.OpenConnection())
			{
				UserModel friend = await GetUserByName(friendName);

				if (friend.Id != -1)
				{
					result = await PostRequestFriendRelation(self, friend.Id, code);
				}

				MySQLHandler.CloseConnection();
			}

			return result;
		}

		public async Task<ObservableCollection<UserModel>> GetFriendRequestList()
		{
			ObservableCollection<UserModel> requests = new ObservableCollection<UserModel>();

			if (MySQLHandler.OpenConnection())
			{
				string query =
					"SELECT REQUESTER_ID FROM " + _friendRelationTable +
					$" WHERE ADDRESSEE_ID = {int.Parse(App.UserInfo["USER_ID"])}" +
					$" AND STATUS_CODE = 'R'";

				int result = 0;
				DataSet ds = MySQLHandler.ExecuteDataSet("requests", query, ref result);

				if (result > 0)
				{
					if (ds.Tables[0].Rows.Count > 0)
					{
						foreach (DataRow row in ds.Tables[0].Rows)
						{
							// user name, is logged in
							UserModel friend = await GetUserByID(int.Parse(row["REQUESTER_ID"].ToString()));
							friend.Id = int.Parse(row["REQUESTER_ID"].ToString());

							if (!await IsFriend(int.Parse(App.UserInfo["USER_ID"]), friend.Id)
								&& !await HasDenied(friend.Id))
							{
								requests.Add(friend);
							}
						}
					}
				}

				MySQLHandler.CloseConnection();
			}

			return requests;
		}

		public async Task<bool> PostRequestFriendRelation(int self, int friend, FriendshipStatusCode code)
		{
			bool result = false;

			if (MySQLHandler.OpenConnection())
			{
				string query =
					"INSERT INTO " + _friendRelationTable +
					" (REQUESTER_ID, ADDRESSEE_ID, STATUS_CODE, SPECIFIER_ID, SPECIFIED_DATETIME)" +
					$" VALUES ({self}, {friend}, '{(char)code}', {self}, NOW())";

				if (MySQLHandler.ExecuteNonQuery(query) == 1)
				{
					result = true;
					switch (code)
					{
						case FriendshipStatusCode.Requested:
							break;
						case FriendshipStatusCode.Accepted:
							bool addResult = await PostAcceptFriendship(self, friend);
							result &= addResult;
							break;
						case FriendshipStatusCode.Blocked:
							break;
						case FriendshipStatusCode.Denied:
							break;
						default:
							break;
					}
				}

				MySQLHandler.CloseConnection();
			}

			return result;
		}
		
		public async Task<ObservableCollection<MessageDataModel>> GetMessageData(int target)
		{
			ObservableCollection<MessageDataModel> models = null;

			if (MySQLHandler.OpenConnection())
			{
				models = new ObservableCollection<MessageDataModel>();
				string query =
					$"SELECT * FROM {_messengerTable}" +
					$" WHERE (SENDER_ID = {int.Parse(App.UserInfo["USER_ID"])} AND RECEIVER_ID = {target})" +
					$" OR (SENDER_ID = {target} AND RECEIVER_ID = {int.Parse(App.UserInfo["USER_ID"])})" +
					$" ORDER BY SENT_DATETIME";

				int result = 0;
				DataSet ds = MySQLHandler.ExecuteDataSet("messages", query, ref result);
				if(result > 0 && ds.Tables[0].Rows.Count > 0)
				{
					foreach(DataRow row in ds.Tables[0].Rows)
					{
						MessageDataModel model = new MessageDataModel();

						model.Sender = await GetUserByID(int.Parse(row["SENDER_ID"].ToString()));
						model.Receiver = await GetUserByID(int.Parse(row["RECEIVER_ID"].ToString()));
						model.Message = row["CONTENTS"].ToString();
						model.MessageDate = DateTime.Parse(row["SENT_DATETIME"].ToString());

						models.Add(model);
					}
				}

				MySQLHandler.CloseConnection();
			}

			return models;
		}

		// Not used ..
		public async Task<ObservableCollection<MessageDataModel>> GetMessagesToSelf()
		{
			ObservableCollection<MessageDataModel> messages = new ObservableCollection<MessageDataModel>();
			
			ObservableCollection<UserModel> friends = await GetFriendList();
			foreach(UserModel user in friends)
			{
				messages = await GetMessageData(user.Id);
			}
			
			return messages;
		}

		public Task<bool> SendMessage(UserModel friend, string content)
		{
			bool result = false;
			if (MySQLHandler.OpenConnection())
			{
				string query =
					$"INSERT INTO {_messengerTable} (SENDER_ID, RECEIVER_ID, SENT_DATETIME, CONTENTS)" +
					$" VALUES({int.Parse(App.UserInfo["USER_ID"].ToString())}, {friend.Id}, NOW(2), \"{content}\")";

				if (MySQLHandler.ExecuteNonQuery(query) == 1)
				{
					result = true;
				}

				MySQLHandler.CloseConnection();
			}

			return Task.FromResult(result);
		}
		#endregion
	}
}
