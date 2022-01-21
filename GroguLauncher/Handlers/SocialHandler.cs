using GroguLauncher.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroguLauncher.Models;

namespace GroguLauncher.Handlers
{
	/// <summary>
	/// SocialHandler
	/// DB: red_db, tb: friendship, my_status, friend_relation
	/// - Post & Get
	/// !OPTIMIZATION!
	/// - make the queries reuseable, control table's properties by enum 
	/// </summary>
	public class SocialHandler
	{
		/** Tables 
		* FRIENDSHIP - requester_id, addressee_id, created_time
		* MY_STATUS - request status table: 'R': "Requested", 'A': "Accepted", 'D': "Denied", 'B': "Blocked"
		* FRIEND_RELATION - requester_id, addresssee_id, status_code, specifier_id, specified_datetime
		*/

		public enum FriendshipStatusCode
		{
			Requested = 'R',
			Accepted = 'A',
			Denied = 'D',
			Blocked = 'B',
		};

		private const string userTable = "RED_USER";
		private const string friendshipTable = "FRIENDSHIP";
		private const string friendRelationTable = "FRIEND_RELATION";

		public SocialHandler()
		{
			MySQLManager.Initialize();
		}

		#region private - used from only inside of this class

		private async Task<bool> PostAcceptFriendship(int self, int friend)
		{
			bool result = false;
			if (MySQLManager.OpenConnection())
			{
				bool isFriend = await IsFriend(self, friend);
				if (isFriend)
				{
					return result;
				}

				string query =
					"INSERT INTO " + friendshipTable +
					" (REQUESTER_ID, ADDRESSEE_ID, CREATED_TIME)" +
					$" VALUES({friend}, {self}, NOW())";

				if (MySQLManager.ExecuteNonQuery(query) == 1)
				{
					result = true;
				}

				MySQLManager.CloseConnection();
			}

			return result;
		}

		private Task<ContactModel> GetUserByID(int id)
		{
			ContactModel friend = new ContactModel();

			if (MySQLManager.OpenConnection())
			{
				string query =
					"SELECT USER_NAME, IS_LOGGED_IN FROM " + userTable +
					$" WHERE USER_ID = {id}";

				int result = 0;
				DataSet ds = MySQLManager.ExecuteDataSet("friends", query, ref result);

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

		private Task<ContactModel> GetUserByName(string name)
		{
			ContactModel friend = new ContactModel();

			if (MySQLManager.OpenConnection())
			{
				string query =
					"SELECT USER_ID, IS_LOGGED_IN FROM " + userTable +
					$" WHERE USER_NAME = '{name}'";
				int result = 0;
				DataSet ds = MySQLManager.ExecuteDataSet("friend info", query, ref result);

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

				MySQLManager.CloseConnection();
			}

			return Task.FromResult(friend);
		}

		private Task<bool> IsFriend(int self, int other)
		{
			bool isFriend = false;

			if (MySQLManager.OpenConnection())
			{
				string query =
					"SELECT REQUESTER_ID FROM " + friendshipTable +
					$" WHERE (REQUESTER_ID = {self} AND ADDRESSEE_ID = {other}) OR (REQUESTER_ID = {other} AND ADDRESSEE_ID = {self})";

				int result = 0;
				DataSet _ = MySQLManager.ExecuteDataSet("results", query, ref result);
				if (result != 0)
				{
					isFriend = true;
				}

				MySQLManager.CloseConnection();
			}

			return Task.FromResult(isFriend);
		}

		#endregion

		#region public - can be used from outside

		public async Task<ObservableCollection<ContactModel>> GetFriendList()
		{
			ObservableCollection<ContactModel> friends = new ObservableCollection<ContactModel>();

			if (MySQLManager.OpenConnection())
			{
				// TODO: 
				string query =
					"SELECT REQUESTER_ID, ADDRESSEE_ID FROM " + friendshipTable +
					$" WHERE REQUESTER_ID = {int.Parse(App.UserInfo["USER_ID"])} OR ADDRESSEE_ID = {int.Parse(App.UserInfo["USER_ID"])}";

				int result = 0;
				DataSet ds = MySQLManager.ExecuteDataSet("friends", query, ref result);

				if (result > 0 && ds.Tables[0].Rows.Count > 0)
				{
					// TODO: !OPTIMIZATION!
					foreach (DataRow row in ds.Tables[0].Rows)
					{
						ContactModel friend;
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
						//friend.Date = DateTime.Parse(row["CREATED_TIME"].ToString());

						friends.Add(friend);
					}
				}

				MySQLManager.CloseConnection();
			}

			return friends;
		}

		public async Task<bool> AddFriendWithName(int self, string friendName, FriendshipStatusCode code)
		{
			bool result = false;

			if (MySQLManager.OpenConnection())
			{
				ContactModel friend = await GetUserByName(friendName);

				if (friend.Id != -1)
				{
					result = await PostRequestFriendRelation(self, friend.Id, code);
				}

				MySQLManager.CloseConnection();
			}

			return result;
		}

		public async Task<ObservableCollection<ContactModel>> GetFriendRequestList()
		{
			ObservableCollection<ContactModel> requests = new ObservableCollection<ContactModel>();

			if (MySQLManager.OpenConnection())
			{
				// 2022-01-14 This query has a problem !
				// TODO: Get friend request list, which still not made friendship with me
				string query =
					"SELECT REQUESTER_ID FROM " + friendRelationTable +
					$" WHERE ADDRESSEE_ID = {int.Parse(App.UserInfo["USER_ID"])}" +
					$" AND STATUS_CODE = 'R'";

				int result = 0;
				DataSet ds = MySQLManager.ExecuteDataSet("requests", query, ref result);

				if (result > 0)
				{
					// TODO: !OPTIMIZATION!
					if (ds.Tables[0].Rows.Count > 0)
					{
						foreach (DataRow row in ds.Tables[0].Rows)
						{
							// user name, is logged in
							ContactModel friend = await GetUserByID(int.Parse(row["REQUESTER_ID"].ToString()));
							friend.Id = int.Parse(row["REQUESTER_ID"].ToString());

							if (!await IsFriend(int.Parse(App.UserInfo["USER_ID"]), friend.Id))
							{
								requests.Add(friend);
							}
						}
					}
				}

				MySQLManager.CloseConnection();
			}

			return requests;
		}

		public async Task<bool> PostRequestFriendRelation(int self, int friend, FriendshipStatusCode code)
		{
			bool result = false;

			if (MySQLManager.OpenConnection())
			{
				string query =
					"INSERT INTO " + friendRelationTable +
					" (REQUESTER_ID, ADDRESSEE_ID, STATUS_CODE, SPECIFIER_ID, SPECIFIED_DATETIME)" +
					$" VALUES ({self}, {friend}, '{(char)code}', {self}, NOW())";

				if (MySQLManager.ExecuteNonQuery(query) == 1)
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

				MySQLManager.CloseConnection();
			}

			return result;
		}
		#endregion
	}
}
