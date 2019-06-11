using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DialogMaker
{
    static class Program
    {
        public static Form1 editorForm;
        [STAThread]
        static void Main(string[] args)
        {
            //System.Threading.Thread t = new System.Threading.Thread(() =>
            //{
            //    while (true)
            //    {
            //        if (System.Diagnostics.Debugger.IsAttached)
            //        {
            //            System.IO.StreamWriter sw = System.IO.File.AppendText("dat.txt");
            //            sw.WriteLine(string.Format("at: {0}", DateTime.Now));
            //            sw.Close();
            //        }
            //        System.Threading.Thread.Sleep(0);
            //    }
            //});
            //t.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            
            editorForm = new Form1();
            if (args != null && args.Length > 0)
            {
                editorForm.DeferLoadJsFile(args[0]);
                Application.Run(editorForm);
            }
            else
                Application.Run(editorForm);
        }
        public static bool HitTest(float Mx, float My, float sX, float Sy, float Lx, float Ly)
        {
            if (Lx > 0 & Ly > 0)
            {
                return (Mx < (sX + Lx) & (Mx >= sX) & (My < (Sy + Ly)) & (My >= Sy));
            }
            if (Lx < 0 & Ly < 0)
            {
                return (Mx > (sX + Lx) & (Mx < sX) & (My < (Sy + Ly)) & (My > Sy));
            }
            if (Lx < 0)
            {
                return (Mx > (sX + Lx) & (Mx < sX) & (My < (Sy + Ly)) & (My > Sy));
            }
            if (Ly < 0)
            {
                return (Mx < (sX + Lx) & (Mx > sX) & (My > (Sy + Ly)) & (My < Sy));
            }
            return false;
        }
    }
}
