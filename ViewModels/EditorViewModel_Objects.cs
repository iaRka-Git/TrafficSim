using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TrafficSim.Models;
using TrafficSim.Services;
using TrafficSim.ViewModels.Items;

namespace TrafficSim.ViewModels
{
    public partial class EditorViewModel
    {
        private RoadNodeVisualModel? _roadStartNode;
        public ObservableCollection<RoadVisualModel> VisualRoads { get; } = new();
        public ObservableCollection<RoadNodeVisualModel> VisualNodes { get; } = new();
        public ObservableCollection<PointNodeVisualModel> VisibleMobilityJoints { get; } = new();


        private bool _isPreviewingRoad;
        public bool IsPreviewingRoad { get => _isPreviewingRoad; set => SetProperty(ref _isPreviewingRoad, value); }

        private double _previewRoadX1;
        public double PreviewRoadX1 { get => _previewRoadX1; set => SetProperty(ref _previewRoadX1, value); }

        private double _previewRoadY1;
        public double PreviewRoadY1 { get => _previewRoadY1; set => SetProperty(ref _previewRoadY1, value); }

        private double _previewRoadX2;
        public double PreviewRoadX2 { get => _previewRoadX2; set => SetProperty(ref _previewRoadX2, value); }

        private double _previewRoadY2;
        public double PreviewRoadY2 { get => _previewRoadY2; set => SetProperty(ref _previewRoadY2, value); }

        public Point2D LastRightClickLogicalPosition { get; private set; }
        public void AddRoadNode(Point2D logicalPosition)
        {
            RoadNode nodeModel = new(Guid.NewGuid(), logicalPosition);

            RoadNodeVisualModel visualNode = new(nodeModel);

            visualNode.UpdateScreenPosition(ViewportWidth, ViewportHeight, Camera.Zoom, Camera.OffsetX, Camera.OffsetY);

            VisualNodes.Add(visualNode);
        }

        public void DeleteRoadNode(RoadNodeVisualModel visualNode)
        {
            if (visualNode == null) return;
            RoadNode nodeToDelete = (RoadNode)visualNode.Model;

            var connectedRoadModels = nodeToDelete.IncomingLanes.Select(l => l.ParentGroup)
                .Concat(nodeToDelete.OutgoingLanes.Select(l => l.ParentGroup))
                .Distinct()
                .ToList();

            var roadsToDelete = VisualRoads.Where(vr => connectedRoadModels.Contains(vr.Model)).ToList();

            foreach (var roadVM in roadsToDelete)
            {
                var jointsToRemove = VisibleMobilityJoints
                    .OfType<MobilityJointVisualModel>()
                    .Where(j => j.ParentRoad == roadVM).ToList();

                foreach (var j in jointsToRemove)
                    VisibleMobilityJoints.Remove(j);

                RoadNode survivorNode = roadVM.Model.StartNode == nodeToDelete ? roadVM.Model.EndNode : roadVM.Model.StartNode;
                foreach (var lane in roadVM.Model.Lanes)
                {
                    nodeToDelete.UnregisterLane(lane);
                    survivorNode.UnregisterLane(lane);
                }

                survivorNode.UpdatePolygon(IntersectionGenerator.RebuildPolygon(survivorNode));

                VisualRoads.Remove(roadVM);
            }

            VisualNodes.Remove(visualNode);

            if (_roadStartNode == visualNode)
            {
                _roadStartNode = null;
                IsPreviewingRoad = false;
            }

            UpdateVisualElementsPositions();
        }
        public void MoveNode(PointNodeVisualModel visualNode, double screenDeltaX, double screenDeltaY)
        {
            double logicalDeltaX = screenDeltaX / Camera.Zoom;
            double logicalDeltaY = -screenDeltaY / Camera.Zoom;

            visualNode.Model.Position = new Point2D(
                visualNode.Model.Position.X + logicalDeltaX,
                visualNode.Model.Position.Y + logicalDeltaY
            );

            if (visualNode.Model is RoadNode roadNode)
            {
                var affectedGroups = new HashSet<RoadGroup>();
                foreach (var lane in roadNode.IncomingLanes) affectedGroups.Add(lane.ParentGroup);
                foreach (var lane in roadNode.OutgoingLanes) affectedGroups.Add(lane.ParentGroup);

                foreach (var group in affectedGroups)
                {
                    RoadGeometryService.GenerateWaypoints(group);
                }
            }
            else if (visualNode.Model is MobilityJoint joint)
            {
                var affectedGroup = VisualRoads.Select(vr => vr.Model).FirstOrDefault(g => g.ControlPoints.Contains(joint));
                if (affectedGroup != null)
                {
                    RoadGeometryService.GenerateWaypoints(affectedGroup);
                }
            }

            UpdateVisualElementsPositions();
        }

        private void ExecuteShowNodeProperties(object parameter)
        {
            if (parameter is RoadNodeVisualModel node)
            {
                MessageBox.Show($"Властивості вузла: {node.Model.Position}");
            }
        }

        public void SelectNode(PointNodeVisualModel node)
        {
            if (node is RoadNodeVisualModel clickedRoadNode)
            {
                if (CurrentMode == EditorMode.AddRoad)
                {
                    if (_roadStartNode == null)
                    {
                        _roadStartNode = clickedRoadNode;
                        IsPreviewingRoad = true;
                    }
                    else
                    {
                        if (_roadStartNode != clickedRoadNode)
                        {
                            CreateRoadGroup((RoadNode)_roadStartNode.Model, (RoadNode)clickedRoadNode.Model);
                        }
                        _roadStartNode = null;
                        IsPreviewingRoad = false;
                    }
                }
            }
        }
        private void CreateRoadGroup(RoadNode start, RoadNode end)
        {
            // 1. Створюємо логічну групу доріг
            var group = new RoadGroup(start, end);

            // 2. Створюємо смуги (використовуємо твій оригінальний конструктор та ширину з конфігу)
            var lane1 = new Lane(start, end, group, SimConfig.DefaultLaneWidth, Guid.NewGuid());
            var lane2 = new Lane(end, start, group, -SimConfig.DefaultLaneWidth, Guid.NewGuid());
            group.AddLane(lane1);
            group.AddLane(lane2);

            // 3. Створюємо візуальну модель дороги
            var visualRoad = new RoadVisualModel(group);
            VisualRoads.Add(visualRoad);

            // 4. Додаємо тестову контрольну точку (MobilityJoint) для вигину
            var visualJoint = new MobilityJointVisualModel(new MobilityJoint(Guid.NewGuid(), new Point2D((start.Position.X + end.Position.X) / 2, (start.Position.Y + end.Position.Y) / 2 + 15)));
            group.AddControlPoint((MobilityJoint)visualJoint.Model);
            VisibleMobilityJoints.Add(visualJoint);

            // [НОВИЙ ФУНКЦІОНАЛ]: Перебудовуємо багатокутники перехресть для стартового та кінцевого вузлів
            start.UpdatePolygon(IntersectionGenerator.RebuildPolygon(start));
            end.UpdatePolygon(IntersectionGenerator.RebuildPolygon(end));

            // 5. Генеруємо точки сплайнів (вони тепер автоматично обріжуться об полігони, які ми щойно згенерували)
            RoadGeometryService.GenerateWaypoints(group);

            // 6. Оновлюємо екран
            UpdateVisualElementsPositions();
        }

        public void SetContextMenuPosition(double screenX, double screenY)
        {
            LastRightClickLogicalPosition = CoordinateTransformService.ToLogical(
                screenX, screenY, ViewportWidth, ViewportHeight,
                Camera.Zoom, Camera.OffsetX, Camera.OffsetY);
        }


        public void AddMobilityJoint(RoadVisualModel? roadVM)
        {
            if (roadVM == null) return;

            RoadGroup road = roadVM.Model;
            Point2D logicalPosition = LogicalCursorPosition;

            var pts = new List<Point2D> { road.StartNode.Position };
            pts.AddRange(road.ControlPoints.Select(cp => cp.Position));
            pts.Add(road.EndNode.Position);

            int insertIndex = 0;
            double minDistance = double.MaxValue;

            for (int i = 0; i < pts.Count - 1; i++)
            {
                double dist = DistanceToSegment(logicalPosition, pts[i], pts[i + 1]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    insertIndex = i;
                }
            }

            var jointModel = new MobilityJoint(Guid.NewGuid(), logicalPosition);

            var updatedPoints = new List<MobilityJoint>(road.ControlPoints);
            updatedPoints.Insert(insertIndex, jointModel);
            road.UpdateControlPoints(updatedPoints);

            var visualJoint = new MobilityJointVisualModel(jointModel, roadVM);
            visualJoint.UpdateScreenPosition(ViewportWidth, ViewportHeight, Camera.Zoom, Camera.OffsetX, Camera.OffsetY);
            VisibleMobilityJoints.Add(visualJoint);

            RoadGeometryService.GenerateWaypoints(road);
            UpdateVisualElementsPositions();
        }
        public void DeleteMobilityJoint(PointNodeVisualModel baseVisualJoint)
        {
            if (baseVisualJoint is not MobilityJointVisualModel visualJoint) return;

            RoadVisualModel parentRoadVM = visualJoint.ParentRoad;
            MobilityJoint jointToDelete = (MobilityJoint)visualJoint.Model;

            var updatedPoints = parentRoadVM.Model.ControlPoints.Where(cp => cp != jointToDelete).ToList();
            parentRoadVM.Model.UpdateControlPoints(updatedPoints);

            RoadGeometryService.GenerateWaypoints(parentRoadVM.Model);

            VisibleMobilityJoints.Remove(visualJoint);
            UpdateVisualElementsPositions();
        }

        private double DistanceToSegment(Point2D p, Point2D a, Point2D b)
        {
            double l2 = Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2);
            if (l2 == 0) return Math.Sqrt(Math.Pow(p.X - a.X, 2) + Math.Pow(p.Y - a.Y, 2));

            double t = Math.Max(0, Math.Min(1, ((p.X - a.X) * (b.X - a.X) + (p.Y - a.Y) * (b.Y - a.Y)) / l2));
            Point2D projection = new Point2D(a.X + t * (b.X - a.X), a.Y + t * (b.Y - a.Y));

            return Math.Sqrt(Math.Pow(p.X - projection.X, 2) + Math.Pow(p.Y - projection.Y, 2));
        }
        private void ClearSelection()
        {
            // TODO
        }

        // synchro screen position with logical 
        private void UpdateVisualElementsPositions()
        {
            foreach (var visualNode in VisualNodes)
            {
                visualNode.UpdateScreenPosition(ViewportWidth, ViewportHeight, Camera.Zoom, Camera.OffsetX, Camera.OffsetY);
            }

            foreach (var jointNode in VisibleMobilityJoints)
            {
                jointNode.UpdateScreenPosition(ViewportWidth, ViewportHeight, Camera.Zoom, Camera.OffsetX, Camera.OffsetY);
            }

            foreach (var visualRoad in VisualRoads)
            {
                visualRoad.UpdateScreenCoordinates(ViewportWidth, ViewportHeight, Camera.Zoom, Camera.OffsetX, Camera.OffsetY);
            }
        }
    }
}