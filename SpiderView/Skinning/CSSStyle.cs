using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spider.Skinning
{
    class CSSStyle : Style
    {
        private Dictionary<String, Image> images;
        public Dictionary<String, Image> Images
        {
            get
            {
                return images;
            }
        }


        public Dictionary<string, Block> Blocks
        {
            get { throw new NotImplementedException(); }
        }

        public void DrawString(Graphics g, string text, Font font, SolidBrush brush, Rectangle pos)
        {
            throw new NotImplementedException();
        }

        public Size MeasureString(string text, Font font)
        {
            throw new NotImplementedException();
        }
    }
}
