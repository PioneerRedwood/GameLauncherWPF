using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;

namespace GroguLauncher.Handlers
{
	/// <summary>
	/// DB: red_db, tb: red_user, user
	/// Create
	/// </summary>
	public class AccountHandler
	{
		private readonly string _tableName = "red_user";
		private readonly string _dsName = "Accounts";

		public AccountHandler()
		{
			MySQLHandler.Initialize();
		}

		public Task<Dictionary<string, string>> Login(string mail, string pwd)
		{
			Dictionary<string, string> userInfo = new Dictionary<string, string>();

			if (MySQLHandler.OpenConnection())
			{
				string query =
					"SELECT * FROM " + _tableName +
					" WHERE ACCOUNT_MAIL = '" + mail + "'" +
					" AND ACCOUNT_PWD = '" + pwd + "'";

				int result = 0;
				DataSet ds = MySQLHandler.ExecuteDataSet(_dsName, query, ref result);

				switch (result)
				{
					case 1:
						if (ds.Tables[0].Rows.Count > 0)
						{
							foreach (DataRow row in ds.Tables[0].Rows)
							{
								if (mail == row["ACCOUNT_MAIL"].ToString() && pwd == row["ACCOUNT_PWD"].ToString())
								{
									userInfo.Add("USER_NAME", row["USER_NAME"].ToString());
									userInfo.Add("USER_ID", row["USER_ID"].ToString());
									break;
								}
							}
						}
						break;
					default:
						break;
				}
				MySQLHandler.CloseConnection();
			}

			return Task.FromResult(userInfo);
		}

		public Task<bool> CheckAccountExsists(string mail, string name)
		{
			bool succeed = false;
			if (MySQLHandler.OpenConnection())
			{
				// TODO: How to distinguish there are accounts the same mail or name
				// execute query double times? .. that is not good
				int result = MySQLHandler.ExecuteSql(
					"SELECT USER_NAME FROM RED_USER " +
					$"WHERE ACCOUNT_MAIL = '{mail}'" +
					$" OR USER_NAME = '{name}'");

				switch (result)
				{
					case 0:
						succeed = true;
						break;
					default:
						// TODO: already exsists.. fail to create account
						succeed = false;
						break;
				}
			}

			return Task.FromResult(succeed);
		}

		public Task<bool> CreateAccount(string mail, string name, string pwd)
		{
			bool succeed = false;
			if (MySQLHandler.OpenConnection())
			{
				int result = MySQLHandler.ExecuteNonQuery(
							"INSERT INTO RED_USER " +
							"(ACCOUNT_MAIL, ACCOUNT_PWD, USER_NAME, SIGNUP_DATE, IS_LOGGED_IN)" +
							$"VALUES('{mail}', '{pwd}', '{name}', now(), 0)");

				switch (result)
				{
					case 1:
						succeed = true;
						break;
					default:
						succeed = false;
						break;
				}
			}

			return Task.FromResult(succeed);
		}

	}
}
