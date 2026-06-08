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

        public void UpdateCursorPosition(double screenX, double screenY)
        {
            LogicalCursorPosition = CoordinateTransformService.ToLogical(
                screenX, screenY, ViewportWidth, ViewportHeight,
                Camera.Zoom, Camera.OffsetX, Camera.OffsetY);
        }
    }
}