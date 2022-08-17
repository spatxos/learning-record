package main

import (
    "encoding/json"
    "net/http"
	"bytes"
	"encoding/binary"
	"net"
)

func main() {
    http.HandleFunc("/", ExampleHandler)
    if err := http.ListenAndServe(":8080", nil); err != nil {
        panic(err)
    }
}

func ExampleHandler(w http.ResponseWriter, r *http.Request) {
    w.Header().Add("Content-Type", "application/json")
	ip := GetIP(r)
    resp, _ := json.Marshal(map[string]string{
        "cip": ip,
		"cname": getCity(ip),
    })
    w.Write(resp)
}

// GetIP gets a requests IP address by reading off the forwarded-for
// header (for proxies) and falls back to use the remote address.
func GetIP(r *http.Request) string {
    forwarded := r.Header.Get("X-FORWARDED-FOR")
    if forwarded != "" {
        return forwarded
    }
    return r.RemoteAddr
}



type IpRange struct {
	Begin uint32
	End   uint32
	City  string
}

type IpData []*IpRange

func (id *IpData) Length() int {
	return len(*id)
}

func ip2Long(ip string) uint32 {
	var long uint32

	binary.Read(bytes.NewBuffer(net.ParseIP(ip).To4()), binary.BigEndian, &long)

	return long
}

func getCity(ip string, list IpData) string {
	head, tail := 0, list.Length()-1

	il := ip2Long(ip)
	if il <= 0 {
		return ""
	}

	for head <= tail {
		mid := head + (tail-head)/2

		ir := list[mid]

		if ir.Begin <= il && ir.End >= il {
			return ir.City
		} else if ir.Begin > il {
			tail = mid - 1
		} else {
			head = mid + 1
		}
	}

	return ""
}