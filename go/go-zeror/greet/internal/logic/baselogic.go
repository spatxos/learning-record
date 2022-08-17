package logic

import (
	"greet/internal/svc"
	"greet/internal/types"
	"net/http"
)

type BaseLogic interface {
	any
	Handler(req types.Request, w http.ResponseWriter, r *http.Request, svcCtx *svc.ServiceContext) //每一个结构体中必须要继承一下Handler方法，例如customlogic.go和greetlogic.go中的Handler方法
}

type logic[T BaseLogic] struct {
	data T
}

func New[T BaseLogic]() logic[T] {
	c := logic[T]{}
	var ins T
	c.data = ins
	return c
}
func (a *logic[T]) LogicHandler(req types.Request, w http.ResponseWriter, r *http.Request, svcCtx *svc.ServiceContext) { //作为一个中转处理方法，最终执行结构体的Handler
	a.data.Handler(req, w, r, svcCtx)
}
