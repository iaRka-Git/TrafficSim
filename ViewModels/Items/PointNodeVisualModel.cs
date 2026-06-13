using System;
using TrafficSim.Models;
using TrafficSim.Services;

namespace TrafficSim.ViewModels.Items
{
    /// <summary>
    /// Візуальан модель абстрактного вузла
    /// </summary>
    public abstract class PointNodeVisualModel : BaseViewModel
    {
        public PointNode Model { get; }

        private double _diameter;
        public double Diameter
        {
            get => _diameter;
            set
            {
                if (SetProperty(ref _diameter, value))
                {
                    OnPropertyChanged(nameof(ScreenX));
                    OnPropertyChanged(nameof(ScreenY));
                }
            }
        }

        public double ScreenX => _centerX - (Diameter / 2);
        public double ScreenY => _centerY - (Diameter / 2);

        private double _centerX;
        private double _centerY;

        protected PointNodeVisualModel(PointNode model, double baseDiameter = 20)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _diameter = baseDiameter;
        }

        public void UpdateScreenPosition(double viewportWidth, double viewportHeight, double zoom, double offsetX, double offsetY)
        {
            Diameter *= zoom;

            var screenPt = CoordinateTransformService.ToScreen(Model.Position, viewportWidth, viewportHeight, zoom, offsetX, offsetY);

            _centerX = screenPt.X;
            _centerY = screenPt.Y;

            OnPropertyChanged(nameof(ScreenX));
            OnPropertyChanged(nameof(ScreenY));
        }
    }

    public class RoadNodeVisualModel : PointNodeVisualModel
    {
        public RoadNodeVisualModel(RoadNode model) : base(model) { }
    }

    public class MobilityJointVisualModel : PointNodeVisualModel
    {
        public RoadVisualModel ParentRoad { get; }
        public MobilityJointVisualModel(MobilityJoint model,RoadVisualModel parentRoad) : base(model,15) 
        {
            ParentRoad = parentRoad ?? throw new ArgumentNullException(nameof(parentRoad));
        }
    }
}