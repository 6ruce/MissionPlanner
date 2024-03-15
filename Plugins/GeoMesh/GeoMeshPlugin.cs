using System;
using GMap.NET;
using GMap.NET.WindowsForms;
using MissionPlanner.GCSViews;

namespace GeoMesh
{
    public class GeoMeshPlugin : MissionPlanner.Plugin.Plugin
    {
        private double _lastZoom = -1;
        private PointLatLng _lastLocation = PointLatLng.Empty;
        private (double unitSize, double unitSizeInPixels) _currentUnitSize = default;
        
        private GMapOverlay _meshOverlay;
        private GeoMeshSettings _geoMeshSettings;

        public override string Name { get; } = "GeoMesh";
        public override string Version { get; } = "0.1";
        public override string Author { get; } = "Cherub Team";
        public override bool Init()
        {
            _geoMeshSettings = new GeoMeshSettings();

            // Separate overlay to hold the mesh. It's addede specifically to gMapControl1 as it represents the map 
            // control that works during the flight.
            _meshOverlay = new GMapOverlay("geoMesh");
            FlightData.instance.gMapControl1.Overlays.Add(_meshOverlay);

            // Frequency of the loop iterations
            loopratehz = 1f;
                        
            return true;
        }

        public override bool Loaded()
        {
            _geoMeshSettings.Load();

            return true;
        }


        public override bool Loop()
        {
            double epsilon = double.Epsilon;

            if (Math.Abs(_lastZoom - FlightData.instance.gMapControl1.Zoom) > epsilon)
            {
                _currentUnitSize = CalculateUnitSize();
                _lastZoom = FlightData.instance.gMapControl1.Zoom;
            }
            
            _meshOverlay.IsVisibile = _geoMeshSettings.Enabled;
            if (_geoMeshSettings.Enabled)
            {
                var meshLocation = Math.Abs(_geoMeshSettings.Latitude) > epsilon && Math.Abs(_geoMeshSettings.Longitude) > epsilon
                    ? new PointLatLng(_geoMeshSettings.Latitude, _geoMeshSettings.Longitude)
                    : new PointLatLng(Host.cs.Location.Lat, Host.cs.Location.Lng);
                
                // The thing that know how to render the mesh itself
                var geoMeshMarker = new GMapMarkerGeoMesh(
                    meshLocation,
                    _geoMeshSettings.MeshColor,
                    _currentUnitSize.unitSize,
                    _currentUnitSize.unitSizeInPixels);
            
                // Marker should be re-added each iteration to update its position
                _meshOverlay?.Markers.Clear();
                _meshOverlay?.Markers.Add(geoMeshMarker);
            }

            return base.Loop();
        }

        private (double unitSize, double unitSizeInPixels) CalculateUnitSize()
        {
            //calculate visible distance in meters on screen between center bottom and center top point
            double heightInMeters =
                (_meshOverlay.Control.MapProvider.Projection.GetDistance(_meshOverlay.Control.FromLocalToLatLng(_meshOverlay.Control.Width / 2, 0),
                    _meshOverlay.Control.FromLocalToLatLng(_meshOverlay.Control.Width / 2, _meshOverlay.Control.Height)) * 1000.0);

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
            var unitSizeInPixels = unitSize /  (heightInMeters / _meshOverlay.Control.Height);

            return (unitSize, unitSizeInPixels);
        }
        
        public override bool Exit()
        {
            _geoMeshSettings.Dispose();
            return true;
        }
    }
}