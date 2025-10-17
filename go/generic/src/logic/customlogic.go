package logic

import (
	"fmt"
)

type CustomLogic struct {
	// guid string
}

// func NewCustomLogic(guid string) *CustomLogic {
// 	return &CustomLogic{
// 		guid: guid,
// 	}
// }

func (a CustomLogic) Handler(guid string) {
	fmt.Printf("CustomLogic: %s\n", guid)
}
