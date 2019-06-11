using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SvgNet.SvgGdi;

namespace DialogMaker
{
    public class DRes
    {
        public static Font useFont = new Font("Consolas", 11, FontStyle.Regular);
        //public static Font useFont = new Font("Tahoma", 12, FontStyle.Regular);
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class ColorSpace
    {
        [JsonProperty]
        public Color[] Colors;
        public string Name;
    }
    [JsonObject(MemberSerialization.OptIn)]
    class EditorDescription
    {
        [JsonProperty]
        public string Editor { get { return "Prospero Tool, 2011-2012"; } set { } }
        [JsonProperty]
        public string Homepage { get { return "github.com/skipme"; } set { } }
        [JsonProperty]
        public string CreatedBy { get { return "Vitaliy Burdenkov aka Cerriun"; } set { } }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class Dialog : IDialog
    {
        EditorDescription xPoweredBy;
        [JsonProperty]
        EditorDescription PoweredBy { get { return xPoweredBy; } }

        public string Json
        {
            get
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.Indented);
            }

        }
        operable.IImageContainer imageContainer;
        public Dialog(operable.IImageContainer imageContainer)
        {
            CurrentImageContainer = this.imageContainer = imageContainer;
            xPoweredBy = new EditorDescription();
        }
        public static operable.IImageContainer CurrentImageContainer;
        public static int CurrentTagCount = 4;

        [JsonProperty]
        public List<Phrase> phrases = new List<Phrase>();
        [JsonProperty]
        public List<TimeLine> timelines = new List<TimeLine>();

        public Point[] VisibleRange = new Point[2];
        public Rectangle VisibleRect
        {
            get
            {
                return new Rectangle(VisibleRange[0].X, VisibleRange[0].Y,
                    VisibleRange[1].X - VisibleRange[0].X,
                    VisibleRange[1].Y - VisibleRange[0].Y);
            }
        }

        public static ColorSpace[] XColors = new ColorSpace[]{
            new ColorSpace(){Colors=new Color[]{Color.FromArgb(8, 25, 53), Color.FromArgb(255, 255, 255)}, Name = "highContrast"},
            new ColorSpace(){Colors=new Color[]{Color.FromArgb(96,124,175), Color.FromArgb(191,241,252)}, Name = "newwave"},
            new ColorSpace(){Colors=new Color[]{Color.FromArgb(71,36,148), Color.FromArgb(249,249,249)}, Name = "BluePrintX1"},
            new ColorSpace(){Colors=new Color[]{Color.FromArgb(32,96,160), Color.FromArgb(249,249,249)}, Name = "BluePrintX2"},
            new ColorSpace(){Colors=new Color[]{Color.FromArgb(209,18,77), Color.FromArgb(249,249,249)}, Name = "BluePrintP"},
            new ColorSpace(){Colors=new Color[]{Color.FromArgb(7,55,99), Color.FromArgb(207,226,243)}, Name = "B"},
            new ColorSpace(){Colors=new Color[]{Color.FromArgb(56,118,29), Color.FromArgb(182,215,168)}, Name = "G"},
            new ColorSpace(){Colors=new Color[]{Color.FromArgb(178,118,29), Color.FromArgb(255,215,168)}, Name = "Y"},
            new ColorSpace(){Colors=new Color[]{Color.FromArgb(204,0,0), Color.FromArgb(233,152,152)}, Name = "R"},
            new ColorSpace(){Colors=new Color[]{Color.FromArgb(31,18,77), Color.FromArgb(217,210,233)}, Name = "M"},
            //new ColorSpace(){Colors=new Color[]{Color.FromArgb(209,18,77), Color.FromArgb(255,210,233)}, Name = "P"},
        };

        public int AddPhrase(Phrase p)
        {
            phrases.Add(p);
            return phrases.Count - 1;
        }
        public int AddPhrase(Phrase p, int connection)
        {
            p.color = XColors[0];
            if (connection == -1)
            {
                if (connection == -1 && phrases.Count == 0)
                    phrases.Add(p);
                return phrases.Count - 1;
            }
            p.color = phrases[connection].color;
            return phrases[connection].AddConnectedPrase(p, this);
        }
        public void AddTimeline(TimeLine tl)
        {
            timelines.Add(tl);
        }
        public void Draw(IGraphics gr, float scale, int selectedEdit, int selectedEditTL, Point Translate, bool Clips, int width)
        {
            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
            m.Translate(Translate.X, Translate.Y);
            m.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
            gr.Transform = m;

            for (int i = 0; i < timelines.Count; i++)
            {
                timelines[i].Draw(gr, -Translate.X, -Translate.X + width);
                if (selectedEditTL == i)
                {
                    gr.FillEllipse(Brushes.Red, new Rectangle((int)-Translate.X + 5, timelines[i].Ylocation - 5, 4, 4));
                }
            }

            int minx = int.MaxValue, miny = int.MaxValue, maxx = int.MinValue, maxy = int.MinValue;
            int maxtext = 0;

            // connections
            for (int i = 0; i < phrases.Count; i++)
            {
                phrases[i].DrawConnections(gr, this);
                // clips
                phrases[i].DrawGClip(gr, Clips);
            }
            // ovals
            for (int i = 0; i < phrases.Count; i++)
            {
                phrases[i].DrawOval(gr);
            }
            for (int i = 0; i < phrases.Count; i++)
            {
                maxtext = phrases[i].MeasuredLabel == SizeF.Empty ? (int)gr.MeasureString(phrases[i].Label, DRes.useFont).Width : (int)phrases[i].MeasuredLabel.Width;
                minx = Math.Min(minx, phrases[i].locationC.X - maxtext);
                miny = Math.Min(miny, phrases[i].locationC.Y);
                SizeF tm = phrases[i].MeasuredText == SizeF.Empty ? gr.MeasureString(phrases[i].ShortText, DRes.useFont) : phrases[i].MeasuredText;
                maxtext = (int)tm.Width; //phrases[i].MeasuredText = tm;

                maxx = Math.Max(maxx, phrases[i].locationC.X + maxtext);
                maxy = Math.Max(maxy, phrases[i].locationC.Y);

                if (Clips && phrases[i].GraphicClipThumbnail != null)
                {
                    Rectangle grrect = phrases[i].GraphicClipRectangle;
                    maxx = Math.Max(maxx, grrect.Right);
                    maxy = Math.Max(maxy, grrect.Bottom);

                    minx = Math.Min(minx, grrect.Left);
                    miny = Math.Min(miny, grrect.Top);
                }
                //phrases[i].DrawConnections(gr, this);
                phrases[i].Draw(gr);

                if (selectedEdit == i)
                {
                    gr.FillEllipse(Brushes.Red, new Rectangle(phrases[i].locationC.X - 2, phrases[i].locationC.Y - 2, 4, 4));
                }
            }
            if (phrases.Count != 0)
            {
                VisibleRange[0].X = minx - 12; VisibleRange[0].Y = miny - 12;
                VisibleRange[1].X = maxx + 12; VisibleRange[1].Y = maxy + 16;
                gr.DrawRectangle(Pens.Aqua, VisibleRect);
            }
        }
        public int GetSelectedPhrase(Point location, float scale, Point Translate)
        {
            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
            //m.Translate(Translate.X, Translate.Y, MatrixOrder.Append);
            m.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);

            System.Drawing.Drawing2D.Matrix mt = new System.Drawing.Drawing2D.Matrix();
            mt.Translate(Translate.X, Translate.Y, MatrixOrder.Append);
            mt.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);

            //e.IGraphics.Transform = m;
            Point[] scaledps = new Point[] { new Point(28, 28) };
            for (int i = phrases.Count - 1; i >= 0; i--)
            {
                Point[] scaledp = new Point[] { new Point(phrases[i].position.X - 4, phrases[i].position.Y - 4) };

                //scaledp
                mt.TransformPoints(scaledp);
                m.TransformPoints(scaledps);
                if (Program.HitTest(location.X, location.Y,
                   scaledp[0].X, scaledp[0].Y, scaledps[0].X, scaledps[0].Y))
                {
                    return i;
                }
            }
            return -1;
        }
        public List<int> RemovePhrase(int Index)
        {
            List<int> connected = new List<int>();
            for (int i = 0; i < phrases.Count; i++)
            {
                if (phrases[i].PhraseConnectReferences.Contains(Index))
                {
                    phrases[i].PhraseConnectReferences.Remove(Index);
                    connected.Add(i);
                }

                for (int j = 0; j < phrases[i].PhraseConnectReferences.Count; j++)
                {
                    if (phrases[i].PhraseConnectReferences[j] > Index)
                        phrases[i].PhraseConnectReferences[j]--;
                }
            }
            phrases.RemoveAt(Index);

            return connected;
        }
        public void ClonePhrase(int Index)
        {
            Phrase pc = phrases[Index].Clone() as Phrase;
            foreach (Phrase prc in phrases)
            {
                if (prc.PhraseConnectReferences.Contains(Index))
                    prc.PhraseConnectReferences.Add(phrases.Count);
            }
            AddPhrase(pc);
        }
        public void RemoveConnection(int phraseIndex, int referencePhrase)
        {
            //if(phrases[phraseIndex].PhraseConnectReferences.Contains(referencePhrase))
            phrases[phraseIndex].PhraseConnectReferences.Remove(referencePhrase);
        }
        public void Connect(int phraseIndex, int referencePhrase)
        {
            phrases[phraseIndex].PhraseConnectReferences.Add(referencePhrase);
        }
        public class StoredPhraseContext
        {
            public int A_Index;
            public int A_i;
            public int A_IndexOfParentClone;
        }
        public void ClonePhraseTree(int Index)
        {
            int baseline = phrases.Count - 1;
            List<int> processed = new List<int>();
            Dictionary<int, int> cloned = new Dictionary<int, int>();
            Phrase pc = phrases[Index].Clone() as Phrase;
            //Connect clone to origin parents
            foreach (Phrase prc in phrases)
            {
                if (prc.PhraseConnectReferences.Contains(Index))
                    prc.PhraseConnectReferences.Add(phrases.Count);
            }
            AddPhrase(pc);

            Stack<StoredPhraseContext> procHelper = new Stack<StoredPhraseContext>();
            procHelper.Push(new StoredPhraseContext() { A_i = 0, A_Index = Index, A_IndexOfParentClone = phrases.Count - 1 });
            __procin:
            while (procHelper.Count > 0)
            {
                StoredPhraseContext sph = procHelper.Pop();
                Index = sph.A_Index;
                int i = sph.A_i;
                //if childs is absent we returns to last context, otherwise we go deeper
                //if (!processed.Contains(Index))
                if (!ProcHContains(Index, procHelper))
                {
                    while (i < phrases[Index].PhraseConnectReferences.Count)
                    {
                        int ci = phrases[Index].PhraseConnectReferences[i];
                        //if (cloned.ContainsValue(ci))
                        if (ci >= baseline)
                        { i++; continue; }
                        if (!cloned.ContainsKey(ci))
                        {
                            Phrase phraseClone = phrases[ci].Clone() as Phrase;
                            phrases[sph.A_IndexOfParentClone].PhraseConnectReferences[i] = phrases.Count;
                            phrases.Add(phraseClone);
                            cloned.Add(ci, phrases.Count - 1);
                        }
                        else
                        {
                            phrases[sph.A_IndexOfParentClone].PhraseConnectReferences[i] = cloned[ci];
                            i++; continue;
                        }
                        //current
                        procHelper.Push(new StoredPhraseContext() { A_i = i + 1, A_Index = Index, A_IndexOfParentClone = sph.A_IndexOfParentClone });
                        //next
                        procHelper.Push(new StoredPhraseContext() { A_i = 0, A_Index = ci, A_IndexOfParentClone = phrases.Count - 1 });

                        //next statement
                        goto __procin;
                    }
                    //processed.Add(Index);
                }
            }
            cloned = null;
        }
        public void MovePhrase(int Index, Point offset)
        {
            // move root
            Point p = phrases[Index].position; p.Offset(offset);
            phrases[Index].position = p;

            phrases[Index].GraphicClipRectangle = Rectangle.Empty;
        }
        public void MovePhraseTree(int Index, Point offset)
        {
            // move root
            Point p;
            List<int> moved = new List<int>();
            p = phrases[Index].position; p.Offset(offset);
            phrases[Index].position = p;
            phrases[Index].GraphicClipRectangle = Rectangle.Empty;
            moved.Add(Index);

            Stack<StoredPhraseContext> procHelper = new Stack<StoredPhraseContext>();
            procHelper.Push(new StoredPhraseContext() { A_i = 0, A_Index = Index, A_IndexOfParentClone = phrases.Count - 1 });

            __procin:
            while (procHelper.Count > 0)
            {
                StoredPhraseContext sph = procHelper.Pop();
                Index = sph.A_Index;
                int i = sph.A_i;
                //if childs is absent we returns to last context, otherwise we go deeper
                while (i < phrases[Index].PhraseConnectReferences.Count)
                {
                    int ci = phrases[Index].PhraseConnectReferences[i];
                    if (!moved.Contains(ci))
                    //if (!ProcHContatains(Index, procHelper))
                    {
                        p = phrases[ci].position; p.Offset(offset);
                        phrases[ci].position = p;
                        moved.Add(ci);

                        phrases[ci].GraphicClipRectangle = Rectangle.Empty;

                        //current
                        procHelper.Push(new StoredPhraseContext() { A_i = i + 1, A_Index = Index, A_IndexOfParentClone = sph.A_IndexOfParentClone });
                        //next
                        procHelper.Push(new StoredPhraseContext() { A_i = 0, A_Index = ci, A_IndexOfParentClone = phrases.Count - 1 });
                    }
                    //next statement
                    goto __procin;
                }
            }
            moved = null;
        }
        static bool ProcHContains(int Index, Stack<StoredPhraseContext> cntxt)
        {
            var g = from z in cntxt
                    where z.A_Index == Index
                    select z;
            foreach (var item in g)
            {
                return true;
            }
            return false;
        }
        public void RemoveTimeline(int Index)
        {
            timelines.RemoveAt(Index);
        }
        public int GetSelectedTimeLine(Point location, float scale, Point Translate)
        {
            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
            m.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
            //e.IGraphics.Transform = m;
            System.Drawing.Drawing2D.Matrix mt = new System.Drawing.Drawing2D.Matrix();
            mt.Translate(Translate.X, Translate.Y, MatrixOrder.Append);
            mt.Scale(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);

            for (int i = 0; i < timelines.Count; i++)
            {
                Point[] scaledp = new Point[] { new Point(timelines[i]._dragtag.X - 4, timelines[i]._dragtag.Y - 4) };
                Point[] scaledps = new Point[] { new Point(19, 19) };
                //scaledp
                mt.TransformPoints(scaledp);
                m.TransformPoints(scaledps);
                if (Program.HitTest(location.X, location.Y,
                   scaledp[0].X, scaledp[0].Y, scaledps[0].X, scaledps[0].Y))
                {
                    return i;
                }
            }
            return -1;
        }


        public void Clear()
        {
            phrases.Clear();
            timelines.Clear();
        }

        public void SetSelectedColorspace(int FocusedEditPhrase, ColorSpace cs)
        {
            if (FocusedEditPhrase == -1 || FocusedEditPhrase >= phrases.Count)
                return;
            phrases[FocusedEditPhrase].color = cs;
        }
        public void SetSelectedAttrib(int FocusedEditPhrase, int attribIndex, byte val)
        {
            if (FocusedEditPhrase == -1 || FocusedEditPhrase >= phrases.Count)
                return;
            phrases[FocusedEditPhrase]._attributes[attribIndex] = val;
        }
        public void SetSelectedText(int FocusedEditTimeline, int FocusedEditPhrase, string text)
        {
            if (FocusedEditPhrase == -1 || FocusedEditPhrase >= phrases.Count)
            {
                if (FocusedEditTimeline == -1 || FocusedEditTimeline >= timelines.Count)
                    return;
                timelines[FocusedEditTimeline].Text = text;
                return;
            }
            phrases[FocusedEditPhrase].Text = text;
        }
        public void SetSelectedImage(int FocusedEditPhrase, Image image)
        {
            if (FocusedEditPhrase == -1 || FocusedEditPhrase >= phrases.Count)
            {
                return;
            }
            if (this.imageContainer == null)
                return;

            phrases[FocusedEditPhrase].ImageRecordName = this.imageContainer.UpdateImageData(phrases[FocusedEditPhrase].ImageRecordName, image);
            phrases[FocusedEditPhrase].ImageHeight = image.Height;
            phrases[FocusedEditPhrase].ImageWidth = image.Width;
            phrases[FocusedEditPhrase].GraphicClipRectangle = Rectangle.Empty;
            phrases[FocusedEditPhrase]._GraphicClipThumbnail = null;
            //phrases[FocusedEditPhrase].GraphicClip = image;
        }

        public Image GetSelectedImage(int FocusedEditPhrase)
        {
            if (FocusedEditPhrase == -1 || FocusedEditPhrase >= phrases.Count)
            {
                return null;
            }
            if (this.imageContainer == null || string.IsNullOrWhiteSpace(phrases[FocusedEditPhrase].ImageRecordName))
                return null;

            return this.imageContainer.GetImageData(phrases[FocusedEditPhrase].ImageRecordName);
            //return phrases[FocusedEditPhrase].GraphicClip;
        }

        public void RemoveSelectedImage(int FocusedEditPhrase)
        {
            if (FocusedEditPhrase == -1 || FocusedEditPhrase >= phrases.Count)
            {
                return;
            }

            //phrases[FocusedEditPhrase].GraphicClip.Dispose();
            phrases[FocusedEditPhrase]._GraphicClipThumbnail.Dispose();
            //phrases[FocusedEditPhrase].GraphicClip = null;
            phrases[FocusedEditPhrase]._GraphicClipThumbnail = null;

            GC.Collect();
            System.Threading.Thread.Sleep(100);
            GC.Collect();
            System.Threading.Thread.Sleep(100);
        }
        public void SetSelectedLabel(int FocusedEditPhrase, string text)
        {
            if (FocusedEditPhrase == -1 || FocusedEditPhrase >= phrases.Count)
                return;
            phrases[FocusedEditPhrase].Label = text;
        }


        public Phrase Phrase(int Index)
        {
            if (Index < 0 || Index >= phrases.Count)
                return null;

            return phrases[Index];
        }

        public TimeLine Timeline(int Index)
        {
            if (Index < 0 || Index >= timelines.Count)
                return null;

            return timelines[Index];
        }


        public void DoNothing()
        {
            throw new NotImplementedException();
        }


        public List<int> GetConnectedFrom(int phrase)
        {
            List<int> pi = new List<int>();

            for (int i = 0; i < phrases.Count; i++)
            {
                Phrase ph = phrases[i];
                if (ph.PhraseConnectReferences.Contains(phrase))
                    pi.Add(i);
            }
            return pi;
        }
        public List<int> GetConnectedTo(int phrase)
        {
            return phrases[phrase].PhraseConnectReferences;
        }
        public string CopyJsonTree(int index)
        {
            SortedSet<int> parsed = new SortedSet<int>();
            Phrase pc = phrases[index];
            parsed.Add(index);
            Queue<Phrase> queue = new Queue<Phrase>();
            queue.Enqueue(pc);

            List<Phrase> export = new List<Phrase>();

            while (queue.Count > 0)
            {
                pc = (Phrase)queue.Dequeue().Clone();
                for (int i = 0; i < pc.PhraseConnectReferences.Count; i++)
                {
                    int realIndex = pc.PhraseConnectReferences[i];
                    if (!parsed.Contains(realIndex))
                    {
                        parsed.Add(realIndex);

                        pc.PhraseConnectReferences[i] = queue.Count + 1 + export.Count;
                        queue.Enqueue(phrases[realIndex]);
                    }
                }
                export.Add(pc);
            }

            return JsonConvert.SerializeObject(export);
        }

        public void ParseJsonTree(string json)
        {
            List<Phrase> import = JsonConvert.DeserializeObject<List<Phrase>>(json);
            int baseIndex = phrases.Count;

            for (int i = 0; i < import.Count; i++)
            {
                Phrase pc = import[i];
                for (int j = 0; j < pc.PhraseConnectReferences.Count; j++)
                {
                    pc.PhraseConnectReferences[j] += baseIndex;
                }
                phrases.Add(pc);
            }

        }



        public List<int> GetPhrasesWithAttribute(int attributeIndex, byte attributeValue)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < phrases.Count; i++)
            {
                if (phrases[i]._attributes[attributeIndex] == attributeValue)
                    result.Add(i);
            }
            return result;
        }

        public void SetFont(Font fontRef)
        {
            DRes.useFont = fontRef;

            for (int i = 0; i < phrases.Count; i++)
            {
                phrases[i].MeasuredText = phrases[i].MeasuredLabel = SizeF.Empty;
            }
        }
    }


}
