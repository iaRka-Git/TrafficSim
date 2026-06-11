using System;
using System.Collections.Generic;

namespace TrafficSim.Models
{
    public class RoadGroup
    {
        public Guid Id { get; }

        public RoadNode StartNode { get; }
        public RoadNode EndNode { get; }

        private readonly List<MobilityJoint> _controlPoints = new();
        public IReadOnlyList<MobilityJoint> ControlPoints => _controlPoints.AsReadOnly();

        private readonly List<Lane> _lanes = new();
        public IReadOnlyList<Lane> Lanes => _lanes.AsReadOnly();

        public const double DefaultLaneWidth = 3.0;
        public double TotalWidth => _lanes.Count * DefaultLaneWidth;

        public RoadGroup(Guid id, RoadNode startNode, RoadNode endNode)
        {
            Id = id;
            StartNode = startNode ?? throw new ArgumentNullException(nameof(startNode));
            EndNode = endNode ?? throw new ArgumentNullException(nameof(endNode));
        }

        public void AddControlPoint(MobilityJoint joint)
        {
            if (joint != null && !_controlPoints.Contains(joint))
                _controlPoints.Add(joint);
        }

        public void AddLane(Lane lane)
        {
            if (lane != null && !_lanes.Contains(lane))
            {
                _lanes.Add(lane);

                StartNode.RegisterLane(lane);
                EndNode.RegisterLane(lane);
            }
        }
    }
}