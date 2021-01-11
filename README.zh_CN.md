# ScenicGuider / 景观导航者

**中南大学2020-2021年高级程序设计**

## 介绍

本程序使用C#和C++\CLI编写，实现了以下功能

* 加载并显示一个地图

* 搜索地图的最小生成树

* 使用*Dijkstra 算法*，寻找两点之间的最短路径

* 使用*DFS算法*，寻找两点间的所有路径

## 功能

* WPF界面支持

* 矢量图支持

* 多语言本地化支持

## 使用方法

### 制作自己地图的方法

尽管适配本程序的地图生成器已经提上日程，但需要本程序同步做出适配，这需要很多时间，在较短时间内不会实现。

本仓库中有一个示例地图文件，这个地图由两个文件组成：*College1.xaml*和*College1.db*。

* *College1.xaml*: 地图的矢量图形式。地图中所有节点（景点）都是 *System.Windows.Shapes.Ellipse*的对象，所有路径都是 *System.Windows.Shapes.Path*的对象。可以使用*Inkscape*设计并导出本XAML文件。

* *College1.db*: 本地图的SQLite数据库文件，文件名应该与上述XAML文件相同（这一限制将在下个版本中，通过增加的地图配置文件解除）。

## 编译方法

### 依赖项

* 本项目中一部分代码是通过.NET的C++\CLR编写，在较新版本的VS中需要额外安装最新的 ***C++/CLI support for v142 build tools(C++/CLI支持 V142生成工具)*** 组件，才能进行编译。

* ***System.Data.SQLite.Core*** nuget包。本包是解析数据库文件的关键。

### 构建

本项目在Visual Studio 2019 Community上编写完成，所以使用VS2019进行构建即可。

启动VS2019并打开本解决方案，先构建**GraphLibrary**项目，再构建**ScenicGuider**项目即可。


