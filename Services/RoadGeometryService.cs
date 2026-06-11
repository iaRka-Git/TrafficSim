using System;
using System.Collections.Generic;
using TrafficSim.Models;

namespace TrafficSim.Services
{
    public static class RoadGeometryService
    {
        public static void GenerateWaypoints(RoadGroup group)
        {
            // Поки що генеруємо найпростіший шлях (пряму лінію)
            // TODO: Згодом сюди додамо розрахунок кривої Без'є (Splines) та радіус обрізки полігоном

            foreach (var lane in group.Lanes)
            {
                lane.Waypoints.Clear();

                // Вектор дороги
                double dx = group.EndNode.Position.X - group.StartNode.Position.X;
                double dy = group.EndNode.Position.Y - group.StartNode.Position.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);

                double dirX = dx / length;
                double dirY = dy / length;

                // Рахуємо нормаль для відступу конкретної смуги
                double nx = -dirY;
                double ny = dirX;

                // Стартова та кінцева точки смуги зі зсувом
                Point2D startWP = new Point2D(
                    group.StartNode.Position.X + nx * lane.NormalOffset,
                    group.StartNode.Position.Y + ny * lane.NormalOffset
                );

                Point2D endWP = new Point2D(
                    group.EndNode.Position.X + nx * lane.NormalOffset,
                    group.EndNode.Position.Y + ny * lane.NormalOffset
                );

                lane.Waypoints.Add(startWP);
                lane.Waypoints.Add(endWP);
            }
        }
    }
}