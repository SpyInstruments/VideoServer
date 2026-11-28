# 📹 VideoServer
A lightweight C# application for retrieving, processing, and displaying video streams from IP cameras.

## Overview
VideoServer provides a modular framework for connecting to IP cameras, capturing real‑time video streams, performing motion detection, and running object detection using YOLO. The project includes components for camera management, DirectShow integration, UI control, and stream handling.

## Features
- Connects to IP cameras via HTTP/RTSP  
- Real‑time video stream acquisition  
- WinForms interface for monitoring and control  
- Modular architecture with clear interface boundaries  
- DirectShow support for local video devices  
- **Motion detection module for identifying activity in the frame**  
- **YOLO-based object detection for recognizing objects in real time**

## Project Structure
| Directory | Description |
|----------|-------------|
| **VideoServer** | Core server logic, configuration, and startup |
| **VideoSeverCamers** | IP camera connection and frame retrieval |
| **VideoSeverForm** | WinForms UI for controlling the server |
| **VideoSeverInterfaces** | Interfaces for cameras, streams, and modules |
| **dshow** | DirectShow wrappers and helper utilities |

## Technologies
- C# / .NET Framework  
- WinForms  
- DirectShow  
- HTTP/RTSP streaming  
- **YOLO object detection**  
- **Motion detection algorithms**

## Getting Started
1. Clone the repository:
   ```bash
   git clone https://github.com/SpyInstruments/VideoServer.git
   ```

## License
VCS source code is licensed under the **GNU Lesser General Public License v3.0** (LGPL-3.0) —
see [COPYING.LESSER](COPYING.LESSER) and [COPYING](COPYING) (GPL-3.0, which LGPL-3.0 supplements).

Object detection uses the Ultralytics YOLOv8 model, which is licensed under the
**GNU Affero General Public License v3.0** ([licenses/AGPL-3.0.txt](licenses/AGPL-3.0.txt)).
Builds and installers that include the YOLOv8 model are therefore distributed under the terms of AGPL-3.0;
the corresponding source code is this repository at the release tag.

Third-party components and their licenses are listed in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
