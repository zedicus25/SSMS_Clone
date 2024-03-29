﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MSSM_Clone.Controllers
{
    public class SqlServerController
    {
        private string _connectionPath;
        public static event Action<string> SendMessage;
        public static event Action<bool> ConnectionResult;
        private DataSet _dataSet;
        private DataTable _lastTable;

        public SqlServerController(string serverName, string dataBaseName)
        {
            CreateConnectionPath(serverName, dataBaseName);
            GetAllTablesNames();
        }


        public List<string> GetAllTablesNames()
        {
            using (SqlConnection con = new SqlConnection(_connectionPath))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", con))
                {
                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        List<string> names = new List<string>();
                        while (reader.Read())
                        {
                            names.Add((string)reader["TABLE_NAME"]);
                        }
                        return names;
                    }
                }
            }
        }


        public void InsertDataToDataBase(string tableName, Dictionary<string, string> data)
        {

            using (SqlConnection connection = new SqlConnection(_connectionPath))
            {
                connection.Open();
                string command = $"SELECT * FROM [{tableName}]";
                using (SqlCommand sqlCommand = GetSqlCommand(connection, command))
                {
                    SqlDataAdapter sqlData = new SqlDataAdapter(sqlCommand);
                    DataSet dt = new DataSet();
                    sqlData.Fill(dt);
                    DataTable table = dt.Tables[0];
                    DataRow dataRow = table.NewRow();
                    foreach (var key in data)
                    {
                        dataRow[key.Key] = key.Value;
                    }
                    table.Rows.Add(dataRow);
                    SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(sqlData);
                    sqlData.Update(dt);
                }
            }
        }

        public List<string> GetFieldsName(string tableName)
        {
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    connection.Open();
                    string command = $"SELECT * FROM [{tableName}]";
                    using (SqlCommand sqlCommand = GetSqlCommand(connection, command))
                    {
                        List<string> names = new List<string>();
                        SqlDataReader sqlData = sqlCommand.ExecuteReader();
                        for (int i = 0; i < sqlData.FieldCount; i++)
                        {
                            names.Add(sqlData.GetName(i));
                        }
                        return names;
                    }
                }
            }
            catch (Exception ex)
            {
                SendMessage?.Invoke(ex.Message);
            }
            return new List<string>();
        }

        public void DeleteData(string tableName,object id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionPath))
            {
                connection.Open();
                string command = $"SELECT * FROM [{tableName}]";

                SqlDataAdapter adapter = new SqlDataAdapter(command, connection);
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                DataTable table = dataSet.Tables[0];
                table.AsEnumerable().FirstOrDefault(x => x[0].Equals(id)).Delete();

                SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(adapter);
                adapter.Update(dataSet);
            }
        }

        public DataTable GetTable(string tableName)
        {
            using (SqlConnection connection = GetSqlConnection())
            {
                string command = $"SELECT * FROM [{tableName}]";
                SqlDataAdapter adapter = new SqlDataAdapter(command, connection);
                _dataSet = new DataSet();
                adapter.Fill(_dataSet);
                _lastTable = _dataSet.Tables[0];
                return _dataSet.Tables[0];
            }
        }


        public void Update(string tableName, object id, List<string> data)
        {
            using (SqlConnection connection = new SqlConnection(_connectionPath))
            {
                connection.Open();
                string command = $"SELECT * FROM [{tableName}]";

                SqlDataAdapter adapter = new SqlDataAdapter(command, connection);
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                DataTable table = dataSet.Tables[0];
                DataRow row = table.AsEnumerable().FirstOrDefault(x => x[0].Equals(id));
                for (int i = 0; i < data.Count; i++)
                {
                    row[i] = data[i];
                }
                SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(adapter);
                adapter.Update(dataSet);
            }
        }

        private void CreateConnectionPath(string serverName, string dataBaseName)
        {
            _connectionPath = TryCreateConnectionPath(serverName, dataBaseName);
            try
            {
                using (SqlConnection connection = GetSqlConnection())
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    ConnectionResult?.Invoke(true);
                    SendMessage?.Invoke("Succesful connection!");

                }
            }
            catch (Exception ex)
            {
                SendMessage?.Invoke(ex.Message);
                ConnectionResult?.Invoke(false);
            }
        }

        private string TryCreateConnectionPath(string serverName, string dataBaseName)
        {
            if (serverName.Equals(String.Empty))
                throw new ArgumentException("Server name cannot be empty");
            if (dataBaseName.Equals(String.Empty))
                throw new ArgumentException("Data base name cannot be empty");
            return $@"Server={serverName.Trim()};Database={dataBaseName.Trim()};Trusted_Connection=True;";
        }

        private SqlConnection GetSqlConnection() => new SqlConnection(_connectionPath);
        private SqlCommand GetSqlCommand(SqlConnection connection, string command) => new SqlCommand(command, connection);
    }
}
