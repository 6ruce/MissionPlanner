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
                    ? 3
                    : 1;
                
                DrawMeshCircle(Convert.ToInt32(unitSize.unitSizeInPixels * i), penWidth, g);
            }
            
            g.TranslateTransform(LocalPosition.X, LocalPosition.Y);
            
            g.Transform = temp;
            
            g.DrawString(
                $"Current unit size: {unitSize.unitSize} meters",
                new Font("Arial", 12),
                new SolidBrush(_meshColor),
                0,
                0);
        }

        private void DrawMeshCircle(int radius, float penWidth, IGraphics g)
        {
            g.DrawEllipse(
                new Pen(Color.FromArgb(255, _meshColor), penWidth),
                -radius, -radius,
                Math.Abs(radius) * 2, Math.Abs(radius) * 2);
        }

        private (double unitSize, double unitSizeInPixels) CalculateUnitSize()
        {
            //calculate visible distance in meters on screen between center bottom and center top point
            double heightInMeters =
                (Overlay.Control.MapProvider.Projection.GetDistance(Overlay.Control.FromLocalToLatLng(Overlay.Control.Width / 2, 0),
                    Overlay.Control.FromLocalToLatLng(Overlay.Control.Width / 2, Overlay.Control.Height)) * 1000.0);

            // calculate the desired unit size - distance between two adjacent circles if we want to have ten circles
            // visible on screen it should be height divided by 20.
            var desiredUnitSize = heightInMeters / 20;
            
            // find the closest higher "convenient" unit size (1,5,10,50,100...)
            var multipliers = new[] { 5, 2 };
            var unitSize = 0.1;
            var diff = desiredUnitSize - unitSize;
            while (diff > 0)
            {
                foreach (var multiplier in multipliers)
                {
                    if (diff <= 0)
                        break;
                    
                    unitSize *= multiplier;
                    diff = desiredUnitSize - unitSize;
                }
            }

            // assume that distance units on maps are even (not entirely true because of surface curvature) and find
            // pixel unit we want to use
            var unitSizeInPixels = unitSize /  (heightInMeters / Overlay.Control.Height);

            return (unitSize, unitSizeInPixels);
        }
    }
}