using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Host.SDK
{
    /// <summary>
    /// 操作结果的类，只带有成功标志和错误信息
    /// </summary>
    public class ReturnResult
    {
        /// <summary>
        /// 具体的错误代码。
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// 具体的错误描述。
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 指示本次操作是否成功。
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ReturnResult CreateSuccessResult()
        {
            return new ReturnResult { IsSuccess = true, ErrorCode = 0 };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ReturnResult CreateFailedResult(string message, int errorCode = -1)
        {
            return new ReturnResult { IsSuccess = false, ErrorCode = errorCode, Message = message };
        }
    }

    /// <summary>
    /// 泛型操作结果类
    /// </summary>
    /// <typeparam name="T">结果数据类型</typeparam>
    public class OperateResult<T>
    {
        /// <summary>
        /// 具体的错误代码。
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// 具体的错误描述。
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 指示本次操作是否成功。
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 操作结果的数据内容
        /// </summary>
        public T Content { get; set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static OperateResult<T> CreateSuccessResult(T content)
        {
            return new OperateResult<T> { IsSuccess = true, ErrorCode = 0, Content = content };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static OperateResult<T> CreateFailedResult(string message, int errorCode = -1)
        {
            return new OperateResult<T> { IsSuccess = false, ErrorCode = errorCode, Message = message };
        }

        /// <summary>
        /// 转换为非泛型结果
        /// </summary>
        public ReturnResult ToOperateResult()
        {
            return new ReturnResult { IsSuccess = IsSuccess, ErrorCode = ErrorCode, Message = Message };
        }
    }
}
