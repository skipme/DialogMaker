// -----------------------------------------------------------------------
// <copyright file="oPhrase.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DialogMaker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using System.Drawing;
    using System.IO;
    using SvgNet.SvgGdi;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Phrase : ICloneable, IDisposable
    {
        public Phrase(int attributesCount, operable.IImageContainer imageSrc)
        {
            //colorpen = new Pen(Color.Black, 2);
            //colorbrush = new SolidBrush(Color.Gray);
            this._attributes = new byte[attributesCount];
            ImageSource = imageSrc;
        }
        public Phrase() : this(Dialog.CurrentTagCount, Dialog.CurrentImageContainer)
        {

        }
        //public static IList<T> CloneList<T>(this IList<T> listToClone) where T : ICloneable
        //{
        //    return listToClone.Select(item => (T)item.Clone()).ToList();
        //}

        public object Clone()
        {
            Phrase p = new Phrase(this._attributes.Length, this.ImageSource)
            {
                Text = this.Text,
                color = this.color,
                Label = this.Label,
                PhraseConnectReferences = this.PhraseConnectReferences.ConvertAll<int>(delegate (int i) { return i; }),
                position = new Point(this.position.X + 32, this.position.Y)
            };
            Array.Copy(this._attributes, p._attributes, this._attributes.Length);
            return p;
        }

        public bool Updated;
        const int MaxTextLength = 15;

        public byte[] _attributes;
        [JsonProperty]
        public byte[] Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                _attributes = value;
            }
        }
        public bool PaintSpecialMark1;

        string _text;
        [JsonProperty]
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                MeasuredText = SizeF.Empty;
                _text = value;

                if (_text.Length < MaxTextLength)
                    ShortText = _text.Replace('\n', ' ');
                else ShortText = Text
                        .Remove(MaxTextLength - 3)
                        .Replace('\n', ' ') + "...";
            }
        }
        string _label;
        [JsonProperty]
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                MeasuredLabel = SizeF.Empty;
                _label = value;
            }
        }
        [JsonProperty]
        public string ImageBS64
        {
            get
            {
                //if (GraphicClip == null)
                //    return "";

                //string Base64String = Convert.ToBase64String(imageToByteArray(GraphicClip));
                //return Base64String;
                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value) || ImageSource == null)
                    return;

                using (Image legacyImage = byteArrayToImage(Convert.FromBase64String(value)))
                {
                    ImageRecordName = ImageSource.UpdateImageData(null, legacyImage);
                    ImageWidth = legacyImage.Width;
                    ImageHeight = legacyImage.Height;
                    GraphicClipThumbRectangle = Rectangle.Empty;
                    _GraphicClipThumbnail = null;
                }
            }
        }
        static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }
        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                Image returnImage = Image.FromStream(ms);
                return returnImage;
            }
        }
        //public Image GraphicClip;
        private operable.IImageContainer ImageSource;
        [JsonProperty]
        public string ImageRecordName { get; set; }
        [JsonProperty]
        public int ImageWidth { get; set; }
        [JsonProperty]
        public int ImageHeight { get; set; }

        public Image _GraphicClipThumbnail;
        public Image GraphicClipThumbnail
        {
            get
            {
                if (ImageRecordName == null) return null;
                if (_GraphicClipThumbnail != null) return _GraphicClipThumbnail;
                Rectangle rectResize = GraphicClipRectangle;
                using (Image GraphicClip = ImageSource.GetImageData(ImageRecordName))
                {
                    if (GraphicClip == null)
                    {
                        System.Windows.Forms.MessageBox.Show("Can't find image in archive: " + ImageRecordName);
                        // remove reference to image
                        ImageRecordName = null;
                        ImageHeight = ImageWidth = 0;
                        return null;
                    }
                    _GraphicClipThumbnail = GraphicClip.GetThumbnailImage(rectResize.Width, rectResize.Height, () => false, IntPtr.Zero);
                    return _GraphicClipThumbnail;
                }
            }
        }
        Rectangle GraphicClipThumbRectangle;
        public Rectangle GraphicClipRectangle
        {
            get
            {
                if (GraphicClipThumbRectangle != Rectangle.Empty) return GraphicClipThumbRectangle;
                if (ImageRecordName == null)
                    return Rectangle.Empty;

                double px = ImageWidth / 128.0;
                double height = ImageHeight / px;

                Rectangle rect = new Rectangle(location.X + 30, location.Y + 20, 128, (int)height);
                if (ImageWidth <= 128 && ImageHeight <= 128)
                {
                    return GraphicClipThumbRectangle = new Rectangle(location.X + 30, location.Y + 20, ImageWidth, ImageHeight);
                }
                return GraphicClipThumbRectangle = rect;
            }
            set
            {
                GraphicClipThumbRectangle = Rectangle.Empty;
            }
        }
        //
        public string ShortText = "";

        //
        public SizeF MeasuredLabel = SizeF.Empty;
        public SizeF MeasuredText = SizeF.Empty;
        [JsonProperty]
        public List<int> PhraseConnectReferences = new List<int>();
        //
        public int AddConnectedPrase(Phrase phrase, Dialog dlgref)
        {
            int index = dlgref.phrases.Count;
            PhraseConnectReferences.Add(index);
            dlgref.phrases.Add(phrase);
            return dlgref.phrases.Count - 1;
        }
        public bool RemoveConnections(int phraseIndex)
        {
            //if (PhraseConnectReferences.Contains(phraseIndex))
            return PhraseConnectReferences.Remove(phraseIndex);
        }
        Point locationText;
        //

        public Point location;
        //
        public Point locationC;
        //
        [JsonProperty]
        public Point position
        {
            get
            {
                return location;
            }
            set
            {
                Updated = true;
                location = value;
                locationC = new Point(value.X + 10, value.Y + 10);
                locationText = new Point(value.X + 22, value.Y + (int)(DRes.useFont.Height * .25));
            }
        }


        Pen colorpen = new Pen(Color.Black, 2);
        SolidBrush colorbrush = new SolidBrush(Color.Gray);
        SolidBrush colorbrushF = new SolidBrush(Color.Gray);

        [JsonProperty]
        public Color cp
        {
            get
            {
                return colorpen.Color;
            }
            set
            {
                colorpen.Color = value;
                colorbrushF.Color = value;
            }
        }
        [JsonProperty]
        public Color cb
        {
            get
            {
                return colorbrush.Color;
            }
            set
            {
                colorbrush.Color = value;
            }
        }

        public ColorSpace color
        {
            get
            {

                return new ColorSpace() { Colors = new Color[] { colorpen.Color, colorbrush.Color } };
            }
            set
            {
                Updated = true;
                colorpen.Color = value.Colors[0];
                colorbrushF.Color = colorpen.Color;
                colorbrush.Color = value.Colors[1];
            }
        }

        public void Draw(IGraphics gr)
        {
            //text
            if (!string.IsNullOrWhiteSpace(ShortText))
            {
                SizeF lsz1 = MeasuredText == SizeF.Empty ? gr.MeasureString(ShortText, DRes.useFont) : MeasuredText;
                MeasuredText = lsz1;
                Rectangle rects1 = new Rectangle(locationText.X, locationText.Y, (int)lsz1.Width, DRes.useFont.Height);

                gr.FillRectangle(new SolidBrush(Color.FromArgb(175, 255, 255, 255)), rects1);
                gr.DrawString(ShortText, DRes.useFont, Brushes.Black, locationText);
            }
            if (!string.IsNullOrWhiteSpace(Label))
            {
                SizeF lsz = MeasuredLabel == SizeF.Empty ? gr.MeasureString(Label, DRes.useFont) : MeasuredLabel;
                MeasuredLabel = lsz;
                Point locl = new Point(locationText.X - (int)lsz.Width - 24, locationText.Y);

                Rectangle rects1 = new Rectangle(locl.X, locl.Y, (int)lsz.Width - 4, (int)lsz.Height);
                gr.FillRectangle(new SolidBrush(Color.FromArgb(175, 255, 255, 255)), rects1);

                gr.DrawString(Label, DRes.useFont, Brushes.Black, locl);
            }
        }
        static Pen pen1 = new Pen(Color.Red, 3);
        public void DrawOval(IGraphics gr)
        {
            //oval
            Rectangle oval = new Rectangle(location, new Size(20, 20));
            gr.FillEllipse(colorbrush, oval);
            gr.DrawEllipse(colorpen, oval);
            if (this.PaintSpecialMark1)
                gr.DrawLine(pen1, location.X, location.Y + 22, location.X + 20, location.Y + 22);
        }
        public void DrawConnections(IGraphics gr, Dialog dlg)
        {
            for (int i = 0; i < PhraseConnectReferences.Count; i++)
            {
                Phrase cphrase = dlg.phrases[PhraseConnectReferences[i]];
                gr.DrawLine(colorpen, locationC, cphrase.locationC);

                // -- Direction point
                // get the point
                float ml = 12.0f;
                float mx = cphrase.locationC.X - locationC.X;
                float my = cphrase.locationC.Y - locationC.Y;

                float md = (float)Math.Sqrt(Math.Pow(mx, 2) + Math.Pow(my, 2));
                float mk = ml / md;

                float x = cphrase.locationC.X - (mk * mx);
                float y = cphrase.locationC.Y - (mk * my);

                float radius = 4.0f;
                float _x = x - radius;
                float _y = y - radius;
                float _width = 2 * radius;
                float _height = 2 * radius;

                gr.FillEllipse(colorbrushF, _x, _y, _width, _height);
            }
        }

        public void DrawGClip(IGraphics gr, bool drawClips)
        {
            if (drawClips)
            {
                if (GraphicClipThumbnail != null)
                {
                    Rectangle rect = GraphicClipRectangle;
                    gr.FillRectangle(new SolidBrush(Color.FromArgb(175, 255, 255, 255)), rect);

                    //gr.DrawImage(GraphicClip, rect);
                    gr.DrawImage(GraphicClipThumbnail, rect);

                    gr.DrawRectangle(colorpen, rect);
                }

            }
            else if (GraphicClipThumbnail != null)
            {
                gr.DrawString("[image]", DRes.useFont, Brushes.CornflowerBlue, new PointF(location.X + 20, location.Y + 20));
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты).
                    _GraphicClipThumbnail.Dispose();
                    //GraphicClip.Dispose();
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить ниже метод завершения.
                // TODO: задать большим полям значение NULL.
                _GraphicClipThumbnail = null;
                //GraphicClip = null;

                disposedValue = true;
            }
        }

        // TODO: переопределить метод завершения, только если Dispose(bool disposing) выше включает код для освобождения неуправляемых ресурсов.
        // ~Phrase()
        // {
        //   // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
        //   Dispose(false);
        // }

        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
            Dispose(true);
            // TODO: раскомментировать следующую строку, если метод завершения переопределен выше.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
