using Consul;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceB.Consul
{
    public class ClientService: IClientService
    {
        private readonly IConfiguration _configuration;
        private readonly ConsulClient _consulClient;

        public ClientService(IConfiguration configuration)
        {
            _configuration = configuration;
            _consulClient = new ConsulClient(c =>
            {
                //consul地址
                c.Address = new Uri(_configuration["Consul:Address"]);
            });
        }

        public void GetServices()
        {
            var serviceNames = _configuration["Consul:ServiceNames"];
            if (!string.IsNullOrWhiteSpace(serviceNames))
            {
                Array.ForEach(serviceNames.Split(','), p =>
                {
                    Startup.ServiceDirectory.Add(p, null);
                    Task.Run(() =>
                    {
                    //WaitTime默认为5分钟
                    var queryOptions = new QueryOptions { WaitTime = TimeSpan.FromMinutes(10) };
                        while (true)
                        {
                            GetServices(queryOptions, p);
                        }
                    });
                });
            }
        }

        private void GetServices(QueryOptions queryOptions, string serviceName)
        {
            var res = _consulClient.Health.Service(serviceName, null, true, queryOptions).Result;

            //控制台打印一下获取服务列表的响应时间等信息
            Console.WriteLine($"{DateTime.Now}获取{serviceName}：queryOptions.WaitIndex：{queryOptions.WaitIndex}  LastIndex：{res.LastIndex}");

            //版本号不一致 说明服务列表发生了变化
            if (queryOptions.WaitIndex != res.LastIndex)
            {
                queryOptions.WaitIndex = res.LastIndex;

                //服务地址列表
                var serviceUrls = res.Response.Select(p => $"http://{p.Service.Address + ":" + p.Service.Port}").ToArray();
                if (Startup.ServiceDirectory.ContainsKey(serviceName))
                {
                    Startup.ServiceDirectory[serviceName] = serviceUrls;
                }
                else
                {
                    Startup.ServiceDirectory.Add(serviceName, serviceUrls);
                }
            }
        }
    }
}
