using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Host.SDK
{
    /// <summary>
    /// 多字节数据格式（类似 HslCommunication）。
    /// ABCD：大端
    /// DCBA：小端
    /// BADC / CDAB：混合端序（某些 PLC 会用）
    /// </summary>
    public enum DataFormat
    {
        ABCD,
        BADC,
        CDAB,
        DCBA
    }

    /// <summary>
    /// 提供字节数组与各种基础数据类型之间相互转换的统一接口。
    /// 用于 Modbus、PLC、自定义协议等通讯场景的数据解析和封包处理。
    /// 支持数据格式（ABCD/BADC/CDAB/DCBA）、布尔位解析、字符串、数组等功能。
    /// 支持转换器的基础接口，规定了实际的数据类型和字节数组进行相互转换的方法。主要为Boolean,Byte,Int16,UInt16,Int32,UInt32, Int64,UInt64,Single,Double,String之间的变换关系
    /// </summary>
    public interface IByteTransform
    {
        /// <summary>
        /// 当前字节转换的数据格式（用于控制多字节序的顺序）。
        /// </summary>
        DataFormat DataFormat { get; set; }

        /// <summary>
        /// 字符串字节编码时是否按字（2字节）进行反转。
        /// 用于部分设备的字符串字节顺序兼容。
        /// </summary>
        bool IsStringReverseByteWord { get; set; }

        /// <summary>
        /// 反转两个字节的数据信息
        /// </summary>
        /// <param name="value">原始字节数据</param>
        /// <param name="index">起始索引，默认值为0</param>
        /// <returns>反转后的字节</returns>
        byte[] ByteTransDataFormat2(byte[] value, int index = 0);

        /// <summary>
        /// 反转多字节的数据信息
        /// </summary>
        /// <param name="value">原始字节数据</param>
        /// <param name="index">起始索引，默认值为0</param>
        /// <returns>反转后的字节</returns>
        byte[] ByteTransDataFormat4(byte[] value, int index = 0);

        /// <summary>
        /// 反转多字节的数据信息
        /// </summary>
        /// <param name="value">原始字节数据</param>
        /// <param name="index">起始索引，默认值为0</param>
        /// <returns>反转后的字节</returns>
        byte[] ByteTransDataFormat8(byte[] value, int index = 0);

        /// <summary>
        /// 根据指定的DataFormat格式，来实例化一个新的对象，除了DataFormat不同，其他都相同
        /// </summary>
        /// <param name="dataFormat">数据格式</param>
        /// <returns>新的IByteTransform对象</returns>
        IByteTransform CreateByDateFormat(DataFormat dataFormat);

        #region Bool

        /// <summary>
        /// 从缓存中提取 bool 结果，需要传入想要提取的位索引，注意：是从0开始的位索引，10表示 buffer[1] 的第二位
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">位的索引，注意：是从0开始的位索引，10则表示 buffer[1] 的第二位。</param>
        /// <returns>bool 结果</returns>
        bool TransBool(byte[] buffer, int index);

        /// <summary>
        /// 从缓存中提取 bool 数组结果，需要传入想要提取的位索引，长度为 bool 数量
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">位的起始索引，需要传入想要提取的位索引，注意：是从0开始的位索引，10则表示 buffer[1] 的第二位</param>
        /// <param name="length">读取的 bool 长度，按照位为单位，传入 10 则表示获取 10 个长度的 bool[]</param>
        /// <returns>bool 数组</returns>
        bool[] TransBool(byte[] buffer, int index, int length);


        #endregion

        #region Byte

        /// <summary>
        /// bool 变量转化为缓存数据，单 bool 转化为 0x01 或 0x00
        /// </summary>
        /// <param name="value">bool 值</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(bool value);

        /// <summary>
        /// bool 数组变量转化缓存数据，如果长度不足 8 的倍数，会自动补0
        /// </summary>
        /// <param name="value">bool 数组</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(bool[] value);

        /// <summary>
        /// byte 变量转化缓存数据
        /// </summary>
        /// <param name="value">byte 值</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(byte value);

        /// <summary>
        /// double 变量转化缓存数据，一个 double 数据占 8 个字节
        /// </summary>
        /// <param name="value">double 值</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(double value);

        /// <summary>
        /// double 数组变量转化缓存数据，长度为 n 的数组转为 8*n 个字节
        /// </summary>
        /// <param name="value">double 数组</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(double[] value);

        /// <summary>
        /// short 变量转化缓存数据，一个 short 数据占 2 个字节
        /// </summary>
        /// <param name="value">short 值</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(short value);

        /// <summary>
        /// short 数组转化缓存数据
        /// </summary>
        /// <param name="value">short 数组</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(short[] value);

        /// <summary>
        /// int 变量转化缓存数据，一个 int 数据占 4 个字节
        /// </summary>
        /// <param name="value">int 值</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(int value);

        /// <summary>
        /// int 数组转化缓存数据
        /// </summary>
        /// <param name="value">int 数组</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(int[] value);

        /// <summary>
        /// long 变量转化缓存数据，一个 long 数据占 8 个字节
        /// </summary>
        /// <param name="value">long 值</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(long value);

        /// <summary>
        /// long 数组转化缓存数据
        /// </summary>
        /// <param name="value">long 数组</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(long[] value);

        /// <summary>
        /// float 变量转化缓存数据，一个 float 数据占 4 个字节
        /// </summary>
        /// <param name="value">float 值</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(float value);

        /// <summary>
        /// float 数组转化缓存数据
        /// </summary>
        /// <param name="value">float 数组</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(float[] value);

        /// <summary>
        /// ushort 变量转化缓存数据，一个 ushort 数据占 2 个字节
        /// </summary>
        /// <param name="value">ushort 值</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(ushort value);

        /// <summary>
        /// ushort 数组转化缓存数据
        /// </summary>
        /// <param name="value">ushort 数组</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(ushort[] value);

        /// <summary>
        /// uint 变量转化缓存数据，一个 uint 数据占 4 个字节
        /// </summary>
        /// <param name="value">uint 值</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(uint value);

        /// <summary>
        /// uint 数组转化缓存数据
        /// </summary>
        /// <param name="value">uint 数组</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(uint[] value);

        /// <summary>
        /// ulong 变量转化缓存数据，一个 ulong 数据占 8 个字节
        /// </summary>
        /// <param name="value">ulong 值</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(ulong value);

        /// <summary>
        /// ulong 数组转化缓存数据
        /// </summary>
        /// <param name="value">ulong 数组</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(ulong[] value);

        /// <summary>
        /// 从缓存中提取指定索引的 byte
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">起始字节索引</param>
        /// <returns>byte 结果</returns>
        byte TransByte(byte[] buffer, int index);

        /// <summary>
        /// 使用指定编码将字符串转化为缓存数据
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(string value, Encoding encoding);

        /// <summary>
        /// 从缓存中提取 byte 数组，需要指定起始索引和长度
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">起始字节索引</param>
        /// <param name="length">读取长度</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(byte[] buffer, int index, int length);


        /// <summary>
        /// 从缓存中提取 byte 数组，需要指定起始索引和长度
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="length">转换之后的数据长度</param>
        /// <param name="encoding">字符串的编码方式</param>
        /// <returns>byte 数组</returns>
        byte[] TransByte(byte[] buffer, int length, Encoding encoding);
        #endregion

        #region Double

        /// <summary>
        /// 从缓存中提取double结果，需要指定起始的字节索引，按照字节为单位，一个double占用八个字节
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns></returns>
        double TransDouble(byte[] buffer, int index);
        /// <summary>
        /// 从缓存中提取double数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 double 数组的长度，如果传入 10 ，则表示提取 10 个连续的 double 数据，该数据共占用 80 字节。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">读取的数组长度</param>
        /// <returns></returns>
        double[] TransDouble(byte[] buffer, int index, int length);
        /// <summary>
        /// 从缓存中提取double二维数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 double 数组的行和列的长度，按照 double 为单位的个数。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">二维数组行</param>
        /// <param name="col">二维数组列</param>
        /// <returns></returns>
        double[,] TransDouble(byte[] buffer, int index, int row, int col);

        #endregion



        #region Int16

        /// <summary>
        /// 从缓存中提取short结果，需要指定起始的字节索引，按照字节为单位，一个short占用两个字节
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns></returns>
        short TransInt16(byte[] buffer, int index);
        /// <summary>
        /// 从缓存中提取short数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 short 数组的长度，如果传入 10 ，则表示提取 10 个连续的 short 数据，该数据共占用 20 字节。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">读取的数组长度</param>
        /// <returns></returns>
        short[] TransInt16(byte[] buffer, int index, int length);
        /// <summary>
        /// 从缓存中提取short二维数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 short 数组的行和列的长度，按照 short 为单位的个数。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">二维数组行</param>
        /// <param name="col">二维数组列</param>
        /// <returns></returns>
        short[,] TransInt16(byte[] buffer, int index, int row, int col);

        #endregion

        #region Int32

        /// <summary>
        /// 从缓存中提取int结果，需要指定起始的字节索引，按照字节为单位，一个int占用四个字节
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns></returns>
        int TransInt32(byte[] buffer, int index);
        /// <summary>
        /// 从缓存中提取int数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 int 数组的长度，如果传入 10 ，则表示提取 10 个连续的 int 数据，该数据共占用 40 字节。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">读取的数组长度</param>
        /// <returns></returns>
        int[] TransInt32(byte[] buffer, int index, int length);
        /// <summary>
        /// 从缓存中提取int二维数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 int 数组的行和列的长度，按照 int 为单位的个数。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">二维数组行</param>
        /// <param name="col">二维数组列</param>
        /// <returns></returns>
        int[,] TransInt32(byte[] buffer, int index, int row, int col);

        #endregion

        #region Int64

        /// <summary>
        /// 从缓存中提取long结果，需要指定起始的字节索引，按照字节为单位，一个long占用八个字节
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns></returns>
        long TransInt64(byte[] buffer, int index);
        /// <summary>
        /// 从缓存中提取long数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 long 数组的长度，如果传入 10 ，则表示提取 10 个连续的 long 数据，该数据共占用 80 字节。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">读取的数组长度</param>
        /// <returns></returns>
        long[] TransInt64(byte[] buffer, int index, int length);
        /// <summary>
        /// 从缓存中提取long二维数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 long 数组的行和列的长度，按照 long 为单位的个数。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">二维数组行</param>
        /// <param name="col">二维数组列</param>
        /// <returns></returns>
        long[,] TransInt64(byte[] buffer, int index, int row, int col);

        #endregion



        #region Single

        /// <summary>
        /// 从缓存中提取float结果，需要指定起始的字节索引，按照字节为单位，一个float占用四个字节
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns>float对象</returns>
        float TransSingle(byte[] buffer, int index);
        /// <summary>
        /// 从缓存中提取float数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 float 数组的长度，如果传入 10 ，则表示提取 10 个连续的 float 数据，该数据共占用 40 字节。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">二维数组行</param>
        /// <param name="col">二维数组列</param>
        /// <returns>float二维数组对象</returns>
        float[,] TransSingle(byte[] buffer, int index, int row, int col);

        #endregion


        #region String

        /// <summary>
        /// 从缓存中提取string结果，使用指定的编码将全部的缓存转为字符串
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="encoding">字符串的编码</param>
        /// <returns></returns>
        string TransString(byte[] buffer, Encoding encoding);
        /// <summary>
        /// 从缓存中的部分字节数组转化为string结果，使用指定的编码，指定起始的字节索引，字节长度信息。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">byte数组长度</param>
        /// <param name="encoding">字符串的编码</param>
        /// <returns></returns>
        string TransString(byte[] buffer, int index, int length, Encoding encoding);

        #endregion

        #region UInt16

        /// <summary>
        /// 从缓存中提取ushort结果，需要指定起始的字节索引，按照字节为单位，一个ushort占用两个字节
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns></returns>
        ushort TransUInt16(byte[] buffer, int index);
        /// <summary>
        /// 从缓存中提取ushort数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 ushort 数组的长度，如果传入 10 ，则表示提取 10 个连续的 ushort 数据，该数据共占用 20 字节。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">读取的数组长度</param>
        /// <returns></returns>
        ushort[] TransUInt16(byte[] buffer, int index, int length);
        /// <summary>
        /// 从缓存中提取ushort二维数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 ushort 数组的行和列的长度，按照 ushort 为单位的个数。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">二维数组行</param>
        /// <param name="col">二维数组列</param>
        /// <returns></returns>
        ushort[,] TransUInt16(byte[] buffer, int index, int row, int col);

        #endregion

        #region UInt32

        /// <summary>
        /// 从缓存中提取uint结果，需要指定起始的字节索引，按照字节为单位，一个uint占用四个字节
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns></returns>
        uint TransUInt32(byte[] buffer, int index);
        /// <summary>
        /// 从缓存中提取uint数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 uint 数组的长度，如果传入 10 ，则表示提取 10 个连续的 uint 数据，该数据共占用 40 字节。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">读取的数组长度</param>
        /// <returns></returns>
        uint[] TransUInt32(byte[] buffer, int index, int length);
        /// <summary>
        /// 从缓存中提取uint二维数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 uint 数组的行和列的长度，按照 uint 为单位的个数。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">二维数组行</param>
        /// <param name="col">二维数组列</param>
        /// <returns></returns>
        uint[,] TransUInt32(byte[] buffer, int index, int row, int col);

        #endregion

        #region UInt64

        /// <summary>
        /// 从缓存中提取ulong结果，需要指定起始的字节索引，按照字节为单位，一个ulong占用八个字节
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns></returns>
        ulong TransUInt64(byte[] buffer, int index);
        /// <summary>
        /// 从缓存中提取ulong数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 ulong 数组的长度，如果传入 10 ，则表示提取 10 个连续的 ulong 数据，该数据共占用 80 字节。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">读取的数组长度</param>
        /// <returns></returns>
        ulong[] TransUInt64(byte[] buffer, int index, int length);
        /// <summary>
        /// 从缓存中提取ulong二维数组结果，需要指定起始的字节索引，按照字节为单位，然后指定提取的 ulong 数组的行和列的长度，按照 ulong 为单位的个数。
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="row">二维数组行</param>
        /// <param name="col">二维数组列</param>
        /// <returns></returns>
        ulong[,] TransUInt64(byte[] buffer, int index, int row, int col);

        #endregion
    }
}