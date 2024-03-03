using System.Linq;
using System.Windows.Forms;
using GMap.NET.WindowsForms;
using MissionPlanner.GCSViews;

namespace GeoMesh
{
    public class GeoMeshPlugin : MissionPlanner.Plugin.Plugin
    {

        private CheckBox _meshVisibilityControl = new CheckBox();
        private TextBox _meshScaleControl = new TextBox();
        
        public override string Name { get; } = "GeoMesh";
        public override string Version { get; } = "0.1";
        public override string Author { get; } = "Cherub Team";
        public override bool Init()
        {
            // Separate overlay to hold the mesh. It's addede specifically to gMapControl1 as it represents the map 
            // control that works during the flight.
            var overlay = new GMapOverlay("positions");
            FlightData.instance.gMapControl1.Overlays.Add(overlay);
            
            _meshVisibilityControl.Text = "Mesh visibility";
            _meshVisibilityControl.Checked = true;
            FlightData.instance.panel_persistent.Controls.Add(_meshVisibilityControl);

            _meshScaleControl.Text = "100";
            FlightData.instance.panel_persistent.Controls.Add(_meshScaleControl);

            
            // Frequency of the loop iterations
            loopratehz = 1f;
                        
            return true;
        }

        public override bool Loaded()
        {
            return true;
        }

        public override bool Loop()
        {
            var meshOverlay = Host.FDGMapControl.Overlays.FirstOrDefault(o => o.Id == "positions");
            meshOverlay.IsVisibile = _meshVisibilityControl.Checked;
            
            if (_meshVisibilityControl.Checked)
            {
                // Thing that know how to render the mesh itself
                var geoMeshMarker = new GMapMarkerGeoMesh2(Host.cs.Location, Host.cs.groundcourse);
            
                // Marker should be re-added each iteration to update its position
                meshOverlay?.Markers.Clear();
                meshOverlay?.Markers.Add(geoMeshMarker);
            }

            return base.Loop();
        }

        public override bool SetupUI(int gui = 0, object data = null)
        {
            return base.SetupUI(gui, data);
        }

        public override bool Exit()
        {
            return true;
        }
    }
}