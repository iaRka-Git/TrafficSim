using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrafficSim.Services;

namespace TrafficSim.ViewModels.Items
{
    /// <summary>
    /// Клас, що зберігає стан камери  та повідомляє UI про зміни через події
    /// </summary>
    public class CameraState : BaseViewModel
    {
        private double _zoom = 1.0;

        public double Zoom
        {
            get => _zoom;
            set => SetProperty(ref _zoom, Math.Clamp(value, 0.1, 20));
        }

        private double _offsetX = 0;
        public double OffsetX
        {
            get => _offsetX;
            set => SetProperty(ref _offsetX, value);
        }

        private double _offsetY = 0;
        public double OffsetY
        {
            get => _offsetY;
            set => SetProperty(ref _offsetY, value);
        }

        public void Reset()
        {
            Zoom = 1.0;
            OffsetX = 0;
            OffsetY = 0;
        }
    }
}
