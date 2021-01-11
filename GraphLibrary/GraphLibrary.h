#pragma once

#include <vector>

using namespace System;
using namespace System::Collections::Generic;

namespace GraphLibrary 
{
	public ref class Graph
	{
	private:
		array<Int32, 2>^ _g;
		int _N;

		void dfsBackend(int, int);
		array<bool>^ _dfs_visited;
		List<List<int>^>^ _dfs_allpath;
		Stack<int>^ _dfs_stack;

	public:
		const static int MaxPathWeight = Int32::MaxValue >> 1;

		Graph(array<Int32, 2>^, int);
		
		List<int>^ GetDijkstraPathNodes(int, int);

		List<Tuple<int, int>^>^ GetPrimMST();

		List<List<int>^>^ GetAllPaths(int, int);
	};
}
