using System;
using System.Collections.Generic;

namespace TrafficSim.Models
{
    /// <summary>
    /// Правила пересічення перезрестя
    /// </summary>
    public class IntersectionRouting
    {
        public RoadNode OwnerNode { get; }

        private readonly HashSet<(Guid FromLaneId, Guid ToLaneId)> _allowedTurns = new();

        public IEnumerable<(Guid FromLaneId, Guid ToLaneId)> AllowedTurns => _allowedTurns;

        public IntersectionRouting(RoadNode ownerNode)
        {
            OwnerNode = ownerNode ?? throw new ArgumentNullException(nameof(ownerNode));
        }

        public void AllowTurn(Guid fromLaneId, Guid toLaneId)
        {
            _allowedTurns.Add((fromLaneId, toLaneId));
        }

        public bool IsTurnAllowed(Guid fromLaneId, Guid toLaneId)
        {
            return _allowedTurns.Contains((fromLaneId, toLaneId));
        }
        public void RemoveAllTurnsForLane(Guid laneId)
        {
            _allowedTurns.RemoveWhere(turn => turn.FromLaneId == laneId || turn.ToLaneId == laneId);
        }

        public void ClearAllTurns()
        {
            _allowedTurns.Clear();
        }
    }
}