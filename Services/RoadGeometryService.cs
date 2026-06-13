using System;
using System.Collections.Generic;
using System.Linq;
using TrafficSim.Models;

namespace TrafficSim.Services
{
    public static class RoadGeometryService
    {
        public static void GenerateWaypoints(RoadGroup group)
        {
            if (group == null || group.Lanes.Count == 0) return;

            List<Point2D> controlPoints = new() { group.StartNode.Position };
            controlPoints.AddRange(group.ControlPoints.Select(cp => cp.Position));
            controlPoints.Add(group.EndNode.Position);

            List<(Point2D Position, Point2D Normal)> centerWaypoints = new();

            if (controlPoints.Count == 2)
            {
                // Проста лінія
                Point2D p0 = controlPoints[0];
                Point2D p1 = controlPoints[1];
                double dx = p1.X - p0.X;
                double dy = p1.Y - p0.Y;
                double len = Math.Sqrt(dx * dx + dy * dy);

                Point2D dir = len > 0.0001 ? new Point2D(dx / len, dy / len) : new Point2D(1, 0);
                Point2D normal = new Point2D(-dir.Y, dir.X);

                int steps = (int)Math.Max(2, len /SimConfig.ResolutionStep);
                for (int i = 0; i <= steps; i++)
                {
                    double t = (double)i / steps;
                    centerWaypoints.Add((new Point2D(p0.X + dx * t, p0.Y + dy * t), normal));
                }
            }
            else
            {
                // Catmull-Rom сплайн
                int segments = controlPoints.Count - 1;
                for (int i = 0; i < segments; i++)
                {
                    Point2D p0 = i == 0 ? Reflect(controlPoints[1], controlPoints[0]) : controlPoints[i - 1];
                    Point2D p1 = controlPoints[i];
                    Point2D p2 = controlPoints[i + 1];
                    Point2D p3 = i == segments - 1 ? Reflect(controlPoints[i], controlPoints[i + 1]) : controlPoints[i + 2];

                    double dist = Distance(p1, p2);
                    int steps = (int)Math.Max(5, dist / SimConfig.ResolutionStep);

                    int startStep = i == 0 ? 0 : 1;

                    for (int j = startStep; j <= steps; j++)
                    {
                        double t = (double)j / steps;
                        Point2D pos = GetCatmullRomPosition(t, p0, p1, p2, p3);
                        Point2D deriv = GetCatmullRomDerivative(t, p0, p1, p2, p3);
                        Point2D dir = Normalize(deriv);
                        Point2D normal = new Point2D(-dir.Y, dir.X);
                        centerWaypoints.Add((pos, normal));
                    }
                }
            }

            // обрізання та зсув смуг
            foreach (var lane in group.Lanes)
            {
                var lanePoints = new List<Point2D>();
                foreach (var (pos, normal) in centerWaypoints)
                {
                    // Зсув смуги 
                    Point2D offsetPos = new Point2D(
                        pos.X + normal.X * lane.NormalOffset,
                        pos.Y + normal.Y * lane.NormalOffset
                    );

                    // обрізання
                    if (IsPointInsidePolygon(offsetPos, group.StartNode.IntersectionPolygon)) continue;
                    if (IsPointInsidePolygon(offsetPos, group.EndNode.IntersectionPolygon)) continue;

                    lanePoints.Add(offsetPos);
                }
                lane.UpdateWaypoints(lanePoints); 
            }
        }

        public static Point2D Reflect(Point2D p1, Point2D p0) => new Point2D(p0.X + (p0.X - p1.X), p0.Y + (p0.Y - p1.Y));
        private static double Distance(Point2D a, Point2D b) => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

        public static Point2D Normalize(Point2D v)
        {
            double len = Math.Sqrt(v.X * v.X + v.Y * v.Y);
            return len < 0.0001 ? new Point2D(1, 0) : new Point2D(v.X / len, v.Y / len);
        }

        public static Point2D GetCatmullRomPosition(double t, Point2D p0, Point2D p1, Point2D p2, Point2D p3)
        {
            double t2 = t * t;
            double t3 = t2 * t;

            double x = 0.5 * ((2.0 * p1.X) + (-p0.X + p2.X) * t + (2.0 * p0.X - 5.0 * p1.X + 4.0 * p2.X - p3.X) * t2 + (-p0.X + 3.0 * p1.X - 3.0 * p2.X + p3.X) * t3);
            double y = 0.5 * ((2.0 * p1.Y) + (-p0.Y + p2.Y) * t + (2.0 * p0.Y - 5.0 * p1.Y + 4.0 * p2.Y - p3.Y) * t2 + (-p0.Y + 3.0 * p1.Y - 3.0 * p2.Y + p3.Y) * t3);

            return new Point2D(x, y);
        }

        public static Point2D GetCatmullRomDerivative(double t, Point2D p0, Point2D p1, Point2D p2, Point2D p3)
        {
            double t2 = t * t;
            double dx = 0.5 * ((-p0.X + p2.X) + 2.0 * (2.0 * p0.X - 5.0 * p1.X + 4.0 * p2.X - p3.X) * t + 3.0 * (-p0.X + 3.0 * p1.X - 3.0 * p2.X + p3.X) * t2);
            double dy = 0.5 * ((-p0.Y + p2.Y) + 2.0 * (2.0 * p0.Y - 5.0 * p1.Y + 4.0 * p2.Y - p3.Y) * t + 3.0 * (-p0.Y + 3.0 * p1.Y - 3.0 * p2.Y + p3.Y) * t2);

            return new Point2D(dx, dy);
        }

        // Алгоритм Ray-Casting для перевірки, чи лежить точка всередині багатокутника (для обрізання дороги)
        private static bool IsPointInsidePolygon(Point2D pt, IReadOnlyList<Point2D> polygon)
        {
            if (polygon == null || polygon.Count < 3) return false;
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > pt.Y) != (polygon[j].Y > pt.Y)) &&
                    (pt.X < (polygon[j].X - polygon[i].X) * (pt.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}