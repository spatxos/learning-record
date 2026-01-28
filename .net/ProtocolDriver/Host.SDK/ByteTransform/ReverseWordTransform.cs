using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Host.SDK.ByteTransform
{
    /// <summary>
    /// 用于将字符串以“字(2字节)”为单位反转的转换器
    /// 例如：0x41 0x42 0x43 0x44  →  "ABCD"
    ///          ↓ ↓   ↓ ↓
    ///       0x42 0x41 0x44 0x43  →  按字反转后的编码数据
    /// </summary>
    public class ReverseWordTransform : RegularByteTransform
    {
        /// <summary>
        /// 创建一个默认 DataFormat=ABCD 的字反转转换器
        /// </summary>
        public ReverseWordTransform()
        {
            this.DataFormat = DataFormat.ABCD;
            this.IsStringReverseByteWord = true;
        }

        /// <summary>
        /// 按照“字(2字节)”为单位反转 byte 数组长度。
        /// 长度为奇数时，最后一个字节保持不动。
        /// </summary>
        private byte[] ReverseByWord(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 2) return buffer;

            byte[] result = new byte[buffer.Length];
            Array.Copy(buffer, result, buffer.Length);

            for (int i = 0; i < result.Length - 1; i += 2)
            {
                // swap: result[i]  <-> result[i+1]
                byte tmp = result[i];
                result[i] = result[i + 1];
                result[i + 1] = tmp;
            }
            return result;
        }

        #region  字符串读取（反转）
        /// <summary>
        /// 从缓存中提取 string 结果（按字反转）
        /// </summary>
        public override string TransString(byte[] buffer, Encoding encoding)
        {
            if (IsStringReverseByteWord)
            {
                buffer = ReverseByWord(buffer);
            }
            return encoding.GetString(buffer ?? Array.Empty<byte>());
        }

        /// <summary>
        /// 从缓存中提取指定范围的 string（按字反转）
        /// </summary>
        public override string TransString(byte[] buffer, int index, int length, Encoding encoding)
        {
            byte[] tmp = new byte[length];
            Array.Copy(buffer, index, tmp, 0, length);

            if (IsStringReverseByteWord)
            {
                tmp = ReverseByWord(tmp);
            }

            return encoding.GetString(tmp);
        }
        #endregion


        #region 字符串写入（反转）
        /// <summary>
        /// 字符串 → byte 数组（按字反转）
        /// </summary>
        public override byte[] TransByte(string value, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(value ?? "");

            if (IsStringReverseByteWord)
            {
                buffer = ReverseByWord(buffer);
            }
            return buffer;
        }

        /// <summary>
        /// 字符串 → 固定长度 byte 数组（按字反转）
        /// </summary>
        public override byte[] TransByte(string value, Encoding encoding, int fixedLength = -1)
        {
            byte[] raw = encoding.GetBytes(value ?? "");
            if (fixedLength > 0)
            {
                byte[] buffer = new byte[fixedLength];
                Array.Copy(raw, buffer, Math.Min(fixedLength, raw.Length));
                raw = buffer;
            }

            if (IsStringReverseByteWord)
            {
                raw = ReverseByWord(raw);
            }
            return raw;
        }
        #endregion


        /// <summary>
        /// 反转两个字节的数据信息（按字反转）
        /// </summary>
        public override byte[] ByteTransDataFormat2(byte[] value, int index = 0)
        {
            if (value == null || value.Length < index + 2) return value;
            
            byte[] tmp = new byte[2];
            Array.Copy(value, index, tmp, 0, 2);
            
            // 按字反转
            byte swap = tmp[0];
            tmp[0] = tmp[1];
            tmp[1] = swap;
            
            return tmp;
        }

        /// <summary>
        /// 反转四字节的数据信息（按字反转）
        /// </summary>
        public override byte[] ByteTransDataFormat4(byte[] value, int index = 0)
        {
            if (value == null || value.Length < index + 4) return value;
            
            byte[] tmp = new byte[4];
            Array.Copy(value, index, tmp, 0, 4);
            
            // 按字反转
            for (int i = 0; i < tmp.Length; i += 2)
            {
                if (i + 1 < tmp.Length)
                {
                    byte swap = tmp[i];
                    tmp[i] = tmp[i + 1];
                    tmp[i + 1] = swap;
                }
            }
            
            return tmp;
        }

        /// <summary>
        /// 反转八字节的数据信息（按字反转）
        /// </summary>
        public override byte[] ByteTransDataFormat8(byte[] value, int index = 0)
        {
            if (value == null || value.Length < index + 8) return value;
            
            byte[] tmp = new byte[8];
            Array.Copy(value, index, tmp, 0, 8);
            
            // 按字反转
            for (int i = 0; i < tmp.Length; i += 2)
            {
                if (i + 1 < tmp.Length)
                {
                    byte swap = tmp[i];
                    tmp[i] = tmp[i + 1];
                    tmp[i + 1] = swap;
                }
            }
            
            return tmp;
        }

        /// <summary>
        /// 根据指定的DataFormat格式，来实例化一个新的对象，除了DataFormat不同，其他都相同
        /// </summary>
        public override IByteTransform CreateByDateFormat(DataFormat dataFormat)
        {
            return new ReverseWordTransform()
            {
                DataFormat = dataFormat,
                IsStringReverseByteWord = this.IsStringReverseByteWord
            };
        }
    }
}
