package logic

type BaseLogic interface {
	any
	Handler(guid string) //每一个结构体中必须要继承一下Handler方法，例如customlogic.go和greetlogic.go中的Handler方法
}

//// 简化写法之后，可以直接不要下面的
// //小写之后，变成类似于一个私有的类，在package外部不能使用
// //这个类的作用是将传入的T当作一个内部参数，后面直接使用var ins T就可以将类的指针拿到
// type logic[T BaseLogic] struct {
// 	data T
// }

// func New[T BaseLogic]() logic[T] {
// 	c := logic[T]{}
// 	//var ins T是基本实例化格式
// 	var ins T
// 	c.data = ins
// 	return c
// }
// func (a *logic[T]) LogicHandler(guid string) { //作为一个中转处理方法，最终执行结构体的Handler
// 	a.data.Handler(guid)
// }
