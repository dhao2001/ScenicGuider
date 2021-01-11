#include "pch.h"

#include "GraphLibrary.h"
#include "Exceptions.h"

using namespace System;

namespace GraphLibrary
{
	Graph::Graph(array<Int32, 2>^ graph, int N)
	{
		_g = graph;
		_N = N;
	}

	List<int>^ Graph::GetDijkstraPathNodes(int src, int dst)
	{
		array<Int32>^ previousNode = gcnew array<Int32>(_N + 1);
		array<Int32>^ distanceTo = gcnew array<Int32>(_N + 1);
		array<bool>^ visited = gcnew array<bool>(_N + 1);

		for (int i = 1; i <= _N; i++)
		{
			distanceTo[i] = _g[src, i];
		}
		distanceTo[src] = 0;
		visited[src] = true;

		for (int loop = 0; loop <= _N - 1; loop++)
		{
			int curMinDstNode = -1;
			int curMinDst = MaxPathWeight;
			for (int i = 0; i <= _N; i++)
			{
				if (!visited[i] && distanceTo[i] < curMinDst)
				{
					curMinDstNode = i;
					curMinDst = distanceTo[i];
				}
			}

			if (curMinDstNode == -1)
			{
				throw gcnew GraphNotFullyConnectedException();
			}

			visited[curMinDstNode] = true;
			for (int i = 0; i <= _N; i++)
			{
				if (!visited[i] &&
					_g[curMinDstNode, i] != MaxPathWeight &&
					distanceTo[curMinDstNode] + _g[curMinDstNode, i] < distanceTo[i])
				{
					distanceTo[i] = distanceTo[curMinDstNode] + _g[curMinDstNode, i];
					previousNode[i] = curMinDstNode;
				}
			}
		}

		Stack<int>^ nodeStack = gcnew Stack<int>();
		for (int i = dst; i != 0 ; i = previousNode[i])
		{
			nodeStack->Push(i);
		}
		nodeStack->Push(src);
		return gcnew List<int>(nodeStack);
	}

	List<Tuple<int, int>^>^ Graph::GetPrimMST()
	{
		List<Tuple<int, int>^>^ edges = gcnew List<Tuple<int, int>^>();
		List<int>^ addedVertex = gcnew List<int>();
		array<bool>^ isAdded = gcnew array<bool>(_N + 1);

		Random^ rd = gcnew Random();
		int initVer = rd->Next(1, _N + 1);
		addedVertex->Add(initVer);
		isAdded[initVer] = true;

		for (int loop = 0; loop < _N && addedVertex->Count < _N; loop++)
		{
			int curMinEdgeStart = -1;
			int curMinEdgeEnd = -1;
			int curMinEdgeWeight = MaxPathWeight;

			for (int i = 1; i <= _N; i++)
			{
				if (isAdded[i])
				{
					for (int j = 1; j <= _N; j++)
					{
						if (!isAdded[j] && _g[i, j] < curMinEdgeWeight)
						{
							curMinEdgeStart = i;
							curMinEdgeEnd = j;
							curMinEdgeWeight = _g[i, j];
						}
					}
				}
			}
			
			addedVertex->Add(curMinEdgeEnd);
			isAdded[curMinEdgeEnd] = true;
			edges->Add(gcnew Tuple<int, int>(curMinEdgeStart, curMinEdgeEnd));
		}

		return edges;
	}

	void Graph::dfsBackend(int src, int dst)
	{
		if (src == dst)
		{
			_dfs_stack->Push(dst);
			_dfs_allpath->Add(gcnew List<int>(_dfs_stack));
			_dfs_stack->Pop();
		}
		else
		{
			_dfs_stack->Push(src);
			_dfs_visited[src] = true;

			for (int i = 1; i <= _N; i++)
			{
				if (!_dfs_visited[i] && _g[src, i] < MaxPathWeight)
				{
					dfsBackend(i, dst);
				}
			}
			_dfs_visited[src] = false;
			_dfs_stack->Pop();
		}
	}


	List<List<int>^>^ Graph::GetAllPaths(int srcIndex, int dstIndex)
	{
		_dfs_allpath = gcnew List<List<int>^>();
		_dfs_stack = gcnew Stack<int>();
		_dfs_visited = gcnew array<bool>(_N + 1);
		dfsBackend(srcIndex, dstIndex);
		return _dfs_allpath;
	}

	//List<List<int>^>^ Graph::GetAllPaths(int srcIndex, int dstIndex)
	//{
	//	List<List<int>^>^ paths = gcnew List<List<int>^>();
	//	Stack<int>^ stack = gcnew Stack<int>();
	//	array<bool>^ visited = gcnew array<bool>(_N + 1);

	//	//stack->Push(srcIndex);
	//	//visited[srcIndex] = true;
	//	//while (stack->Count > 0)
	//	//{
	//	//	int top = stack->Peek();
	//	//	/*if (top == dstIndex)
	//	//	{
	//	//		paths->Add(gcnew List<int>(stack));
	//	//		stack->Pop();
	//	//		continue;
	//	//	}*/

	//	//	bool find = false;
	//	//	for (int i = 1; i <= _N; i++)
	//	//	{
	//	//		if (i == dstIndex && _g[top, i] < MaxPathWeight)
	//	//		{
	//	//			stack->Push(dstIndex);
	//	//			paths->Add(gcnew List<int>(stack));
	//	//			stack->Pop();
	//	//			break;
	//	//		}
	//	//		if (!visited[i] && _g[top, i] < MaxPathWeight)
	//	//		{
	//	//			stack->Push(i);
	//	//			visited[i] = true;
	//	//			find = true;
	//	//			break;
	//	//		}
	//	//	}
	//	//	if (!find)
	//	//	{
	//	//		//visited[stack->Pop()] = false;
	//	//		stack->Pop();
	//	//	}
	//	//}

	//	stack->Push(srcIndex);
	//	visited[srcIndex] = true;

	//	while (stack->Count > 0)
	//	{
	//		int v = stack->Peek();
	//		if (v == dstIndex)
	//		{
	//			paths->Add(gcnew List<int>(stack));
	//		}

	//		int next = -1;
	//		
	//		for (int i = 1; i <= _N; i++)
	//		{
	//			if (!visited[i] && _g[v, i] < MaxPathWeight)
	//			{
	//				next = i;
	//				break;
	//			}
	//		}

	//		if (next == -1)
	//		{
	//			stack->Pop();
	//		}
	//		else
	//		{
	//			visited[next] = true;
	//			stack->Push(next);
	//		}
	//	}

	//	return paths;
	//}

}