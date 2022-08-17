package handler

import (
	"net/http"

	"greet/internal/logic"
	"greet/internal/svc"
	"greet/internal/types"

	"github.com/zeromicro/go-zero/rest/httpx"
)

// func GreetHandler(svcCtx *svc.ServiceContext) http.HandlerFunc {
// 	return BaseHandlerFunc(svcCtx)
// 	// return func(w http.ResponseWriter, r *http.Request) {
// 	// 	var req types.Request
// 	// 	if err := httpx.Parse(r, &req); err != nil {
// 	// 		httpx.Error(w, err)
// 	// 		return
// 	// 	}
// 	// 	l := logic.NewGreetLogic(r.Context(), svcCtx)
// 	// 	resp, err := l.Greet(&req)
// 	// 	if err != nil {
// 	// 		httpx.Error(w, err)
// 	// 	} else {
// 	// 		httpx.OkJson(w, resp)
// 	// 	}
// 	// }
// }

func BaseHandlerFunc[T logic.BaseLogic](svcCtx *svc.ServiceContext, t T) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		var req types.Request
		if err := httpx.Parse(r, &req); err != nil {
			httpx.Error(w, err)
			return
		}
		//通过泛型动态调用不同结构体的Handler方法
		cc := logic.New[T]()
		cc.LogicHandler(req, w, r, svcCtx)
	}
}
