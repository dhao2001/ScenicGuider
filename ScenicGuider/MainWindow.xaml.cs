using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using ScenicGuider.Scenic;

namespace ScenicGuider
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        ScenicMap map;

        private string StatusBarLeftText
        {
            get
            {
                return stbarLeftTextBlock.Text;
            }
            set
            {
                stbarLeftTextBlock.Text = value;
            }
        }
        
        private ICommand _cleanTextboxCommand = new CleanTextboxCommand();
        public ICommand CleanTextboxCommand
        {
            get
            {
                return _cleanTextboxCommand;
            }
            set
            {
                _cleanTextboxCommand = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void menuLoadMap_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
            openFile.Filter = "Map XAML File|*.xaml|All Files|*.*";
            openFile.FileName = string.Empty;
            openFile.RestoreDirectory = true;

            Nullable<bool> result = openFile.ShowDialog();
            if (result != true)
            {
                return;
            }

            /*
            try
            {
                string xmlpath = openFile.FileName;
                XmlReader xmlReader = XmlReader.Create(xmlpath);
                object obj = XamlReader.Load(xmlReader);
                FrameworkElement frameworkElement = (FrameworkElement)obj;
                gridMain.Children.Add(frameworkElement);
            }
            catch (Exception)
            {

                throw;
            }
            */
            map = new ScenicMap(openFile.FileName);
            try
            {
                map.Load();
                map.SelectedNodeChange += on_SelectedNode_Changed;
                gridMap.Children.Add(map.XamlRoot);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void txtBoxScenicName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtBoxScenicName.Text = string.Empty;
            txtBlockDescription.Text = string.Empty;
            map.HighlightNode = null;
        }

        private void on_SelectedNode_Changed(object sender, SelectedNodeChangeEventArgs e)
        {
            showNodeDetail(e);
            map.SetHighlightNode(e.Message);
        }

        private void showNodeDetail(SelectedNodeChangeEventArgs e)
        {
            Tuple<string, string> tuple = map.GetNodeDescription(e.Message);

            txtBoxScenicName.Text = tuple.Item1;
            txtBlockDescription.Text = tuple.Item2;
        }

        private void btnSetStart_Click(object sender, RoutedEventArgs e)
        {
            txtboxStartPoint.Text = txtBoxScenicName.Text;
        }

        private void btnSetEnd_Click(object sender, RoutedEventArgs e)
        {
            txtboxEndPoint.Text = txtBoxScenicName.Text;
        }

        private void btnCalcButton_Click(object sender, RoutedEventArgs e)
        {
            string startPoint = txtboxStartPoint.Text;
            string endPoint = txtboxEndPoint.Text;
            if (string.IsNullOrWhiteSpace(startPoint))
            {
                string msg = Application.Current.FindResource("StartPointCantbeEmpty").ToString();
                MessageBox.Show(msg);
                return;
            }
            if (string.IsNullOrWhiteSpace(endPoint))
            {
                string msg = Application.Current.FindResource("EndPointCantbeEmpty").ToString();
                MessageBox.Show(msg);
                return;
            }
            if (string.Equals(startPoint, endPoint))
            {
                string msg = Application.Current.FindResource("StartEndCantbeSame").ToString();
                MessageBox.Show(msg);
                return;
            }
            try
            {
                int pathlen = map.DrawShortestPath(startPoint, endPoint);
                StatusBarLeftText = $"{Application.Current.FindResource("CurrentPathLength")} {pathlen}";
            }
            catch (PathNotFoundException excption)
            {
                StatusBarLeftText = Application.Current.FindResource("ErrorWhileFindingPath").ToString();
                MessageBox.Show(excption.Message + Application.Current.FindResource("MakeSureConnected").ToString());
            }
            catch (Exception excption)
            {
                StatusBarLeftText = Application.Current.FindResource("ErrorWhileFindingPath").ToString();
                MessageBox.Show(excption.Message + Application.Current.FindResource("MakeSureConnected").ToString());
            }
        }

        private void btnCalcAllPath_Click(object sender, RoutedEventArgs e)
        {
            string startPoint = txtboxStartPoint.Text;
            string endPoint = txtboxEndPoint.Text;
            if (string.IsNullOrWhiteSpace(startPoint))
            {
                string msg = Application.Current.FindResource("StartPointCantbeEmpty").ToString();
                MessageBox.Show(msg);
                return;
            }
            if (string.IsNullOrWhiteSpace(endPoint))
            {
                string msg = Application.Current.FindResource("EndPointCantbeEmpty").ToString();
                MessageBox.Show(msg);
                return;
            }
            if (string.Equals(startPoint, endPoint))
            {
                string msg = Application.Current.FindResource("StartEndCantbeSame").ToString();
                MessageBox.Show(msg);
                return;
            }
            try
            {
                List<List<int>> paths = map.GetAllPaths(txtboxStartPoint.Text, txtboxEndPoint.Text);
                listBoxPaths.ItemsSource = paths;
            }
            catch (PathNotFoundException excption)
            {
                StatusBarLeftText = Application.Current.FindResource("ErrorWhileFindingPath").ToString();
                MessageBox.Show(excption.Message + Application.Current.FindResource("MakeSureConnected").ToString());
            }
        }

        private void cleanTextbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox txb = (TextBox)sender;
            txb.Text = string.Empty;
        }

        private void menuGetMST_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int len = map.DrawMSTPaths();
                StatusBarLeftText = $"{Application.Current.FindResource("CurrentMSTLength")} {len}";
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(Application.Current.FindResource("MapNotLoaded").ToString());
                StatusBarLeftText = Application.Current.FindResource("ErrorWhileMST").ToString();
            }
        }

        private void menuSetEnglish_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            ResourceDictionary resourceDictionary = new ResourceDictionary();
            Uri uri = new Uri(@"/Resources/Languages/Locale.en-US.xaml", UriKind.Relative);
            resourceDictionary.Source = uri;
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        private void menuSetChinese_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            ResourceDictionary resourceDictionary = new ResourceDictionary();
            Uri uri = new Uri(@"/Resources/Languages/Locale.zh-CN.xaml", UriKind.Relative);
            resourceDictionary.Source = uri;
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        private void listBoxPaths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<int> path = listBoxPaths.SelectedItem as List<int>;
            if (path == null)
            {
                return;
            }
            int len = map.DrawPath(path);
            StatusBarLeftText = $"{Application.Current.FindResource("CurrentPathLength")} {len}";
        }

        private void menuAboutItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Power By DHao2001 at CSU. 2021", "About", MessageBoxButton.OK);
        }
    }
}
