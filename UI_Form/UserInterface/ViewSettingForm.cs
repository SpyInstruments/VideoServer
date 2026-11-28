using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace VideoServer.UserInterface
{
    public partial class ViewSettingForm : Form
    {
        
        public ViewSettingForm()
        {
            InitializeComponent();
        }

        private void btNext_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateDialog(true);
                this.DialogResult = DialogResult.OK;
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        int row = 2;
        [DefaultValue(2)]
        public int Row 
        {
            get { return row; }
            set { row = value; }
        }

        int col = 2;
        [DefaultValue(2)]
        public int Col
        {
            get { return col; }
            set { col = value; }
        }
        XmlNode node = null;
        public void  GetNode(ref XmlNode node)
        {
            node = this.node;
        }
        
        XmlDocument setting = null;
        public void SetDocument(ref XmlDocument setting)
        {
            this.setting = setting;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        XmlNode editView = null;

        /// <summary>
        /// Fill the dialog from an existing CamerasView node (edit mode)
        /// </summary>
        public void SetNode(XmlNode view)
        {
            editView = view;
        }

        private void ViewSettingForm_Load(object sender, EventArgs e)
        {
            cbCol.SelectedIndex = 1;
            cbRow.SelectedIndex = 1;

            if (editView != null)
            {
                tbName.Text = editView.Attributes?["ViewName"]?.Value ?? "";
                SelectItem(cbRow, editView.SelectSingleNode("row")?.InnerText);
                SelectItem(cbCol, editView.SelectSingleNode("col")?.InnerText);
                if (editView.SelectSingleNode("width") != null)
                    tbWidth.Text = editView.SelectSingleNode("width").InnerText;
                if (editView.SelectSingleNode("height") != null)
                    tbHeight.Text = editView.SelectSingleNode("height").InnerText;
                rtbDescription.Text = editView.SelectSingleNode("description")?.InnerText ?? "";
            }
        }

        static void SelectItem(ComboBox cb, string value)
        {
            if (value == null)
                return;
            int index = cb.Items.IndexOf(value.Trim());
            if (index >= 0)
                cb.SelectedIndex = index;
        }
        private void UpdateDialog(bool update)
        {
            if(setting==null)
                throw new Exception("Не введён документ настроек");


            if (update)
            {
                if(tbName.Text =="")
                    throw new Exception("Введите имя окна");
                int i = 0;
                if(!int.TryParse(tbWidth.Text,out i))
                    throw new Exception("Введите ширину окна");

                if (!int.TryParse(tbHeight.Text,out i))
                    throw new Exception("Введите высоту окна");


                XmlNode nodeView = setting.CreateNode(XmlNodeType.Element, "CamerasView", "");
                XmlAttribute attribute = setting.CreateAttribute("ViewName");
                nodeView.Attributes.Append(attribute);
                nodeView.Attributes["ViewName"].InnerText = tbName.Text;

                nodeView.InnerXml = "<row>" +cbRow.Text+"</row>"+
                                     "<col>" + cbCol.Text+"</col>" +
                                     "<width>" +tbWidth.Text+"</width>" +
                                     "<height>"+ tbHeight.Text+"</height>"+
                                     "<description>"+rtbDescription.Text+"</description>";

                node = nodeView.Clone();
            }
            else
            {

            }

        }
       
    }
}
