// -----------------------------------------------------------------------
// <copyright file="OpenFileHistory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DialogMaker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
    public class OpenFileHistory
    {
        public delegate void HistoryUpdated(OpenFileHistory hc);
        public static event HistoryUpdated OnUpdate;

        //static string path = Path.GetDirectoryName(
        //Assembly.GetAssembly(typeof(OpenFileHistory)).CodeBase);
        static string fileloc = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "file.history.txt");
        const int limit = 10;

        [JsonProperty]
        public List<string> Paths = new List<string>();

        public static OpenFileHistory DHistory
        {
            get
            {
                string json = null;

                try
                {
                    json = System.IO.File.ReadAllText(fileloc, Encoding.Unicode);
                }
                catch (Exception e)
                {
                    return new OpenFileHistory();
                }
                return Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(OpenFileHistory)) as OpenFileHistory;
            }
        }

        public void Push(string path)
        {
            int iofh = -1;
            if ((iofh = this.Paths.IndexOf(path)) >= 0)
            {
                this.Paths.RemoveAt(iofh);
            }

            this.Paths.Add(path);

            if (this.Paths.Count > limit)
                this.Paths.RemoveAt(0);

            if (OnUpdate != null)
            {
                OnUpdate(this);
            }

            this.Save();
        }

        public void Save()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this, Formatting.Indented);
            System.IO.File.WriteAllText(fileloc, json, Encoding.Unicode);
        }
        public struct MenuString
        {
            public string Title { get; set; }
            public string Path { get; set; }
        }
        public IEnumerable<MenuString> ByLA
        {
            get
            {
                for (int i = this.Paths.Count - 1; i >= 0; i--)
                {
                    yield return new MenuString() { Title = StripPath2(this.Paths[i]), Path = this.Paths[i] };
                }
            }
        }

        static string StripPath2(string data)
        {
            int io1 = data.LastIndexOf('\\');
            int io2 = data.LastIndexOf('\\', io1 - 1, io1 - 1);// what a f**?

            if (io2 <= 0)
                io2 = io1 <= 0 ? 0 : io1;
            io2++;

            return data.Substring(io2, data.Length - io2);
        }
    }
}
