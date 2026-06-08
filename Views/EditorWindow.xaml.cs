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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TrafficSim.ViewModels;

namespace TrafficSim.Views
{
    public partial class EditorWindow : Window
    {
        private Point _lastMousePosition;
        private bool _isPanning = false;

        public EditorWindow()
        {
            InitializeComponent();
        }

        private EditorViewModel ViewModel => DataContext as EditorViewModel;

        private void ViewportGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(ViewportGrid);

            ViewModel.UpdateCursorPosition(currentPosition.X, currentPosition.Y);

            if (_isPanning)
            {
                double deltaX = currentPosition.X - _lastMousePosition.X;
                double deltaY = currentPosition.Y - _lastMousePosition.Y;

                ViewModel.HandlePan(deltaX, deltaY);

                _lastMousePosition = currentPosition;
            }
        }

        private void ViewportGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ViewModel?.HandleZoom(e.Delta);
        }

        private void ViewportGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isPanning = true;
            _lastMousePosition = e.GetPosition(ViewportGrid);

            ViewportGrid.CaptureMouse();
        }

        private void ViewportGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            ViewportGrid.ReleaseMouseCapture();
        }

        private void ViewportGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                ViewportGrid.ReleaseMouseCapture();
            }
        }
        private void ViewportGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ViewportWidth = ViewportGrid.ActualWidth;
                ViewModel.ViewportHeight = ViewportGrid.ActualHeight;
            }
        }
    }
}
