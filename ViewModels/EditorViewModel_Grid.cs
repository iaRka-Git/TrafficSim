using System;
using System.Collections.ObjectModel;
using TrafficSim.Models;
using TrafficSim.ViewModels.Items;

namespace TrafficSim.ViewModels
{
    public partial class EditorViewModel
    {
        public CameraState Camera { get; } = new();
        public ObservableCollection<GridLineModel> VerticalLines { get; } = new();
        public ObservableCollection<GridLineModel> HorizontalLines { get; } = new();
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

        public void UpdateGrid()
        {
            if (ViewportWidth <= 0 || ViewportHeight <= 0) return;

            VerticalLines.Clear();
            HorizontalLines.Clear();
            AxisLabels.Clear();

            double zoom = Camera.Zoom;
            double minorStep = zoom > 2 ? 5 : (zoom < 0.5 ? 50 : 10);
            double majorStep = minorStep * 10;

            double minX = (-Camera.OffsetX - (ViewportWidth / 2)) / zoom;
            double maxX = (ViewportWidth / 2 - Camera.OffsetX) / zoom;
            double minY = (-Camera.OffsetY - (ViewportHeight / 2)) / zoom;
            double maxY = (ViewportHeight / 2 - Camera.OffsetY) / zoom;

            double trueCenterX = (ViewportWidth / 2) + Camera.OffsetX;
            double trueCenterY = (ViewportHeight / 2) + Camera.OffsetY;

            double flipMargin = 40;
            StickyAxisX = Math.Clamp(trueCenterX, flipMargin, ViewportWidth - flipMargin);
            StickyAxisY = Math.Clamp(trueCenterY, flipMargin, ViewportHeight - flipMargin);

            double startX = Math.Floor(minX / minorStep) * minorStep;
            for (double x = startX; x <= maxX; x += minorStep)
            {
                if (x == 0) continue;

                double screenX = (x * zoom) + Camera.OffsetX + (ViewportWidth / 2);
                if (screenX >= 0 && screenX <= ViewportWidth)
                {
                    bool isMajor = Math.Abs((x / majorStep) - Math.Round(x / majorStep)) < 0.0001;

                    VerticalLines.Add(new GridLineModel
                    {
                        ScreenPos = screenX,
                        IsMajor = isMajor
                    });

                    if (isMajor || (minorStep * zoom >= 40))
                    {
                        double xLabelOffsetY = (trueCenterY < flipMargin) ? 5 : -20;
                        AxisLabels.Add(new AxisMarking
                        {
                            ScreenX = screenX + 2,
                            ScreenY = StickyAxisY + xLabelOffsetY,
                            IsMajor = isMajor
                        });
                    }
                }
            }

            double startY = Math.Floor(minY / minorStep) * minorStep;
            for (double y = startY; y <= maxY; y += minorStep)
            {
                if (y == 0) continue;

                double screenY = (ViewportHeight / 2) - (y * zoom) + Camera.OffsetY;
                if (screenY >= 0 && screenY <= ViewportHeight)
                {
                    bool isMajor = Math.Abs((y / majorStep) - Math.Round(y / majorStep)) < 0.0001;

                    HorizontalLines.Add(new GridLineModel
                    {
                        ScreenPos = screenY,
                        IsMajor = isMajor
                    });

                    if (isMajor || (minorStep * zoom >= 40))
                    {
                        double yLabelOffsetX = (trueCenterX > ViewportWidth - flipMargin) ? -35 : 5;
                        AxisLabels.Add(new AxisMarking
                        {
                            ScreenX = StickyAxisX + yLabelOffsetX,
                            ScreenY = screenY - 6,
                            IsMajor = isMajor
                        });
                    }
                }
            }

            UpdateVisualElementsPositions();
        }
    }
}