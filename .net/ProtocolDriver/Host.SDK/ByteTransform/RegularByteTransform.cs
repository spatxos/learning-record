using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Host.SDK.ByteTransform
{

    /// <summary>
    /// 基础字节序（C#原生 Little Endian）
    /// 默认的字节转换实现，支持可配置的 <see cref="DataFormat"/>。
    /// 用于各种通讯协议的数据读写解析。
    /// </summary>
    public class RegularByteTransform : IByteTransform
    {
        public DataFormat DataFormat { get; set; } = DataFormat.ABCD;
        public bool IsStringReverseByteWord { get; set; } = false;

        #region 辅助：字节顺序处理

        /// <summary>
        /// 对原始字节做字节序转换（Endian / Word Swap）
        /// </summary>
        public virtual byte[] TransformByte(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0) return buffer;

            switch (DataFormat)
            {
                case DataFormat.ABCD: // 默认
                    return buffer;

                case DataFormat.BADC: // Word Swap
                    return SwapByWords(buffer);

                case DataFormat.CDAB: // DWord Swap
                    return SwapByDWords(buffer);

                case DataFormat.DCBA: // 全反转
                    byte[] r = (byte[])buffer.Clone();
                    Array.Reverse(r);
                    return r;

                default:
                    return buffer;
            }
        }

        /// <summary>
        /// 格式化字节（最后返回给外部的方法）
        /// </summary>
        public virtual byte[] FormatBytes(byte[] buffer)
        {
            return FormatBytesInternal(buffer);
        }

        /// <summary>
        /// 子类可 override 用于做字符串过滤、清理、反转等
        /// </summary>
        protected virtual byte[] FormatBytesInternal(byte[] buffer)
        {
            return buffer;
        }


        #endregion

        #region Bool

        public virtual bool TransBool(byte[] buffer, int bitIndex)
        {
            int byteIndex = bitIndex / 8;
            int offset = bitIndex % 8;
            return (buffer[byteIndex] & (1 << offset)) != 0;
        }

        public virtual bool[] TransBool(byte[] buffer, int bitIndex, int length)
        {
            bool[] result = new bool[length];

            for (int i = 0; i < length; i++)
                result[i] = TransBool(buffer, bitIndex + i);

            return result;
        }

        public virtual byte[] TransByte(bool value)
        {
            return new byte[1] { (byte)(value ? 0x01 : 0x00) };
        }

        public virtual byte[] TransByte(bool[] values)
        {
            int byteLength = (values.Length + 7) / 8;
            byte[] buffer = new byte[byteLength];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    buffer[i / 8] |= (byte)(0x01 << (i % 8));
                }
            }
            return buffer;
        }

        #endregion

        #region Byte

        public virtual byte TransByte(byte[] buffer, int index)
        {
            return buffer[index];
        }

        public virtual byte[] TransByte(byte value)
        {
            return new byte[] { value };
        }

        public virtual byte[] TransByte(byte[] buffer, int index, int length)
        {
            byte[] res = new byte[length];
            Array.Copy(buffer, index, res, 0, length);
            return res;
        }

        public virtual byte[] TransByte(byte[] buffer, int length, Encoding encoding)
        {
            byte[] res = new byte[length];
            Array.Copy(buffer, res, Math.Min(length, buffer.Length));
            return res;
        }

        #endregion

        #region ByteTransform Methods

        /// <summary>
        /// 反转两个字节的数据信息
        /// </summary>
        /// <param name="value">原始字节数据</param>
        /// <param name="index">起始索引，默认值为0</param>
        /// <returns>反转后的字节</returns>
        public virtual byte[] ByteTransDataFormat2(byte[] value, int index = 0)
        {
            if (value == null || value.Length < index + 2) return value;
            
            byte[] tmp = new byte[2];
            Array.Copy(value, index, tmp, 0, 2);
            
            switch (DataFormat)
            {
                case DataFormat.DCBA:
                case DataFormat.BADC:
                    Array.Reverse(tmp);
                    break;
            }
            
            return tmp;
        }

        /// <summary>
        /// 反转多字节的数据信息
        /// </summary>
        /// <param name="value">原始字节数据</param>
        /// <param name="index">起始索引，默认值为0</param>
        /// <returns>反转后的字节</returns>
        public virtual byte[] ByteTransDataFormat4(byte[] value, int index = 0)
        {
            if (value == null || value.Length < index + 4) return value;
            
            byte[] tmp = new byte[4];
            Array.Copy(value, index, tmp, 0, 4);
            
            switch (DataFormat)
            {
                case DataFormat.DCBA:
                    Array.Reverse(tmp);
                    break;
                case DataFormat.CDAB:
                    // 按双字反转
                    return SwapByDWords(tmp);
                case DataFormat.BADC:
                    // 按字反转
                    return SwapByWords(tmp);
            }
            
            return tmp;
        }

        /// <summary>
        /// 反转多字节的数据信息
        /// </summary>
        /// <param name="value">原始字节数据</param>
        /// <param name="index">起始索引，默认值为0</param>
        /// <returns>反转后的字节</returns>
        public virtual byte[] ByteTransDataFormat8(byte[] value, int index = 0)
        {
            if (value == null || value.Length < index + 8) return value;
            
            byte[] tmp = new byte[8];
            Array.Copy(value, index, tmp, 0, 8);
            
            switch (DataFormat)
            {
                case DataFormat.DCBA:
                    Array.Reverse(tmp);
                    break;
                case DataFormat.CDAB:
                    // 按双字反转
                    for (int i = 0; i < 2; i++)
                    {
                        byte[] dword = new byte[4];
                        Array.Copy(tmp, i * 4, dword, 0, 4);
                        dword = SwapByDWords(dword);
                        Array.Copy(dword, 0, tmp, i * 4, 4);
                    }
                    break;
                case DataFormat.BADC:
                    // 按字反转
                    tmp = SwapByWords(tmp);
                    break;
            }
            
            return tmp;
        }

        /// <summary>
        /// 根据指定的DataFormat格式，来实例化一个新的对象，除了DataFormat不同，其他都相同
        /// </summary>
        /// <param name="dataFormat">数据格式</param>
        /// <returns>新的IByteTransform对象</returns>
        public virtual IByteTransform CreateByDateFormat(DataFormat dataFormat)
        {
            return new RegularByteTransform()
            {
                DataFormat = dataFormat,
                IsStringReverseByteWord = this.IsStringReverseByteWord
            };
        }

        #endregion

        #region Int16 / UInt16

        public virtual short TransInt16(byte[] buffer, int index)
        {
            byte[] tmp = new byte[2];
            Array.Copy(buffer, index, tmp, 0, 2);

            if (DataFormat == DataFormat.DCBA)
                Array.Reverse(tmp);

            return BitConverter.ToInt16(tmp, 0);
        }

        public virtual short[] TransInt16(byte[] buffer, int index, int length)
        {
            short[] res = new short[length];
            for (int i = 0; i < length; i++)
                res[i] = TransInt16(buffer, index + i * 2);
            return res;
        }

        public virtual short[,] TransInt16(byte[] buffer, int index, int row, int col)
        {
            short[,] res = new short[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    res[i, j] = TransInt16(buffer, index + (i * col + j) * 2);
                }
            }
            return res;
        }

        public virtual byte[] TransByte(short value)
            => BitConverter.GetBytes(value);

        public virtual byte[] TransByte(short[] values)
        {
            byte[] buffer = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
                Array.Copy(BitConverter.GetBytes(values[i]), 0, buffer, i * 2, 2);

            return buffer;
        }


        public virtual ushort TransUInt16(byte[] buffer, int index)
        {
            return unchecked((ushort)TransInt16(buffer, index));
        }

        public virtual ushort[] TransUInt16(byte[] buffer, int index, int length)
        {
            ushort[] res = new ushort[length];
            for (int i = 0; i < length; i++)
                res[i] = TransUInt16(buffer, index + i * 2);
            return res;
        }

        public virtual ushort[,] TransUInt16(byte[] buffer, int index, int row, int col)
        {
            ushort[,] res = new ushort[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    res[i, j] = TransUInt16(buffer, index + (i * col + j) * 2);
                }
            }
            return res;
        }

        public virtual byte[] TransByte(ushort value)
            => BitConverter.GetBytes(value);

        public virtual byte[] TransByte(ushort[] values)
        {
            byte[] buffer = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
                Array.Copy(BitConverter.GetBytes(values[i]), 0, buffer, i * 2, 2);

            return buffer;
        }

        #endregion

        #region Int32 / UInt32

        public virtual int TransInt32(byte[] buffer, int index)
        {
            byte[] tmp = new byte[4];
            Array.Copy(buffer, index, tmp, 0, 4);
            tmp = FormatBytes(tmp);
            return BitConverter.ToInt32(tmp, 0);
        }

        public virtual int[] TransInt32(byte[] buffer, int index, int length)
        {
            int[] res = new int[length];
            for (int i = 0; i < length; i++)
                res[i] = TransInt32(buffer, index + i * 4);
            return res;
        }

        public virtual int[,] TransInt32(byte[] buffer, int index, int row, int col)
        {
            int[,] res = new int[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    res[i, j] = TransInt32(buffer, index + (i * col + j) * 4);
                }
            }
            return res;
        }

        public virtual byte[] TransByte(int value)
            => FormatBytes(BitConverter.GetBytes(value));

        public virtual byte[] TransByte(int[] values)
        {
            byte[] buf = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
                Array.Copy(TransByte(values[i]), 0, buf, i * 4, 4);
            return buf;
        }

        public virtual uint TransUInt32(byte[] buffer, int index)
            => unchecked((uint)TransInt32(buffer, index));

        public virtual uint[] TransUInt32(byte[] buffer, int index, int length)
        {
            uint[] res = new uint[length];
            for (int i = 0; i < length; i++)
                res[i] = TransUInt32(buffer, index + i * 4);
            return res;
        }

        public virtual uint[,] TransUInt32(byte[] buffer, int index, int row, int col)
        {
            uint[,] res = new uint[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    res[i, j] = TransUInt32(buffer, index + (i * col + j) * 4);
                }
            }
            return res;
        }

        public virtual byte[] TransByte(uint value)
            => TransByte(unchecked((int)value));

        public virtual byte[] TransByte(uint[] values)
        {
            uint[] arr = values;
            int[] tmp = Array.ConvertAll(arr, x => (int)x);
            return TransByte(tmp);
        }

        #endregion

        #region Int64 / UInt64

        public virtual long TransInt64(byte[] buffer, int index)
        {
            byte[] tmp = new byte[8];
            Array.Copy(buffer, index, tmp, 0, 8);
            Array.Reverse(tmp);
            return BitConverter.ToInt64(tmp, 0);
        }

        public virtual long[] TransInt64(byte[] buffer, int index, int length)
        {
            long[] res = new long[length];
            for (int i = 0; i < length; i++)
                res[i] = TransInt64(buffer, index + i * 8);
            return res;
        }

        public virtual long[,] TransInt64(byte[] buffer, int index, int row, int col)
        {
            long[,] res = new long[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    res[i, j] = TransInt64(buffer, index + (i * col + j) * 8);
                }
            }
            return res;
        }

        public virtual byte[] TransByte(long value)
        {
            var b = BitConverter.GetBytes(value);
            Array.Reverse(b);
            return b;
        }

        public virtual byte[] TransByte(long[] values)
        {
            byte[] buf = new byte[values.Length * 8];
            for (int i = 0; i < values.Length; i++)
                Array.Copy(TransByte(values[i]), 0, buf, i * 8, 8);
            return buf;
        }

        public virtual ulong TransUInt64(byte[] buffer, int index)
            => unchecked((ulong)TransInt64(buffer, index));

        public virtual ulong[] TransUInt64(byte[] buffer, int index, int length)
        {
            ulong[] res = new ulong[length];
            for (int i = 0; i < length; i++)
                res[i] = TransUInt64(buffer, index + i * 8);
            return res;
        }

        public virtual ulong[,] TransUInt64(byte[] buffer, int index, int row, int col)
        {
            ulong[,] res = new ulong[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    res[i, j] = TransUInt64(buffer, index + (i * col + j) * 8);
                }
            }
            return res;
        }

        public virtual byte[] TransByte(ulong value)
            => TransByte(unchecked((long)value));

        public virtual byte[] TransByte(ulong[] values)
        {
            ulong[] arr = values;
            long[] tmp = Array.ConvertAll(arr, x => (long)x);
            return TransByte(tmp);
        }

        #endregion

        #region Float / Double

        public virtual float TransFloat(byte[] buffer, int index)
        {
            byte[] tmp = new byte[4];
            Array.Copy(buffer, index, tmp, 0, 4);
            tmp = FormatBytes(tmp);
            return BitConverter.ToSingle(tmp, 0);
        }

        public virtual float TransSingle(byte[] buffer, int index)
        {
            return TransFloat(buffer, index);
        }

        public virtual float[] TransFloat(byte[] buffer, int index, int length)
        {
            float[] res = new float[length];
            for (int i = 0; i < length; i++)
                res[i] = TransFloat(buffer, index + i * 4);
            return res;
        }

        public virtual float[,] TransSingle(byte[] buffer, int index, int row, int col)
        {
            float[,] res = new float[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    res[i, j] = TransFloat(buffer, index + (i * col + j) * 4);
                }
            }
            return res;
        }

        public virtual byte[] TransByte(float value)
            => FormatBytes(BitConverter.GetBytes(value));

        public virtual byte[] TransByte(float[] values)
        {
            byte[] buf = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
                Array.Copy(TransByte(values[i]), 0, buf, i * 4, 4);
            return buf;
        }

        public virtual double TransDouble(byte[] buffer, int index)
        {
            byte[] tmp = new byte[8];
            Array.Copy(buffer, index, tmp, 0, 8);
            Array.Reverse(tmp);
            return BitConverter.ToDouble(tmp, 0);
        }

        public virtual double[] TransDouble(byte[] buffer, int index, int length)
        {
            double[] res = new double[length];
            for (int i = 0; i < length; i++)
                res[i] = TransDouble(buffer, index + i * 8);
            return res;
        }

        public virtual double[,] TransDouble(byte[] buffer, int index, int row, int col)
        {
            double[,] res = new double[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    res[i, j] = TransDouble(buffer, index + (i * col + j) * 8);
                }
            }
            return res;
        }

        public virtual byte[] TransByte(double value)
        {
            var b = BitConverter.GetBytes(value);
            Array.Reverse(b);
            return b;
        }

        public virtual byte[] TransByte(double[] values)
        {
            byte[] buf = new byte[values.Length * 8];
            for (int i = 0; i < values.Length; i++)
                Array.Copy(TransByte(values[i]), 0, buf, i * 8, 8);
            return buf;
        }

        #endregion

        #region String

        public virtual byte[] TransByte(string value, Encoding encoding)
        {
            byte[] result = encoding.GetBytes(value);

            if (IsStringReverseByteWord)
            {
                for (int i = 0; i < result.Length; i += 2)
                {
                    if (i + 1 < result.Length)
                        (result[i], result[i + 1]) = (result[i + 1], result[i]);
                }
            }

            return result;
        }

        public virtual byte[] TransByte(string value, Encoding encoding, int fixedLength = -1)
        {
            byte[] result = encoding.GetBytes(value);

            if (IsStringReverseByteWord)
            {
                for (int i = 0; i < result.Length; i += 2)
                {
                    if (i + 1 < result.Length)
                        (result[i], result[i + 1]) = (result[i + 1], result[i]);
                }
            }

            if (fixedLength > 0)
            {
                byte[] dst = new byte[fixedLength];
                Array.Copy(result, dst, Math.Min(fixedLength, result.Length));
                return dst;
            }

            return result;
        }

        public virtual string TransString(byte[] buffer, Encoding encoding)
        {
            byte[] tmp = new byte[buffer.Length];
            Array.Copy(buffer, tmp, buffer.Length);

            if (IsStringReverseByteWord)
            {
                for (int i = 0; i < tmp.Length; i += 2)
                {
                    if (i + 1 < tmp.Length)
                        (tmp[i], tmp[i + 1]) = (tmp[i + 1], tmp[i]);
                }
            }

            return encoding.GetString(tmp).TrimEnd('\0');
        }

        public virtual string TransString(byte[] buffer, int index, int length, Encoding encoding)
        {
            byte[] tmp = new byte[length];
            Array.Copy(buffer, index, tmp, 0, length);

            if (IsStringReverseByteWord)
            {
                for (int i = 0; i < tmp.Length; i += 2)
                {
                    if (i + 1 < tmp.Length)
                        (tmp[i], tmp[i + 1]) = (tmp[i + 1], tmp[i]);
                }
            }

            return encoding.GetString(tmp).TrimEnd('\0');
        }

        #endregion

        // =============================
        // 工具方法：交换字节
        // =============================

        protected byte[] SwapByWords(byte[] buffer)
        {
            byte[] result = new byte[buffer.Length];
            for (int i = 0; i < buffer.Length; i += 2)
            {
                if (i + 1 < buffer.Length)
                {
                    result[i] = buffer[i + 1];
                    result[i + 1] = buffer[i];
                }
            }
            return result;
        }

        protected byte[] SwapByDWords(byte[] buffer)
        {
            byte[] result = new byte[buffer.Length];
            for (int i = 0; i < buffer.Length; i += 4)
            {
                if (i + 3 < buffer.Length)
                {
                    result[i] = buffer[i + 3];
                    result[i + 1] = buffer[i + 2];
                    result[i + 2] = buffer[i + 1];
                    result[i + 3] = buffer[i];
                }
            }
            return result;
        }
    }
}
