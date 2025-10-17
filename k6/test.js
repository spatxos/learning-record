import ws from "k6/ws";
import { check } from 'k6';
import http from 'k6/http';

export let options = {

    vus: 100, // 指定要同时运行的虚拟用户数量
    
    duration: '30s', // 指定测试运行的总持续时间
 };

export default function() {   
  let data = {
    "userName": "admin",
    "password": "888888"
  };

  // Using a JSON string as body
  let res = http.post("http://192.168.110.254:5155/api/v1/Account/login", JSON.stringify(data), {
    headers: { 'Content-Type': 'application/json' },
  });
  console.log(res.body)
  // const checkOutput = check(
  //   res,
  //   {
  //     'cmtmc is failed': (res) => res.body.data.menuList[0].children[0].topicKey == "cmtmc",
  //   },
  //   { myTag: "I'm a tag" }
  // );

  // if (!checkOutput) {
  //   fail('unexpected response');
  // }
    // const url = 'ws://192.168.110.244:9999/ws';
    // const params = {"QuerySystemMenuList":[{"SystemCode":"cmpps","IdList":[1,2,3,4,5,6,9,8,10,11,7,41,42,43,44,45,46]}],"RequestType":"GetChildSysMenuInfo","MD5":"SCZNMD5","Token":"SCZNToken"};

    // const response = ws.connect(url, params, function (socket) {
    //   socket.on('open', function open() {
    //     console.log('connected');
    //     socket.send(Date.now());
  
    //     socket.setInterval(function timeout() {
    //       socket.ping();
    //       console.log('Pinging every 1sec (setInterval test)');
    //     }, 1000);
    //   });
  
    //   socket.on('ping', () => console.log('PING!'));
    //   socket.on('pong', () => console.log('PONG!'));
    //   socket.on('pong', () => {
    //     // Multiple event handlers on the same event
    //     console.log('OTHER PONG!');
    //   });
  
    //   socket.on('close', () => console.log('disconnected'));
  
    //   socket.on('error', (e) => {
    //     if (e.error() != 'websocket: close sent') {
    //       console.log('An unexpected error occurred: ', e.error());
    //     }
    //   });
  
    //   socket.setTimeout(function () {
    //     console.log('2 seconds passed, closing the socket');
    //     socket.close();
    //   }, 2000);
    // });
  
    // check(response, { 'status is 101': (r) => r && r.status === 101 });
};
