using System;

namespace TrafficSim.Models
{
    /// <summary>
    /// Базовий абстрактний клас для будь-якої точки на карті
    /// </summary>
    public abstract class PointNode
    {
        // Унікальний ідентифікатор вузла
        public Guid Id { get; }
        public Point2D Position { get; set; }

        protected PointNode(Guid id, Point2D position)
        {
            Id = id;
            Position = position;
        }
    }
}