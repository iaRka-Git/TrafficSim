using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficSim.Models
{
    public class RoadNode:PointNode
    {
        public List<Lane> IncomingLanes { get; } = new();
        public List<Lane> OutgoingLanes { get; } = new();

        public IntersectionRouting Routing { get; }

        public RoadNode(Guid id, Point2D position) : base(id, position)
        {
            Routing = new IntersectionRouting(this);
        }

        public void RegisterLane(Lane lane)
        {
            if (lane == null) throw new ArgumentNullException(nameof(lane));

            if (lane.EndNode == this && !IncomingLanes.Contains(lane))
                IncomingLanes.Add(lane);

            if (lane.StartNode == this && !OutgoingLanes.Contains(lane))
                OutgoingLanes.Add(lane);
        }

    }
}
