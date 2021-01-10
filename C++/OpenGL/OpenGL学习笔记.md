<h1 align="center"><b><p>OpenGL学习笔记</p></b></h1>

***

**注：本文基于 glfw 和 glad 库编写**

***

# **1. OpenGL 入门**

```C++
#include <glad/glad.h> 
#include <GLFW/glfw3.h>

#include <iostream>

// 输入函数
void processInput(GLFWwindow *window)
{
    // 检测是否 按下 Esc 键
    if(glfwGetKey(window, GLFW_KEY_ESCAPE) == GLFW_PRESS)
    {
        // 关闭 OpenGL 窗体
        glfwSetWindowShouldClose(window, true);
    }
}

int main()
{
    // 初始化GLFW
    glfwInit();
    // 配置GLFW
    // @param1: 代表选项的名称
    // @param2: 接受一个整型，用来设置这个选项的值    
    glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
    glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
    glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
#ifdef __APPLE__
	// Mac OS X 系统
	glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GL_TRUE);
#endif
    // 创建一个窗口对象
    // @param1: 窗口的宽
    // @param2: 窗口的高
    // @param3: 窗口的名称
    // @param4: 暂时忽略
    // @param5: 暂时忽略
    GLFWwindow* window = glfwCreateWindow(800, 600, "LearnOpenGL", NULL, NULL);
    if (window == NULL)
    {
        std::cout << "Failed to create GLFW window" << std::endl;
        glfwTerminate();
        return -1;
    }
    // 将我们窗口的上下文设置为当前线程的主上下文
    glfwMakeContextCurrent(window);
    // 初始化 GLAD -- GLAD是用来管理OpenGL的函数指针的
    if (!gladLoadGLLoader((GLADloadproc)glfwGetProcAddress))
    {
        std::cout << "Failed to initialize GLAD" << std::endl;
        return -1;
    }
    // 注册函数 -- 每当窗口调整大小的时候调用
    glfwSetFramebufferSizeCallback(window, [](GLFWwindow* curWindow, int curWidth, int curHeight) {
		glViewport(0, 0, curWidth, curHeight);
	});
    // 渲染循环
    while(!glfwWindowShouldClose(window))
    {
        // 输入
        processInput(window);
        // 设置清空屏幕所用的颜色 -- 状态设置函数
        glClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        // 清除颜色缓冲之后，整个颜色缓冲都会被填充为glClearColor里所设置的颜色 -- 状态使用的函数
        glClear(GL_COLOR_BUFFER_BIT);
        
        glfwSwapBuffers(window);
        glfwPollEvents();    
    }
    // 渲染循环结束后我们需要正确释放/删除之前的分配的所有资源
    glfwTerminate();
    return 0;
}
```



# **2. 图形渲染管线**

**概念：**

- 把3D坐标转换成2D坐标
- 把2D坐标转换为实际的有颜色的像素

**图形渲染管线**可以被划分为几个阶段，每个阶段将会把前一个阶段的输出作为输入。所有这些阶段都是高度专门化的（它们都有一个特定的函数），并且很容易并行执行。正是由于它们具有并行执行的特性，当今大多数显卡都有成千上万的小处理核心，它们在GPU上为每一个（渲染管线）阶段运行各自的小程序，从而在图形渲染管线中快速处理你的数据。这些小程序叫做着色器(Shader)。

OpenGL着色器是用**OpenGL着色器语言(OpenGL Shading Language, GLSL)**写成的

![图形渲染管线](图形渲染管线.png)

**图元装配(Primitive Assembly)**阶段将顶点着色器输出的所有顶点作为输入（如果是GL_POINTS，那么就是一个顶点），并所有的点装配成指定图元的形状；

![标准化设备坐标](标准化设备坐标.png)

顶点缓冲对象是我们在OpenGL教程中第一个出现的OpenGL对象。就像OpenGL中的其它对象一样，这个缓冲有一个独一无二的ID，所以我们可以使用glGenBuffers函数和一个缓冲ID生成一个VBO对象：

```c++
unsigned int VBO;
glGenBuffers(1, &VBO);
```

OpenGL有很多缓冲对象类型，顶点缓冲对象的缓冲类型是GL_ARRAY_BUFFER。OpenGL允许我们同时绑定多个缓冲，只要它们是不同的缓冲类型。我们可以使用glBindBuffer函数把新创建的缓冲绑定到GL_ARRAY_BUFFER目标上：

```c++
glBindBuffer(GL_ARRAY_BUFFER, VBO);  
```

从这一刻起，我们使用的任何（在GL_ARRAY_BUFFER目标上的）缓冲调用都会用来配置当前绑定的缓冲(VBO)。然后我们可以调用glBufferData函数，它会把之前定义的顶点数据复制到缓冲的内存中：

```c++
glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
```

glBufferData是一个专门用来把用户定义的数据复制到当前绑定缓冲的函数。它的第一个参数是目标缓冲的类型：顶点缓冲对象当前绑定到GL_ARRAY_BUFFER目标上。第二个参数指定传输数据的大小(以字节为单位)；用一个简单的`sizeof`计算出顶点数据大小就行。第三个参数是我们希望发送的实际数据。

第四个参数指定了我们希望显卡如何管理给定的数据。它有三种形式：

- **GL_STATIC_DRAW** ：数据不会或几乎不会改变。
- **GL_DYNAMIC_DRAW**：数据会被改变很多。
- **GL_STREAM_DRAW** ：数据每次绘制时都会改变。

三角形的位置数据不会改变，每次渲染调用时都保持原样，所以它的使用类型最好是GL_STATIC_DRAW。如果，比如说一个缓冲中的数据将频繁被改变，那么使用的类型就是GL_DYNAMIC_DRAW或GL_STREAM_DRAW，这样就能确保显卡把数据放在能够高速写入的内存部分。



## **2.1 顶点着色器**

**顶点着色器(Vertex Shader)**是几个可编程着色器中的一个。如果我们打算做渲染的话，现代OpenGL需要我们至少设置一个顶点和一个片段着色器。

```glsl
#version 330 core
layout (location = 0) in vec3 aPos;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
}
```



## **2.2 片段着色器**

**片段着色器(Fragment Shader)**是第二个也是最后一个我们打算创建的用于渲染三角形的着色器。片段着色器所做的是计算像素最后的颜色输出。

**片段着色器**的主要目的是计算一个像素的最终颜色，这也是所有OpenGL高级效果产生的地方。通常，片段着色器包含3D场景的数据（比如光照、阴影、光的颜色等等），这些数据可以被用来计算最终像素的颜色。

```glsl
#version 330 core
out vec4 FragColor;

void main()
{
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
} 
```



# **3. 纹理 Texture**





# **4. 变换**

## **4.1 向量**

**定义：**向量有一个方向(Direction)和大小(Magnitude，也叫做强度或长度)。

- **向量与标量的运算**

  **标量(Scalar)**只是一个数字（或者说是仅有一个分量的向量）。当把一个向量加/减/乘/除一个标量，我们可以简单的把向量的每个分量分别进行该运算。

  其中的+可以是+，-，·或÷，其中·是乘号。注意－和÷运算时不能颠倒（标量-/÷向量），因为颠倒的运算是没有定义的。

- **向量取反**

  对一个向量取反(Negate)会将其方向逆转。我们在一个向量的每个分量前加负号就可以实现取反了（或者说用-1数乘该向量）

- **向量加减**

  向量的加法可以被定义为是分量的(Component-wise)相加，即将一个向量中的每一个分量加上另一个向量的对应分量。

  向量的减法等于加上第二个向量的相反向量。

- **长度**

  我们使用勾股定理(Pythagoras Theorem)来获取向量的长度(Length)/大小(Magnitude)。如果你把向量的x与y分量画出来，该向量会和x与y分量为边形成一个三角形。

  有一个特殊类型的向量叫做单位向量(Unit Vector)。单位向量有一个特别的性质——它的长度是1。

  用任意向量的每个分量除以向量的长度得到它的单位向量，这种方法叫做一个向量的标准化(Normalizing)。

- **向量相乘**

  - **点乘**

    两个向量的点乘等于它们的数乘结果乘以两个向量之间夹角的余弦值。

  - **叉乘**

    叉乘只在3D空间中有定义，它需要两个不平行向量作为输入，生成一个正交于两个输入向量的第三个向量。如果输入的两个向量也是正交的，那么叉乘之后将会产生3个互相正交的向量。

## **4.2 矩阵**

**定义：**矩阵就是一个矩形的数字、符号或表达式数组。矩阵中每一项叫做矩阵的元素(Element)。

- **矩阵的加减**

  矩阵与标量之间的加减，标量值要加到矩阵的每一个元素上。矩阵与标量的减法也相似。

  矩阵与矩阵之间的加减就是两个矩阵对应元素的加减运算，所以总体的规则和与标量运算是差不多的，只不过在相同索引下的元素才能进行运算。这也就是说**加法和减法只对同维度的矩阵才是有定义的**。

- **矩阵的数乘**

  矩阵与标量之间的乘法也是矩阵的每一个元素分别乘以该标量。

  标量就是用它的值**缩放**(Scale)矩阵的所有元素

- **矩阵相乘**

  1. 只有当左侧矩阵的列数与右侧矩阵的行数相等，两个矩阵才能相乘。
  2. 矩阵相乘不遵守交换律(Commutative)，也就是说A⋅B≠B⋅A。
  3. 结果矩阵的维度是(n, m)，n等于左侧矩阵的行数，m等于右侧矩阵的列数。

## **4.3 矩阵与向量相乘**

- **单位矩阵**

  单位矩阵是一个除了对角线以外都是0的**N×N**矩阵。
  $$
  \left[
  \matrix{
  	1\quad0\quad0\quad0\\
  	0\quad1\quad0\quad0\\
  	0\quad0\quad1\quad0\\
  	0\quad0\quad0\quad1
  }
  \right]
  $$

- **缩放**

  对一个向量进行缩放(Scaling)就是对向量的长度进行缩放，而保持它的方向不变。

  每个轴的缩放因子(Scaling Factor)都不一样，这样的缩放操作是不均匀(Non-uniform)缩放。如果每个轴的缩放因子都一样那么就叫均匀缩放(Uniform Scale)。  
  $$
  \left[
  \matrix{
  	S_1 & 0 & 0 & 0\\
  	0 & S_2 & 0 & 0\\
  	0 & 0 & S_3 & 0\\
  	0 & 0 & 0 & 1
  }
  \right] \cdot \left[
  \matrix{
  	x\\
  	y\\
  	z\\
  	1
  }
  \right] = \left[
  \matrix{
  	S1 \cdot x\\
  	S2 \cdot y\\
  	S3 \cdot z\\
  	1
  }
  \right]
  $$
  
- **位移**

  位移(Translation)是在原始向量的基础上加上另一个向量从而获得一个在不同位置的新向量的过程，从而在位移向量基础上**移动**了原始向量。
  $$
  \left[
  \matrix{
  	1 & 0 & 0 & T_x\\
  	0 & 1 & 0 & T_y\\
  	0 & 0 & 1 & T_z\\
  	0 & 0 & 0 & 1
  }
  \right] \cdot \left[
  \matrix{
  	x\\
  	y\\
  	z\\
  	1
  }
  \right] = \left[
  \matrix{
  	x + T_x\\
  	y + T_y\\
  	z + T_z\\
  	1
  }
  \right]
  $$
  
  **齐次坐标(Homogeneous Coordinates)**
  
  向量的w分量也叫**齐次坐标**。想要从齐次向量得到3D向量，我们可以把x、y和z坐标分别除以w坐标。我们通常不会注意这个问题，因为w分量通常是1.0。使用齐次坐标有几点好处：它允许我们在3D向量上进行位移（如果没有w分量我们是不能位移向量的），而且我们会用w值创建3D视觉效果。
  
  如果一个向量的齐次坐标是0，这个坐标就是**方向向量(Direction Vector)**，因为w坐标是0，这个向量就不能位移（译注：这也就是我们说的不能位移一个方向）。
  
- **旋转**

  大多数旋转函数需要用弧度制的角，但幸运的是角度制的角也可以很容易地转化为弧度制的：

  - 弧度转角度：`角度 = 弧度 * (180.0f / PI)`
  - 角度转弧度：`弧度 = 角度 * (PI / 180.0f)`

  `PI`约等于3.14159265359。

  在3D空间中旋转需要定义一个角**和**一个旋转轴(Rotation Axis)。物体会沿着给定的旋转轴旋转特定角度。如果你想要更形象化的感受，可以试试向下看着一个特定的旋转轴，同时将你的头部旋转一定角度。当2D向量在3D空间中旋转时，我们把旋转轴设为z轴（尝试想象这种情况）。

  使用三角学，给定一个角度，可以把一个向量变换为一个经过旋转的新向量。这通常是使用一系列正弦和余弦函数（一般简称sin和cos）各种巧妙的组合得到的。当然，讨论如何生成变换矩阵超出了这个教程的范围。

  旋转矩阵在3D空间中每个单位轴都有不同定义，旋转角度用θθ表示：

  **沿x轴旋转：**
  $$
  \left[
  \matrix{
  	1&0&0&0\\
  	0&\cos\theta&-\sin\theta&0\\
  	0&\sin\theta&\cos\theta&0\\
  	0&0&0&1
  }
  \right]\cdot\left[
  \matrix{
  	x\\
  	y\\
  	z\\
  	1
  }
  \right]=\left[
  \matrix{
  	x\\
  	\cos\theta\cdot y-\sin\theta\cdot z\\
  	\sin\theta\cdot y+\cos\theta\cdot z\\
  	1
  }
  \right]
  $$
  **沿y轴旋转：**
  $$
  \left[
  \matrix{
  	\cos\theta&0&\sin\theta&0\\
  	0&1&0&0\\
  	-\sin\theta&0&\cos\theta&0\\
  	0&0&0&1
  }
  \right]\cdot\left[
  \matrix{
  	x\\
  	y\\
  	z\\
  	1
  }
  \right]=\left[
  \matrix{
  	\cos\theta\cdot x+\sin\theta\cdot z\\
  	x\\
  	-\sin\theta\cdot x+\cos\theta\cdot z\\
  	1
  }
  \right]
  $$
  **沿z轴旋转：**
  $$
  \left[
  \matrix{
  	\cos\theta&0&\sin\theta&0\\
  	0&1&0&0\\
  	-\sin\theta&0&\cos\theta&0\\
  	0&0&0&1
  }
  \right]\cdot\left[
  \matrix{
  	x\\
  	y\\
  	z\\
  	1
  }
  \right]=\left[
  \matrix{
  	\cos\theta\cdot x+\sin\theta\cdot z\\
  	\sin\theta\cdot x+\cos\theta\cdot y\\
  	z\\
  	1
  }
  \right]
  $$
  **任意旋转轴：**
  $$
  \left[
  \matrix{
  	\cos\theta+R_x^2(1-\cos\theta)&R_xR_y(1-\cos\theta)-R_z\sin\theta&R_xR_z(1-\cos\theta)+R_y\sin\theta&0\\
  	R_yR_x(1-\cos\theta)+R_z\sin\theta&\cos\theta+R_y^2(1-\cos\theta)&R_yR_z(1-\cos\theta)-R_x\sin\theta&0\\
  	R_zR_x(1-\cos\theta)-R_y\sin\theta&R_ZR_y(1-\cos\theta)+R_x\sin\theta&\cos\theta+R_x^2(1-\cos\theta)&0\\
  	0&0&0&1
  }
  \right]
  $$
  避免万向节死锁的真正解决方案是使用**四元数(Quaternion)**，它不仅更安全，而且计算会更有效率。

- ## **矩阵的组合**

  使用矩阵进行变换的真正力量在于，根据矩阵之间的乘法，我们可以把多个变换组合到一个矩阵中。让我们看看我们是否能生成一个变换矩阵，让它组合多个变换。假设我们有一个顶点(x, y, z)，我们希望将其缩放2倍，然后位移(1, 2, 3)个单位。我们需要一个位移和缩放矩阵来完成这些变换。结果的变换矩阵看起来像这样：
  $$
  Trans.Scale=\left[
  \matrix{
  	1&0&0&1\\
  	0&1&0&2\\
  	0&0&1&3\\
  	0&0&0&1
  }
  \right]\cdot\left[
  \matrix{
  	2&0&0&0\\
  	0&2&0&0\\
  	0&0&2&0\\
  	0&0&0&1
  }
  \right]=\left[
  \matrix{
  	2&0&0&2\\
  	0&2&0&2\\
  	0&0&2&3\\
  	0&0&0&1
  }
  \right]
  $$
  注意，当矩阵相乘时我们先写位移再写缩放变换的。矩阵乘法是不遵守交换律的，这意味着它们的顺序很重要。当矩阵相乘时，在最右边的矩阵是第一个与向量相乘的，所以你应该从右向左读这个乘法。建议您在组合矩阵时，先进行缩放操作，然后是旋转，最后才是位移，否则它们会（消极地）互相影响。比如，如果你先位移再缩放，位移的向量也会同样被缩放（译注：比如向某方向移动2米，2米也许会被缩放成1米）！

  用最终的变换矩阵左乘我们的向量会得到以下结果：
  $$
  \left[
  \matrix{
  	2&0&0&1\\
  	0&2&0&2\\
  	0&0&2&3\\
  	0&0&0&1
  }
  \right]\cdot\left[
  \matrix{
  	x\\
  	y\\
  	z\\
  	1
  }
  \right]=\left[
  \matrix{
  	2x+1\\
  	2y+2\\
  	2z+3\\
  	1
  }
  \right]
  $$
  不错！向量先缩放2倍，然后位移了(1, 2, 3)个单位。

## **4.4 GLM**

GLM库从0.9.9版本起，默认会将矩阵类型初始化为一个零矩阵（所有元素均为0），而不是单位矩阵（对角元素为1，其它元素为0）。如果你使用的是0.9.9或0.9.9以上的版本，你需要将所有的矩阵初始化改为 `glm::mat4 mat = glm::mat4(1.0f)`。如果你想与本教程的代码保持一致，请使用低于0.9.9版本的GLM，或者改用上述代码初始化所有的矩阵。

**头文件：**

```C++
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>

unsigned int transformLoc = glGetUniformLocation(ourShader.ID, "transform");
glUniformMatrix4fv(transformLoc, 1, GL_FALSE, glm::value_ptr(trans));


glm::mat4 trans;
trans = glm::translate(trans, glm::vec3(0.5f, -0.5f, 0.0f));
trans = glm::rotate(trans, (float)glfwGetTime(), glm::vec3(0.0f, 0.0f, 1.0f));

```

***

# **5. 坐标系统**

OpenGL希望在每次顶点着色器运行后，我们可见的所有顶点都为标准化设备坐标(Normalized Device Coordinate, NDC)。也就是说，每个顶点的**x**，**y**，**z**坐标都应该在**-1.0**到**1.0**之间，超出这个坐标范围的顶点都将不可见。我们通常会自己设定一个坐标的范围，之后再在顶点着色器中将这些坐标变换为标准化设备坐标。然后将这些标准化设备坐标传入光栅器(Rasterizer)，将它们变换为屏幕上的二维坐标或像素。

将坐标变换为标准化设备坐标，接着再转化为屏幕坐标的过程通常是分步进行的，也就是类似于流水线那样子。在流水线中，物体的顶点在最终转化为屏幕坐标之前还会被变换到多个坐标系统(Coordinate System)。将物体的坐标变换到几个**过渡**坐标系(Intermediate Coordinate System)的优点在于，在这些特定的坐标系统中，一些操作或运算更加方便和容易，这一点很快就会变得很明显。对我们来说比较重要的总共有5个不同的坐标系统：

- 局部空间(Local Space，或者称为物体空间(Object Space))
- 世界空间(World Space)
- 观察空间(View Space，或者称为视觉空间(Eye Space))
- 裁剪空间(Clip Space)
- 屏幕空间(Screen Space)

这就是一个顶点在最终被转化为片段之前需要经历的所有不同状态。

## **5.1 概述**

为了将坐标从一个坐标系变换到另一个坐标系，我们需要用到几个变换矩阵，最重要的几个分别是**模型(Model)**、**观察(View)**、**投影(Projection)**三个矩阵。我们的顶点坐标起始于**局部空间(Local Space)**，在这里它称为**局部坐标(Local Coordinate)**，它在之后会变为**世界坐标(World Coordinate)**，**观察坐标(View Coordinate)**，**裁剪坐标(Clip Coordinate)**，并最后以**屏幕坐标(Screen Coordinate)**的形式结束。下面的这张图展示了整个流程以及各个变换过程做了什么：

![坐标系统转换](坐标系统转换.png)

1. 局部坐标是对象相对于局部原点的坐标，也是物体起始的坐标。
2. 下一步是将局部坐标变换为世界空间坐标，世界空间坐标是处于一个更大的空间范围的。这些坐标相对于世界的全局原点，它们会和其它物体一起相对于世界的原点进行摆放。
3. 接下来我们将世界坐标变换为观察空间坐标，使得每个坐标都是从摄像机或者说观察者的角度进行观察的。
4. 坐标到达观察空间之后，我们需要将其投影到裁剪坐标。裁剪坐标会被处理至-1.0到1.0的范围内，并判断哪些顶点将会出现在屏幕上。
5. 最后，我们将裁剪坐标变换为屏幕坐标，我们将使用一个叫做**视口变换(Viewport Transform)**的过程。视口变换将位于-1.0到1.0范围的坐标变换到由**glViewport**函数所定义的坐标范围内。最后变换出来的坐标将会送到光栅器，将其转化为片段。

## **5.2 局部空间**

局部空间是指物体所在的坐标空间，即对象最开始所在的地方。想象你在一个建模软件（比如说Blender）中创建了一个立方体。你创建的立方体的原点有可能位于(0, 0, 0)，即便它有可能最后在程序中处于完全不同的位置。甚至有可能你创建的所有模型都以(0, 0, 0)为初始位置（译注：然而它们会最终出现在世界的不同位置）。所以，你的模型的所有顶点都是在**局部**空间中：它们相对于你的物体来说都是局部的。

我们一直使用的那个箱子的顶点是被设定在-0.5到0.5的坐标范围中，(0, 0)是它的原点。这些都是局部坐标。

## **5.3 世界空间**

如果我们将我们所有的物体导入到程序当中，它们有可能会全挤在世界的原点(0, 0, 0)上，这并不是我们想要的结果。我们想为每一个物体定义一个位置，从而能在更大的世界当中放置它们。世界空间中的坐标正如其名：是指顶点相对于（游戏）世界的坐标。如果你希望将物体分散在世界上摆放（特别是非常真实的那样），这就是你希望物体变换到的空间。物体的坐标将会从局部变换到世界空间；该变换是由**模型矩阵(Model Matrix)**实现的。

模型矩阵是一种变换矩阵，它能通过对物体进行位移、缩放、旋转来将它置于它本应该在的位置或朝向。你可以将它想像为变换一个房子，你需要先将它缩小（它在局部空间中太大了），并将其位移至郊区的一个小镇，然后在y轴上往左旋转一点以搭配附近的房子。你也可以把上一节将箱子到处摆放在场景中用的那个矩阵大致看作一个模型矩阵；我们将箱子的局部坐标变换到场景/世界中的不同位置。

## **5.4 观察空间**

观察空间经常被人们称之OpenGL的**摄像机(Camera)**（所以有时也称为**摄像机空间(Camera Space)**或**视觉空间(Eye Space)**）。观察空间是将世界空间坐标转化为用户视野前方的坐标而产生的结果。因此观察空间就是从摄像机的视角所观察到的空间。而这通常是由一系列的位移和旋转的组合来完成，平移/旋转场景从而使得特定的对象被变换到摄像机的前方。这些组合在一起的变换通常存储在一个**观察矩阵(View Matrix)**里，它被用来将世界坐标变换到观察空间。在下一节中我们将深入讨论如何创建一个这样的观察矩阵来模拟一个摄像机。

## **5.5 裁剪空间**

在一个顶点着色器运行的最后，OpenGL期望所有的坐标都能落在一个特定的范围内，且任何在这个范围之外的点都应该被裁剪掉(Clipped)。被裁剪掉的坐标就会被忽略，所以剩下的坐标就将变为屏幕上可见的片段。这也就是裁剪空间(Clip Space)名字的由来。

因为将所有可见的坐标都指定在-1.0到1.0的范围内不是很直观，所以我们会指定自己的坐标集(Coordinate Set)并将它变换回标准化设备坐标系，就像OpenGL期望的那样。

为了将顶点坐标从观察变换到裁剪空间，我们需要定义一个投影矩阵(Projection Matrix)，它指定了一个范围的坐标，比如在每个维度上的-1000到1000。投影矩阵接着会将在这个指定的范围内的坐标变换为标准化设备坐标的范围(-1.0, 1.0)。所有在范围外的坐标不会被映射到在-1.0到1.0的范围之间，所以会被裁剪掉。在上面这个投影矩阵所指定的范围内，坐标(1250, 500, 750)将是不可见的，这是由于它的x坐标超出了范围，它被转化为一个大于1.0的标准化设备坐标，所以被裁剪掉了。

`如果只是图元(Primitive)，例如三角形，的一部分超出了裁剪体积(Clipping Volume)，则OpenGL会重新构建这个三角形为一个或多个三角形让其能够适合这个裁剪范围。`

由投影矩阵创建的**观察箱(Viewing Box)**被称为**平截头体(Frustum)**，每个出现在平截头体范围内的坐标都会最终出现在用户的屏幕上。将特定范围内的坐标转化到标准化设备坐标系的过程（而且它很容易被映射到2D观察空间坐标）被称之为**投影(Projection**)，因为使用投影矩阵能将3D坐标投影(Project)到很容易映射到2D的标准化设备坐标系中。

一旦所有顶点被变换到裁剪空间，最终的操作——**透视除法(Perspective Division)**将会执行，在这个过程中我们将位置向量的x，y，z分量分别除以向量的齐次w分量；透视除法是将4D裁剪空间坐标变换为3D标准化设备坐标的过程。这一步会在每一个顶点着色器运行的最后被自动执行。

在这一阶段之后，最终的坐标将会被映射到屏幕空间中（使用glViewport中的设定），并被变换成片段。

将观察坐标变换为裁剪坐标的投影矩阵可以为两种不同的形式，每种形式都定义了不同的平截头体。我们可以选择创建一个正射投影矩阵(Orthographic Projection Matrix)或一个**透视投影矩阵(Perspective Projection Matrix)**。

## **5.6 正射投影**

正射投影矩阵定义了一个类似立方体的平截头箱，它定义了一个裁剪空间，在这空间之外的顶点都会被裁剪掉。创建一个正射投影矩阵需要指定可见平截头体的宽、高和长度。在使用正射投影矩阵变换至裁剪空间之后处于这个平截头体内的所有坐标将不会被裁剪掉。它的平截头体看起来像一个容器：

![正射投影](正射投影.png)

上面的平截头体定义了可见的坐标，它由由宽、高、近(Near)平面和远(Far)平面所指定。任何出现在近平面之前或远平面之后的坐标都会被裁剪掉。正射平截头体直接将平截头体内部的所有坐标映射为标准化设备坐标，因为每个向量的w分量都没有进行改变；如果w分量等于1.0，透视除法则不会改变这个坐标。

要创建一个正射投影矩阵，我们可以使用GLM的内置函数`glm::ortho`：

```C++
glm::ortho(0.0f, 800.0f, 0.0f, 600.0f, 0.1f, 100.0f);
```

前两个参数指定了平截头体的左右坐标，第三和第四参数指定了平截头体的底部和顶部。通过这四个参数我们定义了近平面和远平面的大小，然后第五和第六个参数则定义了近平面和远平面的距离。这个投影矩阵会将处于这些x，y，z值范围内的坐标变换为标准化设备坐标。

正射投影矩阵直接将坐标映射到2D平面中，即你的屏幕，但实际上一个直接的投影矩阵会产生不真实的结果，因为这个投影没有将**透视(Perspective)**考虑进去。所以我们需要**透视投影**矩阵来解决这个问题。

**右手坐标系：**

![右手坐标系](右手坐标系.png)

## **5.7 Z缓冲**

OpenGL存储它的所有深度信息于一个Z缓冲(Z-buffer)中，也被称为深度缓冲(Depth Buffer)。GLFW会自动为你生成这样一个缓冲（就像它也有一个颜色缓冲来存储输出图像的颜色）。深度值存储在每个片段里面（作为片段的**z**值），当片段想要输出它的颜色时，OpenGL会将它的深度值和z缓冲进行比较，如果当前的片段在其它片段之后，它将会被丢弃，否则将会覆盖。这个过程称为深度测试(Depth Testing)，它是由OpenGL自动完成的。

然而，如果我们想要确定OpenGL真的执行了深度测试，首先我们要告诉OpenGL我们想要启用深度测试；它默认是关闭的。我们可以通过glEnable函数来开启深度测试。glEnable和glDisable函数允许我们启用或禁用某个OpenGL功能。这个功能会一直保持启用/禁用状态，直到另一个调用来禁用/启用它。现在我们想启用深度测试，需要开启GL_DEPTH_TEST：

```c++
glEnable(GL_DEPTH_TEST);
```

因为我们使用了深度测试，我们也想要在每次渲染迭代之前清除深度缓冲（否则前一帧的深度信息仍然保存在缓冲中）。就像清除颜色缓冲一样，我们可以通过在glClear函数中指定DEPTH_BUFFER_BIT位来清除深度缓冲：

```c++
glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
```

***

# **6. 摄像机**

## **6.1 摄像机/观察空间**

