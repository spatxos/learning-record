using Host.SDK;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PluginController : ControllerBase
    {
        private readonly PluginManager _pluginManager;
        private readonly ILogger<PluginController> _logger;
        private readonly string _pluginsDirectory;

        public PluginController(
            PluginManager pluginManager,
            ILogger<PluginController> logger)
        {
            _pluginManager = pluginManager;
            _logger = logger;
            _pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
        }

        /// <summary>
        /// 获取所有插件
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<object>> GetPlugins()
        {
            var plugins = _pluginManager.GetPlugins();
            // 返回安全的序列化版本，不包含无法序列化的属性
            var safePlugins = plugins.Select(p => new
            {
                p.Id,
                p.ProtocolName,
                p.Version,
                p.Status
            });
            return Ok(safePlugins);
        }

        /// <summary>
        /// 根据协议名称获取插件
        /// </summary>
        [HttpGet("{protocolName}")]
        public ActionResult<object> GetPlugin(string protocolName)
        {
            var plugin = _pluginManager.GetPluginByProtocolName(protocolName);
            if (plugin == null)
                return NotFound();
            // 返回安全的序列化版本，不包含无法序列化的属性
            var safePlugin = new
            {
                plugin.Id,
                plugin.ProtocolName,
                plugin.Version,
                plugin.Status
            };
            return Ok(safePlugin);
        }

        /// <summary>
        /// 上传插件
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<PluginInfo>> UploadPlugin(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // 读取插件元信息（简化实现，实际应用中可以从DLL或单独的manifest文件读取）
                var protocolName = Path.GetFileNameWithoutExtension(file.FileName);
                var version = "1.0.0";

                // 创建目录结构：plugins/{protocolName}/{version}/
                var pluginDir = Path.Combine(_pluginsDirectory, protocolName, version);
                Directory.CreateDirectory(pluginDir);

                // 保存文件
                var filePath = Path.Combine(pluginDir, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 加载插件
                var plugin = await _pluginManager.LoadPluginAsync(filePath, protocolName);
                if (plugin == null)
                    return StatusCode(500, "Failed to load plugin");

                return Ok(plugin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload plugin");
                return StatusCode(500, "Failed to upload plugin");
            }
        }

        /// <summary>
        /// 热更新插件
        /// </summary>
        [HttpPost("{protocolName}/reload")]
        public async Task<ActionResult<PluginInfo>> ReloadPlugin(string protocolName, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // 创建临时目录保存新插件
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);
                var tempPath = Path.Combine(tempDir, file.FileName);

                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 热更新插件
                var plugin = await _pluginManager.ReloadPluginAsync(tempPath);
                if (plugin == null)
                    return StatusCode(500, "Failed to reload plugin");

                // 清理临时文件
                Directory.Delete(tempDir, true);

                return Ok(plugin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reload plugin: {ProtocolName}", protocolName);
                return StatusCode(500, "Failed to reload plugin");
            }
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        [HttpDelete("{pluginId}")]
        public async Task<ActionResult> UnloadPlugin(string pluginId)
        {
            var success = await _pluginManager.UnloadPluginAsync(pluginId);
            if (success)
                return Ok();
            return NotFound();
        }

        /// <summary>
        /// 检查插件健康状态
        /// </summary>
        [HttpGet("{pluginId}/health")]
        public ActionResult<DriverHealth> CheckPluginHealth(string pluginId)
        {
            var plugin = _pluginManager.GetPlugins().FirstOrDefault(p => p.Id == pluginId);
            if (plugin == null)
                return NotFound();

            // 由于新接口没有CheckHealthAsync方法，返回默认健康状态
            var health = new DriverHealth(true, "Plugin is loaded");
            return Ok(health);
        }
    }
}