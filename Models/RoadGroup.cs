using System;
using System.Collections.Generic;
using TrafficSim.Services; 

namespace TrafficSim.Models
{
    /// <summary>
    /// Логічна дорога(група смуг) між двома перехрестями
    /// </summary>
    public class RoadGroup
    {
        public Guid Id { get; } = Guid.NewGuid();

        public RoadNode StartNode { get; }
        public RoadNode EndNode { get; }

        // Приватна колекція смуг
        private readonly List<Lane> _lanes = new();
        public IReadOnlyList<Lane> Lanes => _lanes;

        // Контрольні точки для кривої Катмул-Рома
        private readonly List<MobilityJoint> _controlPoints = new();
        public IReadOnlyList<MobilityJoint> ControlPoints => _controlPoints;

        public double LaneWidth { get; set; }

        public double TotalWidth => _lanes.Count * LaneWidth;

        // Параметри обрізання об багатокутники перехресть
        public double TrimmedStartParameter { get; set; } = 0.0;
        public double TrimmedEndParameter { get; set; } = 1.0;

        public RoadGroup(
            RoadNode startNode,
            RoadNode endNode,
            IEnumerable<Lane> initialLanes = null,
            double? customLaneWidth = null)
        {
            StartNode = startNode ?? throw new ArgumentNullException(nameof(startNode));
            EndNode = endNode ?? throw new ArgumentNullException(nameof(endNode));

            LaneWidth = customLaneWidth ?? SimConfig.DefaultLaneWidth;

            if (initialLanes != null)
            {
                AddLanes(initialLanes);
            }
        }

        public void AddLane(Lane lane)
        {
            if (lane == null) return;

            if (!_lanes.Contains(lane))
            {
                _lanes.Add(lane);

                lane.LogicalStart.RegisterOutgoingLane(lane);
                lane.LogicalEnd.RegisterIncomingLane(lane);
            }
        }

        public void AddLanes(IEnumerable<Lane> lanes)
        {
            if (lanes == null) return;

            foreach (Lane one in lanes)
            {
                AddLane(one);
            }
        }

        public void UpdateControlPoints(IEnumerable<MobilityJoint> points)
        {
            _controlPoints.Clear();
            if (points != null)
            {
                _controlPoints.AddRange(points);
            }
        }
    }
}