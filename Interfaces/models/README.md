# YOLO model

`yolov8n.onnx` is used for object detection (`Interfaces/YoloModel.cs`) with ONNX Runtime, inside the application.
It is copied to `<application folder>\models\` on build and by the installer.

The model is Ultralytics YOLOv8n, licensed under AGPL-3.0 (see `licenses/AGPL-3.0.txt` in the repository root).

## Re-export

```bash
pip install ultralytics onnx
yolo export model=yolov8n.pt format=onnx imgsz=640 opset=17 simplify=False dynamic=False
```

The code expects the standard YOLOv8 detection output `[1, 4 + classes, anchors]` and a fixed 640×640 input.
Class names are read from the model metadata (`names`), COCO names are used as a fallback.
