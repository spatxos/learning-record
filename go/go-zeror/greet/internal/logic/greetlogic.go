package logic

import (
	"context"
	"net/http"

	"greet/internal/svc"
	"greet/internal/types"

	"github.com/zeromicro/go-zero/core/logx"
	"github.com/zeromicro/go-zero/rest/httpx"
)

type GreetLogic struct {
	logx.Logger
	ctx    context.Context
	svcCtx *svc.ServiceContext
}

func NewGreetLogic(ctx context.Context, svcCtx *svc.ServiceContext) *GreetLogic {
	return &GreetLogic{
		Logger: logx.WithContext(ctx),
		ctx:    ctx,
		svcCtx: svcCtx,
	}
}
func (a GreetLogic) Handler(req types.Request, w http.ResponseWriter, r *http.Request, svcCtx *svc.ServiceContext) {
	l := NewGreetLogic(r.Context(), svcCtx)
	resp, err := l.Greet(&req)
	if err != nil {
		httpx.Error(w, err)
	} else {
		httpx.OkJson(w, resp)
	}
}

func (l *GreetLogic) Greet(req *types.Request) (resp *types.Response, err error) {
	// todo: add your logic here and delete this line
	response := new(types.Response)
	if (*req).Name == "me" {
		response.Message = "greetLogic: listen to me, thank you."
	} else {
		response.Message = "greetLogic: listen to you, thank me."
	}

	return response, nil
}
