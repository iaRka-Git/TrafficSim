using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficSim.Models
{
    public static class SimConfig
    {
        public static double DefaultLaneWidth { get; set; } = 3.5;

        public static double ResolutionStep { get; set; } = 0.5;
    }
}
