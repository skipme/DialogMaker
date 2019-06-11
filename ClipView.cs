using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DialogMaker
{
    public partial class ClipView : Form
    {
        public ClipView()
        {
            InitializeComponent();
            this.Paint += new PaintEventHandler(ClipView_Paint);
            this.SizeChanged += new EventHandler(ClipView_SizeChanged);

            this.DoubleBuffered = true;
        }

        void ClipView_SizeChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }

        void ClipView_Paint(object sender, PaintEventArgs e)
        {
            Graphics gr = e.Graphics;

            gr.Clear(Color.Cyan);

            if (Clip == null)
            {
                gr.DrawString("Preview Image unavailable", Font, Brushes.Black, new PointF(4, 4));
            }
            else
            {
                gr.DrawImage(Clip, GraphicClipRectangle);
                gr.DrawString(ctype, Font, Brushes.Red, new PointF(4, 4));
            }
        }

        public Image Clip { get; set; }
        string ctype = "na";
        private Rectangle GraphicClipRectangle
        {
            get
            {
                if (Clip == null)
                    return Rectangle.Empty;

                Rectangle rect = Rectangle.Empty;
                ctype = string.Format("w:{0}px h:{1}px", Clip.Width, Clip.Height);

                //if (Clip.Height > this.Height)
                //{
                double py = (double)Clip.Height / (double)this.Height;
                //ctype = string.Format("{0} py:{1} ", 1, py);

                double width = Clip.Width / py;

                double middletop = this.Width / 2.0;

                rect = new Rectangle((int)(middletop - width * 0.5), 0, (int)width, this.Height);
                //}

                return rect;
            }
        }

    }
}