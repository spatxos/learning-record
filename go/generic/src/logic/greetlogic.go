package logic

import "fmt"

type GreetLogic struct {
	// guid string
}

// func NewGreetLogic(guid string) *GreetLogic {
// 	return &GreetLogic{
// 		guid: guid,
// 	}
// }
func (a GreetLogic) Handler(guid string) {
	fmt.Printf("GreetLogic: %s\n", guid)
}
