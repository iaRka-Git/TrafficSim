using System;
using System.Collections.ObjectModel;
using TrafficSim.Models;
using TrafficSim.Services;

namespace TrafficSim.ViewModels.Items
{
    /// <summary>
    /// Візуальна модель дороги
    /// </summary>
    public class RoadVisualModel : BaseViewModel
    {
        public RoadGroup Model { get; }

        public ObservableCollection<LaneVisualModel> VisualLanes { get; } = new();

        public RoadVisualModel(RoadGroup model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));

            foreach (var lane in model.Lanes)
            {
                VisualLanes.Add(new LaneVisualModel(lane));
            }
        }

        /// <summary>
        /// Делегує перерахунок екранних координат усім дочірнім смугам
        /// </summary>
        public void UpdateScreenCoordinates(double viewportWidth, double viewportHeight, double zoom, double offsetX, double offsetY)
        {
            foreach (var laneVisual in VisualLanes)
            {
                laneVisual.UpdateScreenCoordinates(viewportWidth, viewportHeight, zoom, offsetX, offsetY);
            }
        }
    }
}