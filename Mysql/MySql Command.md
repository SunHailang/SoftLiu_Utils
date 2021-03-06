# -*- coding: utf-8 -*-

以管理员身份运行cmd
启动服务：
    net start 数据库的服务名（不分大小写） ， 例： net start mysql80
停止服务：
    net stop 数据库的服务名（不分大小写） ， 例： net stop mysql80

登录：mysql -u root -p    密码：hlsun123 / hlsun      退出： quit 或者 exit
查看版本号：select version();     （连接后执行）
查看当前时间： select now();
远程连接： mysql -h ip地址 -u 用户名 -p   密码******

数据库操作：


```mysql
1. 创建数据库
	create database 数据库名 charset=utf8;
        
2. 删除数据库
    drop database 数据库名;

3. 切换数据库
    use 数据库名;

4. 查看使用的当前数据库
    select database();

5. 显示所有数据库
    show databases;
	
6. 导出表到本地文件
	select * from 表名 into outfile "文件路径";
```


​		
常用数据类型：
​	数值：
​		 类型			 		大小	  	范围（有符号）					范围（无符号）				  用途
​		TINYINT				1 byte			(-128，127)						(0，255)					小整数值
​		SMALLINT			2 bytes		(-32 768，32 767)				(0，65 535)				大整数值
​		MEDIUMINT		3 bytes	(-8 388 608，8 388 607)			(0，16 777 215)			大整数值
​		INT或INTEGER	4 bytes	(-2 147 483 648，2 147 483 647)	(0，4 294 967 295)		大整数值
​		BIGINT			8 bytes	(-9,223,372,036,854,775,808，9 223 372 036 854 775 807)	(0，18 446 744 073 709 551 615)	极大整数值
​		FLOAT			4 bytes	(-3.402 823 466 E+38，-1.175 494 351 E-38)，0，(1.175 494 351 E-38，3.402 823 466 351 E+38)	0，(1.175 494 351 E-38，3.402 823 466 E+38)	单精度
​		浮点数值
​		DOUBLE			8 bytes	(-1.797 693 134 862 315 7 E+308，-2.225 073 858 507 201 4 E-308)，0，(2.225 073 858 507 201 4 E-308，1.797 693 134 862 315 7 E+308)	0，(2.225 073 858 507 201 4 E-308，1.797 693 134 862 315 7 E+308)	双精度
​		浮点数值
​		DECIMAL			对DECIMAL(M,D) ，如果M>D，为M+2否则为D+2	依赖于M和D的值
​		
​	日期：
​		类型	 		大小( bytes)	范围	格式	用途
​		DATE		3	1000-01-01/9999-12-31	YYYY-MM-DD	日期值
​		TIME		3	'-838:59:59'/'838:59:59'	HH:MM:SS	时间值或持续时间
​		YEAR		1	1901/2155	YYYY	年份值
​		DATETIME	8	1000-01-01 00:00:00/9999-12-31 23:59:59	YYYY-MM-DD HH:MM:SS	混合日期和时间值
​		TIMESTAMP	4	1970-01-01 00:00:00/2038结束时间是第 2147483647 秒，北京时间 2038-1-19 11:14:07，格林尼治时间 2038年1月19日 凌晨 03:14:07 YYYYMMDD HHMMSS	混合日期和时间值，时间戳
​		
```mysql
字符串
	类型				大小						用途
	CHAR		0-255 bytes				定长字符串
	VARCHAR		0-65535 bytes			变长字符串
	TINYBLOB	0-255 bytes				不超过 255 个字符的二进制字符串
	TINYTEXT	0-255 bytes				短文本字符串
	BLOB		0-65 535 bytes			二进制形式的长文本数据
	TEXT		0-65 535 bytes			长文本数据
	MEDIUMBLOB	0-16 777 215 bytes		二进制形式的中等长度文本数据
	MEDIUMTEXT	0-16 777 215 bytes		中等长度文本数据
	LONGBLOB	0-4 294 967 295 bytes	二进制形式的极大文本数据
	LONGTEXT	0-4 294 967 295 bytes	极大文本数据
```

表的操作：
	
```mysql
修改表的介绍：
alter table 表名 comment '修改后的表的注释';

修改字段的注释
alter table 表名 modify column 列名 列数据类型 comment '修改后的字段注释';

查看表介绍， 数据类型等等：
desc 表名;
show columns from 表名;
describe 表名;

查看创建表的详细信息：
show create table 表名;

修改字段的数据类型：
alter table 表名 modify column 字段名 decimal(10, 1) default 0 comment 新注释;

1. 查看当前数据库表
    show tables;

2. 创建表
    create table 表名(列及类型)
        auto_increment 表示自增涨
        primary key 主键(不可以为空)  (联合主键(多个主键)， 只要主键加起来不一样就可以)
        default 默认值
		unique 唯一约束
	-- 添加外键约束
	alter table 表名 add foreign key (列名) references 表名(列名);
	-- 删除外键约束
	alter table 表名 drop foreign key 外键约束名称
	
	-- 设置某一字段唯一性
	alter table 表名 add unique(字段名);
	
	-- 删除字段的唯一性
	drop index 字段名 on 表名;
	
	-- 查看某一张表所有的字段唯一性
	show keys from 表名;
	
    示例：create table Student(id int auto_increment primary key, name varchar(20) not null, age int not null default 18, gender bit default 1, address varchar(50), isDelete bit default 0);

3. 删除表
    drop table 表名;

4. 查看表结构(尽量不要去修改表的结构)
    desc 表名;

5.查看建表语句
    show create table 表名;

6. 重命名表名
    rename table 原表名 to 新表明;

7. 修改表结构
   alter table 表名 add|charge|drop 列名 类型;
```

数据操作：
    增：
		判断表中某一个字段数据是否存在  1：存在 ， 0：不存在
		select 列名(s) from 表名 where 条件 limit 行数; // 默认 1， 表示是否存在
		
		
		
```mysql
    a. 全列插入
        insert into 表名 values(....)
        主键列是自动增长的，但是在全列插入时需要占位，通常使用0，插入成功以后以实际为准

    b. 缺省插入, (可以一次性插入多条)
        insert into 表名(列1, 列2, ...) values(值1, 值2, ...), (值1, 值2, ...), ...;

    c. 同时插入多条数据
        insert into 表名 values(...), (...), ...;

    d. 增加列， 可以选择性的增加在某一列后面或者第一列（默认最后一列）
        after 列名 : 代表你要加在哪一列的后面
        first : 代表加到第一列
        alter table 表名 add column 新加列名 属性(int|varchar|...) not null (after 列名)|(first);

    e. 移动已存在列到指定位置
        after 列名 : 代表你要加在哪一列的后面
        first : 代表加到第一列
        alter table 表名 modify 要移动的列名 带上属性(varchar(11)) (after 某一列的后面)|(first);

删：
    a. delete from 表名; -> 没有条件 删除整张表 (慎用).
    b. delete from 表名 where 条件;

	c. truncate table 表名;
	d. truncate table 表名 where 条件;

改：
    a. update 表名 set 列1=值1, 列2=值2, ... where 条件; -> 如果没有条件 所有的行都被修改 (慎用)

查：
    1. 基本语法
        select * from 表名;
        from 是关键字后面是表名，表示数据库是来源于这个表
        select后面写列明，如果是*号表示在结果集显示所有列
        在select后面的列名部分可以使用as为列起别名，这个别名显示在结果集中
        如果要查询多个列，之间使用逗号分隔

    2. 消除重复
        select distinct 列名 from 表名;

    3. 条件查询
        a. 语法
            select * from 表名 where 条件;

        b. 比较运算符
            等于          =
            大于          >
            小于          <
            大于等于      >=
            小于等于      <=
            不等于       != 或者 <>

        c. 逻辑运算符
            and         并且
            or          或者
            not         非
            查询id值大于7的女同学
                select * from student where id>7 and gender=0;
        d. 模糊查询
            like
            % 表示任意多个任意字符
            _ 表示一个任意字符
            # 查询 name 是 "Xi..."的数据
            select * from student where name like "Xi%";

        e. 范围查询
            in                  表示在一个非连续的范围内
            between...and...    表示在一个连续的范围内
            # 查询id编号为8, 10, 12 的学生
            select * from student where id in (8,10,12);
            # 查询id编号为6~8的学生
            select * from student where id between 6 and 8;

        f. 空判断
            注意： null 与 "" 不同
            判断空： is null
            判断非空： is not null
            # 查询 address 是 null 的同学
            select * from student where address is null;

        g. 优先级
            小括号. not 比较运算符， 逻辑运算符
            and 比 or 优先级高， 如果同时出现并先算or，结合小括号()使用

    4. 聚合
        为了快速得到统计的数据，提供了5个聚合函数
        a. count(*)
            表示计算总行数 ()括号里可以写*也可以写列名
            select count(*) from student;

        b. max(列)   表示求此列的最大值
            select max(id) from student where gender=0;

        c. min(列)   表示求此列的最小值
            select min(age) from student where gender=1;

        d. sum(列)   表示求此列的和
            select sum(age) from student;

        e. avg(列)   表示求此列的平均值
            select avg(age) from student;

    5. 分组
        按照字段分组，表示此字段相同的数据会被放到一个集合中。
        分组后，只能查询出相同的数据列，对于有差异的数据列无法显示在结果集中。
        可以对分组数据进行统计，做聚合运算。
        a. 语法
            select 列1, 列2,..., 聚合... from 表名 group by 列1, 列2, 列3,...;
            # 查询男女生总数
            select gender, count(*) from student group by gender;
            # 分组后数据筛选
            select 列1, 列2,..., 聚合... from 表名 group by 列1, 列2, 列3,... having 列1, 列2,..., 聚合....;
            select gender, count(*) from student group by gender having gender;
            ~ where 是对 from 后面指定的表进行筛选，属于对原始数据的筛选
            ~ having 是对 group by 的结果集中进行筛选

    6. 排序
        语法： select * from 表名 order by 列1 asc|desc, 列2 asc|desc, ....;
        说明：
            a. 将数据按照列1进行排序，如果某些列1的值相同，则按照列2进行排序，以此类推...
            b. 默认按照升序排列
            c. asc 升序
            d. desc 降序
        select * from student where isDelete=0 order by age asc, id desc;

    7. 分页
        start, count : 表示起始行索引(初始行索引 : 0) 和 要查看的行数
        语法： select * from 表名 limit start, count;
        示例：
            select * from student limit 0, 3;
            select * from student limit 3, 3;
            select * from student where gender=1 limit 0, 3;

    8. 关联
        建表语句：
            a. create table class(id int auto_increment primary key, name varchar(20) not null, stuNum int not null);
            b. create table schoulStu(id int auto_increment primary key, name varchar(20) not null, age int not null default 18, gender bit not null default 1, address varchar(60), classid int not null, foreign key(classid) references class(id));

            insert into class values(0,"python01", 50),(0,"python02", 53),(0,"C++01", 35),(0,"Java01", 50);
            insert into schoulstu(name, age, gender, classid) values("你好",18,0,2),("zhangsan",22,1,1),("lisi",18,0,4);
            select schoulstu.name, class.name from class inner join schoulstu on class.id=schoulstu.classid;
            # 分类：
                表A inner join 表B  :   表示表A与表B匹配行会出现在结果集中
                表A left join 表B  ：  表A与表B匹配的行会出现在结果集中，外加表A中独有的数据，未对应的数据使用null填充
                表A right join 表B  ：  表A与表B匹配的行会出现在结果集中，外加表B中独有的数据，未对应的数据使用null填充
```
