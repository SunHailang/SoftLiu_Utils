# 命令行：
	
	su 进入root用户
	
## 显示文件
		ls 			# 只显示可见的文件 和 文件夹
		ls -a 		# 全部显示, 包括可见文件和隐藏文件
		ls -d .*	# 只显示隐藏文件
	
## 删除文件
		rm  -rf (文件夹 / 文件 路径) 
			-r 就是向下递归，不管有多少级目录，一并删除
			-f 就是直接强行删除，不作任何提示的意思
			
## 文件目录属性
- od 查看二进制文件
		
- du 查看当前目录下所有文件夹的大小, h 标记大小K， M， G ...
			
- df 查看当前磁盘的信息，大小, -h 标记大小
		
- which 显示当前命令的所在的路径
	
## 修改文件权限
1. 查看当前用户 ： whoami
2. 修改文件权限
   - 文字设定法 chmod [who] [+|-|=] [mode]
				who r/w ...
					文件所有者： u
					文件所属组： g
					其它人： o
					所有人： a
					+： 添加权限
					-： 减少权限
					=： 覆盖原来的权限
					mode：
						r： 读
						w： 写
						x: 执行
    - 数字设定法
				- ： 没有权限
				r ： 4
				w ： 2
				x ： 1
				765： 
					7 ： -- rwx --- 文件所有者
					6 ： -- rw -- 文件所属组
					5 ： -- rx -- 其他人
					
3. 文件查找
		  按文件属性查找
			a. find [查找目录] -name "name" 按照文件的名字查找， 权限不够用 sudo
			
			b. find [查找目录] [-size [条件]] 按照文件大小查找 (-:小于 , +:大于) eg: find /home -size +10k -size +1m
			
			c. find [查找目录] -type [d/f/b/c/s/p/l] 按照文件类型查找
				d ： 目录
				f ： 普通文件
				l ： 链接符号
				b ： 块设备
				c ： 字符设备
				s ： socket文件
				p ： 管道
		  按文件内容查找
			a. grep -r "查找的内容" [查找的路径]
## 文件压缩和解压
1. gzip [文件路径]  只能压缩文件不能压缩目录
			gunzip [文件路径]  后缀名: .gz

2. bzip2 [文件路径] 
			bunzip2 [文件路径]  后缀名： .bz2

3. tar 压缩文件 (不使用z/j参数，该命令只能对文件或目录打包)
    - 参数： 
     	- c -- 创建(压缩)
     	- x -- 释放(解压缩)
     	- v -- 显示提示信息
     	- f -- 指定压缩文件的名字
     	- z -- 使用gzip的方式压缩文件(.gz)
     	- j -- 使用bzip2的方式压缩文件(.bz2)
	
	- 压缩： 
    	- tar zcvf 生成的压缩包的名字(*.tar.gz) 要压缩的文件或目录
    	- tar jcvf 生成的压缩包的名字(*.tar.bz2) 要压缩的文件或目录
			
	- 解压缩： 
    	- tar (z/j)xvf 压缩包的名字(解压到当前文件目录)
		- tar (z/j)xvf 压缩包的名字(解压到指定文件目录) -C 指定解压的路径

4. rar 压缩文件
			参数： a -- 压缩 ， x -- 解压缩
			压缩： rar a 生成的压缩文件的名字(temp) 压缩的文件或目录
			解压缩： rar x 压缩文件名 (解压缩目录)

5. zip 压缩文件
			参数： -r 递归压缩
			压缩： zip 压缩包的名字 压缩的文件或目录
			解压缩： unzip 压缩包的名字(解压到当前文件目录)
					unzip 压缩包的名字 -d 指定的解压缩目录
					
## 进程管理
1. ps 命令
	- a 所有用户
	- u 
	- x 没有终端的用户

2. kill 杀进程(通过PID)
		
3. evn 查看所有的环境变了
		
4. top 相当于任务管理器
		
## 网络相关命令
1. ifconfig 查看当前的IP信息

2. ping IP地址 -c 数量(4)

3. nslookup 域名 -- 查看当前域名对应的IP地址
		
## 用户管理
1. 创建用户  
   - sudo adduser 用户名(用户名不能用大写字母)
   - sudo useradd -s /bin/bash -g hlsun(用户组名) -d /home/hlsun -m hlsun
				   
2. 添加用户组
   - sudo groupadd 用户组名
		
3. 修改用户密码
   - sudo passwd 用户名(指定用户)
			修改当前用户的密码 -- passwd
			修改root用户的密码
			
4. 删除用户
   - sudo deluser 用户名
   - sudo deluser -r 用户名
  
5. 查看当前所有用户
   - TODO 
		