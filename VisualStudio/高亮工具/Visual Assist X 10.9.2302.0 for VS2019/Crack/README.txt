INSTALLATION

0) Uninstall VA (if you have installed it before).
卸载VA(如果你以前安装过)
0.0) Manually check for alive _folders_ of pervious version of VA in extensions folders (paths are described
below) and remove them by yourself, because of VA uninstaller's bug
手动检查前一个VA版本的安装文件夹,并且手动删除,因为VA的卸载bug,无法卸载干净
1) Locate all places where va_x.dll's are reside in your MSVCs (paths are described below)
找到va_x.dll的位置
2) Replace all found va_x.dll with our one (from this torrent)
 替换找到的va_x.dll
3) PROFIT (you may see "License: trial" - don't worry, all should work without any limitation)
你可能会看到License: trial,不用担心,已经没有任何限制了.

Places where Visual Assist extension (va_x.dll and another stuff) resides in different versions of MS Visual Studio are listed below:
不同版本VS 中va_x.dll的路径
-------------------
MSVC version | path
MSVC 不同版本替换路径
-------------------
msvc2008     | find in installation path安装目录 (by default默认 c:\Program Files (x86)\Visual Assist X\)

msvc2010     | %USERPROFILE%\AppData\Local\Microsoft\VisualStudio\10.0\Extensions\Whole Tomato Software\Visual Assist\__version__\

msvc201[1|2] | %USERPROFILE%\AppData\Local\Microsoft\VisualStudio\11.0\Extensions\__random_dir__\

msvc2013     | %USERPROFILE%\AppData\Local\Microsoft\VisualStudio\12.0\Extensions\__second_random_dir__\

msvc2015     | %USERPROFILE%\AppData\Local\Microsoft\VisualStudio\14.0\Extensions\__random_dir__\

msvc2017     | %USERPROFILE%\AppData\Local\Microsoft\VisualStudio\15.0__%XXX%\Extensions\__second_random_dir__\

msvc2019     | %USERPROFILE%\AppData\Local\Microsoft\VisualStudio\16.0__%XXX%\Extensions\__second_random_dir__\
-------------------

Notes: in my case
__random_dir__ and __second_random_dir__ are generated and looks like "y5ftkxqz.yl1"




=================
