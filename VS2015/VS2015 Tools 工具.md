# VS2015   (Windows 10)

一、 项目配置
1. default namespace , 等等的一些配置


Build or Rebuild 时候的Config

1. 项目右键 -> 属性(Properties)
2. select Build选项 找到 Output 设置 Output path;  后面简称  $(OutDir)  例如： D:\VS2015Program\SoftLiu_VSMainMenuTools\Out\
3. Build Event 中有 Pre-build  和  Post-build  (命令行  遵从 .bat  语法), 一般使用 Post-build 下面有post-event执行的时间点



```bash
$(OutDir)就已经在“常规”栏目的“输出目录”选项赋值了
$(TargetDir)的值是在生成exe文件后自动赋予值为exe文件所在位置

ConfigurationName	配置名字，通常是Debug或者Release

IntDir	编译器使用的中间目录，产出obj文件

OutDir	链接器使用的输出目录

ProjectDir	项目目录

ProjectName	项目名字

SolutionDir	解决方案目录

TargetDir	目标输出文件所在的目录

TargetExt	目标输出的扩展名

TargetFileName	目标输出文件名，包括扩展名

TargetName	目标输出名，不包括扩展名

TargetPath	目标输出文件的全路径名
```

