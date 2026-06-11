using System.Collections.ObjectModel;
using TrafficSim.Models;
using TrafficSim.Services;

namespace TrafficSim.ViewModels
{
    public partial class EditorViewModel
    {
        public CameraState Camera { get; } = new ();
        public ObservableCollection<GridLineModel> VerticalLines { get; } = new();
        public ObservableCollection<GridLineModel> HorizontalLines { get; } = new ();
        public ObservableCollection<AxisMarking> AxisLabels { get; } = new();

        private double _stickyAxisX;
        public double StickyAxisX
        {
            get => _stickyAxisX;
            set => SetProperty(ref _stickyAxisX, value);
        }

        private double _stickyAxisY;
        public double StickyAxisY
        {
            get => _stickyAxisY;
            set => SetProperty(ref _stickyAxisY, value);
        }

        private double _viewportWidth;
        public double ViewportWidth
        {
            get => _viewportWidth;
            set { if (SetProperty(ref _viewportWidth, value)) UpdateGrid(); }
        }

        private double _viewportHeight;
        public double ViewportHeight
        {
            get => _viewportHeight;
            set { if (SetProperty(ref _viewportHeight, value)) UpdateGrid(); }
        }

        public void HandleZoom(double delta)
        {
            double zoomStep = 0.1;
            if (delta > 0) Camera.Zoom += zoomStep;
            else if (delta < 0) Camera.Zoom -= zoomStep;

            if (Camera.Zoom < 0.1) Camera.Zoom = 0.1;

            UpdateGrid();
        }

        public void HandlePan(double deltaX, double deltaY)
        {
            Camera.OffsetX += deltaX;
            Camera.OffsetY += deltaY;

            UpdateGrid();
        }
        
    }
}
