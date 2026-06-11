using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficSim.Models
{
    public class MobilityJoint: PointNode
    {
        public MobilityJoint(Guid id, Point2D position) : base(id, position) { }
    }
}
