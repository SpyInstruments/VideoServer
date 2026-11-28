using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace VideoServer.UserInterface
{
    public partial class AddViewForm : Form
    {
        XmlDocument _setting = null;
        public AddViewForm()
        {
            InitializeComponent();
            viewGrid.DragEnter += new DragEventHandler(viewGrid_DragEnter);
            viewGrid.DragDrop += new DragEventHandler(viewGrid_DragDrop);
            VisibleChanged += AddViewForm_VisibleChanged;
        }

        // dialog is hidden (OK / Cancel / Back) - close the map too
        private void AddViewForm_VisibleChanged(object sender, EventArgs e)
        {
            if (!Visible && mapForm != null && !mapForm.IsDisposed)
                mapForm.Close();
        }

        bool IsKnownCamera(string name)
        {
            return !string.IsNullOrEmpty(name) && lbxCameras.Items.Contains(name.Trim());
        }

        private void btFinish_Click(object sender, EventArgs e)
        {
            UpdateDialog(true);
            this.DialogResult = DialogResult.OK;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btBack_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry;
        }

        XmlNode node = null;
        public void SetNode(ref XmlNode node)
        {
            this.node = node;
            if (node.SelectSingleNode("row") != null)
                viewGrid.Rows = short.Parse(node.SelectSingleNode("row").InnerText);
            if (node.SelectSingleNode("col") != null)
                viewGrid.Cols = short.Parse(node.SelectSingleNode("col").InnerText);
        }

        public void SetDocument(ref XmlDocument setting)
        {
            this._setting = setting;
        }

        private void viewGrid_Click(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddViewForm_Load(object sender, EventArgs e)
        {
            UpdateTree();
        }

        private void UpdateTree()
        {
            lbxCameras.Items.Clear();
            XmlNode appNode = _setting?.SelectSingleNode("/ROOT/Cameras");
            if (appNode!= null)
            {
                foreach (XmlNode node in appNode.ChildNodes)
                {
                    lbxCameras.Items.Add(node.Attributes["CameraName"].InnerText);
                }
            }
        }

        private void UpdateDialog(bool update)
        {
            if (_setting == null)
                throw new Exception("Відсутній файл налаштуваннь");

            if (update)
            {
                XmlNode nodeCamera;
                for (short i = 0; i < viewGrid.Cols; i++)
                {
                    for (short j = 0; j < viewGrid.Rows; j++)
                    {
                        nodeCamera = _setting.CreateNode(XmlNodeType.Element, "SelectedCamera", "");
                        XmlAttribute attribute = _setting.CreateAttribute("CameraName");
                        nodeCamera.Attributes.Append(attribute);
                        nodeCamera.Attributes["CameraName"].InnerText = viewGrid.GetLabel(j, i);
                        nodeCamera.InnerXml = "<row>" + j.ToString() + "</row>" +
                                                "<col>" + i.ToString() + "</col>" +
                                                "<monday/>" +
                                                "<tuesday/>" +
                                                "<wednesday/>" +
                                                "<thursday/>" +
                                                "<friday/>" +
                                                "<saturday/>" +
                                                "<sunday/>";
                        node.AppendChild(nodeCamera.Clone());
                    }
                }
            }
            else
            {
            }

        }

        void lbxCameras_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (((ListBox)sender).SelectedIndex != -1)
                ((ListBox)sender).DoDragDrop(((ListBox)sender).Text, DragDropEffects.All);
        }

        void viewGrid_DragEnter(object sender, DragEventArgs de)
        {
            if (de.Data.GetDataPresent(DataFormats.Text) &&
                IsKnownCamera(de.Data.GetData(DataFormats.Text) as string))
            {
                de.Effect = DragDropEffects.Copy;
            }
            else
            {
                de.Effect = DragDropEffects.None;
            }

        }

        void viewGrid_DragDrop(object sender, DragEventArgs de)
        {
            string cameraName = "";
            cameraName = (de.Data.GetData(DataFormats.Text) as string)?.Trim();
            if (!IsKnownCamera(cameraName))
                return;
            Point pt = viewGrid.PointToClient(new Point(de.X, de.Y));
            viewGrid.SetLabel(cameraName, pt);
        }

        CamerasMapForm mapForm = null;

        private void btMap_Click(object sender, EventArgs e)
        {
            try
            {
                if (mapForm != null && !mapForm.IsDisposed)
                {
                    mapForm.Activate();
                    return;
                }

                mapForm = new CamerasMapForm(_setting);
                mapForm.FormClosed += (s, a) => mapForm = null;

                // place the map next to this window, keep it on screen
                Rectangle screen = Screen.FromControl(this).WorkingArea;
                int x = Right;
                if (x + mapForm.Width > screen.Right)
                    x = Math.Max(screen.Left, screen.Right - mapForm.Width);
                mapForm.Location = new Point(x, Math.Max(screen.Top, Top));

                // owned, non-modal: user can drag cameras from the map into the grid
                mapForm.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
