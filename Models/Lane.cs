using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrafficSim.Models
{
    public class Lane
    {
        public Guid Id { get; }
        public RoadNode LogicalStart { get; }
        public RoadNode LogicalEnd { get; }
        public double LaneWidth{ get; }
        public RoadGroup ParentGroup { get; }

        private readonly List<Point2D> _waypoints = new();
        public IReadOnlyList<Point2D> Waypoints => _waypoints;

        public double NormalOffset { get; }

        public Lane(RoadNode logicalStart, RoadNode logicalEnd, RoadGroup parentGroup, double normalOffset, Guid id,double? width = null)
        {
            LogicalStart = logicalStart ?? throw new ArgumentNullException(nameof(logicalStart));
            LogicalEnd = logicalEnd ?? throw new ArgumentNullException(nameof(logicalEnd));
            ParentGroup = parentGroup ?? throw new ArgumentNullException(nameof(parentGroup));
            NormalOffset = normalOffset;
            Id = id;
            LaneWidth = width ?? SimConfig.DefaultLaneWidth;
        }

        public void UpdateWaypoints(IEnumerable<Point2D> newWaypoints)
        {
            _waypoints.Clear();
            if (newWaypoints != null)
            {
                _waypoints.AddRange(newWaypoints);
            }
        }
    }
}