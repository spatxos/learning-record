# 安装<全部默认安装>
1.otp_win64_23.1.exe erlang安装程序<环境,文件夹C:\Program Files\erl-23.1>  
2.rabbitmq-server-3.8.9.exe rabbitmq安装程序<C:\Program Files\RabbitMQ Server>  
# 启动
1.cmd打开并进入C:\Program Files\RabbitMQServer\rabbitmq_server-3.8.9\sbin文件夹，执行  
>rabbitmq-plugins enable rabbitmq_management  
>
2.浏览器打开http://localhost:15672/#/
>账号密码：guest/guest
>
# 说明
1.RabbitMQ.DbClient是生产者端   
2.RebbitMq.Service是消费者端  
3.erlang安装后目录中必须包含bin文件夹，否则会出现未配置环境变量的错误，但实际并不是。未包含bin文件夹可能的原因是少了c++环境，建议使用DirectX_Repair40修复，修复成功后需要完全删除erlang并重新安装即可   