using System;
using System.Windows.Input;
using TrafficSim.Models;
using TrafficSim.Services;
using TrafficSim.ViewModels.Items;

namespace TrafficSim.ViewModels
{
    public partial class EditorViewModel : BaseViewModel
    {
        private EditorMode _currentMode = EditorMode.Select;
        public EditorMode CurrentMode
        {
            get => _currentMode;
            set => SetProperty(ref _currentMode, value);
        }

        private bool _isMenuOpen;
        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set => SetProperty(ref _isMenuOpen, value);
        }

        public ICommand ChangeModeCommand { get; }
        public ICommand CloseMenuAndResetCommand { get; }
        public ICommand DeleteRoadNodeCommand { get; }
        public ICommand DeleteMobilityJointCommand { get; }
        public ICommand ShowNodePropertiesCommand { get; }
        public ICommand AddMobilityJointCommand { get; }
        public EditorViewModel()
        {
            ChangeModeCommand = new RelayCommand(param => SetMode((EditorMode)param));
            CloseMenuAndResetCommand = new RelayCommand(_ => CloseMenuAndReset());
            DeleteRoadNodeCommand = new RelayCommand(param => DeleteRoadNode(param));
            DeleteMobilityJointCommand = new RelayCommand(param => DeleteMobilityJoint(param));
            AddMobilityJointCommand = new RelayCommand(param => AddMobilityJoint(param as RoadVisualModel));
            ShowNodePropertiesCommand = new RelayCommand(ExecuteShowNodeProperties);
        }

        public void SetMode(EditorMode newMode)
        {
            if (CurrentMode != newMode)
            {
                CurrentMode = newMode;
                _roadStartNode = null;
                IsPreviewingRoad = false;
                ClearSelection();
            }

        }

        private void CloseMenuAndReset()
        {
            IsMenuOpen = false;
            SetMode(EditorMode.Select);
            ClearSelection();
        }
    }
}