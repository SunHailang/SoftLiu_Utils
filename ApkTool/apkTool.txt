1. Decoding 反编译为Smali 语言
	apktool d demo.apk -o demo
	d 表示decode ,反编译，与之对应的是building,编译。demo.apk 是要反编译的目标apk, -o 表示输出地址 ，如果没有 -o 参数，默认在当前文件夹。
	
2.Building 重新编译Smali 文件为 Apk
	apktool b  demo -o demo_unsigned.apk
		b,表示 building，编译，与之对应的是 decoding, demo 文件夹表示要编译的目标文件夹  -o 表示输出文件名和路径
		编译好之后的apk 是没有签名的
		
3.判断是否安装有 framework-res.apk
	apktool if framework-res.apk
	
4.安装 framework-res.apk
	apktool if framework-res.apk
	
	framework.apk 在手机上 /system/framework 目录下，复制出来即可