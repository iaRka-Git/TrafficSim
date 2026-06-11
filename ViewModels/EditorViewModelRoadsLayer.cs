using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrafficSim.Models;

namespace TrafficSim.ViewModels
{
    partial class EditorViewModel
    {
        public ObservableCollection<RoadNodeVisualModel> VisualNodes { get; } = new();
        public ObservableCollection<PointNodeVisualModel> VisibleMobilityJoints { get; } = new();
        public void MoveNode(PointNodeVisualModel visualNode, double screenDeltaX, double screenDeltaY)
        {
            double logicalDeltaX = screenDeltaX / Camera.Zoom;
            double logicalDeltaY = -screenDeltaY / Camera.Zoom;

            visualNode.Model.Position = new Point2D(
                visualNode.Model.Position.X + logicalDeltaX,
                visualNode.Model.Position.Y + logicalDeltaY
            );

            visualNode.ScreenX += screenDeltaX;
            visualNode.ScreenY += screenDeltaY;
        }
        public void SelectLane(Lane lane)
        {
            VisibleMobilityJoints.Clear();

            foreach (var joint in lane.ControlPoints)
            {
                var jointVisual = new MobilityJointVisualModel(joint);
                VisibleMobilityJoints.Add(jointVisual);
            }

            UpdateGrid();
        }

    }
}
