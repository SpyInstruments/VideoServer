using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using VideoServer.Tools;
using System.IO;
using System.Collections;

namespace VideoServer
{
    public partial class MainForm : Form
    {
        CameraCollection camersBaseTypes = null;
        CameraCollection _camers = null;
        Camera _camera = null;
        XmlDocument _setting = null;
        static string appPath = "";
        UserInterface.AddCameraForm addCameraForm = null;
        UserInterface.ViewSettingForm viewSettingForm = null;
        UserInterface.AddViewForm viewForm = null;
        UserInterface.AboutForm aboutForm = new UserInterface.AboutForm();
        UserInterface.SettingForm settingForm = new VideoServer.UserInterface.SettingForm();
        Hashtable tableIAdaptee = null;
        Point mainWindowLocation;
        Size mainWindowSize;


        public MainForm()
        {
            InitializeComponent();
            treeView.MouseClick += new MouseEventHandler(MainForm_MouseClick);
            this.WindowState = FormWindowState.Maximized;

            appPath = Path.GetFullPath("xml");
            if (!Directory.Exists(appPath))
            {
                Directory.CreateDirectory(appPath);
            }

            string filePath = Path.Combine(appPath, "Setting.xml");
            if (!File.Exists(filePath))
            {
                string defaultXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ROOT xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:noNamespaceSchemaLocation=""CameraViwer.xsd"">
	<Cameras/>
	<CamerasViews/>
</ROOT>";
                File.WriteAllText(filePath, defaultXml);
            }

            _setting = new XmlDocument();
            _setting.Load(filePath);

            _camers =  new CameraCollection();

            settingForm = new VideoServer.UserInterface.SettingForm();
            settingForm.OnSaveSettingFile += SettingForm_OnSaveSettingFile;
            settingForm.OnLoadSettingFile += SettingForm_OnLoadSettingFile;
            settingForm.OnAddSettingFile += SettingForm_OnAddSettingFile;
        }

        /// <summary>
        /// Merge cameras from another settings file into current settings
        /// </summary>
        private void SettingForm_OnAddSettingFile(object sender, string path)
        {
            if (_setting == null)
                return;

            XmlDocument source = new XmlDocument();
            source.Load(path);

            XmlNodeList sourceCameras = source.SelectNodes("/ROOT/Cameras/Camera");
            if (sourceCameras == null || sourceCameras.Count == 0)
            {
                MessageBox.Show("У файлі немає камер", "Додати камери", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            XmlNode camerasNode = _setting.SelectSingleNode("/ROOT/Cameras");
            if (camerasNode == null)
            {
                camerasNode = _setting.CreateElement("Cameras");
                _setting.DocumentElement.AppendChild(camerasNode);
            }

            // name -> existing camera node in current settings
            var existing = new System.Collections.Generic.Dictionary<string, XmlNode>();
            foreach (XmlNode node in camerasNode.SelectNodes("Camera"))
            {
                string name = GetCameraName(node);
                if (!string.IsNullOrEmpty(name) && !existing.ContainsKey(name))
                    existing.Add(name, node);
            }

            // collect cameras from file, check name conflicts
            var toImport = new System.Collections.Generic.List<XmlNode>();
            var conflicts = new System.Collections.Generic.List<string>();
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (XmlNode node in sourceCameras)
            {
                string name = GetCameraName(node);
                if (string.IsNullOrEmpty(name) || !seen.Add(name))
                    continue; // no name or duplicated inside the file

                toImport.Add(node);
                if (existing.ContainsKey(name))
                    conflicts.Add(name);
            }

            bool replace = false;
            if (conflicts.Count > 0)
            {
                DialogResult answer = MessageBox.Show(
                    "Камери з такими назвами вже існують:\n\n" + string.Join("\n", conflicts) +
                    "\n\nТак - замінити їх\nНі - пропустити\nСкасувати - нічого не додавати",
                    "Додати камери", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (answer == DialogResult.Cancel)
                    return;
                replace = answer == DialogResult.Yes;
            }

            int added = 0, replaced = 0, skipped = 0;
            foreach (XmlNode node in toImport)
            {
                string name = GetCameraName(node);
                XmlElement imported = (XmlElement)_setting.ImportNode(node, true);
                imported.SetAttribute("CameraName", name);

                XmlNode old;
                if (existing.TryGetValue(name, out old))
                {
                    if (!replace)
                    {
                        skipped++;
                        continue;
                    }
                    camerasNode.ReplaceChild(imported, old);
                    replaced++;
                }
                else
                {
                    camerasNode.AppendChild(imported);
                    added++;
                }
            }

            _setting.Save(appPath + "\\Setting.xml");
            UpdateTree();

            MessageBox.Show(
                string.Format("Додано: {0}\nЗамінено: {1}\nПропущено: {2}", added, replaced, skipped),
                "Додати камери", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Camera name from CameraName attribute, or CameraName element if attribute is missing
        /// </summary>
        static string GetCameraName(XmlNode camera)
        {
            string name = camera.Attributes?["CameraName"]?.Value;
            if (string.IsNullOrWhiteSpace(name))
                name = camera["CameraName"]?.InnerText;
            return name?.Trim();
        }

        private void SettingForm_OnLoadSettingFile(object sender, string path)
        {
            _setting?.Load(path);
            UpdateTree();
        }

        private void SettingForm_OnSaveSettingFile(object sender, string path)
        {
            _setting?.Save(path);
        }

        void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Drawing.Point p = treeView.PointToScreen(e.Location);
                cmenuStrip.Show(p);
            }
        }

        private void addCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddCamera();
        }

        private void delCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DelCamera();
        }

        private void conCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void addViewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AddView();
        }

        private void delViewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DelView();
        }

        private void showViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowView();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                camersBaseTypes = new CameraCollection();
                tableIAdaptee = new Hashtable();
                camersBaseTypes.Load(Path.GetDirectoryName(Application.ExecutablePath), ref tableIAdaptee);
                addCameraForm = new VideoServer.UserInterface.AddCameraForm(camersBaseTypes);


                UpdateTree();
                mainWindowLocation = this.Location;
                mainWindowSize = this.Size;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        void UpdateTree()
        {
            treeView.Nodes.Clear();
            TreeNode camerasNode = treeView.Nodes.Add("1", "Камери");
            TreeNode camerasViewNode = treeView.Nodes.Add("2", "Вікна нагляду");
            int key = 3;
            XmlNode appNode = _setting.SelectSingleNode("/ROOT/Cameras");
            foreach (XmlNode node in appNode.ChildNodes)
            {
                camerasNode.Nodes.Add(key.ToString(), node.Attributes["CameraName"].InnerText);
                key++;
            }
            appNode = _setting.SelectSingleNode("/ROOT/CamerasViews");
            foreach (XmlNode node in appNode.ChildNodes)
            {
                camerasViewNode.Nodes.Add(key.ToString(), node.Attributes["ViewName"].InnerText);
                key++;
            }
            return;
        }

        // Full screen
        private void FullScreen(bool full)
        {
            if (full)
            {
                mainWindowLocation = this.Location;
                mainWindowSize = this.Size;

                this.FormBorderStyle = FormBorderStyle.None;
                this.MainMenuStrip = null;

                int cx = Win32.GetSystemMetrics(Win32.SystemMetrics.CXSCREEN);
                int cy = Win32.GetSystemMetrics(Win32.SystemMetrics.CYSCREEN);

                this.Location = new Point(-4, -0);
                this.Size = new Size(cx + 5, cy + 1);
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.MainMenuStrip = this.menuStrip;

                // restore window position
                this.Location = mainWindowLocation;
                this.Size = mainWindowSize;
            }


            // set/reset top most window
            this.TopMost = full;

            // hide/restore cameras bar
            this.treeView.Visible = !full;
            this.menuStrip.Visible = !full;
            this.statusStrip.Visible = !full;
        }

        private void FitToScreen(bool fit)
        {
            _viewPanel.FitToWindow = fit;
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if ((treeView?.SelectedNode?.Name != null) && (treeView.SelectedNode.Name == "1"))
                {
                    ShowCamera();
                }
                else if ((treeView?.SelectedNode?.Parent != null) && (treeView.SelectedNode.Parent.Name == "1"))
                {
                    ShowCamera();
                }
                else if ((treeView?.SelectedNode?.Name != null) && (treeView.SelectedNode.Name == "2"))
                {
                    ShowView();
                }
                else if ((treeView?.SelectedNode?.Parent != null) && (treeView.SelectedNode.Parent.Name == "2"))
                {
                    ShowView();
                }
            }
            catch (Exception)
            { }

        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if ((treeView?.SelectedNode?.Name != null) && (treeView.SelectedNode.Name == "1"))
                {
                    AddCamera();
                }
                else if ((treeView?.SelectedNode?.Parent != null) && (treeView.SelectedNode.Parent.Name == "1"))
                {
                    AddCamera();
                }
                else if ((treeView?.SelectedNode?.Name != null) && (treeView.SelectedNode.Name == "2"))
                {
                    AddView();
                }
                else if ((treeView?.SelectedNode?.Parent != null) && (treeView.SelectedNode.Parent.Name == "2"))
                {
                    AddView();
                }
            }
            catch (Exception)
            { }

        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if ((treeView?.SelectedNode?.Name != null) && (treeView.SelectedNode.Name == "1"))
                {
                    DelCamera();
                }
                else if ((treeView?.SelectedNode?.Parent != null) && (treeView.SelectedNode.Parent.Name == "1"))
                {
                    DelCamera();
                }
                else if ((treeView?.SelectedNode?.Name != null) && (treeView.SelectedNode.Name == "2"))
                {
                    DelView();
                }
                else if ((treeView?.SelectedNode?.Parent != null) && (treeView.SelectedNode.Parent.Name == "2"))
                {
                    DelView();
                }
            }
            catch (Exception)
            { }


        }

        private void editToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // context menu: edit camera or view depending on the selected tree node
            if (treeView?.SelectedNode?.Parent != null && treeView.SelectedNode.Parent.Name == "2")
                EditView();
            else
                EditCamera();
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                settingForm.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutForm.ShowDialog();
        }

        private void fullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fullToolStripMenuItem.Checked = !fullToolStripMenuItem.Checked;
            FullScreen(fullToolStripMenuItem.Checked);
            FitToScreen(fitToolStripMenuItem.Checked);
        }

        private void fitToolStripMenuItem_Click(object sender, EventArgs e)
        {

            fitToolStripMenuItem.Checked = !fitToolStripMenuItem.Checked;
            FitToScreen(fitToolStripMenuItem.Checked);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditCamera();
        }

        void AddCamera()
        {
            try
            {

                if (_setting == null)
                    return;

                XmlNode node = _setting.CreateNode(XmlNodeType.Element, "Camera", "");
                if (node == null)
                    return;

                if (addCameraForm.ShowDialog() == DialogResult.OK)
                {
                    addCameraForm.GetCameraConfiguration(ref node);
                    XmlNode appNode = _setting.SelectSingleNode("/ROOT/Cameras/Camera[@CameraName=\"" + node["CameraName"].InnerText + "\"]");
                    if (appNode != null)
                    {
                        MessageBox.Show("Камера с такою назвою існуе");
                        return;
                    }
                    appNode = _setting.SelectSingleNode("/ROOT/Cameras");

                    XmlAttribute attribute = _setting.CreateAttribute("CameraName");
                    node.Attributes.Append(attribute);
                    node.Attributes["CameraName"].InnerText = node["CameraName"].InnerText;
                    appNode.AppendChild(node);
                    _setting.Save(appPath + "\\Setting.xml");
                    UpdateTree();
                };

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void DelCamera()
        {
            try
            {

                if ((_setting == null) || (treeView?.SelectedNode?.Name == "1") || (treeView?.SelectedNode?.Name == "2"))
                    return;

                XmlNode node = _setting.SelectSingleNode("/ROOT/Cameras/Camera[@CameraName=\"" + treeView?.SelectedNode?.Text + "\"]");
                if (node == null)
                    return;

                XmlNode appNode = _setting.SelectSingleNode("/ROOT/Cameras");
                appNode.RemoveChild(node);
                _setting.Save(appPath + "\\Setting.xml");
                UpdateTree();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void EditCamera()
        {
            try
            {

                if ((_setting == null) || (treeView?.SelectedNode?.Name == "1") || (treeView?.SelectedNode?.Name == "2"))
                    return;

                XmlNode node = _setting.SelectSingleNode("/ROOT/Cameras/Camera[@CameraName=\"" + treeView.SelectedNode.Text + "\"]");
                if (node == null)
                    return;

                addCameraForm.SetCameraConfiguration(node.Clone());
                if (addCameraForm.ShowDialog() == DialogResult.OK)
                {
                    XmlNode appNode = _setting.SelectSingleNode("/ROOT/Cameras");
                    appNode.RemoveChild(node);

                    node = _setting.CreateNode(XmlNodeType.Element, "Camera", "");
                    addCameraForm.GetCameraConfiguration(ref node);

                    XmlAttribute attribute = _setting.CreateAttribute("CameraName");
                    node.Attributes.Append(attribute);
                    node.Attributes["CameraName"].InnerText = node["CameraName"].InnerText;

                    appNode.AppendChild(node);
                    _setting.Save(appPath + "\\Setting.xml");
                    UpdateTree();

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void ShowCamera()
        {
            try
            {

                if ((_setting == null) || (treeView?.SelectedNode?.Name == "1") || (treeView?.SelectedNode?.Name == "2"))
                    return;

                _camera?.Stop();

                foreach (Camera cam in _camers)
                {
                    cam?.Stop();
                }
                _camers?.Clear();


                _camera = CreateCamera(treeView?.SelectedNode?.Text);
                _viewPanel.SetCamera(0, 0, _camera);
                _viewPanel.Rows = 1;
                _viewPanel.Cols = 1;
                _viewPanel.SingleCameraMode = true;
                _viewPanel.CamerasVisible = true;
                _viewPanel.FitToWindow = true;

                _camera.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void AddView()
        {
            try
            {
                XmlNode node = null;

                viewSettingForm = new VideoServer.UserInterface.ViewSettingForm();
                viewSettingForm.SetDocument(ref _setting);

                viewForm = new VideoServer.UserInterface.AddViewForm();
                viewForm.SetDocument(ref _setting);

                do
                {
                    if (viewSettingForm.ShowDialog() != DialogResult.OK)
                        return;
                    viewSettingForm.GetNode(ref node);

                    viewForm.SetNode(ref node);


                } while (viewForm.ShowDialog() == DialogResult.Retry);

                if (viewForm.DialogResult == DialogResult.OK)
                {
                    XmlNode addNode = _setting?.SelectSingleNode("/ROOT/CamerasViews");
                    addNode.AppendChild(node);
                    _setting.Save(appPath + "\\Setting.xml");
                    UpdateTree();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        XmlNode FindView(string viewName)
        {
            XmlNode views = _setting?.SelectSingleNode("/ROOT/CamerasViews");
            if (views == null)
                return null;
            foreach (XmlNode view in views.SelectNodes("CamerasView"))
            {
                if (view.Attributes?["ViewName"]?.Value == viewName)
                    return view;
            }
            return null;
        }

        void EditView()
        {
            try
            {
                if ((_setting == null) || (treeView?.SelectedNode?.Parent == null) || (treeView.SelectedNode.Parent.Name != "2"))
                    return;

                string oldName = treeView.SelectedNode.Text;
                XmlNode oldNode = FindView(oldName);
                if (oldNode == null)
                    return;

                XmlNode node = null;

                viewSettingForm = new VideoServer.UserInterface.ViewSettingForm();
                viewSettingForm.SetDocument(ref _setting);
                viewSettingForm.SetNode(oldNode);

                viewForm = new VideoServer.UserInterface.AddViewForm();
                viewForm.SetDocument(ref _setting);

                bool first = true;
                do
                {
                    if (viewSettingForm.ShowDialog() != DialogResult.OK)
                        return;
                    viewSettingForm.GetNode(ref node);

                    viewForm.SetNode(ref node);
                    if (first)
                    {
                        // grid keeps its labels on Back -> Next, so fill it only once
                        viewForm.SetSelectedCameras(oldNode);
                        first = false;
                    }

                } while (viewForm.ShowDialog() == DialogResult.Retry);

                if (viewForm.DialogResult == DialogResult.OK)
                {
                    string newName = node.Attributes["ViewName"].Value;
                    XmlNode sameName = FindView(newName);
                    if (sameName != null && sameName != oldNode)
                    {
                        MessageBox.Show("Вікно з такою назвою вже існує");
                        return;
                    }

                    oldNode.ParentNode.ReplaceChild(node, oldNode);
                    _setting.Save(appPath + "\\Setting.xml");
                    UpdateTree();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void DelView()
        {
            try
            {
                if ((_setting == null) || (treeView?.SelectedNode == null) || (treeView?.SelectedNode?.Name == "1") || (treeView?.SelectedNode?.Name == "2"))
                    return;

                XmlNode node = _setting?.SelectSingleNode("/ROOT/CamerasViews/CamerasView[@ViewName=\"" + treeView.SelectedNode.Text + "\"]");
                if (node != null)
                {
                    XmlNode appNode = _setting.SelectSingleNode("/ROOT/CamerasViews");
                    appNode.RemoveChild(node);
                    _setting.Save(appPath + "\\Setting.xml");
                    treeView.Nodes.Remove(treeView.SelectedNode);
                    //UpdateTree();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void ShowView()
        {
            try
            {
                if ((_setting == null) || (treeView?.SelectedNode == null) || (treeView?.SelectedNode?.Name == "1") || (treeView?.SelectedNode?.Name == "2"))
                    return;
                _camera?.Stop();
                foreach (Camera cam in _camers)
                {
                    cam?.Stop();
                }
                _camers.Clear();

                XmlNode node = _setting?.SelectSingleNode("/ROOT/CamerasViews/CamerasView[@ViewName=\"" + treeView.SelectedNode.Text + "\"]");
                if (node != null)
                {

                    if (node.SelectSingleNode("row") != null)
                        _viewPanel.Rows = int.Parse(node.SelectSingleNode("row").InnerText);

                    if (node.SelectSingleNode("col") != null)
                        _viewPanel.Cols = int.Parse(node.SelectSingleNode("col").InnerText);

                    if (node.SelectSingleNode("width") != null)
                        _viewPanel.Width = int.Parse(node.SelectSingleNode("width").InnerText);

                    if (node.SelectSingleNode("height") != null)
                        _viewPanel.Height = int.Parse(node.SelectSingleNode("height").InnerText);

                    _viewPanel.SingleCameraMode = false;
                    _viewPanel.CamerasVisible = true;
                    _viewPanel.FitToWindow = true;

                    XmlNodeList nodesCameras = node.SelectNodes("SelectedCamera");

                    foreach (XmlNode nodeCamera in nodesCameras)
                    {
                        int row = 0;
                        int col = 0;

                        string cameraName = nodeCamera.Attributes["CameraName"].Value;
                        Camera camera = CreateCamera(cameraName);

                        if (nodeCamera.SelectSingleNode("row") != null)
                            row = int.Parse(nodeCamera.SelectSingleNode("row").InnerText);

                        if (nodeCamera.SelectSingleNode("col") != null)
                            col = int.Parse(nodeCamera.SelectSingleNode("col").InnerText);

                        if(camera != null)
                            _viewPanel.SetCamera(row, col, camera);

                        _camers.Add(camera);
                    }

                    foreach (Camera camera in _camers)
                    {
                        camera?.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        Camera CreateCamera(string cameraName)
        {
            Camera camera = null;
            XmlNode node = _setting.SelectSingleNode("/ROOT/Cameras/Camera[@CameraName=\"" + cameraName + "\"]");

            if (node == null)
                return null;

            string cameraType = node.SelectSingleNode("CameraType").InnerText;
            camera = new Camera();
            camera.ISourceAdaptee = (Interfaces.ISourceAdaptee)tableIAdaptee[cameraType];
            camera.SetConfiguration(node);
            return camera;
        }

    }
}
