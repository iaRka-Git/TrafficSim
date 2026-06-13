using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrafficSim.Models.Items;
using TrafficSim.ViewModels;

namespace TrafficSim.Views
{
    public partial class MapWorkSpaceControl : UserControl
    {
        private Point _lastMousePosition;
        private bool _isPanning = false;

        private PointNodeVisualModel _draggedNode = null;
        private bool _isDraggingNode = false;

        public MapWorkSpaceControl()
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

        private void ViewportGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point currentPosition = e.GetPosition(ViewportGrid);

            // Фіксуємо точні координати для контекстного меню
            ViewModel?.SetContextMenuPosition(currentPosition.X, currentPosition.Y);

            // Якщо клікнули по дорозі — відкриється меню, карту НЕ панорамуємо
            if (e.OriginalSource is System.Windows.Shapes.Polyline) return;

            _isPanning = true;
            _lastMousePosition = currentPosition;
            ViewportGrid.CaptureMouse();
        }

        private void ViewportGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                _isPanning = false;
                ViewportGrid.ReleaseMouseCapture();
            }
        }

        private void ViewportGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPosition = e.GetPosition(ViewportGrid);
            ViewModel?.UpdateCursorPosition(currentPosition.X, currentPosition.Y);

            if (_isPanning)
            {
                ViewModel?.HandlePan(currentPosition.X - _lastMousePosition.X, currentPosition.Y - _lastMousePosition.Y);
                _lastMousePosition = currentPosition;
            }

            if (_isDraggingNode && _draggedNode != null)
            {
                double deltaX = currentPosition.X - _lastMousePosition.X;
                double deltaY = currentPosition.Y - _lastMousePosition.Y;

                ViewModel?.MoveNode(_draggedNode, deltaX, deltaY);
                _lastMousePosition = currentPosition;
            }
        }

        private void ViewportGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ViewModel?.HandleZoom(e.Delta);
        }

        private void ViewportGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element &&
                element.DataContext is PointNodeVisualModel clickedNode)
            {
                e.Handled = true;
                _isDraggingNode = true;
                _draggedNode = clickedNode;
                _lastMousePosition = e.GetPosition(ViewportGrid);

                element.CaptureMouse();
                ViewModel?.SelectNode(clickedNode);
                return;
            }

            Point clickPosition = e.GetPosition(ViewportGrid);
            ViewModel?.HandleCanvasLeftClick(clickPosition.X, clickPosition.Y);
        }

        private void ViewportGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingNode)
            {
                _isDraggingNode = false;
                _draggedNode = null;

                if (e.OriginalSource is FrameworkElement element)
                {
                    element.ReleaseMouseCapture();
                }
                e.Handled = true;
            }
        }
    }
}