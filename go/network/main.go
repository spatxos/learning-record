package main

import (
	"fmt"
	"net"
)

func main() {
	// 1. 批量获取
	addrs, err := net.Interfaces()
	if err != nil {
		//fmt.Println("Error:", err)
		return
	}
	fmt.Println("net:", addrs)
	// baseNicPath := "/sys/class/net/"
	// cmd := exec.Command("ls", baseNicPath)
	// buf, err := cmd.Output()
	// if err != nil {
	// 	//fmt.Println("Error:", err)
	// 	return
	// }
	// output := string(buf)

	// for _, device := range strings.Split(output, "\n") {
	// 	if len(device) > 1 {
	// 		fmt.Println(device)
	// 		ethHandle, err := ethtool.NewEthtool()
	// 		if err != nil {
	// 			panic(err.Error())
	// 		}
	// 		defer ethHandle.Close()
	// 		stats, err := ethHandle.LinkState(device)
	// 		if err != nil {
	// 			panic(err.Error())
	// 		}
	// 		fmt.Printf("LinkName: %s LinkState: %d\n", device, stats)
	// 	}

	// }

}

// 3. net包可以获取到的网卡信息
// Interface represents a mapping between network interface name
// and index. It also represents network interface facility
// information.
// type Interface struct {
// 	Index        int          // positive integer that starts at one, zero is never used
// 	MTU          int          // maximum transmission unit
// 	Name         string       // e.g., "en0", "lo0", "eth0.100"
// 	HardwareAddr HardwareAddr // IEEE MAC-48, EUI-48 and EUI-64 form
// 	Flags        Flags        // e.g., FlagUp, FlagLoopback, FlagMulticast
// }
