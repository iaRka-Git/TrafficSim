using TrafficSim.Services;
using System;

namespace TrafficSim.Models
{
    public abstract class PointNodeVisualModel : BaseViewModel
    {
        public PointNode Model { get; }

        private double _screenX;
        public double ScreenX
        {
            get => _screenX;
            set => SetProperty(ref _screenX, value);
        }

        private double _screenY;
        public double ScreenY
        {
            get => _screenY;
            set => SetProperty(ref _screenY, value);
        }

        private double _diameter;
        public double Diameter
        {
            get => _diameter;
            set => SetProperty(ref _diameter, value);
        }

        protected PointNodeVisualModel(PointNode model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }

    public class RoadNodeVisualModel : PointNodeVisualModel
    {
        public RoadNodeVisualModel(RoadNode model) : base(model) { }
    }

    public class MobilityJointVisualModel : PointNodeVisualModel
    {
        public MobilityJointVisualModel(MobilityJoint model) : base(model) { }
    }
}