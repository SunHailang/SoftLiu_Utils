

# **C++基础**

## **1.  数据结构**

1. 整型

   ```c++
   short a = 0; // 短整型， 占2字节， 表示范围 (-2^15 ~ 2^15-1)
   int b = 0; // 整型， 占4字节， 表示范围 (-2^31 ~ 2^32-1)
   long c = 0; // 长整型， 占4字节， 表示范围 (-2^31 ~ 2^32-1)
   long long d = 0; // 长长整型， 占8字节， 表示范围 (-2^63 ~ 2^63-1)
   ```

2. 实型(浮点型) ：(默认情况下，输出一个小数，会显示6位有效数字)

   ```C++
   float a = 0; // 单精度， 占4字节 (7位有效数字)
   double b = 0; // 双精度， 占8字节 (15~16位有效数字)
   ```

3. 字符型 / 字符串型

   ```C++
   #include <string>
   using namespace std;
   
   char ch = 'a'; // 占1字节
   // 转义字符 \n, \\ \t 等等...
   // 字符串表示方法：1.字符数组， 2.string(类型)
   char arr[] = "hello world!";
   string str = "hello world!";
   
   // bool 类型
   bool right = true;
   bool wrong = false;
   ```

***

## **2. 数组**

**放在一段连续的内存中**

**数组中的每一个元素的数据类型都是相同**

### **2.1 一维数组 **

1. 可以统计整个数组在内存中的长度
2. 名表示数组的首地址，也是数组第一个元素的地址

### **2.2 二维数组**

1. 数组名可以查看二维数组占用的内存空间
2. 数组名是二维数组的首地址,也是第一行的首地址,也是第一行的第一个元素的首地址
3.  获取二维数组的行数列数

**示例：**

```C++
#include <iostream>
using namespace std;

int main()
{
    // 一位数组
    int arr[5];
    int arrLen = sizeof(arr) / sizeof(arr[0]);
    for(size_t i = 0; i < arrLen; i++)
    {
        arr[i] = i;
    }
    for(size_t i = 0; i < arrLen; i++)
    {
        cout << "arr[" << i << "]" << arr[i] << endl;
    }
    cout << "arr数组的首地址：" << arr << " or " << &arr[0] << endl; 

    // 二维数组
    int arrTwo[2][3];
    size_t lenTotal = sizeof(arrTwo);
    size_t rows = sizeof(arrTwo) / sizeof(arrTwo[0]); // 行数
    size_t columns = sizeof(arrTwo[0]) / sizeof(arrTwo[0][0]); // 列数
	
    return 0;
}

```

***

## **3. 函数**

**示例：**

```C++
返回值类型 函数名(参数1, 参数2, ...)
{
	函数体;
	return 返回值;
}
```

### **3.1 函数默认参数**

**注意事项：**

- 如果某个位置已经有了默认参数，那么从这个位置往后，从左到右都必须有默认参数
- 如果函数的声明有默认参数，函数的实现就不能有默认参数(函数声明和实现只能有一个默认参数)

```C++
返回值类型 函数名(参数1 = 默认值1, 参数2 = 默认值2, ...)
{
    函数体;
    return 返回值;
}
```

### **3.2 函数占位参数**

C++中函数的形参列表里可以有占位符，用来作占位，调用函数时必须填补该位置

**示例语法：**

```C++
返回值类型 函数名 (数据类型)
{
	函数体;
    return 返回值;
}
// 占位参数也可以有默认参数
void func(int a, int)
{
    cout << "a = " << a << endl;
}
// 调用 , 第二个参数无意义，但是调用必须传入一个参数
func(10, 20);
```

### **3.3 函数重载**

**作用：**函数名可以相同，提高复用性

**函数重载满足条件：**

- 同一个作用域下
- 函数名相同
- 函数参数 **类型不同** 或者 **个数不同** 或者 **顺序不同**

**注意事项：**

- 函数的返回值不可以作为函数重载的条件
- 引用作为重载条件

**示例：**

```C++
void func(int &a)
{
    cout << "func(int &a)" << endl;
}
void func(const int &a)
{
    cout << "func(const int &a)" << endl;
}
int main()
{
    int a = 10;
    func(a); // 调用：void func(int &a); 这个函数
    
    func(10); // 调用：void func(const int &a); 这个函数 因为 const int &a = 10; 合法，   int &a = 10; 不合法
    
    return 0;
}
```

- 函数重载遇到函数的默认参数

**示例：**

```C++
void func(int a)
{
    cout << "func(int a)" << endl;
}
void func(int a, int b = 20)
{
    cout << "func(int a, int b = 20)" << endl;
}
int main()
{
    int a = 10;
    // 当函数重载遇到默认参数，就会出现歧义，报错， 尽量避免默认参数
    //func(10);
    return 0;
}
```



***

## **4. 指针**

**指针作用：** 可以通过指针间接访问内存

- 内存编号是从0开始记录的，一般用十六进制数字表示
- 可以利用指针变量保存地址

**指针定义：** 指针类型 * 变量名;

**占用内存空间：**

- 在32位操作系统下，不管指针是什么数据类型都占用4个字节空间大小
- 在64位操作系统下，不管指针是什么数据类型都占用8个字节空间大小

### **4.1 空指针**

**定义：** 指针变量指向内存中编号为0的空间

**用途：** 初始化指针变量

**注意：** 空指针指向的内存是不可以访问的

**示例：空指针**

```C++
int main()
{
    // 空指针不可以访问
	// 内存编号 0 ~ 255 为系统占用内存，不允许用户访问
	int * p = NULL;
    
    return 0;
}
```

### **4.2 野指针** 

**定义：** 指针变量指向非法的内存空间

```C++
int main()
{
	// 指针变量p指向内存编号地址为 0x11000的空间
    // 访问野指针会报错
	int * p = (int *)0x11000;
    
    return 0;
}
```

### **4.3 const 修饰指针**

1. const 修饰指针	--- 指针常量

   ```C++
   int a = 10;
   // 指针指向不可以改，指针指向的值可以改
   int * const p = &a
   ```

2. const 修饰常量    --- 常量指针

   ```C++
   int a = 10;
   // 指针指向可以改， 指针指向的值不可以改
   const int * p = &a;
   ```

3. const 既修饰指针，又修饰常量

   ```C++
   int a = 10;
   // 指针的指向和指针指向的值都不可以改
   const int * const p = &a;
   ```

### **4.4 指针和数组**

**示例：**

```c++
int main()
{
    // 定义一个数组
    int arr[10] = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
	// 定义一个(int型)指针
    int * p = arr;
    // 一次往后偏移4个字节
    p++; 
    return 0;
}
```

### **4.5 指针、数组、函数**

**示例：冒泡排序**

```C++
#include<iostream>
using namespace std;

void bubbleSort(int * arr, int length);

int main()
{
    // 声明一个数组
    int arr[10] = { 4, 2, 6, 9, 1, 2, 10, 8, 7 }
    int length = sizeof(arr) / sizeof(arr[0]);
    bubbleSort(arr, length);
}
// 冒泡排序
void bubbleSort(int * arr, int length)
{
    for(int i = 0; i < length - 1; i++)
    {
        for(int j = 0; j < length - i - 1; j++)
        {
            if(arr[j] > arr[j + 1])
            {
                int temp = arr[j];
                arr[j] = arr[j + 1];
               	arr[j + 1] = temp;
            }
        }
    }
}
```

***

## **5. 结构体**

### **5.1 概念**

- 结构体属于用户==自定义的数据类型==，允许用户存储不同的数据类型

### **5.2 结构体定义和使用**

**语法：** <u>struct 结构体名 {结构体成员表};</u>

通过结构体创建变量的方式有三种：

- struct 结构体名 变量名;
- struct 结构体名 变量名 = {成员1值, 成员2值, ...}
- 定义结构体时顺便创建变量

**示例：**

```C++
#include<iostream>
#include<string>

using namespace std;

// 自定义的数据类型，一些类型集合组成的一个类型
struct Student
{
    // 成员列表
    string name;
    int age;
    int score;
}std3; // 创建结构体变量

int main()
{
    struct Student std1;
    std1.name = "lisi";
    std1.age = 16;
    std1.score = 76;
    
    struct Student std2 = { "zhangsan", 18, 83 };
    
    std3.name = "wangwu";
    std3.age = 26;
    std3.score = 78;
    
    return 0;
}
```

**总结：**

1. 定义结构体时的关键字是struct，不可以省略
2. 创建结构体变量时，关键字struct可以省略
3. 结构体变量利用操作符 '.' 访问结构体成员

### **5.3 结构体数组**

**作用：**将自定义的结构体放入到数组中方便维护

**语法：**struct 结构体名 数组名[元素个数] = { {}, {}, {}, ... }

**示例：**

```C++
#include<iostream>
#include<string>

using namespace std;

// 自定义的数据类型，一些类型集合组成的一个类型
struct Student
{
    // 成员列表
    string name;
    int age;
    int score;
}std3; // 创建结构体变量

int main()
{
    struct Student stds[2]
    {
        { "zhangsan", 18, 78 },
        { "lisi", 16, 76 },
        { "wangwu", 19, 86 }
    };

    return 0;
}
```

### **5.4 结构体指针**

**作用：**通过指针访问结构体成员

- 利用操作符 -> 可以通过结构体指针访问结构体属性

### **5.5 结构体-结构体嵌套结构体**

**示例：**

```C++
#include<iostream>
#include<string>
#include<ctime>
using namespace std;

struct Student
{
	string name;
	int age;
	int score;
}stdInit; // 创建结构体变量

struct Teacher
{
	int id;
	string name;
	int age;
	struct Student std;
};

int main()
{
    // 生成一个随机数种子 (按照时间计算)
	srand((unsigned int)time(NULL));
    
    struct Teacher tea = { 102, "张老师", 19, { "小李", 18, 86 } };
	tea.id = 10012;
	tea.name = "王老师";
	tea.age = 18;
	tea.std.age = 5;
	tea.std.name = "小王";
	tea.std.score = 90;

	system("pause");
	return 0;
}
```

**总结：**在结构体中可以定义另一个结构体作为成员，用来解决实际问题

### **5.6 结构体做函数参数**

**作用：**将结构体作为参数向函数中传递

传递方式有两种：

- 值传递
- 地址传递(指针传递) (可以节省空间)

**const 使用场景：**使用const 修饰函数形参， 可以防止数据的误操作

***

# **案例分析**

## **1. 通讯录管理系统**

**结构体定义：**

```C++
struct People
{
    string name;
    string gender;
    int age;
    string phoneNum;
    string address;
};
struct AddressBook
{
    int peopleNum;
    struct People peopleArray[8];
};
```



***

# **C++核心编程**

## **1. 内存分区模型**

C++程序执行时，将内存大方向划分为**4个区域**

- **代码区：**存放函数体的二进制代码，由操作系统进行管理的
- **全局区：**存放全局变量和静态变量以及常量
- **栈区：**由编译器自动分配释放，存放函数的参数值、局部变量等
- **堆区：**由程序员分配和释放，若程序员不释放，程序结束时由操作系统回收

**内存四区意义：**

不同区存放的数据，赋予不同的生命周期，给我们更大的灵活编程

### **1.1 程序运行前**

> 在程序编译后，生成了exe可执行文件，**未执行该程序前**分为两个区域

**代码区：**

> 存放CPU执行的机器指令
>
> 代码区是**共享**的，共享的目的是对于频繁执行的程序，只需要在内存中有一份代码即可
>
> 代码区是**只读**的，使其只读的原因是防止程序以外的修改了它的指令

**全局区：**

> 全局变量和静态变量存放在此.
>
> 全局区还包含了常量区，字符串常量和其他常量也存放在此.
>
> ==该区域的数据在程序结束后由操作系统释放.==

### **1.2 程序运行后**

**栈区：**

> 由编译器自动分配释放，存放函数的参数值、局部变量等
>
> 注意事项：不要返回局部变量的地址，栈区开辟的数据由编译器自动释放

**堆区：**

> 由程序员分配释放，若程序员不释放，程序结束时由操作系统回收
>
> 在C++中主要利用new在堆区开辟内存

**new 基本语法：** new 数据类型;

> 在堆区开辟一块内存
>
> 利用new创建的数据， 会返回该数据对应的类型的指针
>
> 释放堆区的数据，利用关键字 delete， 数组 delete[] arr;

**示例：**

```C++
#include<iostream>
using namespace std;

int * func()
{
    int * p = new int(20);
    return p;
}

void func1()
{
    // 创建10个整型数据的数组， 在堆区
    int * arr = new int[10];
    for(int i = 0; i < 10; i++)
    {
        arr[i] = i + 100;
    }
    for(int i = 0; i < 10; i++)
    {
        cout << arr[i] << endl;
    }
    // 释放堆区的数组
    delete[] arr;
}

int main()
{
    int * p = func();
    
    cout << *p << endl;
    delete p;
    // 内存已经被释放，再次访问是非法操作，会报错
    //cout << *p << endl;
    
    system("pause");
    return 0;
}
```

***

## **2.引用**

### **2.1 引用的基本使用**

**作用：**给变量起别名

**语法：**数据类型 &别名 = 原名;

**注意事项：**

1. 引用必须要初始化.
2. 引用一旦初始化，就不可以跟改.

**示例：**

```C++
int a = 10;
int &b = a;
cout << "a = " << a << "   b = " << b << endl;
// 错误的， 引用必须要初始化
//int &c;
```

### **2.2 引用做函数参数**

**作用：**函数传参时，可以利用引用的技术让形参修饰实参.

**优点：**可以简化指针修改实参.

**示例：**

```C++
// 引用传递， 和地址传递效果是一样的， 操作形参实际就是操作实参
void func(int &a, int &b)
{
    int temp = a;
    a = b;
    b = temp;
}
```

**总结：**通过引用参数产生的效果同按地址传递是一样的，引用的语法更清楚简单.

### **2.3 引用做函数的返回值**

**作用：**引用是可以作为函数的返回值存在的

**用法：**函数调用作为左值

**注意：**==不要返回局部变量的引用==

**示例：**

```C++
// 1. 不要返回局部变量的引用
// 调用函数后，第一次打印数据是对的(因为编译器给我们做了一次数据保留)，第二次打印数据就错了(因为局部变量已经被释放，操作属于非法操作)
//int & func()
//{
//    int a = 10;
//    return a;
//}
// 2. 函数的调用可以作为左值
int & func1()
{
    // 静态变量，存放在全局区， 全局区上的数据在程序结束后被系统释放
    static int a = 10;
    return a;
}
// 常规用法：
int &ref = func1();
// 左值用法：
func1() = 100; // 如果函数的返回值是个引用，这个函数的调用可以作为左值
```

### **2.4 引用的本质**

本质：**引用的本质在C++内部实现是一个指针常量.**

**示例：**

```C++
int a = 10;
int& ref = a; // 内部自动转换，int* const ref = &a;
ref = 20; // 内部自动转换： *ref = 20；
```

### **2.5 常量引用**

**使用场景：**用来修饰形参，防止误操作.

- 引用必须引用一块合法的内存空间

**示例：**

```C++
// 加上const 编译器将代码修改 int temp = 10;  const int& ref = temp;
const int& ref = 10;

void showValue(const int& value)
{
    //value = 1000;  // 不可以修改
 	cout << "value = " << value << endle;   
}
```

***

## **3. 类和对象**

C++面向对象的三大特性：**封装** 、**继承 **、**多态**

C++认为==万事万物都皆为对象==，对象上有其属性和行为，具有相同性质的==对象==，我们可以抽象称为==类==，人属于人类，车属于车类.

### **3.1 封装**

**意义：**

- 将属性和行为作为一个整体，表现生活中的事物
- 将属性和行为加以权限控制

**语法：**

```C++
class 类名 
{
	访问权限: 属性 / 行为
};
```

类中的属性和行为都称为**成员**

**属性：** 成员属性、成员变量

**行为：** 成员方法、成员函数

访问权限有三种：

1. **public** 		公共权限 (类**内**、**外**都可以访问)
2. **protected**   保护权限 (类**内**可以访问、类**外**不能访问，但是**子类**可以访问)
3. **private**       私有权限 (只有类内部才能访问)

**struct 和 class 的区别**

在C++中 struct 和 class 唯一的**区别**就在于**默认的访问权限不同**

区别：

- struct 默认的权限是公共权限
- class 默认的权限是私有权限

**成员属性设置为私有：**

**优点1：**将所有成员属性设置为私有，可以自己控制读写权限

**优点2：**对于写权限，我们可以检测数据的有效性

**示例：**

```C++
#include<iostream>
#include<string>
using namespace std;

class Student
{
    // 默认权限是私有权限
    int m_classID;
	// 公共权限
public:
	string Name;
	int ID;

	// 保护权限
protected:
	string m_Car;

	// 私有权限
private:
	int m_passworld;

public:
	void SetName(string _name)
	{
		Name = _name;
	}

	void SetPassword(int _password)
	{
		m_passworld = _password;
	}

	void ShowStudent()
	{
		cout << "Name: " << Name << "  ID: " << ID << endl;
	}
};

struct MyStruct
{
	// 默认权限是公共权限
	int m_structID;
};
```

### **3.2 对象的初始化和清理**

* C++中的面向对象来源于生活，每个对象也都会有初始化的设置以及对象销毁前的清理数据的设置。

#### **3.2.1 构造函数和析构函数**

对象**初始化**和**清理**也是两个非常重要的安全问题

- 一个对象或者变量没有初始状态，对其使用后果是未知的
- 同样使用完一个对象或者变量，没有及时清理，也会造成一定的安全问题

C++利用了**构造函数**和**析构函数**解决上述问题，这两个函数将会被编译器自动调用，完成对象初始化和清理工作。

对象的初始化和清理工作是编译器强制要求我们做的事情，因此**如果我们不提供构造和析构，编译器会提供构造函数和析构函数的空实现**。

* **构造函数：**主要作用在于对象**创建时**为对象的成员属性赋值，构造函数由编译器自动调用，无需手动调用。

  **语法：**

  ```C++
  // 构造函数
  类名() { }
  ```

  1. 构造函数，没有返回值也不用写void
  2. 构造函数名称与类名相同
  3. 构造函数可以有参数，因此可以发生重载
  4. 程序在调用对象时候会自动调用构造函数，无须手动调用，而且只会调用一次

  

* **析构函数：**主要作用在于对象**销毁前**系统自动调用，执行一次清理工作。

  **语法：**

  ```C++
  // 析构函数
  ~类名() { }
  ```

  1. 析构函数没有返回值，也不用写void
  2. 函数名称和类名相同，在名称前加上符号 ~
  3. 析构函数不可以有参数， 因此不可以发生重载
  4. 程序在对象销毁前会自动调用析构，无须手动调用，而且只会调用一次

#### **3.2.2 构造函数的分类及调用**

两种分类方式：

* 按照参数分为：**有参构造和无参构造**
* 按照类型分为：**普通构造和拷贝构造**

三种调用方式：

1. 括号法
2. 显示法
3. 隐式法

注意事项：

* 调用默认构造函数的时候，不要加小括号 ‘()’，加括号'()' ，就不会创建一个对象了(因为编译器会认为这是一个函数声明)
* 匿名对象，特点：当前执行结束后，系统会立即回收掉匿名对象
* 不要用拷贝构造函数初始化匿名对象,编译器会认为 Student(stu2) 等价于 Student stu2; 对象声明重定义

**示例：**

```C++
class Student
{
public:
    // 构造函数
    Student() { }
    Student(int age) { }
	// 拷贝构造 使用引用的方式传递
	Student(const Student &stu) { }
    // 其他的都是普通构造
    
    // 析构函数
    ~Student() { }
}

int main()
{
    // 这样写法，编译器会认为这是一个函数声明，不会创建对象
    //Student stu1();
    Student stu2;
    // 匿名对象，特点：当前执行结束后，系统会立即回收掉匿名对象
    Student(10);
    // 不要用拷贝构造函数初始化匿名对象,编译器会认为 Student(stu2) == Student stu2; 对象声明重定义
    //Student(stu2)
    // 隐式转换法
    Student stu3 = 10; // 调用有参构造函数
    Student stu4 = stu3; // 调用拷贝构造函数
    // 对象作为值传递 或者 作为值方式返回局部变量，实际调用了一次拷贝构造函数
    
    return 0;
}
```

#### **3.2.3 构造函数调用规则**

默认情况下，C++编译器至少给一个类添加3个函数

1. 默认构造函数(无参，函数体为空)
2. 默认析构函数(无参，函数体为空)
3. 默认拷贝构造函数，对属性值进行拷贝

构造函数调用规则如下：

- 如果用户定义有参构造函数，C++不在提供默认无参构造函数，但是会提供默认拷贝构造
- 如果用户定义了拷贝构造函数，C++不会再提供其他构造函数

#### **3.2.3 浅拷贝和深拷贝**

浅拷贝：**简单的赋值拷贝操作**

* 编译器提供的拷贝构造函数会做浅拷贝操作(普通的等号(=)操作)
* 对于堆区的数据，浅拷贝带来的问题就是堆区的内存重复释放(这个问题可以使用自定义的拷贝构造函数进行深拷贝操作)

深拷贝：**在堆区重新申请空间，进行拷贝操作**

**总结：**如果属性有在堆区开辟，一定要自己提供拷贝构造函数，防止浅拷贝带来的内存重复释放的问题

#### **3.2.4 初始化列表**

**作用：**C++提供了初始化列表的语法，用来初始化属性

**语法：**

```C++
构造函数(): 属性1(值1), 属性2(值2), ... {}
```

**示例：**

```C++
class Student
{
public:
    string Name;
    int ID;
	// 初始化列表初始化属性
	Student(string _name, int _id) :Name(_name), ID(_id)
	{
		
	}
}
```

#### **3.2.5 类对象作为类成员**

C++类中的成员可以是另一个类的对象，我们称该成员为**对象成员**

* 当其他类对象作为本类成员， 构造时先构造其他类对象，在构造自身，析构的顺序与构造顺序相反

#### **3.2.6 静态成员**

静态成员就是在成员变量和成员函数前加上关键字 **static** ，称为静态成员。

静态成员分为：

1. **静态成员变量**
   * 所有对象共享同一份数据
   * 在编译阶段分配内存
   * 类内声明，类外初始化
2. **静态成员函数**
   * 所有对象共享同一个函数
   * 静态成员函数只能访问静态成员变量

静态函数两种访问方式：

* 对象访问
* 类名访问

### **3.3 C++对象模型和this指针**

在C++中，类内的成员变量和成员函数是分开存储的

只有非静态成员变量和函数才属于类的对象上

* 空对象占用的内存空间是 1个字节

  **原因：**

  1. C++编译器会给每个空对象也分配一个字节空间，是为了区分空对象占内存的位置
  2. 每个空对象也应该有一个独一无二的内存地址

**示例：**

```C++
class Student
{
    int m_A; // 非静态成员变量，属于类的对象上
    static int m_B; // 静态成员变量，不属于类对象上
    void func1(); // 非静态成员函数，不属于类对象上
    static void func2(); // 静态成员函数，不属于类的对象上
}
```

**总结：**只有非静态成员变量才属于类的对象上，其他成员都不属于类对象上

#### **3.3.1 this指针**

概念：**this指针指向被调用的成员函数所属的对象**

* this 指针是隐含每一个非静态成员函数内的一种指针
* this 指针不需要定义，直接使用即可
* this 指针的本质是指针常量， 指针的指向是不可以修改的，指针指向的值是可以修改的

this 指针的用途：

1. 当形参和成员变量同名时，可用 this 指针来区分

2. 在类的非静态成员函数中返回对象本身，可使用 return *this;

   * 一般使用 引用方式返回这样会返回当前的对象，如果采用值返回，会返回一个当前对象的拷贝对象后的返回 

     ```C++
     class Student
     {
     public:
         void ShowStudent()
         {
             // this 指针不可以修改指针的指向的
             // 本质 ： const Student * const this;
             //this = NULL;
         }
         // 值返回 会调用拷贝构造重新生成一个对象
         Student GetStudentA()
         {
             return *this;
         }
         // 引用返回 返回当前对象
         Student& GetStudentA()
         {
             return *this;
         }
     };
     ```

     

#### **3.3.2 空指针调用成员函数**

C++中空指针也是可以调用成员函数的，但是也要注意有没有用到this指针，如果用到this指针，需要加以判断保证代码的健壮性

空对象也是可以调用成员函数的：

```C++
class Student
{
public:
    int m_A;
    void ShowUI()
    {
        cout << "show UI" << endl;
    }
    void ShowThisUI()
    {
        // 在对象为NULL的时候 需要判断 this 是否为 NULL 否则会报错
        if(this == NULL) return;
        cont << "show This UI: " << this->m_A << endl;
    }
};

int main()
{
    Student stu = NULL;
    stu.ShowUI(); // 正常
    stu.ShowThisUI(); // 判断 this 是否为 NULL    
    return 0;
}
```

#### **3.3.3 const修饰成员函数**

**常函数：**

* 成员函数后加 const 后我们称这个函数为**常函数**
* 常函数内不可以修改成员属性
* 成员属性声明时加关键字 mutable 后，在常函数中就可以修改

**常对象：**

* 声明对象前加 const 称该对象为常对象
* 常对象只能调用常函数

```C++
class Student
{
public:
    int m_A;
    // m_B 特殊属性， 既可以在常函数中修改，也能在常对象中修改
    mutable int m_B;
    
    // 常函数
    void ShowStudent() const
    {
        // 在常函数不能修改成员属性， 但是在成员属性前加 mutable 就可以修改了
        //this->m_A = 10; // 报错
        this->m_B = 20;
    }    
};

int main()
{
    // 常对象, 只能调用常函数
    const Student stu;
    stu.ShowStudent();
    
    return 0;
}
```

### **3.4 友元**

在程序里，有些私有属性也想让类外的特殊的一些函数或者类进行访问，就需要用到友元的技术

友元的目的就是让一个函数或者类访问另一个类中的私有成员

友元的关键字：**friend**

友元的三种实现：

1. 全局函数做友元
2. 类做友元
3. 成员函数做友元

**示例：友元**

```C++
#include<iostream>
#include<string>
using namespace std;

class GoodGay
{
public:
	GoodGay()
    {
        this->m_build = new Build;
    }
	~GoodGay()
    {
        delete this->m_build;
		this->m_build = NULL;
    }
	void Visit()
    {
        cout << "Good Gay Visit Build: " << this->m_build->m_sittingRoom << endl;
        // GoodGay 类做 Build 的友元， GoodGay类中就可以访问Build类中的私有成员
        cout << "Good Gay Visit Build: " << this->m_build->m_bedRoom << endl;
    }
   
    
private:
	Build * m_build;
};

class GoodGay2
{
public:
    GoodGay()
    {
        this->m_build = new Build;
    }
	~GoodGay()
    {
        delete this->m_build;
		this->m_build = NULL;
    }
    void Visit()
    {
        cout << "Good Gay Visit Build: " << this->m_build->m_sittingRoom << endl;
        // GoodGay 类做 Build 的友元， GoodGay类中就可以访问Build类中的私有成员
        cout << "Good Gay Visit Build: " << this->m_build->m_bedRoom << endl;
    }
private:
	Build * m_build;
}

class Build
{
	// 友元函数， 此时 goodGay 函数就可以访问 Build 类中的私有成员了
	friend void goodGay(Build * _build);
    // 类做友元, 此时 GoodGay 类中就可以访问 Build 类中的私有成员了
    friend class GoodGay;
    // 成员函数作为友元  GoodGay2 类中 的 Visit做友元
    friend void GoodGay2::Visit();
public:
	Build(){ }
	~Build(){ }
	string m_sittingRoom;
    
private:
	string m_bedRoom;
};
// 全局函数
void goodGay(Build * _build)
{
	cout << "访问 sittingroom： " << _build->m_sittingRoom << endl;
	// 将 goodGay 作为 Build 友元函数 就可以访问 Build 的私有成员了
	cout << "访问 bedgroom： " << _build->m_bedRoom << endl;
}
```

### **3.5 运算符重载**

概念：对已有的运算符重新进行定义，赋予其另一种功能，以适应不同的数据类型

运算符重载，也可以发生运算符函数的函数重载

1. 对于内置的数据类型的表达式的运算符是不可能发生重载的
2. 不要滥用运算符重载

* 全局函数重载

  ```C++
  // 全局的
  类名 operation(运算符) (参数1, 参数2, ...) { }
  ```

* 成员函数重载

  ```C++
  class 类名
  {
  public:
      返回值 operation(运算符) (参数1, 参数2, ...)
      {
          函数体;
          return 返回值;
      }
  }
  ```

#### **3.5.1 加号运算符重载**

* 成员函数运算符重载
* 全局函数运算符重载

**作用：**实现两个自定义数据类型相加的运算

```C++
class Vector3
{
public:
	Vector3();
	Vector3(float x);
	Vector3(float x, float y);
	Vector3(float x, float y, float z);
	~Vector3();

	Vector3 operator + (Vector3 & v3);
	Vector3 operator - (Vector3 & v3);
	float Dot(Vector3 & v3);
	Vector3 Cross(Vector3 & v3);

private:
	float m_x;
	float m_y;
	float m_z;
};
Vector3::Vector3(float x, float y, float z)
{
	this->m_x = x;
	this->m_y = y;
	this->m_z = z;
}

Vector3 & Vector3::operator+(Vector3 & v3)
{
	this->m_x += v3.m_x;
    this->m_y += v3.m_y;
    this->m_z += v3.m_z;
    return *this;
}


int main()
{
    Vector3 v1 = Vector3(1,2,3);
    Vector3 v2 = Vector3(1,2,3);
    // 成员函数运算符重载
    // 本质： Vector3 v3 = v1.operator+(v2);
    Vector3 v3 = v1 + v2;
    
    return 0;
}
```

#### **3.5.2 左移运算符重载**

**作用：**可以输出自定义类型

**只能通过全局函数实现左移运算符重载**

```C++
#include<ostream>
using namespace std;

class Vector3
{
    // 全局函数 做友元
	friend ostream & operator<<(ostream & cout, Vector3 & ve3);
public:
	Vector3(float x, float y, float z)
    {
        this->m_x = x;
        this->m_y = y;
        this->m_z = z;
    }
private:
	float m_x;
	float m_y;
	float m_z;
};
// 左移运算符重载
ostream & operator<<(ostream & cout, Vector3 & ve3)
{
    cout << "X: " << ve3.m_x << ", Y:" << ve3.m_y << ", Z:" << ve3.m_z;
	return cout;
}

int main()
{
    Vector3 v3 = Vector3(1,2,3);
    cout << v3 << endl;
    return 0;
}

```

#### **3.5.3 递增运算符重载**

**作用：**通过重载递增运算符，实现自己的整型数据

1. 重载前置 ++ 运算符
2. 重载后置 ++ 运算符

```C++
#include<ostream>
using namespace std;

class MyInteger
{
	// 全局函数做友元
	friend ostream & operator<<(ostream & cout, MyInteger & num);

public:
	MyInteger() :m_num(0){ }
	MyInteger(int num) : m_num(num){ }
    
	// 前置 ++ 运算符重载， 返回的是引用
	MyInteger& operator++()
  	{
        this->m_num++;
        return *this;
	}
    
	// 后置 ++ 运算符重载， 返回的是值
	// int 代表占位参数，可以用于区分前置和后置递增
	MyInteger operator++(int)
    {
        MyInteger preNum = *this;
        this->m_num++;
        return preNum;
    }
    
	~MyInteger();
private:
	int m_num;
};

ostream & operator<<(ostream & cout, MyInteger & num)
{
	cout << num.m_num;
	return cout;
}

int main()
{
    MyInteger num = MyInteger(10);
	cout << (++num)++ << endl; // 输出 11 
	cout << num << endl; // 输出 12

	system("pause");
    return 0;
}
```

#### **3.5.4 赋值运算符重载**

C++编译器至少给一个类添加4个函数

1. 默认构造器(无参，函数体为空)
2. 默认析构函数(无参，函数体为空)
3. 默认拷贝构造函数，对属性进行值拷贝
4. 赋值运算符 operator= ，对属性进行值拷贝

如果类中有属性指向堆区，做赋值操作时也会出现深浅拷贝的问题





***