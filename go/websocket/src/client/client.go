// Copyright 2013 The Gorilla WebSocket Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

package client

import (
	"bytes"
	"fmt"
	"log"
	"net/http"
	"time"

	"github.com/gorilla/websocket"
)

const (
	// 允许向对等方写入消息的时间.
	writeWait = 10 * time.Second

	// 允许从对等方读取下一条 pong 消息的时间.
	pongWait = 60 * time.Second

	// 在此期间向对等方发送 ping。 必须小于 pongWait.
	pingPeriod = (pongWait * 9) / 10

	// 对等方允许的最大消息大小.
	maxMessageSize = 512
)

var (
	newline = []byte{'\n'}
	space   = []byte{' '}
)

var upgrader = websocket.Upgrader{
	ReadBufferSize:  1024,
	WriteBufferSize: 1024,
	// 解决跨域问题
	CheckOrigin: func(r *http.Request) bool {
		return true
	},
} // use default options

// 客户端是 websocket 连接和集线器之间的中间人.
type Client struct {
	name []byte
	pwd  []byte

	hub *Hub

	// The websocket connection.
	conn *websocket.Conn

	// Buffered channel of outbound messages.
	send chan []byte
}

// readPump 将消息从 websocket 连接泵送到集线器。
//
// 应用程序在每个连接的 goroutine 中运行 readPump。 应用程序
// 通过执行 all 来确保连接上最多有一个读取器
// 从这个 goroutine 中读取。
func (c *Client) ReadPump() {
	defer func() {
		c.hub.unregister <- c
		c.conn.Close()
	}()
	c.conn.SetReadLimit(maxMessageSize)
	c.conn.SetReadDeadline(time.Now().Add(pongWait))
	c.conn.SetPongHandler(func(string) error { c.conn.SetReadDeadline(time.Now().Add(pongWait)); return nil })
	for {
		_, message, err := c.conn.ReadMessage()
		if err != nil {
			if websocket.IsUnexpectedCloseError(err, websocket.CloseGoingAway, websocket.CloseAbnormalClosure) {
				log.Printf("read error: %v", err)
			}
			break
		}
		form := append(c.name, []byte(":")...)
		message = bytes.TrimSpace(append(form, bytes.Replace(message, newline, space, -1)...))
		c.hub.broadcast <- message
	}
}

// writePump 将消息从集线器泵送到 websocket 连接。
//
// 为每个连接启动一个运行 writePump 的 goroutine。 这
// 应用程序通过以下方式确保一个连接最多有一个写入者
// 执行来自这个 goroutine 的所有写入。
func (c *Client) WritePump() {
	ticker := time.NewTicker(pingPeriod)
	defer func() {
		ticker.Stop()
		c.conn.Close()
	}()
	for {
		select {
		case message, ok := <-c.send:
			c.conn.SetWriteDeadline(time.Now().Add(writeWait))
			if !ok {
				// The hub closed the channel.
				c.conn.WriteMessage(websocket.CloseMessage, []byte{})
				return
			}

			w, err := c.conn.NextWriter(websocket.TextMessage)
			if err != nil {
				return
			}
			w.Write(message)

			// Add queued chat messages to the current websocket message.
			n := len(c.send)
			for i := 0; i < n; i++ {
				w.Write(newline)
				w.Write(<-c.send)
			}

			if err := w.Close(); err != nil {
				return
			}
		case <-ticker.C:
			c.conn.SetWriteDeadline(time.Now().Add(writeWait))
			if err := c.conn.WriteMessage(websocket.PingMessage, nil); err != nil {
				return
			}
		}
	}
}

// serveWs 处理来自 peer 的 websocket 请求。
func ServeWs(hub *Hub, w http.ResponseWriter, r *http.Request) {
	name := ""
	pwd := ""
	err := r.ParseForm() //func (r *Request) ParseForm() error 这个函数将解析URL中的查询字符串，并将解析结果更新到r.Form字段
	if err != nil {
		fmt.Println(err)
	}
	fmt.Println(r.Form)
	for i, v := range r.Form {
		if i == "name" {
			name = v[0]
		}
		if i == "pwd" {
			pwd = v[0]
		}
	}
	conn, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println(err)
		return
	}
	client := &Client{name: []byte(name), pwd: []byte(pwd), hub: hub, conn: conn, send: make(chan []byte, 256)}
	client.hub.register <- client

	// 允许通过执行所有工作来收集调用者引用的内存
	// 新的 goroutine。
	go client.WritePump()
	go client.ReadPump()
}
