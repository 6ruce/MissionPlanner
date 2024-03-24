using System;
using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;
using hires;
using Stopwatch = System.Diagnostics.Stopwatch;

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
            
            // We want to fill all the screen with a mesh, therefore we draw all the circles with diameter less or equal
            // than the map control diagonal.
            var mapDiagonal = Math.Sqrt(Overlay.Control.Height * Overlay.Control.Height +
                                        Overlay.Control.Width * Overlay.Control.Width);

            int circlesNumber = Convert.ToInt32(mapDiagonal / (_unitSizeInPixels * 2)) + 1;
            
            for (int i = 0; i <= circlesNumber; i++)
            {
                // Each fifth circle should be bold
                var penWidth = i % 5 == 0
                    ? 3
                    : 1;
                
                DrawMeshCircle(Convert.ToInt32(_unitSizeInPixels * i), penWidth, g);
            }
            
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
                    Convert.ToInt32(_unitSizeInPixels * circlesNumber), 
                    0,
                    Convert.ToInt32(-_unitSizeInPixels * circlesNumber),
                    0);
                g.RotateTransform(10);
            }
            
            g.TranslateTransform(LocalPosition.X, LocalPosition.Y);
            
            g.Transform = temp;

            
            DrawUnitSizeString(g);
        }

        private void DrawUnitSizeString(IGraphics g)
        {
            // Calculate and fill the rect in the map right bottom corner.
            RectangleF rect = new RectangleF(
                Overlay.Control.Width / 2 - 200,
                Overlay.Control.Height / 2 - 40,
                200,
                40);
            g.FillRectangle(Brushes.Black, rect);
            g.DrawString(
                $"Current mesh unit size: {_unitSize} meters",
                new Font("Arial", 10),
                new SolidBrush(_meshColor),
                rect);
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