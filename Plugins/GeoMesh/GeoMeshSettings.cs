using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace GeoMesh
{
    public class GeoMeshSettings : IDisposable
    {
        private readonly Subscriptions _subscriptions = new Subscriptions();

        private readonly CheckBox _meshVisibilityControl = new CheckBox();
        private readonly TextBox _longtitudeControl = new TextBox();
        private readonly TextBox _latitudeControl = new TextBox();
        private readonly ComboBox _meshColor = new ComboBox();
        private readonly Label _errorNotification = new Label();
        private readonly List<string> _errorMessages = new List<string>();

        public bool Enabled => _meshVisibilityControl.Checked;

        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

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
            
            var meshPositionLabel = new Label();
            meshPositionLabel.Text = "Mesh position:";
            meshPositionLabel.Location = new Point(10, 35);
            meshPositionLabel.Size = new Size(150, 20);
            tabGeoMesh.Controls.Add(meshPositionLabel);
            
            var latitudeLabel = new Label();
            latitudeLabel.Text = "Latitude:";
            latitudeLabel.Location = new Point(20, 55);
            latitudeLabel.Size = new Size(120, 20);
            tabGeoMesh.Controls.Add(latitudeLabel);

            _latitudeControl.Text = "0";
            _latitudeControl.Location = new Point(145, 55);
            _latitudeControl.Size = new Size(50, 20);
            _latitudeControl.TextChanged += LatitudeTextChanged;
            _subscriptions.Add(() => _latitudeControl.TextChanged -= LatitudeTextChanged);
            tabGeoMesh.Controls.Add(_latitudeControl);
            
            var longtitudeLabel = new Label();
            longtitudeLabel.Text = "Longtitude:";
            longtitudeLabel.Location = new Point(20, 75);
            longtitudeLabel.Size = new Size(120, 20);
            tabGeoMesh.Controls.Add(longtitudeLabel);

            _longtitudeControl.Text = "0";
            _longtitudeControl.Location = new Point(145, 75);
            _longtitudeControl.Size = new Size(50, 20);
            _longtitudeControl.TextChanged += LongitudeTextChanged;
            _subscriptions.Add(() => _longtitudeControl.TextChanged += LongitudeTextChanged);
            tabGeoMesh.Controls.Add(_longtitudeControl);
            
            var meshColorLabel = new Label();
            meshColorLabel.Text = "Mesh color:";
            meshColorLabel.Location = new Point(10, 95);
            meshColorLabel.Size = new Size(130, 20);
            tabGeoMesh.Controls.Add(meshColorLabel);
            
            _meshColor.DataSource = Enum.GetNames(typeof(KnownColor));
            _meshColor.Location = new Point(140, 95);
            _meshColor.DrawMode = DrawMode.OwnerDrawFixed;
            _meshColor.DropDownStyle = ComboBoxStyle.DropDownList;
            _meshColor.FormattingEnabled = true;
            _meshColor.DrawItem += OnDrawMeshColorItem;
            _subscriptions.Add(() => _meshColor.DrawItem -= OnDrawMeshColorItem);
            tabGeoMesh.Controls.Add(_meshColor);
            
            _errorNotification.Location = new Point(10, 120);
            _errorNotification.ForeColor = Color.Red;
            _errorNotification.Size = new Size(250, 20);
            _errorNotification.AutoSize = true;
            tabGeoMesh.Controls.Add(_errorNotification);
            
        }
        
        public void Dispose() => _subscriptions.Dispose();

        private double ExtractCoordinate(TextBox control, string errorMessage, double min, double max)
        {
            double value = 0;
            if (double.TryParse(control.Text, out value)
                && value >= min && value < max)
            {
                _errorMessages.Remove(errorMessage);
            }
            else
            {
                if (!_errorMessages.Contains(errorMessage))
                {
                    _errorMessages.Add(errorMessage);
                }
                value = 0;
            }

            _errorNotification.Text = _errorMessages.Any() ? string.Join("\n", _errorMessages) : string.Empty;
            return value;
        }

        private void LatitudeTextChanged(object sender, EventArgs args)
        {
            Latitude = ExtractCoordinate(_latitudeControl, "Error: Invalid latitude. Valid range is [-90, 90)", -90, 90);
        }

        private void LongitudeTextChanged(object sender, EventArgs args)
        {
            Longitude = ExtractCoordinate(_longtitudeControl, "Error: Invalid longtitude. Valid range is [-180, 180)", -180, 180);
        }
        
        private void OnDrawMeshColorItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
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
                _meshColor.Font, brush, rect.X + 35, rect.Top + rect.Height - _meshColor.Font.Height);
        }
    }
}