using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficSim.Models
{
    public abstract class PointNode
    {
        public Guid Id { get; }

        public Point2D Position { get; set; } = new Point2D();

        protected PointNode(Guid id, Point2D position)
        {
            Id = id;
            Position = position;
        }
    }
}
