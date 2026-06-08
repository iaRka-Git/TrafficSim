using System.Collections.ObjectModel;
using TrafficSim.Models;
using TrafficSim.Services;

namespace TrafficSim.ViewModels
{
    public partial class EditorViewModel
    {
        public CameraState Camera { get; } = new ();
        public ObservableCollection<GridLineModel> VerticalLines { get; } = new ();
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
        
        public void UpdateGrid()
        {
            if (ViewportWidth <= 0 || ViewportHeight <= 0) return;

            VerticalLines.Clear();
            HorizontalLines.Clear();
            AxisLabels.Clear();

            double trueCenterX = Camera.OffsetX + (ViewportWidth / 2);
            double trueCenterY = (ViewportHeight / 2) + Camera.OffsetY;

            StickyAxisX = Math.Clamp(trueCenterX, 0, ViewportWidth);
            StickyAxisY = Math.Clamp(trueCenterY, 0, ViewportHeight);

            double[] niceSteps = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000, 100000 };
            double step = niceSteps[niceSteps.Length - 1];
            foreach (double s in niceSteps)
            {
                if (s * Camera.Zoom >= 65)
                {
                    step = s;
                    break;
                }
            }

            Point2D topLeft = CoordinateTransformService.ToLogical(0, 0, ViewportWidth, ViewportHeight, Camera.Zoom, Camera.OffsetX, Camera.OffsetY);
            Point2D bottomRight = CoordinateTransformService.ToLogical(ViewportWidth, ViewportHeight, ViewportWidth, ViewportHeight, Camera.Zoom, Camera.OffsetX, Camera.OffsetY);

            double minX = Math.Min(topLeft.X, bottomRight.X);
            double maxX = Math.Max(topLeft.X, bottomRight.X);
            double minY = Math.Min(topLeft.Y, bottomRight.Y);
            double maxY = Math.Max(topLeft.Y, bottomRight.Y);

            double flipMargin = 25;

            double xLabelOffsetY = 2;
            if (trueCenterY > ViewportHeight - flipMargin)
            {
                xLabelOffsetY = -15;
            }
            else if (trueCenterY < flipMargin)
            {
                xLabelOffsetY = 2;
            }

            double startX = Math.Floor(minX / step) * step;
            for (double x = startX; x <= maxX; x += step)
            {
                double screenX = (x * Camera.Zoom) + Camera.OffsetX + (ViewportWidth / 2);
                if (screenX >= 0 && screenX <= ViewportWidth)
                {
                    VerticalLines.Add(new GridLineModel { ScreenPos = screenX });

                    AxisLabels.Add(new AxisMarking
                    {
                        ScreenX = screenX + 4,
                        ScreenY = StickyAxisY + xLabelOffsetY,
                        Text = x.ToString("F0")
                    });
                }
            }

            double startY = Math.Floor(minY / step) * step;
            for (double y = startY; y <= maxY; y += step)
            {
                if (y == 0) continue;

                double screenY = (ViewportHeight / 2) - (y * Camera.Zoom) + Camera.OffsetY;
                if (screenY >= 0 && screenY <= ViewportHeight)
                {
                    HorizontalLines.Add(new GridLineModel { ScreenPos = screenY });

                    string text = y.ToString("F0");
                    double yLabelOffsetX = 5;

                    if (trueCenterX > ViewportWidth - flipMargin)
                    {
                        yLabelOffsetX = -(text.Length * 7 + 5);
                    }
                    else if (trueCenterX < flipMargin)
                    {
                        yLabelOffsetX = 5;
                    }

                    AxisLabels.Add(new AxisMarking
                    {
                        ScreenX = StickyAxisX + yLabelOffsetX,
                        ScreenY = screenY - 6,
                        Text = text
                    });
                }
            }
        }
    }
}
