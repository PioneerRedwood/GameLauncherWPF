using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroguLauncher.Managers;
using System.Data;

namespace GroguLauncher.Handlers
{
	class AccountHandler
	{
		private readonly string tbName = "red_user";
		private readonly string dsName = "Accounts";

		public AccountHandler()
		{
			MySQLManager.Initialize();
		}

		public Task<bool> SyncLogin(string id, string pwd)
		{
			bool succeed = false;
			Debug.WriteLine("TryLogin");
			if (MySQLManager.OpenConnection())
			{
				string query =
					"SELECT * FROM " + tbName +
					" WHERE ACCOUNT_ID = '" + id + "'" +
					" AND ACCOUNT_PWD = '" + pwd + "'";

				int result = 0;
				DataSet ds = MySQLManager.ExecuteDataSet(dsName, query, ref result);

				switch (result)
				{
					case 0:
						// TODO
						succeed = false;
						break;
					case 1:
						if (ds.Tables[0].Rows.Count > 0)
						{
							foreach(DataRow row in ds.Tables[0].Rows)
							{
								if(id == row["ACCOUNT_ID"].ToString() && pwd == row["ACCOUNT_PWD"].ToString())
								{
									succeed = true;
									break;
								}
							}
						}
						break;
					default:
						// TODO
						succeed = false;
						break;
				}
				MySQLManager.CloseConnection();
			}

			return Task.FromResult(succeed);
		}

		public void UpdateAccountDataAsync(string id, string pwd)
		{

		}
	}
}
