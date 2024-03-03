using System;
using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace GeoMesh
{
    public class GMapMarkerGeoMesh2 : GMapMarker
    {
        private readonly float _groundCourse;
        private const int _radius = 100;

        public GMapMarkerGeoMesh2(PointLatLng pos, float groundCourse) : base(pos)
        {
            _groundCourse = groundCourse;
            Size = new Size(2 * _radius, 2 * _radius);
        }

        public override void OnRender(IGraphics g)
        {
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
                    new Pen(Color.FromArgb(180, Color.HotPink), penWidth),
                    0, 
                    -Overlay.Control.Height / 2, 
                    0,
                    Overlay.Control.Height / 2);
                g.RotateTransform(10);
            }
            
            for (int i = 0; i <= 10; i++)
            {
                // Each fifth circle should be bold
                var penWidth = i % 5 == 0
                    ? 3
                    : 1;
                
                DrawMeshCircle(Overlay.Control.Height / 20 * i, 1, penWidth, g);
            }
            //g.TranslateTransform(LocalPosition.X, LocalPosition.Y);
            //g.RotateTransform(-Overlay.Control.Bearing);
            
            double width =
                (Overlay.Control.MapProvider.Projection.GetDistance(Overlay.Control.FromLocalToLatLng(0, 0),
                     Overlay.Control.FromLocalToLatLng(Overlay.Control.Width, 0)) * 1000.0);

            double scale = Overlay.Control.Width / width;
            
            g.Transform = temp;
        }

        private void DrawMeshCircle(int radius, double scale, float penWidth, IGraphics g)
        {
            var scaledRadius = radius * (float) scale;
            g.DrawEllipse(
                new Pen(Color.FromArgb(180, Color.HotPink), penWidth),
                -scaledRadius, -scaledRadius,
                Math.Abs(scaledRadius) * 2, Math.Abs(scaledRadius) * 2);
        }
    }
}