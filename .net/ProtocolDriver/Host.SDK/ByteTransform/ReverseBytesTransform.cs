using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Host.SDK.ByteTransform
{
    /// <summary>
    /// 整体字节全部反序（高字节在前）
    /// 大端顺序的字节转换类。
    /// 本类用于将 C# 的原生字节顺序（小端：低字节在前，高字节在后）
    /// 完全反转为大端顺序（高字节在前，低字节在后），用于部分 PLC、Modbus、
    /// 嵌入式设备等使用“反字节序”的场景。
    ///
    /// 本类继承自 <see cref="RegularByteTransform"/>，
    /// 并将所有多字节数据的字节顺序自动反转（等同于 DataFormat = DCBA）。
    /// </summary>
    public class ReverseBytesTransform : RegularByteTransform
    {
        /// <summary>
        /// 初始化反字节序转换对象，强制使用 DCBA（反转）的数据格式。
        /// </summary>
        public ReverseBytesTransform()
        {
            // 强制设置为 "DCBA"（完全反转）
            // 与 HslCommunication 行为保持一致
            this.DataFormat = DataFormat.DCBA;
        }

        /// <summary>
        /// 强制使用 DCBA，全反转
        /// </summary>
        public override byte[] TransformByte(byte[] buffer)
        {
            if (buffer == null) return buffer;
            byte[] tmp = (byte[])buffer.Clone();
            Array.Reverse(tmp);
            return tmp;
        }

        /// <summary>
        /// 字节序转换后调用 Format 处理
        /// </summary>
        public override byte[] FormatBytes(byte[] buffer)
        {
            return base.FormatBytes(buffer);
        }

        protected override byte[] FormatBytesInternal(byte[] buffer)
        {
            return buffer; // 不做额外处理
        }

        /// <summary>
        /// 反转两个字节的数据信息（强制反转）
        /// </summary>
        public override byte[] ByteTransDataFormat2(byte[] value, int index = 0)
        {
            if (value == null || value.Length < index + 2) return value;
            
            byte[] tmp = new byte[2];
            Array.Copy(value, index, tmp, 0, 2);
            Array.Reverse(tmp);
            return tmp;
        }

        /// <summary>
        /// 反转四字节的数据信息（强制反转）
        /// </summary>
        public override byte[] ByteTransDataFormat4(byte[] value, int index = 0)
        {
            if (value == null || value.Length < index + 4) return value;
            
            byte[] tmp = new byte[4];
            Array.Copy(value, index, tmp, 0, 4);
            Array.Reverse(tmp);
            return tmp;
        }

        /// <summary>
        /// 反转八字节的数据信息（强制反转）
        /// </summary>
        public override byte[] ByteTransDataFormat8(byte[] value, int index = 0)
        {
            if (value == null || value.Length < index + 8) return value;
            
            byte[] tmp = new byte[8];
            Array.Copy(value, index, tmp, 0, 8);
            Array.Reverse(tmp);
            return tmp;
        }

        /// <summary>
        /// 根据指定的DataFormat格式，来实例化一个新的ReverseBytesTransform对象
        /// </summary>
        public override IByteTransform CreateByDateFormat(DataFormat dataFormat)
        {
            return new ReverseBytesTransform()
            {
                DataFormat = dataFormat, // 保持原行为，即使设置了其他格式，TransformByte仍会强制反转
                IsStringReverseByteWord = this.IsStringReverseByteWord
            };
        }
    }
}
