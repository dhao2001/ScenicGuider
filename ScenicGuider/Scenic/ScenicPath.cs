using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenicGuider.Scenic
{
    class ScenicPath
    {
        public static readonly int MaxPathWeight = Int32.MaxValue / 2;
        private int[,] graph;
        private int N;
        
        public ScenicPath(int[,] graph, int graphN)
        {
            this.graph = graph;
            N = graphN;
        }

        public List<int> GetDijkstraShortestPath(int srcIndex, int dstIndex)
        {
            List<int> pathPoints = new List<int>();
            int[] previousPoint = new int[N + 1];
            int[] distanceTo = new int[N + 1];
            bool[] visited = new bool[N + 1];

            for (int i = 1; i <= N; i++)
            {
                distanceTo[i] = graph[srcIndex, i];
            }
            distanceTo[srcIndex] = 0;
            visited[srcIndex] = true;


            for (int loop = 1; loop <= N - 1; loop++)
            {
                int curMinDstPoint = -1;
                int curMinDst = MaxPathWeight;
                for (int i = 1; i <= N; i++)
                {
                    if (!visited[i] && distanceTo[i] < curMinDst)
                    {
                        curMinDstPoint = i;
                        curMinDst = distanceTo[i];
                    }
                }

                if (curMinDstPoint == -1)
                {
                    throw new GraphNotFullyConnectedException("Graph Not Fully Connected.");
                }

                visited[curMinDstPoint] = true;
                for (int i = 1; i <= N; i++)
                {
                    if (!visited[i] && graph[curMinDstPoint, i] != MaxPathWeight && distanceTo[curMinDstPoint] + graph[curMinDstPoint, i] < distanceTo[i])
                    {
                        distanceTo[i] = distanceTo[curMinDstPoint] + graph[curMinDstPoint, i];
                        previousPoint[i] = curMinDstPoint;
                    }
                }
            }

            Stack<int> stackPoint = new Stack<int>();
            for (int i = dstIndex; i != 0; i = previousPoint[i])
            {
                stackPoint.Push(i);
            }
            stackPoint.Push(srcIndex);
            return new List<int>(stackPoint);
        }
    }
}
