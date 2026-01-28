using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Host.Storage
{
    public class SqliteStore
    {
        private readonly string _dbPath;
        
        public SqliteStore(string dbPath)
        {
            _dbPath = dbPath;
            EnsureTables();
        }

        private void EnsureTables()
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS ConnectionConfigs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PluginName TEXT,
                ProtocolName TEXT,
                Host TEXT,
                Port INTEGER,
                Parameters TEXT,
                IsEnabled BOOLEAN DEFAULT 1,
                RetryInterval INTEGER DEFAULT 30000
            );
            
            CREATE TABLE IF NOT EXISTS Connections (
                Id TEXT PRIMARY KEY,
                PluginName TEXT,
                ProtocolName TEXT,
                Host TEXT,
                Port INTEGER,
                Parameters TEXT,
                CreatedTime TEXT
            );
            
            CREATE TABLE IF NOT EXISTS Requests (
                Id TEXT PRIMARY KEY,
                ConnectionId TEXT,
                Action TEXT,
                Address INTEGER,
                DataType TEXT,
                Payload TEXT,
                CreatedTime TEXT
            );
            
            CREATE TABLE IF NOT EXISTS Notifications (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ConnectionId TEXT,
                EventType TEXT,
                Message TEXT,
                Timestamp TEXT
            );
            
            CREATE INDEX IF NOT EXISTS idx_connections_pluginname ON Connections(PluginName);
            CREATE INDEX IF NOT EXISTS idx_requests_connectionid ON Requests(ConnectionId);
            CREATE INDEX IF NOT EXISTS idx_notifications_connectionid ON Notifications(ConnectionId);
            
            CREATE INDEX IF NOT EXISTS idx_notifications_timestamp ON Notifications(Timestamp);
            
            CREATE INDEX IF NOT EXISTS idx_connectionconfigs_enabled ON ConnectionConfigs(IsEnabled);";
            cmd.ExecuteNonQuery();
        }

        public void InsertConnection(string id, string pluginName, string protocolName, string host, int port, string parameters)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Connections (Id, PluginName, ProtocolName, Host, Port, Parameters, CreatedTime) VALUES ($id, $pluginName, $protocolName, $host, $port, $parameters, $createdTime)";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$pluginName", pluginName);
            cmd.Parameters.AddWithValue("$protocolName", protocolName);
            cmd.Parameters.AddWithValue("$host", host);
            cmd.Parameters.AddWithValue("$port", port);
            cmd.Parameters.AddWithValue("$parameters", parameters);
            cmd.Parameters.AddWithValue("$createdTime", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public ConnectionInfo GetConnection(string id)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, PluginName, ProtocolName, Host, Port, Parameters, CreatedTime FROM Connections WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new ConnectionInfo
                {
                    Id = reader.GetString(0),
                    PluginName = reader.GetString(1),
                    ProtocolName = reader.GetString(2),
                    Host = reader.GetString(3),
                    Port = reader.GetInt32(4),
                    Parameters = reader.GetString(5),
                    CreatedTime = reader.GetString(6)
                };
            }
            
            throw new KeyNotFoundException($"Connection with id {id} not found");
        }

        public void InsertRequest(string id, string connectionId, string action, int address, string dataType, string payload)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Requests (Id, ConnectionId, Action, Address, DataType, Payload, CreatedTime) VALUES ($id, $connectionId, $action, $address, $dataType, $payload, $createdTime)";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$connectionId", connectionId);
            cmd.Parameters.AddWithValue("$action", action);
            cmd.Parameters.AddWithValue("$address", address);
            cmd.Parameters.AddWithValue("$dataType", dataType);
            cmd.Parameters.AddWithValue("$payload", payload);
            cmd.Parameters.AddWithValue("$createdTime", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }

        public void InsertNotification(string connectionId, string eventType, string message)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Notifications (ConnectionId, EventType, Message, Timestamp) VALUES ($cid, $evt, $msg, $ts)";
            cmd.Parameters.AddWithValue("$cid", connectionId);
            cmd.Parameters.AddWithValue("$evt", eventType);
            cmd.Parameters.AddWithValue("$msg", message);
            cmd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();
        }
        
        public List<ConnectionInfo> GetAllConnections()
        {
            var connections = new List<ConnectionInfo>();
            
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, PluginName, ProtocolName, Host, Port, Parameters, CreatedTime FROM Connections";
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                connections.Add(new ConnectionInfo
                {
                    Id = reader.GetString(0),
                    PluginName = reader.GetString(1),
                    ProtocolName = reader.GetString(2),
                    Host = reader.GetString(3),
                    Port = reader.GetInt32(4),
                    Parameters = reader.GetString(5),
                    CreatedTime = reader.GetString(6)
                });
            }
            
            return connections;
        }
        
        public List<ConnectionConfig> GetAllEnabledConnectionConfigs()
        {
            var configs = new List<ConnectionConfig>();
            
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, PluginName, ProtocolName, Host, Port, Parameters, IsEnabled, RetryInterval FROM ConnectionConfigs WHERE IsEnabled = 1";
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                configs.Add(new ConnectionConfig
                {
                    Id = reader.GetInt32(0),
                    PluginName = reader.GetString(1),
                    ProtocolName = reader.GetString(2),
                    Host = reader.GetString(3),
                    Port = reader.GetInt32(4),
                    Parameters = reader.GetString(5),
                    IsEnabled = reader.GetBoolean(6),
                    RetryInterval = reader.GetInt32(7)
                });
            }
            
            return configs;
        }
        
        public void InsertConnectionConfig(string pluginName, string protocolName, string host, int port, string parameters)
        {
            using var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO ConnectionConfigs (PluginName, ProtocolName, Host, Port, Parameters, IsEnabled) VALUES ($pluginName, $protocolName, $host, $port, $parameters, 1)";
            cmd.Parameters.AddWithValue("$pluginName", pluginName);
            cmd.Parameters.AddWithValue("$protocolName", protocolName);
            cmd.Parameters.AddWithValue("$host", host);
            cmd.Parameters.AddWithValue("$port", port);
            cmd.Parameters.AddWithValue("$parameters", parameters);
            cmd.ExecuteNonQuery();
        }
    }
    
    public class ConnectionInfo
    {
        public string? Id { get; set; }
        public string? PluginName { get; set; }
        public string? ProtocolName { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? Parameters { get; set; }
        public string? CreatedTime { get; set; }
    }
    
    public class ConnectionConfig
    {
        public int Id { get; set; }
        public string? PluginName { get; set; }
        public string? ProtocolName { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }
        public string? Parameters { get; set; }
        public bool IsEnabled { get; set; }
        public int RetryInterval { get; set; }
    }
}