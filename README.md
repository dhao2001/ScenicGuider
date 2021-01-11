# ScenicGuider / 景观导航者

**Central South University 2020-2021 Advanced Programming Course Design**

## Description

Using C# and C++/CLI, this program is able to:

* Load a map, draw it on the screen

* Search the map's MST(*Minimum Spanning Tree*)

* Search the Shortest Path between 2 vertexs of the map, using the *Dijkstra Algorithm*

* Search all paths between 2 vertexs of the map, using the *DFS Algorithm*

## Features

* WPF Support(*That's the seccret makes me load map easily.*)

* Scalable Vector Graphics Support

* Localization Support

## How to use

### How to make a custom map

The Map Generator of this program is put on the agenda. But the code of this program needs changes to fit the configure file.

There is a ***Example Map*** in this repository, the map is consisted of two files: *College1.xaml*, *College1.db*.

* *College1.xaml*: The *Scalable Vector Graphics* of the map. All Vertex is an object of *System.Windows.Shapes.Ellipse*, and All Path is an object of *System.Windows.Shapes.Path*. A xaml file can be designed and exported using the *Inkscape*.

* *College1.db*: The SQLite Database file of the map, whose name is the same as the xaml file.

## How to build

### Dependence

* Some code of this project is written in C++\CLI, which needs ***C++/CLI support for v142 build tools*** component of Visual Studio to build.

* ***System.Data.SQLite.Core*** nuget package is also needed, without which this program is unable to load the database of the map.

### Build

This project is developed with Visual Studio 2019 Community, so use VS 2019 to build this project is OK. 

Just open the solution in VS 2019, build the **GraphLibrary** Project firstly, and build the **ScenicGuider** Project finally.
