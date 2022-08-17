package main

import (
	"github.com/allegro/bigcache/v3"
	"time"
	"fmt"
	"strconv"
)
func main() {
	cache, _ := bigcache.NewBigCache(bigcache.DefaultConfig(10 * time.Minute))

	cache.Set("my-unique-key", []byte("value"))

	entry, _ := cache.Get("my-unique-key")
	fmt.Println(string(entry))

	for i := 0; i <= 100; i++ {
		result := isPowerOfTwo(i)
		fmt.Printf("\r\n i:%s isPowerOfTwo:%s ", strconv.Itoa(i),result)
	}
	fmt.Printf("\r\n i:%s isPowerOfTwo:%s", strconv.Itoa(1024),isPowerOfTwo(1024))
}

func isPowerOfTwo(number int) bool {
	return (number & (number - 1)) == 0
}