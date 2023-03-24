import ws from "k6/ws";
import { check } from 'k6';
import http from 'k6/http';

export let options = {

    vus: 100, // 指定要同时运行的虚拟用户数量
    
    duration: '1000s', // 指定测试运行的总持续时间
 };

export default function() {   
  var res = http.get("https://spatxos.cn/returncity")
    // const url = 'ws://101.43.82.98:8380/ws';
    // const params = { tags: { my_tag: 'hello' } };

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
