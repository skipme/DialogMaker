using System;
using System.Drawing;

namespace DialogMaker.operable
{
    public interface IImageContainer : IDisposable
    {
        string FileName { get; }
        Image GetImageData(string name);
        void RemoveImageData(string name);
        string UpdateImageData(string oldName, Image data);
    }
}
