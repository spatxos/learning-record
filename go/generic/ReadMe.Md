# 泛型
 * 泛型生命周期只在编译期，旨在为程序员生成代码，减少重复代码的编写
 * 需要go版本大于等于1.18

### 内置的泛型类型any和comparable
 * any: 表示go里面所有的内置基本类型，等价于interface{}
  ```
  type any = interface{}
  any is an alias for interface{} and is equivalent to interface{} in all ways.
  
  any 是 interface{} 的别名，在所有方面都等效于 interface{}。

  any on pkg.go.dev
  ```
 * comparable: 表示go里面所有内置的可比较类型：int、uint、float、bool、struct、指针等一切可以比较的类型
  ```
  type comparable interface{ comparable }
  comparable is an interface that is implemented by all comparable types (booleans, numbers, strings, pointers, channels, arrays of comparable types, structs whose fields are all comparable types). The comparable interface may only be used as a type parameter constraint, not as the type of a variable.

  可比较是由所有可比较类型（布尔值、数字、字符串、指针、通道、可比较类型的数组、其字段都是可比较类型的结构）实现的接口。 可比较接口只能用作类型参数约束，不能用作变量的类型。

  comparable on pkg.go.dev
  ```
### 自定义类型
 * type x interface{} ,类型之间用符号 "|" 隔开
  ```
  type Ordered interface {
    int  | float32  | string
  }
  ```
 * 符号"~",表示支持该类型的衍生类型
     1. ~后面的类型不能为接口
     2. ~后面的类型必须为基本类型
  ```
  type int16A int16
  type int16B = int16
  //myint16包含了int16、int16A、int16B
  type myint16 interface{
    ~int16
  }
  ```