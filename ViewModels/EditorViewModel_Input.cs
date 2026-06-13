using System;
using TrafficSim.Models;
using TrafficSim.Services;

namespace TrafficSim.ViewModels
{
    public partial class EditorViewModel
    {
        private Point2D _logicalCursorPosition;
        public Point2D LogicalCursorPosition
        {
            get => _logicalCursorPosition;
            set => SetProperty(ref _logicalCursorPosition, value);
        }

        public void UpdateCursorPosition(double screenX, double screenY)
        {
            LogicalCursorPosition = CoordinateTransformService.ToLogical(
                screenX, screenY, ViewportWidth, ViewportHeight,
                Camera.Zoom, Camera.OffsetX, Camera.OffsetY);

            if (CurrentMode == EditorMode.AddRoad && _roadStartNode != null)
            {
                Point2D startScreen = CoordinateTransformService.ToScreen(
                    _roadStartNode.Model.Position, ViewportWidth, ViewportHeight,
                    Camera.Zoom, Camera.OffsetX, Camera.OffsetY);

                PreviewRoadX1 = startScreen.X;
                PreviewRoadY1 = startScreen.Y;
                PreviewRoadX2 = screenX;
                PreviewRoadY2 = screenY;
            }
        }

        public void HandleCanvasLeftClick(double screenX, double screenY)
        {
            Point2D logicalPos = CoordinateTransformService.ToLogical(
                screenX, screenY, ViewportWidth, ViewportHeight,
                Camera.Zoom, Camera.OffsetX, Camera.OffsetY);

            switch (CurrentMode)
            {
                case EditorMode.AddNode:
                    AddNewNode(logicalPos);
                    break;

                case EditorMode.AddRoad:
                    // TODO
                    UpdateGrid();
                    break;

                case EditorMode.Select:
                    // TODO
                    UpdateGrid();
                    break;
            }
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