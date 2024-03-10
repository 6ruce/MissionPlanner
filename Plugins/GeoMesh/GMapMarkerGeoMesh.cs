using System;
using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace GeoMesh
{
    public class GMapMarkerGeoMesh : GMapMarker
    {
        private readonly Color _meshColor;
        private const int _radius = 100;

        public GMapMarkerGeoMesh(PointLatLng pos, Color meshColor) : base(pos)
        {
            _meshColor = meshColor;
            Size = new Size(2 * _radius, 2 * _radius);
        }

        public override void OnRender(IGraphics g)
        {
            var unitSize = CalculateUnitSize();
            
            var temp = g.Transform;

            // Instead of calculating the angled lines coordinates we draw the vertical line and rotate the control
            // until we do a full 360 circle
            for (int i = 0; i < 18; i++)
            {
                // Each line with an angle dividable by 30 should be bold
                var penWidth = i % 3 == 0
                    ? 3
                    : 1;
                
                g.DrawLine(
                    new Pen(Color.FromArgb(180, _meshColor), penWidth),
                    -Overlay.Control.Width / 2, 
                    0,
                    Overlay.Control.Width / 2,
                    0);
                g.RotateTransform(10);
            }
            
            for (int i = 0; i <= 10; i++)
            {
                // Each fifth circle should be bold
                var penWidth = i % 5 == 0
                    ? 5
                    : 3;
                
                DrawMeshCircle(unitSize.unitSizeInPixels * i, 1, penWidth, g);
            }
            
            g.TranslateTransform(LocalPosition.X, LocalPosition.Y);
            
            g.Transform = temp;
            
            g.DrawString(
                $"Current unit size: {unitSize.unitSize} meters",
                new Font("Arial", 16),
                new SolidBrush(_meshColor),
                0,
                0);
        }

        private void DrawMeshCircle(int radius, double scale, float penWidth, IGraphics g)
        {
            var scaledRadius = radius * (float) scale;
            g.DrawEllipse(
                new Pen(Color.FromArgb(255, _meshColor), penWidth),
                -scaledRadius, -scaledRadius,
                Math.Abs(scaledRadius) * 2, Math.Abs(scaledRadius) * 2);
        }

        private (double unitSize, int unitSizeInPixels) CalculateUnitSize()
        {
            //calculate visible distance in meters on screen between lowest and highest point
            double heightInMeters =
                (Overlay.Control.MapProvider.Projection.GetDistance(Overlay.Control.FromLocalToLatLng(Overlay.Control.Width / 2, 0),
                    Overlay.Control.FromLocalToLatLng(Overlay.Control.Width / 2, Overlay.Control.Height)) * 1000.0);

            // calculate the desired unit size;
            var desiredUnitSize = heightInMeters / 20;
            
            // find the closest lower unit size that starts from 5 or 10 in km
            var unitSize = 0.1;
            var diff = desiredUnitSize - unitSize;
            while (true)
            {
                if (diff <= 0.1)
                {
                    break;
                }
                
                var nextUnitSize = unitSize * 5;
                var nextDiff = desiredUnitSize - nextUnitSize;
                diff = nextDiff;
                unitSize = nextUnitSize;
                if (diff <= 0.1)
                {
                    break;
                }

                nextUnitSize = unitSize * 2;
                nextDiff = desiredUnitSize - nextUnitSize;

                diff = nextDiff;
                unitSize = nextUnitSize;
            }

            // assume that distance units on maps are even (not entirely true because of surface curvature) and find
            // pixel unit we want to use
            var unitSizeInPixels = unitSize /  (heightInMeters / Overlay.Control.Height);

            return (unitSize, Convert.ToInt32(unitSizeInPixels));
        }
    }
}