using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace IPCamera
{
    /// <summary>
    /// Map window (WebView2 + Leaflet + OpenStreetMap) for viewing / choosing camera location
    /// </summary>
    public class MapForm : Form
    {
        const string VirtualHost = "videoserver.map";

        readonly WebView2 webView;
        readonly Label lbCoordinates;
        readonly Button btOk;
        readonly Button btCancel;

        double? latitude;
        double? longitude;

        /// <summary>
        /// Selected latitude (valid when DialogResult == OK)
        /// </summary>
        public double? Latitude { get { return latitude; } }

        /// <summary>
        /// Selected longitude (valid when DialogResult == OK)
        /// </summary>
        public double? Longitude { get { return longitude; } }

        public MapForm(string cameraName, double? latitude, double? longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;

            Text = string.IsNullOrWhiteSpace(cameraName) ? "Мапа" : "Мапа - " + cameraName;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 750);
            MinimumSize = new Size(500, 400);
            ShowInTaskbar = false;

            webView = new WebView2 { Dock = DockStyle.Fill };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };

            lbCoordinates = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            btCancel = new Button { Text = "Скасувати", Dock = DockStyle.Right, Width = 110, DialogResult = DialogResult.Cancel };
            btOk = new Button { Text = "Вибрати", Dock = DockStyle.Right, Width = 110 };
            btOk.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            bottom.Controls.Add(lbCoordinates);
            bottom.Controls.Add(btOk);
            bottom.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 8 });
            bottom.Controls.Add(btCancel);

            Controls.Add(webView);
            Controls.Add(bottom);

            AcceptButton = btOk;
            CancelButton = btCancel;

            UpdateCoordinatesLabel();

            Load += MapForm_Load;
        }

        async void MapForm_Load(object sender, EventArgs e)
        {
            try
            {
                string baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "VideoServer");
                string mapDir = Path.Combine(baseDir, "map");
                Directory.CreateDirectory(mapDir);
                File.WriteAllText(Path.Combine(mapDir, "map.html"), BuildHtml(), Encoding.UTF8);

                var env = await CoreWebView2Environment.CreateAsync(null, Path.Combine(baseDir, "WebView2"));
                await webView.EnsureCoreWebView2Async(env);

                // page is served from https://videoserver.map/ so tile requests carry a proper Referer
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    VirtualHost, mapDir, CoreWebView2HostResourceAccessKind.Allow);
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                webView.CoreWebView2.Navigate("https://" + VirtualHost + "/map.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Не вдалося відкрити мапу.\n" +
                    "Перевірте, що встановлено Microsoft Edge WebView2 Runtime.\n\n" + ex.Message,
                    "Мапа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        // message from page: "lat;lng"
        void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string msg = e.TryGetWebMessageAsString();
            if (string.IsNullOrEmpty(msg))
                return;

            string[] parts = msg.Split(';');
            double lat, lon;
            if (parts.Length == 2 &&
                double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat) &&
                double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
            {
                latitude = Math.Round(lat, 6);
                longitude = Math.Round(lon, 6);
                UpdateCoordinatesLabel();
            }
        }

        void UpdateCoordinatesLabel()
        {
            if (latitude.HasValue && longitude.HasValue)
            {
                lbCoordinates.Text = string.Format(CultureInfo.InvariantCulture,
                    "Широта: {0:0.######}    Довгота: {1:0.######}", latitude.Value, longitude.Value);
                btOk.Enabled = true;
            }
            else
            {
                lbCoordinates.Text = "Клацніть на мапі, щоб вказати розташування камери";
                btOk.Enabled = false;
            }
        }

        string BuildHtml()
        {
            var ci = CultureInfo.InvariantCulture;
            bool hasPoint = latitude.HasValue && longitude.HasValue;
            string lat = hasPoint ? latitude.Value.ToString(ci) : "null";
            string lon = hasPoint ? longitude.Value.ToString(ci) : "null";

            return @"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"">
<script src=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.js""></script>
<style>html, body, #map { height: 100%; margin: 0; }</style>
</head>
<body>
<div id=""map""></div>
<script>
  var lat = " + lat + @", lng = " + lon + @";
  var map = L.map('map');
  L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; OpenStreetMap contributors'
  }).addTo(map);

  var marker = null;
  function send(ll) { window.chrome.webview.postMessage(ll.lat + ';' + ll.lng); }
  function setMarker(ll) {
    if (marker) { marker.setLatLng(ll); }
    else {
      marker = L.marker(ll, { draggable: true }).addTo(map);
      marker.on('dragend', function () { send(marker.getLatLng()); });
    }
  }

  if (lat !== null && lng !== null) {
    map.setView([lat, lng], 16);
    setMarker([lat, lng]);
  } else {
    map.setView([48.5, 31.2], 6);
  }

  map.on('click', function (e) { setMarker(e.latlng); send(e.latlng); });
</script>
</body>
</html>";
        }
    }
}
