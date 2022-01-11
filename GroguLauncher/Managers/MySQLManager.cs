using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Data;

namespace GroguLauncher.Managers
{
	public static class MySQLManager
	{
		private static MySqlConnection connection;

		private static bool initialized = false;
		public static void Initialize()
		{
			if(!initialized)
			{
				initialized = true;
				Debug.WriteLine("Database Initialize");

				string path = "Server=127.0.0.1;port=3306;Database=red_db;Uid=admin;pwd=5303aa!@;";

				connection = new MySqlConnection(path);

			}
			else
			{
				Debug.WriteLine("Database already Initialized");
			}
		}

		public static bool OpenConnection()
		{
			try
			{
				connection.Open();
				return true;
			}
			catch (MySqlException e)
			{
				switch (e.Number)
				{
					case 0:
						Debug.WriteLine("Unable to connect to the server");
						break;
					case 1045:
						Debug.WriteLine("Please check the connection user ID or password");
						break;
					default:
						Debug.WriteLine(e.Message);
						break;
				}
				return false;
			}
		}

		public static bool CloseConnection()
		{
			try
			{
				connection.Close();
				return true;
			}
			catch (MySqlException e)
			{
				Debug.WriteLine(e.Message);
				return false;
			}
		}

		// 데이터셋에 쿼리 실행후 담기
		public static DataSet ExecuteDataSet(string dsName, string query, ref int result)
		{
			try
			{
				DataSet ds = new DataSet();
				MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, connection);
				result = dataAdapter.Fill(ds, dsName);
				return ds;
			}
			catch (MySqlException e)
			{
				Debug.WriteLine(e.Message);
				result = -1;
				return null;
			}
		}

		public static int ExecuteSql(string query)
		{
			int affected = -1;
			try
			{
				if (OpenConnection())
				{
					using (MySqlCommand cmd = new MySqlCommand(query, connection))
					{
						affected = cmd.ExecuteNonQuery();
						CloseConnection();
					}
					return affected;
				}
			}
			catch (MySqlException e)
			{
				Debug.WriteLine(e.Message);
				return -1;
			}

			return affected;
		}
	}
}
