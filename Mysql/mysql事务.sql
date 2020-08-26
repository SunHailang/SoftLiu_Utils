-- MySQL 事务

-- mysql 中， 事务其实就是一个最小的不可分割的工作单元，事务能够保证一个业务的完整性。

-- 银行转账：
    -- a -> -100
    update user set money=money-100 where name='a';
    -- b -> +100
    update user set money=money+100 where name='b';

-- 实际程序中，如果只有一条语句执行成功了，而另外一条没有执行成功？
-- 出现前后不一致
update user set money=money-100 where name='zhangsan';
update user set money=money+100 where name='lisi';

-- 多条sql语句可能会有同时成功的要求，要么同时失败

-- 1. mysql默认是开启事务的(自动提交) 
    select @@autocommit -- 查看mysql事务 1：开启， 0：关闭

    -- 默认开启事务的作用是什么？
    -- 当我们去执行一个sql语句的时候，效果会立即体现出来，不允许回滚

    create database bank;
    create table user(
        id int primary key,
        name varchar(20),
        money int
    );

    insert into user values(1, 'a', 1000);

    -- 事务回滚： 撤销sql语句的执行效果
    rollback;

    -- 设置mysql自动提交为 false
    set autocommit=0;

-- begin; 或者 start transaction; 都可以手动开启一个事务
    begin;
    update user set money=money-100 where name='zhangsan';
    update user set money=money+100 where name='lisi';

    start transaction;
    update user set money=money-100 where name='zhangsan';
    update user set money=money+100 where name='lisi';
    -- 如果不想允许回滚 则加一个 commit
    -- 事务开启之后，有单commit提交，就不可以回滚(表示当前事务就结束了)

-- 2.事务的特征
    -- A: 原子性 -- 事务是最小的单位不可以在分割
    -- C: 一致性 -- 事务要求，同一事务中的sql语句，必须保证同时成功，或同时失败。
    -- I: 隔离性 -- 事务1和事务2之间是具有隔离性。
    -- D: 持久性 -- 事务一旦结束(commit;)，就不可以回滚(rollback;)。

-- 事务开启： 
    -- 1. 修改默认提交 set autocommit=0;
    -- 2. begin;
    -- 3. start transaction;

-- 事务手动提交：
    commit;

-- 事务手动回滚：
    rollback;

-- 查看事务的隔离性： 
-- 8.x version
select @@global.transaction_isolation; -- 系统级别的
select @@transaction_isolation; -- 会话级别的
-- 5.x version
select @@global.tx_isolation; -- 系统级别的
select @@tx_isolation; -- 会话级别的

-- 设置/修改事务的隔离性:
set global transaction isolation level read uncommitted;

-- 事务的隔离性：
    -- 1. read uncommitted;    -- 读未提交的
        -- 如果有事务a, 和事务b
        -- a事务对数据进行操作，在操作的过程中，事务没有被提交，但是事务b可以看到a操作的结果
        -- 脏读 ：一个事务读到了另外一个事务未提交的数据(实际开发是不允许脏读的)
    -- 2. read committed;      -- 读已经提交的
        -- 

    -- 3. repeatable read;     -- 可以重复读(mysql默认的隔离级别)
        -- 出现幻读

    -- 4. serializable;        -- 串行化

-- 性能：  隔离级别越高性能越差













