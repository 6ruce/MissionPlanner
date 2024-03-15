using System;
using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace GeoMesh
{
    public class GMapMarkerGeoMesh : GMapMarker
    {
        private readonly Color _meshColor;
        private readonly double _unitSize;
        private readonly double _unitSizeInPixels;
        private const int _radius = 100;

        public GMapMarkerGeoMesh(
            PointLatLng pos,
            Color meshColor,
            double unitSize,
            double unitSizeInPixels) : base(pos)
        {
            _meshColor = meshColor;
            _unitSize = unitSize;
            _unitSizeInPixels = unitSizeInPixels;

            IsVisible = true;
            Position = pos;
            Size = new Size(2 * _radius, 2 * _radius);
        }

        public override void OnRender(IGraphics g)
        {
            var temp = g.Transform;
            g.TranslateTransform(LocalPosition.X, LocalPosition.Y);
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
                    Convert.ToInt32(_unitSizeInPixels * 10), 
                    0,
                    Convert.ToInt32(-_unitSizeInPixels * 10),
                    0);
                g.RotateTransform(10);
            }
            
            for (int i = 0; i <= 10; i++)
            {
                // Each fifth circle should be bold
                var penWidth = i % 5 == 0
                    ? 3
                    : 1;
                
                DrawMeshCircle(Convert.ToInt32(_unitSizeInPixels * i), penWidth, g);
            }
            
            g.TranslateTransform(LocalPosition.X, LocalPosition.Y);
            
            g.Transform = temp;
            
            g.DrawString(
                $"Current unit size: {_unitSize} meters",
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
    }
}