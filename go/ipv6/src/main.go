package main

import (
	"fmt"
	"net"
	"net/http"
)

type helloHandler struct{}

func (h *helloHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	w.Write([]byte("Hello, world!"))
}
func main() {
	var err error
	http.Handle("/", &helloHandler{})
	//err = http.ListenAndServe(":8083", nil)
	// IPv4 或 IPv6
	ipv6str := GetLocalIP()
	fmt.Println("获取到IPV6")
	ipv6url := fmt.Sprintf("[%s]:8083", ipv6str)
	fmt.Println(ipv6url)
	err = http.ListenAndServe(ipv6url, nil)
	// 具体指定，仅 IPv6
	// err = ListenAndServe(":8083", nil)
	// 重构 ListenAndServe 函数
	if err != nil {
		fmt.Println(err)
	}
}

type tcpKeepAliveListener struct{ *net.TCPListener }

func ListenAndServe(addr string, handler http.Handler) error {
	srv := &http.Server{Addr: addr, Handler: handler}
	addr = srv.Addr
	if addr == "" {
		addr = ":http"
	}
	ln, err := net.Listen("tcp6", addr)
	// 仅指定 IPv6
	if err != nil {
		return err
	}
	return srv.Serve(tcpKeepAliveListener{ln.(*net.TCPListener)})
}

func GetLocalIP() string {
	// var ipStr []string
	netInterfaces, err := net.Interfaces()
	if err != nil {
		fmt.Println("net.Interfaces error:", err.Error())
		return ""
	}

	for i := 0; i < len(netInterfaces); i++ {
		if (netInterfaces[i].Flags & net.FlagUp) != 0 {
			addrs, _ := netInterfaces[i].Addrs()
			for _, address := range addrs {
				if ipnet, ok := address.(*net.IPNet); ok && !ipnet.IP.IsLoopback() {
					//获取IPv6
					if ipnet.IP.To16() != nil && ipnet.IP.To4() == nil {
						fmt.Println(ipnet.IP.String())
						return ipnet.IP.String()
						//ipStr = append(ipStr, ipnet.IP.String())
					}
					//获取IPv4
					// if ipnet.IP.To4() != nil {
					// 	fmt.Println(ipnet.IP.String())
					// 	ipStr = append(ipStr, ipnet.IP.String())

					// }
				}
			}
		}
	}
	return ""

}
