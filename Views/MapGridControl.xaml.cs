using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrafficSim.Models;
using TrafficSim.ViewModels;

namespace TrafficSim.Views
{
    public partial class MapGridControl : UserControl
    {
        private Point _lastMousePosition;
        private bool _isPanning = false;
        private PointNodeVisualModel _draggedNode = null;
        private bool _isDraggingNode = false;

        public MapGridControl()
        {
            InitializeComponent();
        }

        private EditorViewModel ViewModel => DataContext as EditorViewModel;

        private void ViewportGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ViewportWidth = ViewportGrid.ActualWidth;
                ViewModel.ViewportHeight = ViewportGrid.ActualHeight;
            }
        }

        private void ViewportGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(ViewportGrid);
            ViewModel.UpdateCursorPosition(currentPosition.X, currentPosition.Y);

            if (_isPanning)
            {
                ViewModel.HandlePan(currentPosition.X - _lastMousePosition.X, currentPosition.Y - _lastMousePosition.Y);
                _lastMousePosition = currentPosition;
            }
        }

        private void ViewportGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ViewModel.HandleZoom(e.Delta);
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

        private void ViewportGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            Point clickPosition = e.GetPosition(ViewportGrid);

            ViewModel.HandleCanvasLeftClick(clickPosition.X, clickPosition.Y);
        }

        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var ellipse = (FrameworkElement)sender;
            var clickedNode = (PointNodeVisualModel)ellipse.DataContext;

            _isDraggingNode = true;
            _draggedNode = clickedNode;
            _lastMousePosition = e.GetPosition(ViewportGrid);

            ellipse.CaptureMouse();

            ViewModel.SelectNode(clickedNode);
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingNode && _draggedNode != null)
            {
                Point currentPosition = e.GetPosition(ViewportGrid);

                double deltaX = currentPosition.X - _lastMousePosition.X;
                double deltaY = currentPosition.Y - _lastMousePosition.Y;

                ViewModel.MoveNode(_draggedNode, deltaX, deltaY);

                _lastMousePosition = currentPosition;
            }
        }

        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingNode)
            {
                _isDraggingNode = false;
                _draggedNode = null;

                ((FrameworkElement)sender).ReleaseMouseCapture();
                e.Handled = true;
            }
        }
    }
}