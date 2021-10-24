using System.Drawing;

namespace DialogMaker.jzip
{
    internal class EmptyContainer : operable.IImageContainer
    {
        public string FileName => null;

        public void Dispose()
        {

        }

        public Image GetImageData(string name)
        {
            return null;
        }

        public void RemoveImageData(string name)
        {
        }

        public string UpdateImageData(string oldName, Image data)
        {
            return oldName;
        }
    }
}
