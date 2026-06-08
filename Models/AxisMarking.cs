using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficSim.Models
{
    public class AxisMarking
    {
        public double ScreenX { get; set; }
        public double ScreenY { get; set; }
        public string Text { get; set; }
    }

    public class GridLineModel
    {
        public double ScreenPos { get; set; }
    }
}
