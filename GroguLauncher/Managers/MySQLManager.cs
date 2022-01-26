using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Data;

namespace GroguLauncher.Managers
{
	public static class MySQLManager
	{
		private static MySqlConnection _connection;

		private static bool _initialized = false;

		public static void Initialize()
		{
			if (!_initialized)
			{
				_initialized = true;
				Debug.WriteLine("Database Initialize");

				string path = "Server=127.0.0.1;port=3306;Database=red_db;Uid=admin;pwd=5303aa!@;";

				_connection = new MySqlConnection(path);
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
				if (_connection.State == ConnectionState.Open)
				{
					return true;
				}
				else
				{
					_connection.Open();
				}

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
			}

			return false;
		}

		public static void CloseConnection()
		{
			_connection.Close();
		}

		// Use `Select` getting one or more data
		public static DataSet ExecuteDataSet(string dsName, string query, ref int result)
		{
			try
			{
				DataSet ds = new DataSet();
				MySqlDataAdapter dataAdapter = new MySqlDataAdapter(query, _connection);
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

		// Use when Update, Insert, Delete
		public static int ExecuteNonQuery(string query)
		{
			int affected = -1;
			try
			{
				if (OpenConnection())
				{
					using (MySqlCommand cmd = new MySqlCommand(query, _connection))
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

		// just for test
		// see https://stackoverflow.com/questions/35928312/c-sharp-mysqlcommand-executenonquery-return-1
		public static int ExecuteSql(string query)
		{
			int result = 0;
			try
			{
				if (OpenConnection())
				{
					using (MySqlCommand cmd = new MySqlCommand(query, _connection))
					{
						object temp = cmd.ExecuteScalar();
						if (temp != null)
						{
							return -1;
						}

						CloseConnection();
					}
					return result;
				}
			}
			catch (MySqlException e)
			{
				Debug.WriteLine(e.Message);
				return -1;
			}

			return result;
		}
	}
}
