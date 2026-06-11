using System;
using System.Collections.Generic;
using System.Linq;
using TrafficSim.Models;

namespace TrafficSim.Services
{
    public static class IntersectionGenerator
    {
        public static List<Point2D> RebuildPolygon(RoadNode node)
        {
            var connectedGroups = GetUniqueGroups(node);
            var polygonVertices = new List<Point2D>();

            if (connectedGroups.Count < 2)
                return polygonVertices; 

            var sortedGroups = connectedGroups.OrderBy(g =>
            {
                Point2D dir = GetOutwardDirection(node, g);
                return Math.Atan2(dir.Y, dir.X);
            }).ToList();

            for (int i = 0; i < sortedGroups.Count; i++)
            {
                var currentRoad = sortedGroups[i];
                var nextRoad = sortedGroups[(i + 1) % sortedGroups.Count];

                var currentRightEdge = GetEdgeRay(node, currentRoad, isRightEdge: true);
                var nextLeftEdge = GetEdgeRay(node, nextRoad, isRightEdge: false);

                if (TryIntersect(currentRightEdge, nextLeftEdge, out Point2D intersection))
                {
                    polygonVertices.Add(intersection);
                }
                else
                {
                    polygonVertices.Add(currentRightEdge.Start);
                }
            }

            return polygonVertices;
        }

        private static List<RoadGroup> GetUniqueGroups(RoadNode node)
        {
            return node.IncomingLanes.Select(l => l.ParentGroup)
                .Union(node.OutgoingLanes.Select(l => l.ParentGroup))
                .Distinct()
                .ToList();
        }

        private static Point2D GetOutwardDirection(RoadNode node, RoadGroup group)
        {
            RoadNode otherNode = group.StartNode == node ? group.EndNode : group.StartNode;

            double dx = otherNode.Position.X - node.Position.X;
            double dy = otherNode.Position.Y - node.Position.Y;

            double length = Math.Sqrt(dx * dx + dy * dy);
            return new Point2D(dx / length, dy / length);
        }

        private static (Point2D Start, Point2D Direction) GetEdgeRay(RoadNode node, RoadGroup group, bool isRightEdge)
        {
            Point2D dir = GetOutwardDirection(node, group);

            Point2D normal = new Point2D(-dir.Y, dir.X);

            double offset = group.TotalWidth / 2.0;
            if (!isRightEdge) offset = -offset;

            Point2D edgeStart = new Point2D(
                node.Position.X + normal.X * offset,
                node.Position.Y + normal.Y * offset
            );

            return (edgeStart, dir);
        }

        private static bool TryIntersect((Point2D Start, Point2D Dir) ray1, (Point2D Start, Point2D Dir) ray2, out Point2D intersection)
        {
            intersection = new Point2D(0, 0);
            double det = ray1.Dir.X * ray2.Dir.Y - ray1.Dir.Y * ray2.Dir.X;

            if (Math.Abs(det) < 0.0001) return false;

            double dx = ray2.Start.X - ray1.Start.X;
            double dy = ray2.Start.Y - ray1.Start.Y;

            double t1 = (dx * ray2.Dir.Y - dy * ray2.Dir.X) / det;

            intersection = new Point2D(
                ray1.Start.X + ray1.Dir.X * t1,
                ray1.Start.Y + ray1.Dir.Y * t1
            );
            return true;
        }
    }
}