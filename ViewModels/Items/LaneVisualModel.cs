using System;
using System.Windows.Media;
using TrafficSim.Models;
using TrafficSim.Services;

namespace TrafficSim.ViewModels.Items
{
    /// <summary>
    /// Візуальна модель смуги
    /// </summary>
    public class LaneVisualModel : BaseViewModel
    {
        public Lane Model { get; }

        private PointCollection _screenPoints = new PointCollection();
        public PointCollection ScreenPoints
        {
            get => _screenPoints;
            set => SetProperty(ref _screenPoints, value);
        }

        private double _screenThickness = 4.0;
        public double ScreenThickness
        {
            get => _screenThickness;
            set => SetProperty(ref _screenThickness, value);
        }

        public LaneVisualModel(Lane model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void UpdateScreenCoordinates(double viewportWidth, double viewportHeight, double zoom, double offsetX, double offsetY)
        {
            if (Model.Waypoints == null || Model.Waypoints.Count == 0)
            {
                PointCollection ScreenPoints = new ();
                return;
            }

            ScreenThickness *= zoom;

            PointCollection newPoints = new (Model.Waypoints.Count);

            foreach (Point2D logicalPoint in Model.Waypoints)
            {
                Point2D screenPt = CoordinateTransformService.ToScreen(logicalPoint, viewportWidth, viewportHeight, zoom, offsetX, offsetY);

                newPoints.Add(new System.Windows.Point(screenPt.X, screenPt.Y));
            }

            ScreenPoints = newPoints;
        }
    }
}