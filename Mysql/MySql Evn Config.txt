
允许远程访问MySql：

Windows环境：

	一、
		# 修改 mysql(数据库名字) 数据库 下的 user表 将 host 设置成 '%'
		1. update user set host = '%' where user = 'root';
		# 刷新
		2. flush privileges;
	
	二、
		修改防火墙 添加 高级设置 -> 站内规则 ->  新建规则 ->  选择(端口)Port  设置 3306(默认端口) 为全部可访问
		
	 
Linux 远程访问数据库配置：
	如果出现错误： (我使用 python3.7.1版本)
		RuntimeError: cryptography is required for sha256_password or caching_sha2_password
		
	解决问题：
		1. setup a virtual env to install the package (recommended):
			python3 -m venv env
			source ./env/bin/activate 
			python3 -m pip install google-assistant-sdk[samples]
			
		2. Install the package to the user folder:
			python3 -m pip install --user google-assistant-sdk[samples]

		3. use sudo to install to the system folder (not recommended)
			sudo python3 -m pip install google-assistant-sdk[samples]
	
	安装： (可有可无， 我安装了)
		python3 -m pip install PyMySQL[rsa]
		pip3 install cryptography
		
			
	host = 'IP地址'
	userName='root'
	password='***'
	database='database name'


                                                                                                