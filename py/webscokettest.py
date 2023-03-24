import threading
from websocket import create_connection
import time

req_count = 0
fail_count = 0
last_name = ""
thread = []
def socket():
    # while True:
    global req_count
    global fail_count
    global last_name
    req_count = req_count + 1
    ts = time.time()
    ws = create_connection("ws://101.43.82.98:8380/ws?name="+str(ts))
    # 2、获取连接状态
    print("获取连接状态：", ws.connected)
    print("获取服务器返回的连接结果", ws.recv())
    if ws.connected == True:
        initialize = '{"code":2001, "msg": "","data": {}}&to='+last_name
        ws.send(initialize)
        result_initialize = ws.recv()  # 获取返回结果
        last_name = str(ts)
        print("接收结果：", result_initialize)
        if "远程" in result_initialize:
            fail_count = fail_count+1
        # time.sleep(2)
        # ws.close()
    if "远程" in result_initialize:
        fail_count = fail_count+1
    print("失败次数：",fail_count)
    print("总请求次数：",req_count)
if __name__ == "__main__":
    for i in range(5000):
        t = threading.Thread(target=socket, args=())
        t.start()