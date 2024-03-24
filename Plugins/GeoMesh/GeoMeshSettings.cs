﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Threading;
using MissionPlanner.Utilities;
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
        private readonly ComboBox _meshColorControl = new ComboBox();
        private readonly Label _errorNotification = new Label();
        private readonly List<string> _errorMessages = new List<string>();
        private Dispatcher _initDispatcher;
        private TabPage _tabGeoMesh;

        public bool Enabled => _meshVisibilityControl.Checked;

        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        public Color MeshColor { get; private set; } = Color.Red;

        public void Load()
        {
            var tabControls = MissionPlanner.MainV2.instance.FlightData.tabControlactions.Controls;
            if (tabControls.OfType<TabPage>().Any(tab => tab.Text == "GeoMesh"))
            {
                return;
            }

            var copiedTabControls = tabControls.OfType<Control>().ToList();
            // Readding GeoMesh tab if the sidebar control got modified
            if (_initDispatcher != null)
            {
                _initDispatcher.Invoke(() =>
                {
                    tabControls.Clear();
                    copiedTabControls.Insert(3, _tabGeoMesh);
                    copiedTabControls.ForEach(tabControls.Add);
                });

                return;
            }

            _initDispatcher = Dispatcher.CurrentDispatcher;

            _tabGeoMesh = new TabPage();
            _tabGeoMesh.Text = "GeoMesh";
            tabControls.Clear();
            copiedTabControls.Insert(3, _tabGeoMesh);
            copiedTabControls.ForEach(tabControls.Add);

            _meshVisibilityControl.Text = "Mesh visibility";
            _meshVisibilityControl.Location = new Point(10, 10);
            _meshVisibilityControl.Checked = bool.Parse(Settings.Instance["geomesh_enabled"] ?? "false");;
            _meshVisibilityControl.CheckedChanged += (sender, args) => Settings.Instance["geomesh_enabled"] = _meshVisibilityControl.Checked.ToString();
            _tabGeoMesh.Controls.Add(_meshVisibilityControl);
            
            var meshPositionLabel = new Label();
            meshPositionLabel.Text = "Mesh position:";
            meshPositionLabel.Location = new Point(10, 35);
            meshPositionLabel.Size = new Size(150, 20);
            _tabGeoMesh.Controls.Add(meshPositionLabel);
            
            var latitudeLabel = new Label();
            latitudeLabel.Text = "Latitude:";
            latitudeLabel.Location = new Point(20, 55);
            latitudeLabel.Size = new Size(120, 20);
            _tabGeoMesh.Controls.Add(latitudeLabel);

            _latitudeControl.Text = Settings.Instance["geomesh_pos_lat"] ?? "0";
            _latitudeControl.Location = new Point(145, 55);
            _latitudeControl.Size = new Size(50, 20);
            _latitudeControl.TextChanged += LatitudeTextChanged;
            _subscriptions.Add(() => _latitudeControl.TextChanged -= LatitudeTextChanged);
            Latitude = double.Parse(_latitudeControl.Text);
            _tabGeoMesh.Controls.Add(_latitudeControl);
            
            var longtitudeLabel = new Label();
            longtitudeLabel.Text = "Longtitude:";
            longtitudeLabel.Location = new Point(20, 75);
            longtitudeLabel.Size = new Size(120, 20);
            _tabGeoMesh.Controls.Add(longtitudeLabel);

            _longtitudeControl.Text = Settings.Instance["geomesh_pos_long"] ?? "0";
            _longtitudeControl.Location = new Point(145, 75);
            _longtitudeControl.Size = new Size(50, 20);
            _longtitudeControl.TextChanged += LongitudeTextChanged;
            _subscriptions.Add(() => _longtitudeControl.TextChanged -= LongitudeTextChanged);
            Longitude = double.Parse(_longtitudeControl.Text);
            _tabGeoMesh.Controls.Add(_longtitudeControl);
            
            var meshColorLabel = new Label();
            meshColorLabel.Text = "Mesh color:";
            meshColorLabel.Location = new Point(10, 95);
            meshColorLabel.Size = new Size(130, 20);
            _tabGeoMesh.Controls.Add(meshColorLabel);
            
            _meshColorControl.DataSource = Enum.GetNames(typeof(KnownColor));
            _meshColorControl.BindingContext = new BindingContext();
            _meshColorControl.Location = new Point(140, 95);
            _meshColorControl.SelectedIndex =
                _meshColorControl.FindStringExact(Settings.Instance["geomesh_color"] ?? nameof(KnownColor.Red));
            _meshColorControl.DrawMode = DrawMode.OwnerDrawFixed;
            _meshColorControl.DropDownStyle = ComboBoxStyle.DropDownList;
            _meshColorControl.FormattingEnabled = true;
            _meshColorControl.SelectedValueChanged += OnMeshColorChange;
            _subscriptions.Add(() => _meshColorControl.SelectedValueChanged -= OnMeshColorChange);
            _meshColorControl.DrawItem += OnDrawMeshColorControlItem;
            _subscriptions.Add(() => _meshColorControl.DrawItem -= OnDrawMeshColorControlItem);
            MeshColor = Color.FromName(_meshColorControl.SelectedItem.ToString());
            _tabGeoMesh.Controls.Add(_meshColorControl);
            
            _errorNotification.Location = new Point(10, 120);
            _errorNotification.ForeColor = Color.Red;
            _errorNotification.Size = new Size(250, 20);
            _errorNotification.AutoSize = true;
            _tabGeoMesh.Controls.Add(_errorNotification);

            MissionPlanner.MainV2.instance.FlightData.tabControlactions.Refresh();
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
            Settings.Instance["geomesh_pos_lat"] = Latitude.ToString();
        }

        private void LongitudeTextChanged(object sender, EventArgs args)
        {
            Longitude = ExtractCoordinate(_longtitudeControl, "Error: Invalid longtitude. Valid range is [-180, 180)", -180, 180);
            Settings.Instance["geomesh_pos_long"] = Longitude.ToString();
        }

        private void OnMeshColorChange(object sender, EventArgs args)
        {
            MeshColor = Color.FromName((string)_meshColorControl.SelectedItem);
            Settings.Instance["geomesh_color"] = _meshColorControl.SelectedItem.ToString();
        }
        
        private void OnDrawMeshColorControlItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            var g = e.Graphics;
            var rect = e.Bounds;
            Brush brush = null;

            if ((e.State & DrawItemState.Selected) == 0)
                brush = new SolidBrush(_meshColorControl.BackColor);
            else
                brush = SystemBrushes.Highlight;

            g.FillRectangle(brush, rect);

            brush = new SolidBrush(Color.FromName((string)_meshColorControl.Items[e.Index]));

            g.FillRectangle(brush, rect.X + 2, rect.Y + 2, 30, rect.Height - 4);
            g.DrawRectangle(Pens.Black, rect.X + 2, rect.Y + 2, 30, rect.Height - 4);

            if ((e.State & DrawItemState.Selected) == 0)
                brush = new SolidBrush(_meshColorControl.ForeColor);
            else
                brush = SystemBrushes.HighlightText;
            g.DrawString(_meshColorControl.Items[e.Index].ToString(),
                _meshColorControl.Font, brush, rect.X + 35, rect.Top + rect.Height - _meshColorControl.Font.Height);
        }
    }
}