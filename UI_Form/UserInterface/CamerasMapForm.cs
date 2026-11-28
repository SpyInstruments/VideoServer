using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Interfaces;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace VideoServer.UserInterface
{
    /// <summary>
    /// Map with all cameras from settings (WebView2 + Leaflet + OpenStreetMap).
    /// Camera markers can be dragged (as text = camera name) to the view grid,
    /// the same way as from the cameras list.
    /// </summary>
    public class CamerasMapForm : Form
    {
        const string VirtualHost = "videoserver.map";
        const string PageName = "cameras.html";

        readonly WebView2 webView;
        readonly Label lbInfo;
        readonly List<CameraPoint> cameras;

        class CameraPoint
        {
            public string Name;
            public double? Latitude;
            public double? Longitude;
        }

        public CamerasMapForm(XmlDocument setting)
        {
            cameras = ReadCameras(setting);

            Text = "Мапа камер";
            StartPosition = FormStartPosition.Manual;
            Size = new Size(900, 700);
            MinimumSize = new Size(400, 300);
            ShowInTaskbar = false;

            webView = new WebView2 { Dock = DockStyle.Fill };
            lbInfo = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Text = "Перетягніть камеру з мапи в квадрат вікна нагляду"
            };

            Controls.Add(webView);
            Controls.Add(lbInfo);

            Load += CamerasMapForm_Load;
        }

        static List<CameraPoint> ReadCameras(XmlDocument setting)
        {
            var list = new List<CameraPoint>();
            XmlNodeList nodes = setting?.SelectNodes("/ROOT/Cameras/Camera");
            if (nodes == null)
                return list;

            foreach (XmlNode node in nodes)
            {
                string name = node.Attributes?["CameraName"]?.Value;
                if (string.IsNullOrWhiteSpace(name))
                    name = node["CameraName"]?.InnerText;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                double? lat = CameraAdapter.ParseCoordinate(node.SelectSingleNode("Geolocation/Latitude")?.InnerText);
                double? lon = CameraAdapter.ParseCoordinate(node.SelectSingleNode("Geolocation/Longitude")?.InnerText);
                if (lat < -90 || lat > 90 || lon < -180 || lon > 180)
                    lat = lon = null;

                list.Add(new CameraPoint { Name = name.Trim(), Latitude = lat, Longitude = lon });
            }
            return list;
        }

        async void CamerasMapForm_Load(object sender, EventArgs e)
        {
            try
            {
                string baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "VideoServer");
                string mapDir = Path.Combine(baseDir, "map");
                Directory.CreateDirectory(mapDir);
                File.WriteAllText(Path.Combine(mapDir, PageName), BuildHtml(), Encoding.UTF8);

                var env = await CoreWebView2Environment.CreateAsync(null, Path.Combine(baseDir, "WebView2"));
                await webView.EnsureCoreWebView2Async(env);

                // served from https://videoserver.map/ so tile requests carry a proper Referer
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    VirtualHost, mapDir, CoreWebView2HostResourceAccessKind.Allow);
                webView.CoreWebView2.Navigate("https://" + VirtualHost + "/" + PageName);
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

        static string JsString(string s)
        {
            var sb = new StringBuilder("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        // escape <, >, & and non-ASCII so the value can't break out of <script>
                        if (c < 0x20 || c > 0x7e || c == '<' || c == '>' || c == '&')
                            sb.AppendFormat("\\u{0:x4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.Append('"').ToString();
        }

        string BuildHtml()
        {
            var ci = CultureInfo.InvariantCulture;
            var items = new List<string>();
            foreach (var c in cameras)
            {
                items.Add("{name:" + JsString(c.Name) +
                          ",lat:" + (c.Latitude.HasValue ? c.Latitude.Value.ToString("R", ci) : "null") +
                          ",lng:" + (c.Longitude.HasValue ? c.Longitude.Value.ToString("R", ci) : "null") + "}");
            }
            string data = "[" + string.Join(",", items) + "]";

            return @"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"">
<script src=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.js""></script>
<style>
  html, body, #map { height: 100%; margin: 0; font-family: Segoe UI, sans-serif; }
  .cam { display: inline-flex; align-items: center; gap: 4px; white-space: nowrap;
         background: #fff; border: 2px solid #1565c0; border-radius: 12px;
         padding: 2px 8px 2px 4px; font-size: 13px; color: #111; cursor: grab;
         box-shadow: 0 1px 4px rgba(0,0,0,.35); transform: translate(-12px, -50%); }
  .cam:active { cursor: grabbing; }
  .cam .dot { width: 10px; height: 10px; border-radius: 50%; background: #1565c0; flex: none; }
  .cam-icon { background: none; border: none; }
  .nocoords { background: #fff; padding: 6px 8px; border-radius: 6px; max-height: 40vh; overflow: auto;
              box-shadow: 0 1px 4px rgba(0,0,0,.35); font-size: 13px; }
  .nocoords b { display: block; margin-bottom: 4px; }
  .nocoords .cam { transform: none; margin: 2px 0; display: flex; border-color: #888; }
  .nocoords .cam .dot { background: #888; }
</style>
</head>
<body>
<div id=""map""></div>
<script>
  var cameras = " + data + @";

  var map = L.map('map');
  L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; OpenStreetMap contributors'
  }).addTo(map);

  // draggable camera label; payload = camera name as plain text (same as the cameras list)
  function camElement(name) {
    var el = document.createElement('div');
    el.className = 'cam';
    el.draggable = true;
    el.title = name;
    var dot = document.createElement('span'); dot.className = 'dot';
    var lbl = document.createElement('span'); lbl.textContent = name;
    el.appendChild(dot); el.appendChild(lbl);
    el.addEventListener('dragstart', function (e) {
      e.dataTransfer.setData('text/plain', name);
      e.dataTransfer.effectAllowed = 'copy';
    });
    // don't let Leaflet start panning the map when the drag starts on a camera
    L.DomEvent.disableClickPropagation(el);
    L.DomEvent.on(el, 'pointerdown', L.DomEvent.stopPropagation);
    return el;
  }

  var bounds = [];
  var noCoords = [];
  cameras.forEach(function (c) {
    if (c.lat === null || c.lng === null) { noCoords.push(c); return; }
    L.marker([c.lat, c.lng], {
      icon: L.divIcon({ className: 'cam-icon', html: camElement(c.name), iconSize: null }),
      keyboard: false
    }).addTo(map);
    bounds.push([c.lat, c.lng]);
  });

  if (bounds.length === 1) map.setView(bounds[0], 16);
  else if (bounds.length > 1) map.fitBounds(bounds, { padding: [40, 40], maxZoom: 17 });
  else map.setView([48.5, 31.2], 6);

  if (noCoords.length) {
    var box = L.control({ position: 'topright' });
    box.onAdd = function () {
      var div = L.DomUtil.create('div', 'nocoords');
      var title = document.createElement('b');
      title.textContent = 'Без координат';
      div.appendChild(title);
      noCoords.forEach(function (c) { div.appendChild(camElement(c.name)); });
      L.DomEvent.disableClickPropagation(div);
      L.DomEvent.disableScrollPropagation(div);
      return div;
    };
    box.addTo(map);
  }
</script>
</body>
</html>";
        }
    }
}
