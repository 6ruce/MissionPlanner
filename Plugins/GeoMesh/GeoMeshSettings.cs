extern alias Drawing;
using System;
using System.Linq;
using System.Windows.Forms;
using Drawing::System.Drawing;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace GeoMesh
{
    public class GeoMeshSettings
    {
        private CheckBox _meshVisibilityControl = new CheckBox();
        private TextBox _longtitudeControl = new TextBox();
        private TextBox _latitudeControl = new TextBox();
        private ComboBox _meshColor = new ComboBox();

        public bool Enabled => _meshVisibilityControl.Checked;

        public double Latitude => _latitudeControl.Text != string.Empty ? double.Parse(_latitudeControl.Text) : 0;

        public double Longitude => _longtitudeControl.Text != string.Empty ? double.Parse(_longtitudeControl.Text) : 0;
        
        public Color MeshColor => Color.FromName((string)_meshColor.SelectedItem);

        public void Load()
        {
            var tabGeoMesh = new TabPage();
            tabGeoMesh.Text = "GeoMesh";
            var tabControls = MissionPlanner.MainV2.instance.FlightData.tabControlactions.Controls;
            var copiedTabControls = tabControls.OfType<Control>().ToList();
            tabControls.Clear();
            copiedTabControls.Insert(3, tabGeoMesh);
            copiedTabControls.ForEach(tabControls.Add);
            
            _meshVisibilityControl.Text = "Mesh visibility";
            _meshVisibilityControl.Location = new Point(10, 10);
            _meshVisibilityControl.Checked = true;
            tabGeoMesh.Controls.Add(_meshVisibilityControl);
            
            var longtitudeLabel = new Label();
            longtitudeLabel.Text = "Mesh longtitude position:";
            longtitudeLabel.Location = new Point(10, 35);
            longtitudeLabel.Size = new Size(150, 20);
            tabGeoMesh.Controls.Add(longtitudeLabel);

            _longtitudeControl.Text = "0";
            _longtitudeControl.Location = new Point(10, 55);
            _longtitudeControl.Size = new Size(50, 20);
            tabGeoMesh.Controls.Add(_longtitudeControl);
            
            var latitudeLabel = new Label();
            latitudeLabel.Text = "Mesh latitude position:";
            latitudeLabel.Location = new Point(10, 75);
            latitudeLabel.Size = new Size(150, 20);
            tabGeoMesh.Controls.Add(latitudeLabel);

            _latitudeControl.Text = "0";
            _latitudeControl.Location = new Point(10, 95);
            _latitudeControl.Size = new Size(50, 20);
            tabGeoMesh.Controls.Add(_latitudeControl);
            
            _meshColor.DataSource = Enum.GetNames(typeof(KnownColor));
            _meshColor.Location = new Point(10, 115);
            _meshColor.DrawMode = DrawMode.OwnerDrawFixed;
            _meshColor.DropDownStyle = ComboBoxStyle.DropDownList;
            _meshColor.FormattingEnabled = true;
            _meshColor.DrawItem += OnDrawMeshColorItem;
            tabGeoMesh.Controls.Add(_meshColor);
        }
        
        private void OnDrawMeshColorItem(object sender, DrawItemEventArgs e)
        {
            /*if (e.Index < 0)
                return;

            var g = e.Graphics;
            var rect = e.Bounds;
            Brush brush = null;

            if ((e.State & DrawItemState.Selected) == 0)
                brush = new SolidBrush(_meshColor.BackColor);
            else
                brush = SystemBrushes.Highlight;

            g.FillRectangle(brush, rect);

            brush = new SolidBrush(Color.FromName((string)_meshColor.Items[e.Index]));

            g.FillRectangle(brush, rect.X + 2, rect.Y + 2, 30, rect.Height - 4);
            g.DrawRectangle(Pens.Black, rect.X + 2, rect.Y + 2, 30, rect.Height - 4);

            if ((e.State & DrawItemState.Selected) == 0)
                brush = new SolidBrush(_meshColor.ForeColor);
            else
                brush = SystemBrushes.HighlightText;
            g.DrawString(_meshColor.Items[e.Index].ToString(),
                _meshColor.Font, brush, rect.X + 35, rect.Top + rect.Height - _meshColor.Font.Height);*/
        }
    }
}