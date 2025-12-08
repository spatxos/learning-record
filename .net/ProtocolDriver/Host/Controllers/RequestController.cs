using Microsoft.AspNetCore.Mvc;
using Host.PluginRuntime;
using Host.Storage;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Host.Controllers
{
    public class RequestActionDto
    {
        public string ConnectionId { get; set; }
        public string Action { get; set; }  // read, write
        public int Address { get; set; }
        public int Count { get; set; } = 1;
        public string DataType { get; set; }  // bool, int16, uint16, int32, uint32, float
        public byte[]? Payload { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly ConnectionManager _connectionManager;
        private readonly SqliteStore _store;

        public RequestController(ConnectionManager connectionManager, SqliteStore store)
        {
            _connectionManager = connectionManager;
            _store = store;
        }

        [HttpPost]
        public async Task<ActionResult> CreateRequest([FromBody] RequestActionDto dto)
        {
            try
            {
                // 生成请求ID
                var requestId = Guid.NewGuid().ToString();
                
                // 保存请求到数据库
                _store.InsertRequest(
                    requestId,
                    dto.ConnectionId,
                    dto.Action,
                    dto.Address,
                    dto.DataType,
                    dto.Payload != null ? Convert.ToBase64String(dto.Payload) : null
                );

                // 准备协议请求参数
                var props = new Dictionary<string, string>
                {
                    { "Address", dto.Address.ToString() },
                    { "Count", dto.Count.ToString() },
                    { "DataType", dto.DataType }
                };

                var protocolRequest = new Host.SDK.ProtocolRequest(
                    dto.Action,
                    props,
                    dto.Payload
                );

                // 执行请求
                var response = await _connectionManager.ExecuteAsync(dto.ConnectionId, protocolRequest);

                // 返回结果
                return Ok(new {
                    RequestId = requestId,
                    ConnectionId = dto.ConnectionId,
                    Success = response.Success,
                    Payload = response.Payload != null ? Convert.ToBase64String(response.Payload) : null,
                    Parsed = response.Parsed,
                    Error = response.Error
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}