using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Host.SDK
{
    public interface IReadWriteNet
    {
        /// <summary>
        /// 批量读取字节数组信息，需要指定地址和长度，返回原始的字节数组
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>带有成功标识的byte[]数组</returns>
        OperateResult<byte[]> Read(string address, ushort length);

        /// <summary>
        /// 读取支持Hsl特性的数据内容，该特性为DeviceAddressAttribute
        /// </summary>
        /// <typeparam name="T">自定义的数据类型对象</typeparam>
        /// <returns>包含是否成功的结果对象</returns>
        OperateResult<T> Read<T>() where T : class, new();

        /// <summary>
        /// 异步批量读取字节数组信息，需要指定地址和长度，返回原始的字节数组
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>带有成功标识的byte[]数组</returns>
        Task<OperateResult<byte[]>> ReadAsync(string address, ushort length);

        /// <summary>
        /// 异步读取支持特性的数据内容，该特性为DeviceAddressAttribute
        /// </summary>
        /// <typeparam name="T">自定义的数据类型对象</typeparam>
        /// <returns>包含是否成功的结果对象</returns>
        Task<OperateResult<T>> ReadAsync<T>() where T : class, new();

        /// <summary>
        /// 读取单个的Boolean数据信息
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <returns>带有成功标识的 bool 值</returns>
        OperateResult<bool> ReadBool(string address);

        /// <summary>
        /// 批量读取Boolean数组信息，需要指定地址和长度，返回Boolean 数组
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>带有成功标识的 bool[] 数组</returns>
        OperateResult<bool> ReadBool(string address, ushort length);


        /// <summary>
        /// 读取单个的Boolean数据信息
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <returns>带有成功标识的 bool 值</returns>
        Task<OperateResult<bool>> ReadBoolAsync(string address);

        /// <summary>
        /// 批量读取Boolean数组信息，需要指定地址和长度，返回Boolean 数组
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>带有成功标识的 bool[] 数组</returns>
        Task<OperateResult<bool>> ReadBoolAsync(string address, ushort length);

        #region UInt32
        /// <summary>
        /// 读取单个 UInt32 数据信息
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <returns>带有成功标识的 uint 值</returns>
        OperateResult<uint> ReadUInt32(string address);

        /// <summary>
        /// 批量读取 UInt32 数组，需要指定地址和长度
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>带有成功标识的 uint[] 数组</returns>
        OperateResult<uint[]> ReadUInt32(string address, ushort length);

        /// <summary>
        /// 读取单个 UInt32 数据信息（异步）
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <returns>带有成功标识的 uint 值</returns>
        Task<OperateResult<uint>> ReadUInt32Async(string address);

        /// <summary>
        /// 批量读取 UInt32 数组（异步）
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="length">数据长度</param>
        /// <returns>带有成功标识的 uint[] 数组</returns>
        Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length);
        #endregion


        #region UInt16
        /// <summary>
        /// 读取单个 UInt16 数据信息
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <returns>带有成功标识的 ushort 值</returns>
        OperateResult<ushort> ReadUInt16(string address);

        /// <summary>
        /// 批量读取 UInt16 数组
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="length">数组长度</param>
        /// <returns>带有成功标识的 ushort[] 数组</returns>
        OperateResult<ushort[]> ReadUInt16(string address, ushort length);

        /// <summary>
        /// 异步读取单个 UInt16 数据信息
        /// </summary>
        Task<OperateResult<ushort>> ReadUInt16Async(string address);

        /// <summary>
        /// 异步批量读取 UInt16 数组
        /// </summary>
        Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length);
        #endregion


        #region UInt64
        /// <summary>
        /// 读取单个 UInt64 数据信息
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <returns>带有成功标识的 ulong 值</returns>
        OperateResult<ulong> ReadUInt64(string address);

        /// <summary>
        /// 批量读取 UInt64 数组
        /// </summary>
        OperateResult<ulong[]> ReadUInt64(string address, ushort length);

        /// <summary>
        /// 异步读取单个 UInt64 数据信息
        /// </summary>
        Task<OperateResult<ulong>> ReadUInt64Async(string address);

        /// <summary>
        /// 异步批量读取 UInt64 数组
        /// </summary>
        Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length);
        #endregion


        #region Int32
        /// <summary>
        /// 读取单个 Int32 数据信息
        /// </summary>
        OperateResult<int> ReadInt32(string address);

        /// <summary>
        /// 批量读取 Int32 数组
        /// </summary>
        OperateResult<int[]> ReadInt32(string address, ushort length);

        /// <summary>
        /// 异步读取单个 Int32
        /// </summary>
        Task<OperateResult<int>> ReadInt32Async(string address);

        /// <summary>
        /// 异步批量读取 Int32 数组
        /// </summary>
        Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length);
        #endregion


        #region Int16
        /// <summary>
        /// 读取单个 Int16 数据信息
        /// </summary>
        OperateResult<short> ReadInt16(string address);

        /// <summary>
        /// 批量读取 Int16 数组
        /// </summary>
        OperateResult<short[]> ReadInt16(string address, ushort length);

        /// <summary>
        /// 异步读取单个 Int16
        /// </summary>
        Task<OperateResult<short>> ReadInt16Async(string address);

        /// <summary>
        /// 异步批量读取 Int16 数组
        /// </summary>
        Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length);
        #endregion


        #region Int64
        /// <summary>
        /// 读取单个 Int64 数据信息
        /// </summary>
        OperateResult<long> ReadInt64(string address);

        /// <summary>
        /// 批量读取 Int64 数组
        /// </summary>
        OperateResult<long[]> ReadInt64(string address, ushort length);

        /// <summary>
        /// 异步读取单个 Int64
        /// </summary>
        Task<OperateResult<long>> ReadInt64Async(string address);

        /// <summary>
        /// 异步批量读取 Int64 数组
        /// </summary>
        Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length);
        #endregion


        #region Double
        /// <summary>
        /// 读取单个 double 数据信息
        /// </summary>
        OperateResult<double> ReadDouble(string address);

        /// <summary>
        /// 批量读取 double 数组
        /// </summary>
        OperateResult<double[]> ReadDouble(string address, ushort length);

        /// <summary>
        /// 异步读取单个 double
        /// </summary>
        Task<OperateResult<double>> ReadDoubleAsync(string address);

        /// <summary>
        /// 异步批量读取 double 数组
        /// </summary>
        Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length);
        #endregion


        #region Float
        /// <summary>
        /// 读取单个 float 数据信息
        /// </summary>
        OperateResult<float> ReadFloat(string address);

        /// <summary>
        /// 批量读取 float 数组
        /// </summary>
        OperateResult<float[]> ReadFloat(string address, ushort length);

        /// <summary>
        /// 异步读取单个 float
        /// </summary>
        Task<OperateResult<float>> ReadFloatAsync(string address);

        /// <summary>
        /// 异步批量读取 float 数组
        /// </summary>
        Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length);
        #endregion


        #region String
        /// <summary>
        /// 读取单个字符串（需指定长度），默认ASCII编码
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="length">字符长度</param>
        /// <returns>带有成功标识的 string 值</returns>
        OperateResult<string> ReadString(string address, ushort length);

        /// <summary>
        /// 异步读取单个字符串（需指定长度），默认ASCII编码
        /// </summary>
        Task<OperateResult<string>> ReadStringAsync(string address, ushort length);

        /// <summary>
        /// 读取单个字符串（需指定长度）
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="length">字符长度</param>
        /// <param name="encoding">编码</param>
        /// <returns>带有成功标识的 string 值</returns>
        OperateResult<string> ReadString(string address, ushort length,Encoding encoding);

        /// <summary>
        /// 异步读取单个字符串（需指定长度）
        /// <param name="address">数据地址</param>
        /// <param name="length">字符长度</param>
        /// <param name="encoding">编码</param>
        /// </summary>
        Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding);
        #endregion

        /// <summary>
        /// 写入单个的Boolean数据，返回是否成功
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">写入值</param>
        /// <returns>是否写入成功</returns>
        ReturnResult Write(string address, bool value);

        /// <summary>
        /// 批量写入Boolean数组数据，返回是否成功
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">写入值</param>
        /// <returns>是否写入成功</returns>
        ReturnResult Write(string address, bool[] value);


        /// <summary>
        /// 异步写入单个的Boolean数据，返回是否成功
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">写入值</param>
        /// <returns>是否写入成功</returns>
        Task<ReturnResult> WriteAsync(string address, bool value);

        /// <summary>
        /// 异步批量写入Boolean数组数据，返回是否成功
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">写入值</param>
        /// <returns>是否写入成功</returns>
        Task<ReturnResult> WriteAsync(string address, bool[] value);


        #region UInt32
        /// <summary>
        /// 写入单个的 UInt32 数据，返回是否成功
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">写入值</param>
        /// <returns>是否写入成功</returns>
        ReturnResult Write(string address, uint value);

        /// <summary>
        /// 批量写入 UInt32 数组数据，返回是否成功
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">写入值</param>
        /// <returns>是否写入成功</returns>
        ReturnResult Write(string address, uint[] value);

        /// <summary>
        /// 异步写入单个的 UInt32 数据，返回是否成功
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, uint value);

        /// <summary>
        /// 异步批量写入 UInt32 数组数据，返回是否成功
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, uint[] value);
        #endregion


        #region UInt16
        /// <summary>
        /// 写入单个的 UInt16 数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, ushort value);

        /// <summary>
        /// 批量写入 UInt16 数组数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, ushort[] value);

        /// <summary>
        /// 异步写入单个的 UInt16 数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, ushort value);

        /// <summary>
        /// 异步批量写入 UInt16 数组数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, ushort[] value);
        #endregion


        #region UInt64
        /// <summary>
        /// 写入单个的 UInt64 数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, ulong value);

        /// <summary>
        /// 批量写入 UInt64 数组数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, ulong[] value);

        /// <summary>
        /// 异步写入单个的 UInt64 数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, ulong value);

        /// <summary>
        /// 异步批量写入 UInt64 数组数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, ulong[] value);
        #endregion


        #region Int32
        /// <summary>
        /// 写入单个的 Int32 数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, int value);

        /// <summary>
        /// 批量写入 Int32 数组数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, int[] value);

        /// <summary>
        /// 异步写入单个的 Int32 数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, int value);

        /// <summary>
        /// 异步批量写入 Int32 数组数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, int[] value);
        #endregion


        #region Int16
        /// <summary>
        /// 写入单个的 Int16 数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, short value);

        /// <summary>
        /// 批量写入 Int16 数组数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, short[] value);

        /// <summary>
        /// 异步写入单个的 Int16 数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, short value);

        /// <summary>
        /// 异步批量写入 Int16 数组数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, short[] value);
        #endregion


        #region Int64
        /// <summary>
        /// 写入单个的 Int64 数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, long value);

        /// <summary>
        /// 批量写入 Int64 数组数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, long[] value);

        /// <summary>
        /// 异步写入单个的 Int64 数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, long value);

        /// <summary>
        /// 异步批量写入 Int64 数组数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, long[] value);
        #endregion


        #region Double
        /// <summary>
        /// 写入单个的 Double 数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, double value);

        /// <summary>
        /// 批量写入 Double 数组数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, double[] value);

        /// <summary>
        /// 异步写入单个的 Double 数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, double value);

        /// <summary>
        /// 异步批量写入 Double 数组数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, double[] value);
        #endregion


        #region Float
        /// <summary>
        /// 写入单个的 Float 数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, float value);

        /// <summary>
        /// 批量写入 Float 数组数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, float[] value);

        /// <summary>
        /// 异步写入单个的 Float 数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, float value);

        /// <summary>
        /// 异步批量写入 Float 数组数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, float[] value);
        #endregion


        #region String
        /// <summary>
        /// 写入字符串（需要指定长度）
        /// </summary>
        /// <param name="address">起始地址</param>
        /// <param name="value">写入文本</param>
        /// <returns>是否写入成功</returns>
        ReturnResult Write(string address, string value);

        /// <summary>
        /// 异步写入字符串
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, string value);
        #endregion


        #region Byte
        /// <summary>
        /// 写入单个 Byte 数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, byte value);

        /// <summary>
        /// 批量写入 Byte 数组数据，返回是否成功
        /// </summary>
        ReturnResult Write(string address, byte[] value);

        /// <summary>
        /// 异步写入单个 Byte 数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, byte value);

        /// <summary>
        /// 异步批量写入 Byte 数组数据
        /// </summary>
        Task<ReturnResult> WriteAsync(string address, byte[] value);
        #endregion

    }
}
