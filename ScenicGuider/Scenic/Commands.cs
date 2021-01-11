using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace ScenicGuider.Scenic
{
    class TestCommand1 : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MessageBox.Show("Test! Your Parameter: " + parameter.ToString());
        }
    }

    class NodeClickCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                Tuple<ScenicMap, Ellipse> tuple = (Tuple<ScenicMap, Ellipse>)parameter;
                ScenicMap map = tuple.Item1;
                Ellipse ellipse = tuple.Item2;
                SelectedNodeChangeEventArgs args = new SelectedNodeChangeEventArgs(ScenicMap.SelectedNodeChangeEvent, map);
                args.Message = ellipse.Name;
                map.RaiseEvent(args);
            }
            catch (InvalidCastException)
            {

                throw;
            }
        }
    }

    class CleanTextboxCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MessageBox.Show("Clean");
        }
    }

}
