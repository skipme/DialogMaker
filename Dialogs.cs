using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SvgNet.SvgGdi;
using DialogMaker.operable;

namespace DialogMaker
{
    public partial class Dialogs : UserControl
    {
        public delegate void SelectEventHandler(Phrase p);
        public event SelectEventHandler SelectedChanged;
        public event SelectEventHandler SelectedChanging;

        public delegate void SelectEventHandlerTL(TimeLine p);
        public event SelectEventHandlerTL SelectedChangedTL;
        public event SelectEventHandlerTL SelectedChangingTL;

        public delegate void StateChanged(bool changed);
        public event StateChanged OnStateChanged;

        public Dialogs(jzip.Container imageContainer)
        {
            SetPhraseAttributesCount(4);
            InitializeComponent();
#if !PocketPC
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.Opaque, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
#endif
            this.imageContainer = imageContainer;
            dlginstance = new DBacklog(new Dialog(imageContainer));

        }
        public void SetPhraseAttributesCount(int phraseAttributesCount)
        {
            this.phraseAttributesCount = phraseAttributesCount;
        }
        private int phraseAttributesCount;
        public int phraseAttributesCountGet
        {
            get
            {
                return phraseAttributesCount;
            }
        }
        public int FocusedEditPhrase = -1;
        int FocusedPhrase = -1;
        int FocusedTimeline = -1;
        public int FocusedEditTimeline = -1;
        private int spottedIndex = -1;
        bool TranslateFocus = false;
        bool ChooseConstraint = false;
        bool ChooseConstraintLost = false;
        bool ChooseByAlign = false;

        public bool TreeOperationMode { get; set; }
        public bool DrawClips { get; set; }
        bool _changed;
        public bool Changed
        {
            get
            {
                return _changed;
            }
            set
            {
                _changed = value;
                if (OnStateChanged != null)
                    OnStateChanged.Invoke(value);
            }
        }
        AlignOrientation orientation;

        Point movestart;
        public jzip.Container imageContainer;
        DBacklog dlginstance;
        Point Translate = new Point();

        static Pen PenRed = new Pen(Color.Red, 2);
        static Pen PenGreen = new Pen(Color.Green, 2);

        public DBacklog dlg
        {
            get
            {
                return dlginstance;
            }
            set
            {
                FocusedEditPhrase = -1;
                FocusedPhrase = -1;
                FocusedTimeline = -1;
                FocusedEditTimeline = -1;
                TranslateFocus = false;
                ChooseConstraint = false;
                ChooseConstraintLost = false;
                ChooseByAlign = false;
                spottedIndex = -1;

                dlginstance = value;
            }
        }


        float zoom = 1.0f;
        public float ScaleZoom
        {
            get
            {
                return zoom;
            }
            set
            {
                zoom = value;
                this.Refresh();
            }
        }
        public string DrawDiagramSvg()
        {
            SvgNet.SvgGdi.SvgGraphics ig;

            ig = new SvgNet.SvgGdi.SvgGraphics();

            ig.Clear(Color.FromArgb(255, 255, 255, 255));
            ig.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            ig.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            dlginstance.Draw(ig, 1.0f, -1, -1,
                new Point(-(dlginstance.VisibleRect.Location.X - 1), -(dlginstance.VisibleRect.Location.Y - 1))
                , DrawClips, dlginstance.VisibleRect.Width);


            //string s = ig.WriteSVGString(dlginstance.VisibleRect.Width, dlginstance.VisibleRect.Height);
            string s = ig.WriteSVGString();
            int IncIdx = s.IndexOf("SvgGdi_output");
            if (IncIdx > 0)
            {
                string marker = "2000/svg\"";
                IncIdx = s.IndexOf(marker, IncIdx);
                if (IncIdx > 0)
                {
                    s = s.Insert(IncIdx + marker.Length,
                        string.Format(" width=\"{0}px\" height=\"{1}px\" ", dlginstance.VisibleRect.Width + 1, dlginstance.VisibleRect.Height + 1));
                }
            }


            return s;
        }
        public Image DrawDiagram()
        {
            //return DrawDiagramAlpha();
            Bitmap img = new Bitmap(dlginstance.VisibleRect.Size.Width - 1, dlginstance.VisibleRect.Size.Height - 1,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Graphics gr = Graphics.FromImage(img);
            gr.Clear(Color.FromArgb(255, 255, 255, 255));
            gr.Clip = new Region(new Rectangle(0, 0, img.Width, img.Height));
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            GdiGraphics gg = new GdiGraphics(gr);
            dlginstance.Draw(gg, 1.0f, -1, -1,
                new Point(-(dlginstance.VisibleRect.Location.X + 1), -(dlginstance.VisibleRect.Location.Y + 1))
                , DrawClips, dlginstance.VisibleRect.Width);

            return img;
        }
        public Image DrawDiagramAlpha()
        {
            Bitmap img = new Bitmap(dlginstance.VisibleRect.Size.Width - 1, dlginstance.VisibleRect.Size.Height - 1,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Graphics gr = Graphics.FromImage(img);
            gr.Clear(Color.FromArgb(0, 255, 255, 255));
            gr.Clip = new Region(new Rectangle(0, 0, img.Width, img.Height));
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            GdiGraphics gg = new GdiGraphics(gr);
            dlginstance.Draw(gg, 1.0f, -1, -1,
                new Point(-(dlginstance.VisibleRect.Location.X + 1), -(dlginstance.VisibleRect.Location.Y + 1))
                , DrawClips, dlginstance.VisibleRect.Width);

            return img;
        }
        public void SetSelectedColorspace(ColorSpace cs)
        {
            //if (FocusedEditPhrase == -1 || FocusedEditPhrase >= dlginstance.phrases.Count)
            //    return;
            //dlginstance.phrases[FocusedEditPhrase].color = cs;
            dlg.SetSelectedColorspace(FocusedEditPhrase, cs);
            this.Refresh();
            Changed = true;
        }
        public void SetSelectedAttrib(int AttribIndex, byte AttribValue)
        {
            dlg.SetSelectedAttrib(FocusedEditPhrase, AttribIndex, AttribValue);
            this.Refresh();
            Changed = true;
        }
        public void SetSelectedText(string text)
        {
            //if (FocusedEditPhrase == -1 || FocusedEditPhrase >= dlginstance.phrases.Count)
            //{
            //    if (FocusedEditTimeline == -1 || FocusedEditTimeline >= dlginstance.timelines.Count)
            //        return;
            //    dlginstance.timelines[FocusedEditTimeline].Text = text;
            //    this.Refresh();
            //    return;
            //}
            //dlginstance.phrases[FocusedEditPhrase].Text = text;
            dlg.SetSelectedText(FocusedEditTimeline, FocusedEditPhrase, text);
            this.Refresh();

            Changed = true;
        }
        public void SetSelectedImage(Image image)
        {
            //if (FocusedEditPhrase == -1 || FocusedEditPhrase >= dlginstance.phrases.Count)
            //{
            //    return;
            //}
            //dlginstance.phrases[FocusedEditPhrase].GraphicClip = image;
            dlg.SetSelectedImage(FocusedEditPhrase, image);
            this.Refresh();

            Changed = true;
        }

        public Image GetSelectedImage()
        {
            //if (FocusedEditPhrase == -1 || FocusedEditPhrase >= dlginstance.phrases.Count)
            //{
            //    return null;
            //}
            //return dlginstance.phrases[FocusedEditPhrase].GraphicClip;
            return dlg.GetSelectedImage(FocusedEditPhrase);
        }

        public void RemoveSelectedImage()
        {
            //if (FocusedEditPhrase == -1 || FocusedEditPhrase >= dlginstance.phrases.Count)
            //{
            //    return;
            //}
            //dlginstance.phrases[FocusedEditPhrase].GraphicClip = null;
            dlg.RemoveSelectedImage(FocusedEditPhrase);
            this.Refresh();

            Changed = true;
        }
        public void SetSelectedLabel(string text)
        {
            //if (FocusedEditPhrase == -1 || FocusedEditPhrase >= dlginstance.phrases.Count)
            //    return;
            //dlginstance.phrases[FocusedEditPhrase].Label = text;
            dlg.SetSelectedLabel(FocusedEditPhrase, text);
            this.Refresh();

            Changed = true;
        }
        DateTime lastprint = DateTime.Now;
        private void Dialogs_Paint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;
            gr.Clear(Color.White);
            //gr.Clear(Color.FromArgb(249, 249, 249));
            //gr.Clear(Color.FromArgb(247, 247, 247));
            //
            //gr.DrawString(string.Format("{0}ms.", (DateTime.Now - lastprint).TotalMilliseconds), SystemFonts.CaptionFont, Brushes.BlueViolet, 12, 44);
            lastprint = DateTime.Now;

            if (!this.DesignMode)
            {
                System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
                //m.Scale(ScaleZoom, ScaleZoom, System.Drawing.Drawing2D.MatrixOrder.Append);
                m.Translate(Translate.X, Translate.Y, System.Drawing.Drawing2D.MatrixOrder.Append);
                int xskippedL = 0, xskippedR = 0;
                int yskippedU = 0, yskippedD = 0;

                Point[] dz = new Point[] {
                    new Point(dlginstance.VisibleRect.Left,dlginstance.VisibleRect.Top),
                    new Point(dlginstance.VisibleRect.Right,dlginstance.VisibleRect.Bottom),
                };
                m.TransformPoints(dz);

                xskippedL = Math.Max(0, e.ClipRectangle.Left - dz[0].X);
                xskippedR = Math.Max(0, dz[1].X - e.ClipRectangle.Right);
                yskippedU = Math.Max(0, e.ClipRectangle.Top - dz[0].Y);
                yskippedD = Math.Max(0, dz[1].Y - e.ClipRectangle.Bottom);
                //gr.DrawString(string.Format("{0}x{1}|{2}x{3}", xskippedL, xskippedR, yskippedU, yskippedD),
                //    Font, Brushes.Black, 0, 0);
                //
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                //
                if (ChooseConstraint || ChooseConstraintLost || ChooseByAlign)
                    gr.DrawString("Choose Phrase [click on it]... )",
                        Font, Brushes.BlueViolet, 110, 0);

                //gr.DrawString(string.Format("Zoom: {0:.0}", ScaleZoom),
                //    Font, Brushes.Black, 0, 0);
                GdiGraphics gg = new GdiGraphics(gr);
                dlginstance.Draw(gg, ScaleZoom, FocusedEditPhrase, FocusedEditTimeline, Translate, DrawClips, (int)gr.ClipBounds.Right);

                if (spottedIndex >= 0)
                {
                    var spot = dlginstance.Phrase(spottedIndex);
                    spot.DrawSpot(gg);
                }
                //DrawTranslation Movement State
                gr.Transform = new System.Drawing.Drawing2D.Matrix();
                {
                    int xmiddle = (int)(this.Width * .5); //(int)(e.ClipRectangle.Right * .5);
                    int allskippedhor = xskippedL + xskippedR;
                    if (allskippedhor == 0) goto skipwide;
                    double scalehor = 100.0 / (double)allskippedhor;
                    double scalehorm = (double)xmiddle / 100.0;

                    double prcl = scalehor * (double)xskippedL;
                    prcl = prcl * scalehorm;

                    double prcr = scalehor * (double)xskippedR;
                    prcr = prcr * scalehorm;

                    gr.DrawLine(PenGreen, new Point((int)(xmiddle - prcl), this.Height - 4), new Point(xmiddle, this.Height - 4));
                    gr.DrawLine(PenRed, new Point(xmiddle, this.Height - 4), new Point((int)(xmiddle + prcr), this.Height - 4));
                }
            skipwide:
                {
                    int ymiddle = (int)(this.Height * .5);// (int)(e.ClipRectangle.Bottom * .5);
                    int allskippedhor = yskippedU + yskippedD;
                    if (allskippedhor == 0) return;
                    double scalehor = 100.0 / (double)allskippedhor;
                    double scalehorm = (double)ymiddle / 100.0;

                    double prcu = scalehor * (double)yskippedU;
                    prcu = prcu * scalehorm;

                    double prcd = scalehor * (double)yskippedD;
                    prcd = prcd * scalehorm;

                    //gr.DrawLine(Pens.Green, new Point(e.ClipRectangle.Right - 1, (int)(ymiddle - prcu)), new Point(e.ClipRectangle.Right - 1, ymiddle));
                    //gr.DrawLine(Pens.Red, new Point(e.ClipRectangle.Right - 1, ymiddle), new Point(e.ClipRectangle.Right - 1, (int)(ymiddle + prcd)));
                    gr.DrawLine(PenGreen, new Point(this.Width - 4, (int)(ymiddle - prcu)), new Point(this.Width - 4, ymiddle));
                    gr.DrawLine(PenRed, new Point(this.Width - 4, ymiddle), new Point(this.Width - 4, (int)(ymiddle + prcd)));

                }
            }
        }
        private void Dialogs_MouseDown(object sender, MouseEventArgs e)
        {
            if (!ChooseConstraint && !ChooseConstraintLost && !ChooseByAlign && SelectedChanged != null)
            {
                int phindexx = dlginstance.GetSelectedPhrase(e.Location, ScaleZoom, Translate);
                if (phindexx == -1)
                {
                    phindexx = dlginstance.GetSelectedTimeLine(e.Location, ScaleZoom, Translate);
                    if (phindexx > -1)
                    {
                        if (FocusedEditPhrase != -1)
                            SelectedChanging(dlginstance.Phrase(FocusedEditPhrase));
                        if (FocusedEditTimeline != -1)
                            SelectedChangingTL(dlginstance.Timeline(FocusedEditTimeline)); FocusedEditTimeline = phindexx;

                        FocusedEditPhrase = -1;
                        FocusedEditTimeline = phindexx;
                        this.Refresh();
                        TimeLine t = dlginstance.Timeline(phindexx);
                        SelectedChangedTL.Invoke(t);

                        //return;
                        goto next;
                    }
                    //else
                    //TranslateFocus = true;
                    //movestart = e.Location;
                    //return;
                }
                else
                {
                    Phrase p = dlginstance.Phrase(phindexx);
                    if (FocusedEditPhrase != -1)
                        SelectedChanging(dlginstance.Phrase(FocusedEditPhrase));
                    if (FocusedEditTimeline != -1)
                        SelectedChangingTL(dlginstance.Timeline(FocusedEditTimeline));
                    if (FocusedEditPhrase != phindexx)
                    {
                        FocusedEditPhrase = phindexx;
                        FocusedEditTimeline = -1;
                        this.Refresh();
                        SelectedChanged.Invoke(p);
                    }
                }
            }
        next:
            int phindex = dlginstance.GetSelectedPhrase(e.Location, ScaleZoom, Translate);

            if (phindex == -1)
            {
                if (ChooseConstraint || ChooseConstraintLost || ChooseByAlign)
                {
                    if (Program.HitTest(e.X, e.Y, 0, 0, this.Width, 40))
                    { ChooseConstraint = ChooseConstraintLost = ChooseByAlign = false; }
                    this.Refresh();
                    return;
                }
                phindex = dlginstance.GetSelectedTimeLine(e.Location, ScaleZoom, Translate);

                if (phindex == -1)
                {
                    TranslateFocus = true;
                    movestart = e.Location;
                    return;
                }

                FocusedTimeline = phindex;
                movestart = e.Location;
                return;
            }
            if (ChooseConstraintLost)
            {
                if (dlginstance.Phrase(FocusedEditPhrase).PhraseConnectReferences.Contains(phindex))
                {
                    dlginstance.RemoveConnection(FocusedEditPhrase, phindex);
                    ChooseConstraintLost = false;
                    this.Refresh();
                    Changed = true;
                }
                else if (dlginstance.Phrase(phindex).PhraseConnectReferences.Contains(FocusedEditPhrase))
                {
                    dlginstance.RemoveConnection(phindex, FocusedEditPhrase);
                    ChooseConstraintLost = false;
                    this.Refresh();
                    Changed = true;
                }
                return;
            }
            if (ChooseConstraint)
            {
                if (phindex != FocusedEditPhrase &&
                    !dlginstance.Phrase(FocusedEditPhrase).PhraseConnectReferences.Contains(phindex) &&
                    !dlginstance.Phrase(phindex).PhraseConnectReferences.Contains(FocusedEditPhrase))
                {
                    //dlginstance.Phrase(FocusedEditPhrase).PhraseConnectReferences.Add(phindex);
                    dlginstance.Connect(FocusedEditPhrase, phindex);
                    ChooseConstraint = false;
                    this.Refresh();
                    Changed = true;
                }
                return;
            }
            if (ChooseByAlign)
            {
                if (phindex != FocusedEditPhrase)
                {
                    //dlginstance.phrases[FocusedEditPhrase].PhraseConnectReferences.Add(phindex);
                    Point newpos = dlginstance.Phrase(FocusedEditPhrase).position;
                    switch (orientation)
                    {
                        case AlignOrientation.horisontal:
                            newpos.Y = dlginstance.Phrase(phindex).position.Y;
                            dlginstance.Phrase(FocusedEditPhrase).position = newpos;
                            break;
                        case AlignOrientation.vertical:
                            newpos.X = dlginstance.Phrase(phindex).position.X;
                            dlginstance.Phrase(FocusedEditPhrase).position = newpos;
                            break;
                    }
                    ChooseByAlign = false;
                    this.Refresh();
                    Changed = true;
                }
                return;
            }
            FocusedPhrase = phindex;
            movestart = e.Location;
        }
        private void Dialogs_MouseMove(object sender, MouseEventArgs e)
        {
            if (movestart == null) return;

            if (FocusedPhrase == -1 && FocusedTimeline == -1 && !TranslateFocus)
                return;

            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
            //m.Translate(Translate.X, Translate.Y, System.Drawing.Drawing2D.MatrixOrder.Append);
            m.Scale(ScaleZoom, ScaleZoom, System.Drawing.Drawing2D.MatrixOrder.Append);
            m.Invert();

            Point movement = new Point(movestart.X - e.X, movestart.Y - e.Y);
            movement.X = -movement.X;
            movement.Y = -movement.Y;
            Point[] t = new Point[] { movement };
            m.TransformPoints(t);
            movement = t[0];

            if (FocusedPhrase != -1)
            {
                if (TreeOperationMode)
                    dlg.MovePhraseTree(FocusedPhrase, movement);
                else dlg.MovePhrase(FocusedPhrase, movement);

                Changed = true;
            }
            else if (FocusedTimeline >= 0)
            {
                Point x = new Point(0, dlginstance.Timeline(FocusedTimeline).Ylocation);
                x.Offset(movement);
                dlginstance.Timeline(FocusedTimeline).Ylocation = x.Y;

                Changed = true;
            }
            else
            {
                Translate.X -= movestart.X - e.X;
                Translate.Y -= movestart.Y - e.Y;
                //Translate.Offset(movement);
            }

            movestart = e.Location;
            this.Refresh();
        }

        private void Dialogs_MouseUp(object sender, MouseEventArgs e)
        {
            TranslateFocus = false;
            FocusedTimeline = -1;
            FocusedPhrase = -1;
        }
        public void RemoveSelected()
        {
            if (dlginstance.Phrase(FocusedEditPhrase) == null)
            {
                if (dlginstance.Timeline(FocusedEditTimeline) == null)
                    return;
                //
                dlginstance.RemoveTimeline(FocusedEditTimeline);

                this.Refresh();
                Changed = true;
                FocusedEditTimeline = -1;

                return;
            }

            dlginstance.RemovePhrase(FocusedEditPhrase);

            this.Refresh();

            Changed = true;
            FocusedEditPhrase = -1;
            SelectedChanged.Invoke(null);
        }
        public void Clear()
        {
            FocusedEditPhrase = -1;
            FocusedPhrase = -1;
            FocusedTimeline = -1;
            FocusedEditTimeline = -1;
            dlg.Clear();
            dlg.ClearHistory();

            if (imageContainer != null)
            {
                imageContainer.Dispose();
                File.Delete(imageContainer.FileName);
            }
            imageContainer = new jzip.Container();

            dlg = new DBacklog(new Dialog(imageContainer));

            SelectedChanged.Invoke(null);
        }
        public void AddAsChild(string _Label)
        {
            if (dlginstance.Phrase(0) == null)
            {
                dlginstance.AddPhrase(new Phrase(phraseAttributesCount, this.imageContainer)
                {
                    Text = "",
                    Label = _Label,
                    color = Dialog.XColors[0],
                    position =
                        new Point(this.Size.Width / 2, this.Size.Width / 2)
                },
                FocusedEditPhrase);

                this.Refresh();
                Changed = true;

                return;
            }
            if (dlginstance.Phrase(FocusedEditPhrase) == null)
                return;

            dlginstance.AddPhrase(new Phrase(phraseAttributesCount, this.imageContainer)
            {
                Text = "",
                Label = _Label,
                color = Dialog.XColors[0],
                position =
                    new Point(dlginstance.Phrase(FocusedEditPhrase).position.X, dlginstance.Phrase(FocusedEditPhrase).position.Y - 40)
            },
                FocusedEditPhrase);

            this.Refresh();

            Changed = true;
        }
        public void AddTL(string name)
        {
            dlginstance.AddTimeline(new TimeLine() { Text = name, Ylocation = this.Height / 2 });
            this.Refresh();

            Changed = true;
        }
        public void ConnectTo()
        {
            if (dlginstance.Phrase(FocusedEditPhrase) == null)
                return;
            ChooseConstraint = true;
            this.Refresh();
        }
        public enum AlignOrientation
        {
            none,
            horisontal,
            vertical
        }
        public void AlignByWith(AlignOrientation orientation)
        {
            if (dlginstance.Phrase(FocusedEditPhrase) == null)
                return;

            ChooseByAlign = true;
            this.orientation = orientation;
            this.Refresh();
        }
        public void SeparateWith()
        {
            Phrase p = dlginstance.Phrase(FocusedEditPhrase);
            if (p == null)
                return;

            List<int> pi = dlginstance.GetConnectedFrom(FocusedEditPhrase);

            if (p.PhraseConnectReferences.Count == 1 && pi.Count == 0)
            {
                //p.PhraseConnectReferences.Clear();
                dlginstance.RemoveConnection(FocusedEditPhrase, p.PhraseConnectReferences[0]);
            }
            else if (pi.Count == 1 && p.PhraseConnectReferences.Count == 0)
            {
                //dlginstance.Phrase(pi[0]).RemoveConnections(FocusedEditPhrase);
                dlginstance.RemoveConnection(pi[0], FocusedEditPhrase);
            }
            else
                ChooseConstraintLost = true;

            this.Refresh();
        }
        public void Clone()
        {
            if (dlginstance.Phrase(FocusedEditPhrase) == null)
                return;
            dlg.ClonePhrase(FocusedEditPhrase);
            this.Refresh();

            Changed = true;
        }
        public void CloneTree()
        {
            if (dlginstance.Phrase(FocusedEditPhrase) == null)
                return;
            dlg.ClonePhraseTree(FocusedEditPhrase);
            this.Refresh();

            Changed = true;
        }
        public void Spot(int index, Phrase phrase, bool translateTo = true)
        {
            this.spottedIndex = index;
            if (translateTo)
            {
                this.Translate = new Point((int)(-phrase.location.X + this.Size.Width * 0.5), (int)(-phrase.location.Y + this.Size.Height * 0.5));
            }

            this.Refresh();
        }
        public void SpotSelected(bool translateTo = true)
        {
            if (FocusedEditPhrase >= 0)
            {
                Spot(FocusedEditPhrase, dlginstance.Phrase(FocusedEditPhrase), translateTo);
            }
        }
        public void UnSpot()
        {
            this.spottedIndex = -1;

            this.Refresh();
        }
    }
}
