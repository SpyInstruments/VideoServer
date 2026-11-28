# Third-party notices

VCS is licensed under LGPL-3.0 (see [COPYING.LESSER](COPYING.LESSER) and [COPYING](COPYING)).
It uses the following third-party components, each under its own license.

## Components distributed with the installer

| Component | Version | License |
|---|---|---|
| [Ultralytics YOLOv8](https://github.com/ultralytics/ultralytics) model (`yolov8n`) | 8.x | AGPL-3.0 — [licenses/AGPL-3.0.txt](licenses/AGPL-3.0.txt) |
| [LibVLC](https://www.videolan.org/vlc/libvlc.html) (`VideoLAN.LibVLC.Windows`) | 3.0.21 | LGPL-2.1-or-later |
| [LibVLCSharp](https://github.com/videolan/libvlcsharp) | 3.9.4 | LGPL-2.1-or-later |
| [DirectShowLib](https://sourceforge.net/projects/directshownet/) | 1.0.0 | LGPL-2.1 |
| [Microsoft Edge WebView2 SDK](https://www.nuget.org/packages/Microsoft.Web.WebView2) | 1.0.3719.77 | BSD-3-Clause-style Microsoft license (see package `LICENSE.txt`) |
| System.Memory, System.Buffers, System.Text.Json, System.IO.Pipelines, System.Text.Encodings.Web, System.Threading.Tasks.Extensions, System.Numerics.Vectors, System.Runtime.CompilerServices.Unsafe, Microsoft.Bcl.AsyncInterfaces | see `packages.config` | MIT |
| System.IO.Compression.ZipFile and other `System.*` 4.3.x facades | 4.3.x | Microsoft .NET Library License |

When an installer includes the YOLOv8 model, the combined program is distributed under the terms of
AGPL-3.0. Its complete corresponding source code is this repository at the release tag.

## Used at runtime, not distributed

| Component | License |
|---|---|
| [Leaflet](https://leafletjs.com/) 1.9.4 (loaded from unpkg.com in map windows) | BSD-2-Clause |
| [OpenStreetMap](https://www.openstreetmap.org/copyright) map tiles and data | ODbL 1.0 — © OpenStreetMap contributors (attribution shown on the map) |
| Microsoft Edge WebView2 Runtime | Microsoft software license (system component, installed separately) |
| Python YOLO server (`ServerYOLO/`): FastAPI, Uvicorn, Pillow, Ultralytics | MIT / BSD / MIT-CMU / AGPL-3.0 (installed separately via pip) |
