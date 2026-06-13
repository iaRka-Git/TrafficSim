namespace TrafficSim.Models
{
    /// <summary>
    /// Модель для позначок на осях координат
    /// </summary>
    public struct AxisMarking
    {
        public double ScreenX { get; set; }
        public double ScreenY { get; set; }
        public bool IsMajor { get; set; }
    }

    /// <summary>
    /// Модель для ліній сітки
    /// </summary>
    public struct GridLineModel
    {
        public double ScreenPos { get; set; }
        public bool IsMajor { get; set; }
    }
}