using System;
using System.Collections.Generic;

namespace TrafficSim.Models
{
    /// <summary>
    /// Логічний вузол-перехрестя, що з'єднує дороги 
    /// </summary>
    public class RoadNode : PointNode
    {
        public IntersectionRouting Routing { get; }

        // Списки суміжності графа 
        private readonly List<Lane> _incomingLanes = new();
        public IReadOnlyList<Lane> IncomingLanes => _incomingLanes;

        private readonly List<Lane> _outgoingLanes = new();
        public IReadOnlyList<Lane> OutgoingLanes => _outgoingLanes;

        // фігура перехрестя
        private readonly List<Point2D> _intersectionPolygon = new();
        public IReadOnlyList<Point2D> IntersectionPolygon => _intersectionPolygon;

        public RoadNode(Guid id, Point2D position) : base(id, position)
        {
            Routing = new IntersectionRouting(this);
        }

        //internal методи
        internal void RegisterIncomingLane(Lane lane)
        {
            if (lane != null && !_incomingLanes.Contains(lane))
                _incomingLanes.Add(lane);
        }

        internal void RegisterOutgoingLane(Lane lane)
        {
            if (lane != null && !_outgoingLanes.Contains(lane))
                _outgoingLanes.Add(lane);
        }
        internal void UnregisterLane(Lane lane)
        {
            if (lane == null) return;

            bool wasRemoved = false;

            if (_incomingLanes.Contains(lane))
            {
                _incomingLanes.Remove(lane);
                wasRemoved = true;
            }

            if (_outgoingLanes.Contains(lane))
            {
                _outgoingLanes.Remove(lane);
                wasRemoved = true;
            }

            if (wasRemoved)
            {
                Routing.RemoveAllTurnsForLane(lane.Id);
            }
        }

        /// <summary>
        /// Оновлює геометричну форму перехрестя на основі підключених доріг
        /// </summary>
        public void UpdatePolygon(IEnumerable<Point2D> vertices)
        {
            _intersectionPolygon.Clear();
            if (vertices != null)
            {
                _intersectionPolygon.AddRange(vertices);
            }
        }
    }
}