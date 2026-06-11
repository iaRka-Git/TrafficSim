using System;
using System.Collections.Generic;

namespace TrafficSim.Models
{
    public class IntersectionRouting
    {
        public RoadNode OwnerNode { get; }

        public HashSet<(Guid FromLaneId, Guid ToLaneId)> AllowedTurns { get; } = new();

        public IntersectionRouting(RoadNode ownerNode)
        {
            OwnerNode = ownerNode ?? throw new ArgumentNullException(nameof(ownerNode));
        }

        public void AllowTurn(Guid fromLaneId, Guid toLaneId)
        {
            AllowedTurns.Add((fromLaneId, toLaneId));
        }

        public bool IsTurnAllowed(Guid fromLaneId, Guid toLaneId)
        {
            return AllowedTurns.Contains((fromLaneId, toLaneId));
        }
    }
}