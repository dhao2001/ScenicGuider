using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScenicGuider.Scenic
{
    class DatabaseFileNotFoundException : ApplicationException
    {
        public DatabaseFileNotFoundException(string msg) : base(msg) { }
    }

    class NodeNotFoundException : ApplicationException
    {
        public NodeNotFoundException(string msg) : base(msg) { }
    }

    class PathNotFoundException : ApplicationException
    {
        public PathNotFoundException(string msg) : base(msg) { }
    }

    class GraphNotFullyConnectedException : ApplicationException
    {
        public GraphNotFullyConnectedException(string msg) : base(msg) { }
    }
}
