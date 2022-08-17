package logic

import (
	"context"
	"net/http"

	"greet/internal/svc"
	"greet/internal/types"

	"github.com/zeromicro/go-zero/core/logx"
	"github.com/zeromicro/go-zero/rest/httpx"
)

type CustomLogic struct {
	logx.Logger
	ctx    context.Context
	svcCtx *svc.ServiceContext
}

func NewCustomLogic(ctx context.Context, svcCtx *svc.ServiceContext) *CustomLogic {
	return &CustomLogic{
		Logger: logx.WithContext(ctx),
		ctx:    ctx,
		svcCtx: svcCtx,
	}
}

func (a CustomLogic) Handler(req types.Request, w http.ResponseWriter, r *http.Request, svcCtx *svc.ServiceContext) {
	l := NewCustomLogic(r.Context(), svcCtx)
	resp, err := l.Custom(&req)
	if err != nil {
		httpx.Error(w, err)
	} else {
		httpx.OkJson(w, resp)
	}
}

func (l *CustomLogic) Custom(req *types.Request) (resp *types.Response, err error) {
	// todo: add your logic here and delete this line
	response := new(types.Response)
	if (*req).Name == "me" {
		response.Message = "customLogic: listen to me, thank you."
	} else {
		response.Message = "customLogic: listen to you, thank me."
	}

	return response, nil
}
