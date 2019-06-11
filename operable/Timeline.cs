// -----------------------------------------------------------------------
// <copyright file="Timeline.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DialogMaker
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using SvgNet.SvgGdi;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class TimeLine
    {
        [JsonProperty]
        public int Ylocation;

        string _text;

        [JsonProperty]
        public string Text
        {
            set
            {
                _text = value;
            }
            get
            {
                return _text;
            }
        }
        public Rectangle _dragtag = new Rectangle();
        Pen colorpen = new Pen(Color.FromArgb(105, 160, 32), 2);
        SolidBrush colorbrush = new SolidBrush(Color.FromArgb(249, 249, 249));

        public void Draw(IGraphics gr, int left, int right)
        {
            Point ls = new Point(left/*(int)gr.ClipBounds.Left*/, Ylocation);
            Point es = new Point(right/*(int)gr.ClipBounds.Right*/, Ylocation);
            gr.DrawLine(colorpen, ls, es);
            //oval
            Rectangle oval = new Rectangle(new Point(ls.X, Ylocation - 10), new Size(15, 15));
            gr.FillRectangle(colorbrush, oval);
            gr.DrawRectangle(colorpen, oval);
            _dragtag = oval;
            //text
            gr.DrawString(Text, DRes.useFont, new SolidBrush(colorpen.Color), ls.X + 18, Ylocation - 16);
        }
        public string ViewSVG(IGraphics gr, int layer)//1:lines,2:figures,3:text
        {
            Point ls = new Point((int)gr.ClipBounds.Left, Ylocation);
            Point es = new Point((int)gr.ClipBounds.Right, Ylocation);
            switch (layer)
            {
                case 1:
                    //<line stroke="Maroon" stroke-width="2" x1="150" y1="150" x2="200" y2="200" />
                    return string.Format("<line stroke=\"#{0:X}{1:X}{2:X}\" stroke-width=\"2\" x1=\"{3}\" y1=\"{4}\" x2=\"{5}\" y2=\"{6}\" />",
                    colorpen.Color.R, colorpen.Color.G, colorpen.Color.B,
                        ls.X, ls.Y, es.X, es.Y);
                case 2:
                    //<rect fill="Red" stroke="Maroon" x="0" y="15" width="15" height="15" />
                    return string.Format("<rect fill=\"#{0:X}{1:X}{2:X}\" stroke=\"#{3:X}{4:X}{5:X}\" x=\"{6}\" y=\"{7}\" width=\"15\" height=\"15\" />",
                    colorbrush.Color.R, colorbrush.Color.G, colorbrush.Color.B,
                    colorpen.Color.R, colorpen.Color.G, colorpen.Color.B,
                        ls.X, Ylocation - 10);
                case 3:
                    //<text x="250" y="250" font-family="Verdana" font-size="9">label</text>
                    return string.Format("<text x=\"{0}\" y=\"{1}\" font-family=\"Verdana\" font-size=\"9\">{2}</text>",
                    ls.X + 18, Ylocation - 16, Text);
            }
            return "";
        }
    }
}
