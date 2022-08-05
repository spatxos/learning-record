package main

import (
	"fmt"
	"generic/src/logic"
)

func main() {
	fmt.Printf("Starting server \n")

	var ia int16A = 2
	var ib int16B = 1
	fmt.Printf("~int16: %d\n", add(ia, ib))

	fmt.Printf("int: %d\n", get[int]())
	fmt.Printf("logic.CustomLogic: %d\n", get[logic.CustomLogic]())
	fmt.Printf("logic.GreetLogic: %d\n", get[logic.GreetLogic]())

	//通过泛型动态调用不同结构体的Handler方法
	BaseHandlerFunc("vvvvvvvvvvvv", logic.CustomLogic{})
	BaseHandlerFunc("uuuuuuuuuuuuuuuu", logic.GreetLogic{})
}

func BaseHandlerFunc[T logic.BaseLogic](guid string, t T) {
	var ins T
	ins.Handler(guid)

	//// 简化写法之后，可以直接不要下面的
	//new一下结构体
	// cc := logic.New[T]()
	// //调用中转方法，这个过程类似于调用起基类里面的方法，然后基类方法自行判断调用哪一个继承类的Handler
	// cc.LogicHandler(guid)
}

//泛型使用switch
func get[T any]() T {
	var t T
	var ti interface{} = &t
	switch v := ti.(type) {
	case *int:
		*v = 1
	case *logic.CustomLogic:
		v.Handler("sss")
	case *logic.GreetLogic:
		v.Handler("qqq")
	}
	return t
}

type Point struct {
	X int
	Y int
}

type int16A = int16
type int16B = int16

//myint16包含了int16、int16A、int16B
type myint16 interface {
	~int16
}

func add[myint16 ~int16](x, y myint16) myint16 {
	return x + y
}
