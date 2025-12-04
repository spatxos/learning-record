using Host.SDK;
using System;

namespace Modbus
{
    /// <summary>
    /// Modbus读取请求
    /// </summary>
    public class ModbusReadRequest : ReadRequestBase
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        public Type DataType { get; set; }
    }

    /// <summary>
    /// Modbus写入请求
    /// </summary>
    public class ModbusWriteRequest : WriteRequestBase
    {
    }
}