using System;
using GMap.NET;
using GMap.NET.WindowsForms;
using MissionPlanner.GCSViews;

namespace GeoMesh
{
    public class GeoMeshPlugin : MissionPlanner.Plugin.Plugin
    {

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
            _meshOverlay.IsVisibile = _geoMeshSettings.Enabled;
            
            if (_geoMeshSettings.Enabled)
            {
                double epsilon = double.Epsilon;
                var meshLocation = Math.Abs(_geoMeshSettings.Latitude) > epsilon && Math.Abs(_geoMeshSettings.Longitude) > epsilon
                    ? new PointLatLng(_geoMeshSettings.Latitude, _geoMeshSettings.Longitude)
                    : new PointLatLng(Host.cs.Location.Lat, Host.cs.Location.Lng);
                
                // The thing that know how to render the mesh itself
                var geoMeshMarker = new GMapMarkerGeoMesh(meshLocation, _geoMeshSettings.MeshColor);
            
                // Marker should be re-added each iteration to update its position
                _meshOverlay?.Markers.Clear();
                _meshOverlay?.Markers.Add(geoMeshMarker);
            }

            return base.Loop();
        }

        public override bool Exit()
        {
            _geoMeshSettings.Dispose();
            return true;
        }
    }
}