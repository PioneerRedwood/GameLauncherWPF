using GroguLauncher.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroguLauncher.Handlers
{
	public class SocialHandler
	{
		public enum StatusCode
		{
			Requested = 'R',
			Accepted = 'A',
			Denied = 'D',
			Blocked = 'B',
		};

		/** Tables 
		 * FRIENDSHIP - requester_id, addressee_id, created_time
		 * MY_STATUS - request status table: 'R': "Requested", 'A': "Accepted", 'D': "Denied", 'B': "Blocked"
		 * FRIEND_RELATION - requester_id, addresssee_id, status_code, specifier_id, specified_datetime
		 */

		private const string friendshipTable = "FRIENDSHIP";
		private const string friendRelationTable = "FRIEND_RELATION";

		public SocialHandler()
		{
			MySQLManager.Initialize();
		}

		// TODO: select all friends
		// select friend

		// TODO: Request friend relation table
		public async Task<bool> RequestFriendRelation(int self, int friend, StatusCode code)
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
						case StatusCode.Requested:
							break;
						case StatusCode.Accepted:
							bool addResult = await AddFriendship(self, friend);
							result &= addResult;
							break;
						case StatusCode.Blocked:
							break;
						case StatusCode.Denied:
							break;
						default:
							break;
					}
				}

				MySQLManager.CloseConnection();
			}

			return result;
		}

		// TODO: Add friendship in FRIENDSHIP table
		public Task<bool> AddFriendship(int self, int friend)
		{
			bool result = false;
			if (MySQLManager.OpenConnection())
			{
				string query =
					"INSERT INTO " + friendshipTable +
					" (REQUESTER_ID, ADDRESSEE_ID, CREATED_TIME)" +
					$" VALUES({self}, {friend}, NOW())";

				int affected = MySQLManager.ExecuteNonQuery(query);

				switch (affected)
				{
					case 1:
						result = true;
						break;
					default:
						break;
				}

				MySQLManager.CloseConnection();
			}

			return Task.FromResult(result);
		}

		// TODO: Get my friend list // Need to edit
		public async Task<ObservableCollection<Social.Friend>> GetMyFriendList()
		{
			ObservableCollection<Social.Friend> friends = new ObservableCollection<Social.Friend>();

			if (MySQLManager.OpenConnection())
			{
				string query =
					"SELECT ADDRESSEE_ID, CREATED_TIME FROM " + friendshipTable +
					$" WHERE REQUESTER_ID = {int.Parse(App.UserInfo["USER_ID"])}";

				int result = 0;
				DataSet ds = MySQLManager.ExecuteDataSet("friends", query, ref result);

				if (result > 0)
				{
					// TODO: !OPTIMIZATION!
					if (ds.Tables[0].Rows.Count > 0)
					{
						foreach (DataRow row in ds.Tables[0].Rows)
						{
							Social.Friend friend = await GetFriendByID(int.Parse(row["ADDRESSEE_ID"].ToString()));

							friend.id = int.Parse(row["ADDRESSEE_ID"].ToString());
							friend.date = DateTime.Parse(row["CREATED_TIME"].ToString());
							friends.Add(friend);
						}
					}
				}

				MySQLManager.CloseConnection();
			}

			return friends;
		}

		// TODO: Get the friend info
		public Task<Social.Friend> GetFriendByID(int id)
		{
			Social.Friend friend = new Social.Friend();

			if (MySQLManager.OpenConnection())
			{
				string query =
					"SELECT USER_NAME, IS_LOGGED_IN" +
					" FROM RED_USER " +
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
							friend.name = row["USER_NAME"].ToString();
							friend.isLoggedIn = bool.Parse(row["IS_LOGGED_IN"].ToString());
						}
					}
				}
			}

			return Task.FromResult(friend);
		}

		public async Task<bool> AddFriendWithName(int self, string friendName, StatusCode code)
		{
			bool result = false;

			if (MySQLManager.OpenConnection())
			{
				// TODO: select and get user_id of friend
				Social.Friend friend = await GetUserByName(friendName);

				if (friend.id != -1)
				{
					result = await RequestFriendRelation(self, friend.id, code);
				}

				MySQLManager.CloseConnection();
			}

			return result;
		}

		// TODO: Get user info searching by name
		public Task<Social.Friend> GetUserByName(string name)
		{
			Social.Friend friend = new Social.Friend();

			if (MySQLManager.OpenConnection())
			{
				string query =
					$"SELECT USER_ID, IS_LOGGED_IN FROM RED_USER WHERE USER_NAME = '{name}'";
				int result = 0;
				DataSet ds = MySQLManager.ExecuteDataSet("friend info", query, ref result);

				// TODO: !OPTIMIZATION!
				if (ds.Tables[0].Rows.Count > 0)
				{
					foreach (DataRow row in ds.Tables[0].Rows)
					{
						friend.id = int.Parse(row["USER_ID"].ToString());
						friend.name = name;
						friend.isLoggedIn = bool.Parse(row["IS_LOGGED_IN"].ToString());
					}
				}

				MySQLManager.CloseConnection();
			}

			return Task.FromResult(friend);
		}

		// TODO: Requested to me
		public async Task<ObservableCollection<Social.Friend>> GetFriendshipRequestList() 
		{
			ObservableCollection<Social.Friend> requests = new ObservableCollection<Social.Friend>();

			if (MySQLManager.OpenConnection())
			{
				string query =
					"SELECT REQUESTER_ID FROM " + friendRelationTable +
					$" WHERE ADDRESSEE_ID = {int.Parse(App.UserInfo["USER_ID"])} AND STATUS_CODE = 'R'";

				int result = 0;
				DataSet ds = MySQLManager.ExecuteDataSet("friends", query, ref result);

				if (result > 0)
				{
					// TODO: !OPTIMIZATION!
					if (ds.Tables[0].Rows.Count > 0)
					{
						foreach (DataRow row in ds.Tables[0].Rows)
						{
							//Social.Friend friend = await GetUserByName();

							//friend.id = int.Parse(row["ADDRESSEE_ID"].ToString());
							//friend.date = DateTime.Parse(row["CREATED_TIME"].ToString());
							//friends.Add(friend);

						}
					}
				}

				MySQLManager.CloseConnection();
			}

			return requests;
		}
	}
}
