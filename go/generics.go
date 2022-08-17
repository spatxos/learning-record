package main

import (
	"fmt"
	"reflect"
)

func main() {
	// create a new category
	category := Category{
		ID:   1,
		Name: "Go Generics",
		Slug: "go-generics",
	}
	// create cache for blog.Category struct
	cc := New[Category]()
	// add category to cache
	cc.Set(category.Slug, category)
	fmt.Printf("cp get:%+v\n", cc.Get(category.Slug))
	// create a new post
	post := Post{
		ID: 1,
		Categories: []Category{
			{ID: 1, Name: "Go Generics", Slug: "go-generics"},
		},
		Title: "Generics in Golang structs",
		Text:  "Here go's the text",
		Slug:  "generics-in-golang-structs",
	}
	// create cache for blog.Post struct
	cp := New[Post]()
	// add post to cache
	cp.Set(post.Slug, post)

	fmt.Printf("cp get:%+v\n", cp.Get(post.Slug))
	fmt.Printf("cp get:%T\n", cp)
	gettype(Category{})
}
func gettype[T any](t T) {
	st := reflect.TypeOf(t)
	fmt.Printf("cp get1:%s\n", st) //获取类型
	vl := reflect.New(st)          //创建一个指向st类型的指针,本身为val类型
	fmt.Printf("%v,%T\n", vl, vl)  //&{ 0},reflect.Value{ 0}
	m := vl.Interface()
	m = m.(*T)
	st1 := reflect.TypeOf(m)
	fmt.Printf("cp get3:%s\n", st1) //获取类型
}

type Category struct {
	ID   int32
	Name string
	Slug string
}

type Post struct {
	ID         int32
	Categories []Category
	Title      string
	Text       string
	Slug       string
}
type cacheable interface {
	Category | Post
}
type cache[T cacheable] struct {
	data map[string]T
}

func (c *cache[T]) Set(key string, value T) {
	c.data[key] = value
}

func (c *cache[T]) Get(key string) (v T) {
	if v, ok := c.data[key]; ok {
		return v
	}

	return
}
func New[T cacheable]() *cache[T] {
	c := cache[T]{}
	c.data = make(map[string]T)

	return &c
}
