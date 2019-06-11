using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DialogMaker
{
    public partial class Form1 : Form
    {
        private int attributesForPhrase;
        public Form1()
        {
            InitializeComponent();

            comboBox1.Items.Clear();
            foreach (ColorSpace cn in Dialog.XColors)
                comboBox1.Items.Add(cn.Name);


            dialogs1.SelectedChanging += new Dialogs.SelectEventHandler(dialogs1_SelectedChanging);
            dialogs1.SelectedChangingTL += new Dialogs.SelectEventHandlerTL(dialogs1_SelectedChangingTL);
            dialogs1.SelectedChanged += new Dialogs.SelectEventHandler(dialogs1_SelectedChanged);
            dialogs1.SelectedChangedTL += new Dialogs.SelectEventHandlerTL(dialogs1_SelectedChangedTL);
            dialogs1.OnStateChanged += new Dialogs.StateChanged(dialogs1_OnStateChanged);
            updateStatusLabel();

            attributesForPhrase = dialogs1.phraseAttributesCountGet;
            this.Text = "DMPRO";

            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            this.Shown += Form1_Shown;
            PrepareHistoryMenu(OpenFileHistory.DHistory);
            OpenFileHistory.OnUpdate += new OpenFileHistory.HistoryUpdated(PrepareHistoryMenu);

            dialogs1.TreeOperationMode = checkBox1.Checked = true;

            // init empty container
            jzip.Container container = new jzip.Container(null);
            Dialog dlg = new Dialog(container);
            dialogs1.dlg = new DBacklog(dlg);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (deferredLoadingPath != null)
            {
                LoadJsFile(deferredLoadingPath);
                deferredLoadingPath = null;
            }
        }
        string LastLoadedJson = "";
        bool LastLoadedEncoded = false;

        string deferredLoadingPath = null;

        public void DeferLoadJsFile(string path)
        {
            deferredLoadingPath = path;
        }
        void dialogs1_OnStateChanged(bool changed)
        {
            updateStatusLabel();
        }

        void PrepareHistoryMenu(OpenFileHistory h)
        {
            historyToolStripMenuItem.DropDownItems.Clear();
            //OpenFileHistory h = OpenFileHistory.DHistory;
            foreach (OpenFileHistory.MenuString item in h.ByLA)
            {
                ToolStripMenuItem mi = new ToolStripMenuItem(item.Title, null,
                    (object sender, EventArgs ea) =>
                    {
                        if (Event_LostData())
                            return;
                        LoadJsFile((sender as ToolStripMenuItem).ToolTipText);
                        dialogs1.Refresh();
                    });
                mi.ToolTipText = item.Path;
                historyToolStripMenuItem.DropDownItems.Add(mi);
            }
        }
        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = Event_LostData();
            if (!e.Cancel)
            {
                if (dialogs1.imageContainer != null)
                {
                    dialogs1.imageContainer.Dispose();
                    File.Delete(dialogs1.imageContainer.FileName);
                }
            }
        }
        bool Event_LostData()
        {
            //returns true if cancel appear
            if (dialogs1.Changed)
            {
                DialogResult dr = MessageBox.Show(
                  "Project has been changed.\r\n\r\nSave It?", "Project changed", MessageBoxButtons.YesNoCancel);
                switch (dr)
                {
                    case DialogResult.Cancel:
                        return true;
                    case DialogResult.No:
                        break;
                    case DialogResult.Yes:
                        if (!SaveLastSavedLoaded())
                            return true;
                        break;
                    default:
                        break;
                }
            }
            return false;
        }

        void updateStatusLabel()
        {
            label4.Text = dialogs1.Changed ? "unsaved" : "saved";
            if (loading)
                label4.Text = "loading";

        }


        void dialogs1_SelectedChangingTL(TimeLine p)
        {
            if (textBox1.Text != CHT)
                dialogs1.SetSelectedText(textBox1.Text);
        }
        string CHT;
        string CHL;
        void dialogs1_SelectedChangedTL(TimeLine p)
        {
            CHT = p.Text;
            CHL = "";

            textBox1.Text = p.Text;
            comboBox1.Enabled = false;
            textBox2.Enabled = false;
        }
        void dialogs1_SelectedChanging(Phrase p)
        {
            if (textBox1.Text != CHT)
                dialogs1.SetSelectedText(textBox1.Text);
            if (textBox2.Text != CHL)
                dialogs1.SetSelectedLabel(textBox2.Text);
        }
        void dialogs1_SelectedChanged(Phrase p)
        {
            if (p == null)
            {
                panel1.BackgroundImage = null;
                return;
            }
            CHT = p.Text;
            CHL = p.Label;

            string cbt = "B";
            int c0 = p.color.Colors[0].R;
            for (int i = 0; i < Dialog.XColors.Length; i++)
            {
                if (c0 == Dialog.XColors[i].Colors[0].R)
                { cbt = Dialog.XColors[i].Name; break; }
            }

            comboBox1.Text = cbt;
            textBox1.Text = p.Text;
            textBox2.Text = p.Label;
            comboBox1.Enabled = true;
            textBox2.Enabled = true;

            if (p.GraphicClipThumbnail != null)
            {
                panel1.BackgroundImage = p.GraphicClipThumbnail;
            }
            else
            {
                panel1.BackgroundImage = null;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem is string)
            {
                string si = comboBox1.SelectedItem as string;
                for (int i = 0; i < Dialog.XColors.Length; i++)
                {
                    if (si == Dialog.XColors[i].Name)
                    {
                        dialogs1.SetSelectedColorspace(Dialog.XColors[i]);
                        break;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != CHT)
            {
                dialogs1.SetSelectedText(textBox1.Text);
                CHT = textBox1.Text;
            }
            if (textBox2.Text != CHL)
            {
                dialogs1.SetSelectedLabel(textBox2.Text);
                CHL = textBox2.Text;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dialogs1.ScaleZoom += .1f;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dialogs1.ScaleZoom = Math.Max(0.1f, dialogs1.ScaleZoom - .1f);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            dialogs1.AddAsChild("!");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            dialogs1.ConnectTo();
        }
        SaveFileDialog sfdDiagramm = new SaveFileDialog();
        private void button7_Click(object sender, EventArgs e)
        {
            SavePng();
        }

        private void SavePng()
        {
            sfdDiagramm.Filter = "Png Images|*.png";
            DialogResult dr = sfdDiagramm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Image img = dialogs1.DrawDiagram();
                img.Save(sfdDiagramm.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        private void SaveSvg()
        {
            sfdDiagramm.Filter = "Svg Images|*.svg";
            DialogResult dr = //DialogResult.OK;//
            sfdDiagramm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string img = dialogs1.DrawDiagramSvg();
                StreamWriter tw = new StreamWriter(sfdDiagramm.FileName, false);
                tw.Write(img);
                tw.Close();
            }
        }
        SaveFileDialog sfdJson = new SaveFileDialog();
        private void button8_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void SaveAs()
        {
            //sfdJson.Filter = "Json Encoded|*.json.e|Json Dialog|*.json";
            sfdJson.Filter = "Json Encoded Container|*.jzipe|Json Container|*.jzip";
            DialogResult dr = sfdJson.ShowDialog();
            if (dr == DialogResult.OK)
            {
                bool encode = sfdJson.FilterIndex == 1;
                SaveTo(sfdJson.FileName, encode);
            }
        }
        OpenFileDialog ofdJson = new OpenFileDialog();
        OpenFileDialog ofdImage = new OpenFileDialog();
        private bool loading;

        private void button9_Click(object sender, EventArgs e)
        {
            LoadOpen();
        }

        private void LoadOpen()
        {
            if (Event_LostData())
                return;
            ofdJson.Filter = "Json Encoded Container|*.jzipe|Json Container|*.jzip|Json Encoded|*.json.e|Json Dialog|*.json|All Compatiable|*.jzipe;*.jzip;*.json.e;*.json";
            DialogResult dr = ofdJson.ShowDialog();
            if (dr == DialogResult.OK)
            {
                LoadJsFile(ofdJson.FileName);
            }
            dialogs1.Refresh();
        }
        private void LoadOpenImage()
        {
            ofdImage.Filter = "Image Files(*.JPG;*.PNG;*.GIF;*.BMP)|*.JPG;*.JPEG;*.PNG;*.GIF;*.BMP";
            //ofdImage.Reset();

            DialogResult dr = ofdImage.ShowDialog();
            if (dr == DialogResult.OK)
            {
                try
                {
                    Image img = Image.FromFile(ofdImage.FileName);
                    dialogs1.SetSelectedImage(img);
                    panel1.BackgroundImage = dialogs1.dlg.Phrase(dialogs1.FocusedEditPhrase).GraphicClipThumbnail;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error while open image: " + e.Message);
                    return;
                }

            }
            dialogs1.Refresh();
        }
        private string PasswordRequest()
        {
            string pwd_p = "";
            PWD_INPUT pwd_dlg = new PWD_INPUT();
            pwd_dlg.Icon = this.Icon;
            DialogResult dr = pwd_dlg.ShowDialog();
            if (dr != System.Windows.Forms.DialogResult.OK) return null;

            pwd_p = pwd_dlg.pwd_ref;
            pwd_dlg.Dispose();

            return pwd_p;
        }
        public void LoadJsFile(string path)
        {
            loading = true;
            updateStatusLabel(); this.Refresh();
            string json = string.Empty;
            bool encoded = false;
            try
            {
                string workingFile = null;
                using (System.IO.Stream sp = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    int a = sp.ReadByte(); int b = sp.ReadByte();

                    if (encoded = (a == 0xae && (b == 0xfe || b == 0xfa)) || (a == 0x9e && b == 0x3f))
                    {
                        int fixedLength = 0;
                        if (b == 0xfa || (a == 0x9e && b == 0x3f))
                        {
                            byte[] jslength = new byte[4];
                            if (sp.Read(jslength, 0, 4) != 4)
                                throw new Exception("file corrupted.");
                            fixedLength = BitConverter.ToInt32(jslength, 0);
                            sp.Seek(6, SeekOrigin.Begin);
                        }
                        else if (b == 0xfe)
                        {
                            fixedLength = ((int)sp.Length) - 2;
                            sp.Seek(2, SeekOrigin.Begin);
                        }

                        string pwd_p = PasswordRequest();
                        if (pwd_p == null) return;

                        DialogMaker.SecurityStream sec = new DialogMaker.SecurityStream(sp, pwd_p);
                        if (!(a == 0x9e && b == 0x3f))//not zip encoded
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                sec.CopyTo(ms);
                                json = Encoding.UTF8.GetString(ms.GetBuffer(), 0, fixedLength);
                            }


                            sec.Close();

                            sec = null;
                            //!!!
                            LastLoadedEncoded = true;
                        }
                        else
                        {
                            workingFile = Path.GetTempFileName();
                            using (FileStream fs = new FileStream(workingFile, FileMode.Create))
                            {
                                sec.CopyTo(fs);
                                fs.Flush();
                                fs.SetLength(fixedLength);
                            }
                            LastLoadedEncoded = true;
                        }
                    }
                    else if ((a == 'P' && b == 'K'))
                    {
                        workingFile = Path.GetTempFileName();
                        sp.Seek(0, SeekOrigin.Begin);
                        using (FileStream fs = new FileStream(workingFile, FileMode.Create))
                        {
                            sp.CopyTo(fs);
                        }
                    }
                    else
                    {
                        sp.Seek(0, SeekOrigin.Begin);
                        StreamReader sr = new StreamReader(sp, Encoding.UTF8);
                        json = sr.ReadToEnd();
                        sr.Close();
                        //!!!!
                        LastLoadedEncoded = false;
                    }
                }
                //string json = sr.ReadToEnd();//System.IO.File.ReadAllText(path, Encoding.UTF8);
                //sr.Close();
                //!!!
                LastLoadedJson = path;

                //Dialog dlg = Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(Dialog)) as Dialog;
                //dialogs1.Clear();
                jzip.Container container = new jzip.Container(workingFile);
                Dialog dlg = new Dialog(container);
                if (workingFile == null)
                    Newtonsoft.Json.JsonConvert.PopulateObject(json, dlg);
                else
                    container.ExtractLinkedData(dlg);
                foreach (var item in dlg.phrases)
                {
                    if (item._attributes == null)
                        item._attributes = new byte[attributesForPhrase];
                }

                dialogs1.dlg = new DBacklog(dlg);
                dialogs1.imageContainer = container;
                this.Text = "DMPRO \"" + System.IO.Path.GetFileName(path) + "\"";
                dialogs1.Changed = false;
                OpenFileHistory.DHistory.Push(path);

                this.Refresh();
            }
            catch (Exception exc)
            {
                if (!string.IsNullOrEmpty(json))
                {
                    string failOverPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_failover_" + Path.GetExtension(path));
                    if (encoded)
                    {
                        try
                        {
                            File.WriteAllText(failOverPath, json, Encoding.UTF8);
                        }
                        catch
                        {
                            failOverPath = Path.Combine(Path.GetFileNameWithoutExtension(path) + "_failover_" + Path.GetExtension(path));
                            File.WriteAllText(failOverPath, json, Encoding.UTF8);
                        }
                    }
                    else
                    {
                        failOverPath = null;
                    }
                    System.Windows.Forms.MessageBox.Show(
                        string.Format("Error parsing saved dialog.\r\n{0} \r\n json saved as: {1}",
                        exc.Message, failOverPath ?? "not saved"),
                        "open file error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(
                       string.Format("Error occured while loading: {0}",
                       exc.Message),
                       "open file error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                json = null;
                loading = false;
                updateStatusLabel();
            }
            GC.Collect();
            System.Threading.Thread.Sleep(100);
            GC.Collect();
            System.Threading.Thread.Sleep(100);
        }
        private void button10_Click(object sender, EventArgs e)
        {
            dialogs1.AddTL("###");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            dialogs1.RemoveSelected();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            dialogs1.SeparateWith();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            dialogs1.Clone();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            dialogs1.CloneTree();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            dialogs1.TreeOperationMode = checkBox1.Checked;
        }
        void SaveTo(string file, bool encode)
        {
            dialogs1_SelectedChanging(null);
            dialogs1_SelectedChangingTL(null);

            if (encode)
            {
                (Dialog.CurrentImageContainer as jzip.Container).SaveDialogData(file, dialogs1.dlg, (src, dst) =>
                {
                    string pwd_p = PasswordRequest();
                    if (pwd_p == null) return;

                    using (System.IO.Stream sfs = new FileStream(src, FileMode.Open, FileAccess.Read))
                    using (System.IO.Stream sp = new FileStream(dst, FileMode.Create, FileAccess.Write))
                    {
                        sp.Write(new byte[] { (byte)0x9e, (byte)0x3f }, 0, 2);
                        sp.Write(BitConverter.GetBytes(sfs.Length), 0, 4);

                        DialogMaker.SecurityStream sec = new DialogMaker.SecurityStream(sp, pwd_p);

                        sfs.CopyTo(sec);
                        sec.Close();

                        sp.Close();
                        sfs.Close();
                    }
                });
            }
            else
            {
                (Dialog.CurrentImageContainer as jzip.Container).SaveDialogData(file, dialogs1.dlg);
            }
            LastLoadedJson = file;
            LastLoadedEncoded = encode;

            dialogs1.Changed = false;
            this.Text = "DMPRO \"" + System.IO.Path.GetFileName(file) + "\"";
            OpenFileHistory.DHistory.Push(file);

        }
        //void SaveTo(string file, bool encode)
        //{
        //    dialogs1_SelectedChanging(null);
        //    dialogs1_SelectedChangingTL(null);

        //    string json = dialogs1.dlg.Json;
        //    byte[] result = Encoding.UTF8.GetBytes(json);

        //    System.IO.Stream sp = new FileStream(file, FileMode.Create, FileAccess.Write);

        //    if (encode)
        //    {
        //        sp.Write(new byte[] { (byte)0xae, (byte)0xfa }, 0, 2);
        //        sp.Write(BitConverter.GetBytes(result.Length), 0, 4);
        //        //System.IO.StreamWriter sw;

        //        string pwd_p = "";
        //        PWD_INPUT pwd_dlg = new PWD_INPUT();
        //        pwd_dlg.Icon = this.Icon;
        //        DialogResult dr = pwd_dlg.ShowDialog();
        //        if (dr != System.Windows.Forms.DialogResult.OK) return;

        //        pwd_p = pwd_dlg.pwd_ref;
        //        pwd_dlg.Dispose();

        //        YaDiskBackup.Mod.SecurityStream sec = new YaDiskBackup.Mod.SecurityStream(sp, pwd_p);

        //        sec.Write(result, 0, result.Length);
        //        sec.Close();
        //    }
        //    else
        //    {
        //        sp.Write(result, 0, result.Length);
        //        sp.Flush();
        //        sp.Close();
        //    }

        //    //System.IO.File.WriteAllText(file, json, Encoding.UTF8);
        //    LastLoadedJson = file;
        //    LastLoadedEncoded = encode;

        //    dialogs1.Changed = false;
        //    this.Text = "DMPRO \"" + System.IO.Path.GetFileName(file) + "\"";
        //    OpenFileHistory.DHistory.Push(file);
        //}
        bool SaveLastSavedLoaded()
        {
            if (string.IsNullOrWhiteSpace(LastLoadedJson) ||
                    !System.IO.File.Exists(LastLoadedJson))
            {
                MessageBox.Show("Sorry, cant find last saved/loaded file.");
                return false;
            }
            SaveTo(LastLoadedJson, LastLoadedEncoded);
            return true;
        }
        private void button14_Click(object sender, EventArgs e)
        {
            SaveLastSavedLoaded();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadOpen();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastLoadedJson))
            {
                SaveAs();
            }
            else
            {
                SaveLastSavedLoaded();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SavePng();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void addChildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialogs1.AddAsChild("");
        }

        private void linkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialogs1.ConnectTo();
        }

        private void separateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialogs1.SeparateWith();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialogs1.RemoveSelected();
        }

        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialogs1.AlignByWith(Dialogs.AlignOrientation.vertical);
        }

        private void horisontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialogs1.AlignByWith(Dialogs.AlignOrientation.horisontal);
        }

        private void newClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Event_LostData())
                return;

            dialogs1.Changed = false;
            this.LastLoadedJson = "";
            LastLoadedEncoded = false;

            dialogs1.Clear();

            this.Text = "DMPRO ";
            dialogs1.Refresh();

            GC.Collect();
            System.Threading.Thread.Sleep(100);
            GC.Collect();
            System.Threading.Thread.Sleep(100);
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            LoadOpenImage();
        }

        private void setImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadOpenImage();
        }

        private void removeImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialogs1.RemoveSelectedImage();
            panel1.BackgroundImage = null;
        }

        private void showOriginalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image img = dialogs1.GetSelectedImage();
            if (img == null)
                return;

            ClipView secondForm = new ClipView()
            {
                Text = " Graphic clip view",
                //BackgroundImage = dialogs1.GetSelectedImage(),
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                //BackgroundImageLayout = ImageLayout.Stretch,
                Clip = img
            };
            secondForm.ShowDialog();
            secondForm.Dispose();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            dialogs1.DrawClips = checkBox2.Checked;
            dialogs1.Refresh();
        }

        private void fromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Image img = Clipboard.GetImage();
                dialogs1.SetSelectedImage(img);
                panel1.BackgroundImage = dialogs1.dlg.Phrase(dialogs1.FocusedEditPhrase).GraphicClipThumbnail;
                dialogs1.Refresh();
            }
        }

        private void sVGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSvg();
        }

        private void stepBackwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox1.Focused)
            {
                textBox1.Undo();
                return;
            }
            if (textBox2.Focused)
            {
                textBox2.Undo();
                return;
            }
            bool ok = dialogs1.dlg.Undo();
            if (ok)
            {
                dialogs1.Changed = true;
                dialogs1.Refresh();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //while (true)
            //{
            //    System.Threading.Thread.Sleep(1);
            //}
        }

        private void copyTreeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string tree = dialogs1.dlg.CopyJsonTree(dialogs1.FocusedEditPhrase);

            Clipboard.SetText("ref$dlg1$phrases---" + tree, TextDataFormat.UnicodeText);
        }

        private void pasteTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = Clipboard.GetText();
            if (text.StartsWith("ref$dlg1$phrases---"))
            {
                dialogs1.dlg.ParseJsonTree(text.Substring("ref$dlg1$phrases---".Length));
                dialogs1.Refresh();
            }
        }

        private void stepForwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (textBox1.Focused)
            //{
            //    textBox1.Undo();
            //    return;
            //}
            //if (textBox2.Focused)
            //{
            //    textBox2.Undo();
            //    return;
            //}
            //dialogs1.dlg.Undo();
            //dialogs1.Refresh();
        }

        private void ToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image img = dialogs1.GetSelectedImage();
            if (img == null)
                return;
            Clipboard.SetImage(img);
        }

        private void ClearBacklogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialogs1.dlg.ClearHistory();
        }

        private void SaveSecurityKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fontDialog1.Font = DRes.useFont;
            try
            {
                if (fontDialog1.ShowDialog() == DialogResult.OK)
                {
                    dialogs1.dlg.SetFont(fontDialog1.Font);
                    this.Refresh();
                }
            }
            catch
            {
                // true type fonts supported only...
            }
        }
    }
}
