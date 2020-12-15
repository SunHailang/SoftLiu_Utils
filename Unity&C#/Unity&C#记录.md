<h1 align="center"><b>Unity&C# 学习笔记</b></h1>

# **Time时间相关**

## **1.1 Unity Time类**

```c#
Time.time;					// 表示从游戏开发到现在的时间，会随着游戏的暂停而停止计算。

Time.timeSinceLevelLoad;	// 表示从当前Scene开始到目前为止的时间，也会随着暂停操作而停止。

Time.deltaTime;				// 表示从上一帧到当前帧时间，以秒为单位。计时器通过这个使用，update里把Time.deltaTime累加起来，就是经过的时间

Time.fixedTime;				// 表示以秒计游戏开始的时间，固定时间以定期间隔更新（相当于fixedDeltaTime）直到达到time属性。

Time.fixedDeltaTime;		// 表示以秒计间隔，在物理和其他固定帧率进行更新，在Edit->ProjectSettings->Time的Fixed Timestep可以自行设置。

Time.SmoothDeltaTime;		//表示一个平稳的deltaTime，根据前N帧的时间加权平均的值。

Time.timeScale;				// 时间缩放，默认值为1，若设置<1，表示时间减慢，若设置>1,表示时间加快，可以用来加速和减速游戏，非常有用。

Time.frameCount;			// 总帧数

Time.realtimeSinceStartup;	// 表示自游戏开始后的总时间，即使暂停也会不断的增加。

Time.captureFramerate;		// 表示设置每秒的帧率，然后不考虑真实时间。

Time.unscaledDeltaTime;		// 不考虑timescale时候与deltaTime相同，若timescale被设置，则无效。

Time.unscaledTime;			// 不考虑timescale时候与time相同，若timescale被设置，则无效。
```

## **1.2 C# - System.DateTime**

```C#
// 构造函数：
DateTime(Int32 年, Int32 月, Int32 日)
DateTime(年，月，日，时，分，秒，(可再加毫秒)/协调世界时 (UTC) 或本地时间)

// 属性：
DateTime dt = DateTime.Now;
dt.ToString();	// 1/22/2017 3:43:19 PM
//获取一个DateTime对象，该对象设置为此计算机上的当前日期和时间，表示为本地时间。[受时区影响;我们中国使用的是东八区的时间，所以和UTC大了8个小时]

DateTime dt2 = DateTime.Today;
dt2.ToString();//1/22/2017 12:00:00AM 当天日期，其时间组成部分设置为00:00:00。

DateTime dt3 = DateTime.UtcNow ;
dt3.ToString();//1/22/2017 7:43:19 AM 此计算机上的当前日期和时间，表示为协调通用时间 (UTC,0时区的时间)

dt.Year.ToString();//2017

dt.Date.ToString();// 1/22/2017 12:00:00 AM获取此实例的日期部分；其日期与此实例相同，其时间值设置为午夜 12:00:00 (00:00:00)。

dt.Day.ToString(); //22 获取此实例所表示的日期为该月中的第几天。

dt.DayOfWeek.ToString();//Sunday 获取此实例所表示的日期是星期几。

dt.DayOfYear.ToString();//22 获取此实例所表示的日期是该年中的第几天。

dt.Hour.ToString();//14 获取此实例所表示日期的小时部分。(24h制)

dt.Kind.ToString();//Local 返回基于本地时间Local、协调世界时 (UTC)，还是两者皆否

dt.Millisecond.ToString();//630 获取此实例所表示日期的毫秒部分。

dt.Minute.ToString();//35 获取此实例所表示日期的分钟部分

dt.Month.ToString();//1 获取此实例所表示日期的月部分

dt.Second.ToString();//28 获取此实例所表示日期的秒部分

dt.Ticks.ToString();//632667942284412864获取表示此实例的日期和时间的计时周期数。
/*--------------------------------------------------------------------------------------------------------------------------------------
每个计时周期表示一百纳秒，即一千万分之一秒。 1 毫秒内有 10,000 个计时周期。此属性的值表示自 0001 年 1 月 1 日午夜 12:00:00(表示DateTime.MinValue)以来经过的以 100 纳秒为间隔的间隔数。 它不包括归属于闰秒的刻度数。
--------------------------------------------------------------------------------------------------------------------------------------*/

dt.TimeOfDay.ToString();//15:20:43.1911987获取此实例的当天的时间。
/*------------------------------------------------------------------------------------------------------------------------------------*/
//方法：
dt.ToFileTime().ToString();//131295394816200492将当前 DateTime 对象的值转换为Windows文件时间。

dt.ToFileTimeUtc().ToString();//131295683210873066将当前 DateTime 对象的值转换为 Windows 文件时间

dt.ToLocalTime().ToString();// 1/22/2017 2:19:17 PM将当前 DateTime 对象的值转换为本地时间

dt.ToLongDateString().ToString();//Sunday,January 22,2017转为长日期字符串表示形式

dt.ToLongTimeString().ToString();//2:20:33 PM转为其等效的长时间字符串表示形式。

dt.ToOADate().ToString();//42757.5978641782转换为等效的OLE自动化日期

dt.ToShortDateString().ToString();// 1/22/2017

dt.ToShortTimeString().ToString();//2:21 PM 

dt.ToUniversalTime().ToString();//1/22/2017 6:22:10AM 转换为协调世界时(UTC

dt.AddYears(1).ToString();//1/22/2018 3:51:20 PM增加/减少年(参数可为负)
dt.AddDays(1.1).ToString();//1/23/2017 6:16:36 PM
dt.AddHours(1.1).ToString();//2005-11-5 14:53:04
dt.AddMilliseconds(1.1).ToString();//2005-11-5 13:47:04
dt.AddMonths(1).ToString();//2005-12-5 13:47:04
dt.AddSeconds(1.1).ToString();//2005-11-5 13:47:05
dt.AddMinutes(1.1).ToString();//2005-11-5 13:48:10
dt.AddTicks(1000).ToString();//2005-11-5 13:47:04 计时周期数

dt.CompareTo(dt).ToString();//0 两时间比较:早于<0，同时==0，迟于>0[static&&not static]

dt.Add(?).ToString();	//问号为一个时间段TimeSpan

dt.Equals("2005-11-6 16:11:04").ToString();	//False [static&&not static]返回是否具有相同日期和时间
dt.Equals(dt).ToString();//True

dt.GetHashCode().ToString();//1474088234

dt.GetType().ToString();//System.DateTime

dt.GetTypeCode().ToString();//DateTime

dt.GetDateTimeFormats('s')[0].ToString();//2005-11-05T14:06:25

dt.GetDateTimeFormats('t')[0].ToString();//14:06

dt.GetDateTimeFormats('y')[0].ToString();//2005年11月

dt.GetDateTimeFormats('D')[0].ToString();//2005年11月5日

dt.GetDateTimeFormats('D')[1].ToString();//2005 11 05

dt.GetDateTimeFormats('D')[2].ToString();//星期六 2005 11 05

dt.GetDateTimeFormats('D')[3].ToString();//星期六 2005年11月5日

dt.GetDateTimeFormats('M')[0].ToString();//11月5日

dt.GetDateTimeFormats('f')[0].ToString();//2005年11月5日 14:06

dt.GetDateTimeFormats('g')[0].ToString();//2005-11-5 14:06

dt.GetDateTimeFormats('r')[0].ToString();//Sat, 05 Nov 2005 14:06:25 GMT

//注：string格式字符串：
string.Format("{0:d}",dt);//2005-11-5
string.Format("{0:D}",dt);//2005年11月5日
string.Format("{0:f}",dt);//2005年11月5日 14:23
string.Format("{0:F}",dt);//2005年11月5日 14:23:23
string.Format("{0:g}",dt);//2005-11-5 14:23
string.Format("{0:G}",dt);//2005-11-5 14:23:23
string.Format("{0:M}",dt);//11月5日
string.Format("{0:R}",dt);//Sat, 05 Nov 2005 14:23:23 GMT
string.Format("{0:s}",dt);//2005-11-05T14:23:23
string.Format("{0:t}",dt);//14:23
string.Format("{0:T}",dt);//14:23:23
string.Format("{0:u}",dt);//2005-11-05 14:23:23Z
string.Format("{0:U}",dt);//2005年11月5日 6:23:23
string.Format("{0:Y}",dt);//2005年11月
string.Format("{0}",dt);//2005-11-5 14:23:23

string.Format("{0:yyyyMMddHHmmssffff}",dt);//201701221603353262
//DateTime.Now.ToString(IFormatProvider provider)用法：
//注意：HH为24小时制hh为12小时制,年月日时分秒yyyy MM dd HH mm ss
//DateTime.Now.ToString("yyyy年MM月dd日"); yyyy年MM月dd日
//DateTime.Now.ToString("HH:mm"); 19:48
/*-----------------------------------------------*/
//计算某年某月的天数
int days = DateTime.DaysInMonth(2007, 8);
days = 31;
/*-----------------------------------------------*/
//给日期增加一天、减少一天
DateTime dt =DateTime.Now;
dt.AddDays(1); //增加一天
dt.AddDays(-1);//减少一天
```

## **1.3 时间戳**

- 时间戳 (timestamp)，通常是一个字符序列，唯一地标识某一刻的时间。
- 时间戳是指格林威治时间1970年01月01日00时00分00秒(北京时间1970年01月01日08时00分00秒)起至现在的总毫秒数。

```C#
// C#生成一个时间戳：(这里以秒为单位)
private static string GetTimeStamp()
{
    TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);  
    return Convert.ToInt64(ts.TotalSeconds).ToString();  
}

// C#时间戳转换为格式时间：(这里以秒为单位)
private DateTime GetTime(string timeStamp)  
{  
  DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));  //时间戳起始点转为目前时区
  long lTime = long.Parse(timeStamp + "0000000");//转为long类型  
  TimeSpan toNow = new TimeSpan(lTime); //时间间隔
  return dtStart.Add(toNow); //加上时间间隔得到目标时间
}
```

