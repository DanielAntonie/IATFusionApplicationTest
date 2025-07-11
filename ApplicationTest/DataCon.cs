using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace ApplicationTest
{
    public class DataCon
    {
        public struct SQLJob
        {
            public string SQLText;
            public ArrayList ListOfSQLParameters;
        }

        // Update this method if you want to pull the connection string from appsettings.json instead.
        public static string GetConnectionString()
        {
            return "server=localhost;port=3306;database=ApplicationTextDB;user=root;password=SqlRoot5217!;";
        }

        public static MySqlConnection GetConnectionStrings()
        {
            string connectionString = GetConnectionString();
            return new MySqlConnection(connectionString);
        }

        public static DataSet BuildDataSet(string SQLQuery)
        {
            string connectionString = GetConnectionString();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlDataAdapter adapter = new MySqlDataAdapter(SQLQuery, connection);
                adapter.SelectCommand.CommandTimeout = 600;

                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet, "0");
                return dataSet;
            }
        }

        public static DataSet BuildDataSet(string SQLQuery, ArrayList ListOfSQLParameters)
        {
            string connectionString = GetConnectionString();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(SQLQuery, connection);
                command.CommandTimeout = 600;

                foreach (MySqlParameter param in ListOfSQLParameters)
                {
                    command.Parameters.Add(param);
                }

                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet, "0");

                return dataSet;
            }
        }

        public static void ExecNonQuery(string SQLQuery)
        {
            string connectionString = GetConnectionString();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(SQLQuery, connection);
                command.CommandTimeout = 600;

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void ExecNonQuery(string SQLQuery, ArrayList ListOfSQLParameters)
        {
            string connectionString = GetConnectionString();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(SQLQuery, connection);
                command.CommandTimeout = 600;

                foreach (MySqlParameter param in ListOfSQLParameters)
                {
                    command.Parameters.Add(param);
                }

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static bool ExecNonQueryTrans(List<SQLJob> SQLJobList)
        {
            bool JobsSuccessful = false;
            string connectionString = GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.CommandTimeout = 600;

                MySqlTransaction transaction = connection.BeginTransaction();
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    foreach (SQLJob job in SQLJobList)
                    {
                        command.CommandText = job.SQLText;
                        foreach (MySqlParameter param in job.ListOfSQLParameters)
                        {
                            command.Parameters.Add(param);
                        }

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }

                    transaction.Commit();
                    JobsSuccessful = true;
                }
                catch
                {
                    transaction.Rollback();
                }

                connection.Close();
            }

            return JobsSuccessful;
        }

        public static string ExecNonQueryTransWithErrMessage(List<SQLJob> SQLJobList)
        {
            string connectionString = GetConnectionString();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlCommand command = connection.CreateCommand();
                command.CommandTimeout = 600;

                MySqlTransaction transaction = connection.BeginTransaction();
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    foreach (SQLJob job in SQLJobList)
                    {
                        command.CommandText = job.SQLText;
                        foreach (MySqlParameter param in job.ListOfSQLParameters)
                        {
                            command.Parameters.Add(param);
                        }

                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }

                    transaction.Commit();
                    connection.Close();
                    return "";
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    connection.Close();
                    return e.Message;
                }
            }
        }
    }
}
