
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using System.Xml;

namespace ScenicGuider.Scenic
{
    class SelectedNodeChangeEventArgs : RoutedEventArgs
    {
        public SelectedNodeChangeEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }

        public string Message { get; set; }
    }

    class ScenicMap : UIElement
    {
        public string XmlPath { get; set; }
        private XmlReader xml;

        private FrameworkElement _xaml;
        public FrameworkElement XamlRoot
        {
            get
            {
                return _xaml;
            }
        }

        private SQLiteConnection dbConnection;

        private bool _isLoad = false;
        public bool IsLoad
        {
            get
            {
                return _isLoad;
            }
        }

        public string NodesTableName { get; set; } = "nodes";
        public string PathsTableName { get; set; } = "paths";
        private List<string> nodeList;
        private List<string> pathList;

        public System.Windows.Media.Brush DefaultNodeBackground { get; set; }
        public System.Windows.Media.Brush HighlightNodeBackground { get; set; }
        public System.Windows.Media.Brush DefaultPathColor { get; set; }
        public double DefaultPathStrokeThickness { get; set; }
        public double LightingPathStrokeThickness { get; set; }

        private int[,] mapGraph;
        private int graphN;

        private Ellipse _highnode;
        public Ellipse HighlightNode
        {
            get
            {
                return _highnode;
            }
            set
            {
                resetDefaultColor();
                _highnode = value;
                if (_highnode != null)
                {
                    _highnode.Fill = HighlightNodeBackground;
                    styleChanged[_highnode] = true;
                }
            }
        }

        private Dictionary<Shape, bool> styleChanged;

        public static readonly RoutedEvent SelectedNodeChangeEvent = EventManager.RegisterRoutedEvent("SelectedNodeChange", RoutingStrategy.Bubble, typeof(SelectedNodeChangeRoutedEventHandler), typeof(ScenicMap));

        public delegate void SelectedNodeChangeRoutedEventHandler(object sender, SelectedNodeChangeEventArgs e);

        public event SelectedNodeChangeRoutedEventHandler SelectedNodeChange
        {
            add { AddHandler(SelectedNodeChangeEvent, value); }
            remove { RemoveHandler(SelectedNodeChangeEvent, value); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xmlPath"></param>
        public ScenicMap(string xmlPath)
        {
            this.XmlPath = xmlPath;
            _isLoad = false;
        }


        /// <summary>
        /// Load Method
        /// </summary>
        public void Load()
        {
            try
            {
                xml = XmlReader.Create(XmlPath);

                string dbPath = XmlPath.Substring(0, XmlPath.LastIndexOf('.')) + ".db";
                dbConnection = new SQLiteConnection($"Data Source={dbPath};FailIfMissing=True");
                dbConnection.Open();
                loadDatabase();

                buildMapGraph();

                XmlReader xml2 = XmlReader.Create(xml, new XmlReaderSettings());
                _xaml = (FrameworkElement)XamlReader.Load(xml2);
                addEllipseClick();

                setDefaultStyle();

                styleChanged = new Dictionary<Shape, bool>();

                _isLoad = true;
            }
            catch (FileNotFoundException e)
            {
                throw e;
            }
            catch (SQLiteException e)
            {
                throw e;
            }
            catch (InvalidCastException e)
            {
                throw e;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void loadDatabase()
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = $"SELECT svgid FROM {NodesTableName}";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            nodeList = new List<string>();
                            while (reader.Read())
                            {
                                nodeList.Add(reader[0].ToString());
                            }
                        }
                    }
                    cmd.CommandText = $"SELECT svgid FROM {PathsTableName}";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            pathList = new List<string>();
                            while (reader.Read())
                            {
                                pathList.Add(reader[0].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void addEllipseClick()
        {
            foreach (string s in nodeList)
            {
                object obj = _xaml.FindName(s);
                if (!(obj is Ellipse))
                {
                    throw new XamlParseException($"{s} is Not a Ellipse.");
                }

                Ellipse ellipse = (Ellipse)obj;
                MouseGesture gesture = new MouseGesture(MouseAction.LeftClick);
                MouseBinding binding = new MouseBinding(new NodeClickCommand(), gesture);
                binding.CommandParameter = new Tuple<ScenicMap, Ellipse>(this, ellipse);
                ellipse.InputBindings.Add(binding);
            }
        }

        private void setDefaultStyle()
        {
            Ellipse ellipse = _xaml.FindName(nodeList[0]) as Ellipse;
            if (ellipse == null)
            {
                throw new NodeNotFoundException(nodeList[0] + " Not Found.");
            }
            DefaultNodeBackground = ellipse.Fill;
            System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush();
            brush.Color = System.Windows.Media.Color.FromRgb(43, 95, 117);
            HighlightNodeBackground = brush;

            System.Windows.Shapes.Path p = _xaml.FindName(pathList[0]) as System.Windows.Shapes.Path;
            if (p == null)
            {
                throw new PathNotFoundException(pathList[0] + " Not Found.");
            }
            DefaultPathColor = p.Stroke;
            DefaultPathStrokeThickness = p.StrokeThickness;
            LightingPathStrokeThickness = p.StrokeThickness * 1.5;
        }

        private void buildMapGraph()
        {
            graphN = nodeList.Count;
            mapGraph = new int[graphN + 1, graphN + 1];
            for (int i = 0; i < graphN + 1; i++)
            {
                for (int j = 0; j < graphN + 1; j++)
                {
                    mapGraph[i, j] = Int32.MaxValue >> 1;
                }
            }
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = $"SELECT begin_nid, end_nid, length FROM {PathsTableName}";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int start = Convert.ToInt32(reader["begin_nid"].ToString());
                                int end = Convert.ToInt32(reader["end_nid"].ToString());
                                int len = Convert.ToInt32(reader["length"].ToString());
                                mapGraph[start, end] = len;
                                mapGraph[end, start] = len;
                            }
                        }
                    }
                }
            }

            catch (Exception)
            {

                throw;
            }

        }

        private void resetDefaultColor()
        {
            foreach (var item in styleChanged)
            {
                if (item.Key is System.Windows.Shapes.Ellipse)
                {
                    Ellipse e = item.Key as Ellipse;
                    e.Fill = DefaultNodeBackground;
                }
                else if (item.Key is System.Windows.Shapes.Path)
                {
                    System.Windows.Shapes.Path p = item.Key as System.Windows.Shapes.Path;
                    p.Stroke = DefaultPathColor;
                    p.StrokeThickness = DefaultPathStrokeThickness;
                }
            }
            styleChanged.Clear();

            //foreach (string node in nodeList)
            //{
            //    Ellipse ellipse = _xaml.FindName(node) as Ellipse;
            //    if (ellipse == null)
            //    {
            //        throw new NodeNotFoundException(node + " Not Found.");
            //    }
            //    ellipse.Fill = DefaultNodeBackground;
            //}

            //foreach (string path in pathList)
            //{
            //    System.Windows.Shapes.Path p = _xaml.FindName(path) as System.Windows.Shapes.Path;
            //    if (p == null)
            //    {
            //        throw new PathNotFoundException(p + " Not Found.");
            //    }
            //    p.Stroke = DefaultPathColor;
            //}
        }

        public void SetHighlightNode(string svgid)
        {
            HighlightNode = _xaml.FindName(svgid) as Ellipse;
        }

        public Tuple<string, string> GetNodeDescription(string svgid)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
            {
                cmd.CommandText = $"SELECT name,description FROM nodes WHERE svgid = \'{svgid}\'";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new NodeNotFoundException(svgid + " Not Found.");
                    }
                    reader.Read();
                    return new Tuple<string, string>(reader["name"].ToString(), reader["description"].ToString());
                }
            }
        }

        public int DrawPath(List<int> path)
        {
            resetDefaultColor();
            int pathLength = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                int p1 = path[i], p2 = path[i + 1];
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = $"SELECT svgid,length FROM paths WHERE (begin_nid = {p1} AND end_nid = {p2}) OR (begin_nid = {p2} AND end_nid = {p1})";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            throw new PathNotFoundException($"Path betweent {p1} and {p2} Not Found.");
                        }
                        reader.Read();
                        string pathSvg = reader["svgid"].ToString();
                        object obj = _xaml.FindName(pathSvg);
                        if (obj == null)
                        {
                            throw new PathNotFoundException($"Path betweent {p1} and {p2} Not Found.");
                        }
                        if (!(obj is System.Windows.Shapes.Path))
                        {
                            throw new PathNotFoundException($"Path betweent {p1} and {p2} Not Found.");
                        }
                        System.Windows.Shapes.Path p = (System.Windows.Shapes.Path)obj;
                        System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush();
                        brush.Color = System.Windows.Media.Color.FromRgb(255, 0, 0);
                        p.Stroke = brush;
                        p.StrokeThickness = LightingPathStrokeThickness;
                        styleChanged[p] = true;
                        pathLength += Convert.ToInt32(reader["length"].ToString());
                    }
                }
            }
            return pathLength;
        }

        public int DrawShortestPath(string startPointName, string endPointName)
        {
            resetDefaultColor();
            List<int> pathPoints = GetShortestPath(startPointName, endPointName);
            return DrawPath(pathPoints);
        }

        public List<int> GetShortestPath(string startPointName, string endPointName)
        {
            int startID, endID;
            using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
            {
                cmd.CommandText = $"SELECT nid FROM nodes WHERE name = \'{startPointName}\'";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new NodeNotFoundException(startPointName + " Not Found.");
                    }
                    reader.Read();
                    startID = Convert.ToInt32(reader["nid"].ToString());
                }
                cmd.CommandText = $"SELECT nid FROM nodes WHERE name = \'{endPointName}\'";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new NodeNotFoundException(endPointName + " Not Found.");
                    }
                    reader.Read();
                    endID = Convert.ToInt32(reader["nid"].ToString());
                }
            }
            GraphLibrary.Graph g = new GraphLibrary.Graph(mapGraph, graphN);
            //List<int> pathPoints = path.GetDijkstraShortestPath(startID, endID);
            return g.GetDijkstraPathNodes(startID, endID);
        }

        public List<List<int>> GetAllPaths(string startPointName, string endPointName)
        {
            int startID, endID;
            using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
            {
                cmd.CommandText = $"SELECT nid FROM nodes WHERE name = \'{startPointName}\'";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new NodeNotFoundException(startPointName + " Not Found.");
                    }
                    reader.Read();
                    startID = Convert.ToInt32(reader["nid"].ToString());
                }
                cmd.CommandText = $"SELECT nid FROM nodes WHERE name = \'{endPointName}\'";
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new NodeNotFoundException(endPointName + " Not Found.");
                    }
                    reader.Read();
                    endID = Convert.ToInt32(reader["nid"].ToString());
                }
            }
            GraphLibrary.Graph g = new GraphLibrary.Graph(mapGraph, graphN);
            return g.GetAllPaths(startID, endID);
        }

        public List<Tuple<int, int>> GetMSTPaths()
        {
            GraphLibrary.Graph g = new GraphLibrary.Graph(mapGraph, graphN);
            return g.GetPrimMST();
        }

        public int DrawMSTPaths()
        {
            resetDefaultColor();
            List<Tuple<int, int>> paths = GetMSTPaths();
            int mstLength = 0;
            foreach (var pa in paths)
            {
                int p1 = pa.Item1;
                int p2 = pa.Item2;
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = $"SELECT svgid,length FROM paths WHERE (begin_nid = {p1} AND end_nid = {p2}) OR (begin_nid = {p2} AND end_nid = {p1})";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            throw new PathNotFoundException($"Path betweent {p1} and {p2} Not Found.");
                        }
                        reader.Read();
                        string pathSvg = reader["svgid"].ToString();
                        object obj = _xaml.FindName(pathSvg);
                        if (obj == null)
                        {
                            throw new PathNotFoundException($"Path betweent {p1} and {p2} Not Found.");
                        }
                        if (!(obj is System.Windows.Shapes.Path))
                        {
                            throw new PathNotFoundException($"Path betweent {p1} and {p2} Not Found.");
                        }
                        System.Windows.Shapes.Path p = (System.Windows.Shapes.Path)obj;
                        System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush();
                        brush.Color = System.Windows.Media.Color.FromRgb(255, 0, 0);
                        p.Stroke = brush;
                        p.StrokeThickness = LightingPathStrokeThickness;
                        styleChanged[p] = true;
                        mstLength += Convert.ToInt32(reader["length"].ToString());
                    }
                }
            }
            return mstLength;
        }
    }
}
