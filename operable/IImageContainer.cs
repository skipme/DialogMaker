using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogMaker.operable
{
    public interface IImageContainer
    {
        Image GetImageData(string name);
        void RemoveImageData(string name);
        string UpdateImageData(string oldName, Image data);
    }
}
