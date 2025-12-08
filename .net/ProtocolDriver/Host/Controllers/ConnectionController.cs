using Microsoft.AspNetCore.Mvc;
using Host.PluginRuntime;
using Host.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Host.SDK;
using Microsoft.Extensions.Logging;

namespace Host.Controllers
{
    public class CreateConnectionDto
    {
        public string PluginName { get; set; }
        public string ProtocolName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }

    public class RequestDto
    {
        public string? ConnectionId { get; set; }
        public string Action { get; set; }
        public int Address { get; set; }
        public string DataType { get; set; }
        public byte[]? Payload { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ConnectionController : ControllerBase
    {
        private readonly ConnectionManager _mgr;
        private readonly SqliteStore _store;
        private readonly SingletonConnectionManager _singletonMgr;
        private readonly ILogger<ConnectionController> _logger;

        public ConnectionController(ConnectionManager mgr, SqliteStore store, SingletonConnectionManager singletonMgr, ILogger<ConnectionController> logger)
        {
            _mgr = mgr;
            _store = store;
            _singletonMgr = singletonMgr;
            _logger = logger;
        }

        [HttpPost]
        public ActionResult Create([FromBody] CreateConnectionDto dto)
        {
            var id = Guid.NewGuid().ToString();
            _store.InsertConnection(
                id, 
                dto.PluginName, 
                dto.ProtocolName, 
                dto.Host, 
                dto.Port, 
                JsonSerializer.Serialize(dto.Parameters)
            );
            return Ok(new { ConnectionId = id });
        }
        
        /// <summary>
        /// 创建连接配置
        /// </summary>
        [HttpPost("config")]
        public async Task<ActionResult> CreateConnectionConfig([FromBody] CreateConnectionDto dto)
        {
            // 插入连接配置到数据库
            _store.InsertConnectionConfig(
                dto.PluginName, 
                dto.ProtocolName, 
                dto.Host, 
                dto.Port, 
                JsonSerializer.Serialize(dto.Parameters)
            );
            
            // 获取所有连接配置，找到最新添加的那个
            var allConfigs = _store.GetAllEnabledConnectionConfigs();
            var newConfig = allConfigs.LastOrDefault(c => 
                c.PluginName == dto.PluginName && 
                c.ProtocolName == dto.ProtocolName && 
                c.Host == dto.Host && 
                c.Port == dto.Port);
            
            if (newConfig != null)
            {
                // 将新添加的连接配置添加到单例连接管理器
                await _singletonMgr.AddConnectionConfigAsync(newConfig);
            }
            
            return Ok(new { Message = "Connection config created successfully" });
        }

        [HttpPost("{id}/start")]
        public async Task<ActionResult> Start(string id)
        {
            try
            {
                _logger.LogInformation("Starting connection with id: {Id}", id);
                
                var cfg = _store.GetConnection(id);
                if (cfg == null)
                {
                    _logger.LogWarning("Connection not found with id: {Id}", id);
                    return NotFound(new { Message = "Connection not found" });
                }
                
                _logger.LogInformation("Found connection configuration: PluginName={PluginName}, ProtocolName={ProtocolName}, Host={Host}, Port={Port}, Parameters={Parameters}", 
                    cfg.PluginName, cfg.ProtocolName, cfg.Host, cfg.Port, cfg.Parameters);
                
                var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(cfg.Parameters);
                if (settings == null)
                {
                    settings = new Dictionary<string, string>();
                    _logger.LogInformation("Parameters deserialized to null, using empty dictionary");
                }
                
                // 添加Host和Port到settings
                if (!string.IsNullOrEmpty(cfg.Host))
                {
                    settings["Host"] = cfg.Host;
                    _logger.LogInformation("Added Host to settings: {Host}", cfg.Host);
                }
                if (cfg.Port > 0)
                {
                    settings["Port"] = cfg.Port.ToString();
                    _logger.LogInformation("Added Port to settings: {Port}", cfg.Port);
                }
                
                _logger.LogInformation("Creating connection with plugin: {PluginName}, settings: {Settings}", cfg.PluginName, string.Join(", ", settings.Select(kv => $"{kv.Key}={kv.Value}")));
                
                var inst = await _mgr.CreateConnectionAsync(cfg.PluginName, settings);
                
                _logger.LogInformation("Connection created with instance id: {InstanceId}", inst.ConnectionId);
                
                await _mgr.StartConnectionAsync(inst.ConnectionId);
                
                _logger.LogInformation("Connection started successfully with instance id: {InstanceId}", inst.ConnectionId);
                
                return Ok(new { ConnectionId = inst.ConnectionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting connection with id: {Id}", id);
                return StatusCode(500, new { Message = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        [HttpPost("{id}/stop")]
        public async Task<ActionResult> Stop(string id)
        {
            try
            {
                await _mgr.StopConnectionAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPost("{id}/execute")]
        public async Task<ActionResult> Execute(string id, [FromBody] RequestDto dto)
        {
            try
            {
                _logger.LogInformation("Executing command for connection id: {Id}, Action: {Action}, Address: {Address}, DataType: {DataType}", 
                    id, dto.Action, dto.Address, dto.DataType);
                
                var request = new Host.SDK.ProtocolRequest(
                    dto.Action, 
                    new Dictionary<string, string> {
                        { "Address", dto.Address.ToString() },
                        { "DataType", dto.DataType }
                    },
                    dto.Payload
                );
                
                _logger.LogInformation("Sending protocol request to connection manager");
                var response = await _mgr.ExecuteAsync(id, request);
                
                _logger.LogInformation("Received response from connection manager: Success={Success}, Error={Error}", 
                    response.Success, response.Error);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command for connection id: {Id}", id);
                return StatusCode(500, new { Message = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public ActionResult GetAllConnections()
        {
            // 获取所有活跃连接
            var activeConnections = _mgr.GetActiveConnections();
            
            // 返回活跃连接信息
            var result = new List<object>();
            foreach (var connection in activeConnections)
            {
                result.Add(new
                {
                    ConnectionId = connection.ConnectionId,
                    PluginName = connection.PluginName,
                    State = connection.Connection?.State.ToString(),
                    Settings = connection.Settings
                });
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// 获取所有连接配置及其状态
        /// </summary>
        [HttpGet("configs")]
        public ActionResult GetAllConnectionConfigs()
        {
            // 获取所有连接配置及其状态
            var connectionInstances = _singletonMgr.GetAllConnectionInstances();
            
            var result = new List<object>();
            foreach (var (config, instanceInfo) in connectionInstances)
            {
                result.Add(new
                {
                    ConfigId = config.Id,
                    PluginName = config.PluginName,
                    ProtocolName = config.ProtocolName,
                    Host = config.Host,
                    Port = config.Port,
                    Parameters = config.Parameters,
                    RetryInterval = config.RetryInterval,
                    Status = instanceInfo.State.ToString(),
                    CurrentConnectionId = instanceInfo.ConnectionId
                });
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// 执行读指令
        /// </summary>
        [HttpPost("{connectionId}/read")]
        public async Task<ActionResult> Read(string connectionId, [FromBody] RequestDto dto)
        {
            // 设置Action为read
            dto.Action = "read";
            return await Execute(connectionId, dto);
        }
        
        /// <summary>
        /// 执行写指令
        /// </summary>
        [HttpPost("{connectionId}/write")]
        public async Task<ActionResult> Write(string connectionId, [FromBody] RequestDto dto)
        {
            // 设置Action为write
            dto.Action = "write";
            return await Execute(connectionId, dto);
        }
    }
}