using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceB.Consul
{
    public interface IClientService
    {
        /// <summary>
        /// 获取服务列表
        /// </summary>
        void GetServices();
    }
}
