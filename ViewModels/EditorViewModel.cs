using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TrafficSim.Models;
using TrafficSim.Services;

namespace TrafficSim.ViewModels
{
    public partial class EditorViewModel : BaseViewModel
    {

        private Point2D _logicalCursorPosition;
        public Point2D LogicalCursorPosition
        {
            get => _logicalCursorPosition;
            set => SetProperty(ref _logicalCursorPosition, value);
        }

        private EditorMode _currentMode = EditorMode.Select;
        public EditorMode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (SetProperty(ref _currentMode, value))
                {
                    ClearSelection();
                }
            }
        }

        private bool _isMenuOpen;
        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set => SetProperty(ref _isMenuOpen, value);
        }
        public ICommand ChangeModeCommand { get; }
        public ICommand CloseMenuAndResetCommand { get; }
        public ICommand DeleteNodeCommand { get; }
        public ICommand ShowNodePropertiesCommand { get; }

        public EditorViewModel()
        {
            ChangeModeCommand = new RelayCommand(param => SetMode((EditorMode)param));
            CloseMenuAndResetCommand = new RelayCommand(_ => CloseMenuAndReset());
            DeleteNodeCommand = new RelayCommand(ExecuteDeleteNode);
            ShowNodePropertiesCommand = new RelayCommand(ExecuteShowNodeProperties);
        }
        private void ExecuteDeleteNode(object parameter)
        {
            if (parameter is RoadNodeVisualModel nodeToDelete)
            {
                VisualNodes.Remove(nodeToDelete);
                // Graph.Nodes.Remove(nodeToDelete.Model); 
            }
        }

        private void ExecuteShowNodeProperties(object parameter)
        {
            if (parameter is RoadNodeVisualModel node)
            {
                MessageBox.Show($"Властивості вузла: {node.Model.Position}");
            }
        }

        public void SelectNode(PointNodeVisualModel node)
        {
        }

        private void CloseMenuAndReset()
        {
            IsMenuOpen = false;
            SetMode(EditorMode.Select);
            ClearSelection();
        }

        public void SetMode(EditorMode newMode)
        {
            if (CurrentMode != newMode)
            {
                CurrentMode = newMode;
                ClearSelection();
            }
        }

        public void UpdateCursorPosition(double screenX, double screenY)
        {
            LogicalCursorPosition = CoordinateTransformService.ToLogical(
                screenX, screenY, ViewportWidth, ViewportHeight,
                Camera.Zoom, Camera.OffsetX, Camera.OffsetY);
        }

        public void HandleCanvasLeftClick(double screenX, double screenY)
        {
            Point2D logicalPos = CoordinateTransformService.ToLogical(
                        screenX, screenY, ViewportWidth, ViewportHeight,
                        Camera.Zoom, Camera.OffsetX, Camera.OffsetY);

            switch (CurrentMode)
            {
                case EditorMode.AddNode:

                    RoadNode pureNode = new(Guid.NewGuid(), logicalPos);

                    RoadNodeVisualModel visualNode = new(pureNode);

                    VisualNodes.Add(visualNode);

                    UpdateGrid();
                    break;

                case EditorMode.AddRoad:
                    UpdateGrid();
                    break;
                case EditorMode.Select:
                    UpdateGrid();
                    break;
            }
        }

        

        private void ClearSelection()
        {
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

                double baseDiameter = 12.0;

                foreach (var visualNode in VisualNodes)
                {
                    Point2D screenPos = CoordinateTransformService.ToScreen(
                        visualNode.Model.Position,
                        ViewportWidth, ViewportHeight,
                        Camera.Zoom, Camera.OffsetX, Camera.OffsetY
                    );

                    double currentDiameter = Math.Max(4.0, baseDiameter * Camera.Zoom);
                    double radius = currentDiameter / 2.0;

                    visualNode.ScreenX = screenPos.X - radius;
                    visualNode.ScreenY = screenPos.Y - radius;
                    visualNode.Diameter = currentDiameter;
                }

                foreach (var jointNode in VisibleMobilityJoints)
                {
                    Point2D screenPos = CoordinateTransformService.ToScreen(jointNode.Model.Position, ViewportWidth, ViewportHeight, Camera.Zoom, Camera.OffsetX, Camera.OffsetY);
                    double currentDiameter = Math.Max(4.0, (baseDiameter - 2) * Camera.Zoom); // квадратики трохи менші
                    jointNode.ScreenX = screenPos.X - (currentDiameter / 2.0);
                    jointNode.ScreenY = screenPos.Y - (currentDiameter / 2.0);
                    jointNode.Diameter = currentDiameter;
                }
            }
        }
    }
}