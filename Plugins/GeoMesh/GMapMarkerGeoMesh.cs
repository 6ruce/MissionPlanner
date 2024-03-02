using System;
using System.Drawing;
using System.Runtime.Serialization;
using GMap.NET;
using GMap.NET.WindowsForms;
using MissionPlanner.Utilities;

namespace GeoMesh
{
    public class GMapMarkerGeoMesh : GMapMarker
    {
        private readonly float _groundCourse;
        private const int _radius = 100;

        public GMapMarkerGeoMesh(PointLatLng pos, float groundCourse) : base(pos)
        {
            _groundCourse = groundCourse;
            Size = new Size(2 * _radius, 2 * _radius);
        }

        public override void OnRender(IGraphics g)
        {
            var temp = g.Transform;
            
            g.TranslateTransform(LocalPosition.X, LocalPosition.Y);
            g.RotateTransform(-Overlay.Control.Bearing);
            
            double width =
                (Overlay.Control.MapProvider.Projection.GetDistance(Overlay.Control.FromLocalToLatLng(0, 0),
                     Overlay.Control.FromLocalToLatLng(Overlay.Control.Width, 0)) * 1000.0);

            double scale = Overlay.Control.Width / width;
            
            DrawMeshCircle(_radius, scale, g);
            DrawMeshCircle(_radius / 2, scale, g);
            DrawMeshCircle(_radius * 2, scale, g);
            
            g.Transform = temp;
        }

        private void DrawMeshCircle(int radius, double scale, IGraphics g)
        {
            var scaledRadius = radius * (float) scale;

            g.DrawEllipse(
                new Pen(Color.HotPink, 2),
                -scaledRadius, -scaledRadius,
                Math.Abs(scaledRadius) * 2, Math.Abs(scaledRadius) * 2);
        }
    }
}