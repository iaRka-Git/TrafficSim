using System;
using System.Collections.Generic;
using System.Linq;
using TrafficSim.Models;

namespace TrafficSim.Services
{
    /// <summary>
    /// Сервіс для математичної генерації полігонів перехресть
    /// Розраховує точки перетину сусідніх доріг для побудови правильного багатокутника відсікання
    /// </summary>
    public static class IntersectionGenerator
    {
        /// <summary>
        /// Перебудовує багатокутник перехрестя для заданого вузла
        /// </summary>
        public static IReadOnlyList<Point2D> RebuildPolygon(RoadNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));

            IReadOnlyList<RoadGroup> connectedGroups = GetUniqueGroups(node);
            List<Point2D> polygonVertices = new();

            if (connectedGroups.Count < 2) return polygonVertices;

            // Сортуємо дороги за годинниковою стрілкою (від -ПІ до ПІ)
            List<RoadGroup> sortedGroups = connectedGroups.OrderBy(g =>
            {
                Point2D dir = GetOutwardDirection(node, g);
                return Math.Atan2(dir.Y, dir.X);
            }).ToList();

            for (int i = 0; i < sortedGroups.Count; i++)
            {
                RoadGroup currentRoad = sortedGroups[i];
                RoadGroup nextRoad = sortedGroups[(i + 1) % sortedGroups.Count];

                (Point2D,Point2D) currentRightEdge = GetEdgeRay(node, currentRoad, isRightEdge: true);
                (Point2D, Point2D) nextLeftEdge = GetEdgeRay(node, nextRoad, isRightEdge: false);

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

        /// <summary>
        /// Отримує список унікальни[ доріг, що підключені до цього вузла
        /// </summary>
        private static IReadOnlyList<RoadGroup> GetUniqueGroups(RoadNode node)
        {
            return node.IncomingLanes.Select(l => l.ParentGroup)
                       .Concat(node.OutgoingLanes.Select(l => l.ParentGroup))
                       .Distinct()
                       .ToList();
        }

        /// <summary>
        /// Знаходить вектор напрямку дороги ВІД центру перехрестя НАЗОВНІ
        /// Враховує вигини, якщо дорога має контрольні точки.
        /// </summary>
        private static Point2D GetOutwardDirection(RoadNode node, RoadGroup group)
        {
            if (group.ControlPoints.Count == 0)
            {
                Point2D other = group.StartNode == node ? group.EndNode.Position : group.StartNode.Position;
                return RoadGeometryService.Normalize(new Point2D(other.X - node.Position.X, other.Y - node.Position.Y));
            }

            List<Point2D> pts = new(group.ControlPoints.Count + 2) { group.StartNode.Position };
            pts.AddRange(group.ControlPoints.Select(cp => cp.Position));
            pts.Add(group.EndNode.Position);

            if (group.StartNode == node)
            {
                // Дотична на початку (t=0)
                Point2D p1 = pts[0];
                Point2D p2 = pts[1];
                // Віддзеркалення для старту
                Point2D p0 = RoadGeometryService.Reflect(p2, p1);

                Point2D p3 = pts[2];

                Point2D deriv = RoadGeometryService.GetCatmullRomDerivative(0.0, p0, p1, p2, p3);
                return RoadGeometryService.Normalize(deriv);
            }
            else
            {
                // Дотична в кінці (t=1). Вектор має дивитися назовні, тому розвертаємо його (множимо на -1)
                int n = pts.Count;
                Point2D p2 = pts[n - 1];
                Point2D p1 = pts[n - 2];
                // Віддзеркалення для кінця
                Point2D p3 = RoadGeometryService.Reflect(p1,p2); 

                Point2D p0 = pts[n - 3];

                Point2D deriv = RoadGeometryService.GetCatmullRomDerivative(1.0, p0, p1, p2, p3);
                return RoadGeometryService.Normalize(new Point2D(-deriv.X, -deriv.Y));
            }
        }

        /// <summary>
        /// Знаходить координату початку та напрямок крайньої полоси дороги
        /// </summary>
        private static (Point2D Start, Point2D Dir) GetEdgeRay(RoadNode node, RoadGroup group, bool isRightEdge)
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

        /// <summary>
        /// Математичне знаходження точки перетину двох променів (алгоритм через детермінант)
        /// </summary>
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