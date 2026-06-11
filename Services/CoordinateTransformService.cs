using TrafficSim.Models;

namespace TrafficSim.Services
{
    public static class CoordinateTransformService
    {
        public static Point2D ToLogical(double screenX, double screenY, double viewportWidth, double viewportHeight, double zoom, double offsetX, double offsetY)
        {
            double logicX = (screenX - offsetX - (viewportWidth / 2)) / zoom;

            double logicY = ((viewportHeight / 2) + offsetY - screenY) / zoom;

            return new Point2D(logicX, logicY);
        }

        public static Point2D ToScreen(Point2D logicalPoint, double viewportWidth, double viewportHeight, double zoom, double offsetX, double offsetY)
        {
            double screenX = (logicalPoint.X * zoom) + offsetX + (viewportWidth / 2);
            double screenY = (viewportHeight / 2) - (logicalPoint.Y * zoom) + offsetY;

            return new Point2D(screenX, screenY);
        }
    }
}
