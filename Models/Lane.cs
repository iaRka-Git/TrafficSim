using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace TrafficSim.Models
{
    public class Lane
    {
        public Guid Id { get; }

        public RoadNode StartNode { get; }
        public RoadNode EndNode { get; }

        public RoadGroup ParentGroup { get; }
        public double NormalOffset { get; } 

        public List<Point2D> Waypoints { get; set; } = new();

        public Lane(Guid id, RoadNode startNode, RoadNode endNode, RoadGroup parentGroup, double normalOffset)
        {
            Id = id;
            StartNode = startNode ?? throw new ArgumentNullException(nameof(startNode));
            EndNode = endNode ?? throw new ArgumentNullException(nameof(endNode));
            ParentGroup = parentGroup ?? throw new ArgumentNullException(nameof(parentGroup));
            NormalOffset = normalOffset;
        }
    }
}