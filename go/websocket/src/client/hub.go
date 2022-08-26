// Copyright 2013 The Gorilla WebSocket Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

package client

import (
	"bytes"
	"fmt"
)

var (
	mark = []byte("&to=")
)

//集线器，用于注册、注销客户端和客户端的管理、信息广播
type Hub struct {
	onlines map[string]bool
	// 已注册的客户端.
	clients map[*Client]bool

	// 来自客户端的入站消息.
	broadcast chan []byte

	// 注册客户端.
	register chan *Client

	// 注销客户端.
	unregister chan *Client
}

func NewHub() *Hub {
	return &Hub{
		broadcast:  make(chan []byte),
		register:   make(chan *Client),
		unregister: make(chan *Client),
		clients:    make(map[*Client]bool),
		onlines:    make(map[string]bool),
	}
}

func (h *Hub) Run() {
	for {
		select {
		case client := <-h.register:
			h.clients[client] = true
			h.onlines[string(client.name)] = true
			online(h)
			// fmt.Println("register", client.name, client.pwd)
		case client := <-h.unregister:
			if _, ok := h.clients[client]; ok {
				delete(h.clients, client)
				close(client.send)
			}
			if ok := h.onlines[string(client.name)]; ok {
				delete(h.onlines, string(client.name))
			}
			online(h)
		case message := <-h.broadcast:
			secret := false
			if bytes.Contains(message, mark) {
				secret = true
			}
			for client := range h.clients {
				send := false
				sendmessage := []byte{}
				if secret {
					send, sendmessage = secretfn(message, client, h)
				} else {
					sendmessage = message
					send = true
				}
				if send {
					select {
					case client.send <- sendmessage:
					default:
						close(client.send)
						delete(h.clients, client)
					}
				}
			}
		}
	}
}
func online(h *Hub) {
	var buffer bytes.Buffer
	buffer.Write([]byte(fmt.Sprintf("在线%d人：", len(h.onlines))))
	for online := range h.onlines {
		buffer.Write([]byte(online))
		buffer.Write([]byte(","))
	}

	for client := range h.clients {
		select {
		case client.send <- buffer.Bytes():
		default:
			close(client.send)
			delete(h.clients, client)
		}
	}
}
func secretfn(message []byte, client *Client, h *Hub) (bool, []byte) {
	index := bytes.Index(message, mark)
	index1 := bytes.Index(message, []byte(":")) + 1
	sendmessage := []byte{}
	send := false

	formclientname := message[0 : index1-1]
	toclientname := message[index+len(mark):]
	messagebody := message[index1:index]
	fmt.Println(formclientname, toclientname, messagebody)
	fmt.Println(string(formclientname), string(toclientname), string(messagebody))
	if toclientname != nil {
		if bytes.Equal(toclientname, client.name) {
			var buffer bytes.Buffer
			buffer.Write(formclientname)
			buffer.Write([]byte("的私信："))
			buffer.Write([]byte(string(messagebody)))
			sendmessage = buffer.Bytes()
			fmt.Println("to", string(sendmessage))
			send = true
		}
		fmt.Println("判断是不是自己", string(formclientname), string(client.name))
		if bytes.Equal(formclientname, client.name) {
			isonline := false
			for c := range h.clients {
				if bytes.Equal(toclientname, c.name) {
					isonline = true
				}
			}
			var buffer bytes.Buffer
			buffer.Write([]byte("给"))
			buffer.Write(toclientname)
			buffer.Write([]byte("的私信："))
			buffer.Write(messagebody)
			if isonline {
				buffer.Write([]byte(" >发送成功！"))
			} else {
				buffer.Write([]byte(" >发送失败！对方离线"))
			}
			sendmessage = buffer.Bytes()
			send = true
		}
	}
	return send, sendmessage
}
