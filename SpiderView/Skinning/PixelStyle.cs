using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Spider.Skinning
{
    public class PixelStyle : Style
    {

        public PixelStyle()
        {

            this.blocks = new Dictionary<string, Block>();
            Skin = Properties.Resources.skin;
            // Partialize skin
        }
        private Bitmap skin;
        public Bitmap Skin
        {
            get
            {
                return this.skin;
            }
            set
            {
                this.skin = value;
                Slice(skin);
            }
        }
        public void Slice(Bitmap bitmap)
        {
            // Get tab bar
            this.Blocks.Add("TabBar", new Block(sliceBitmap(bitmap, new Rectangle(48, 1, 2, 23)), bitmap.GetPixel(9, 0), bitmap.GetPixel(4, 0), bitmap.GetPixel(0, 0)));
            this.Blocks.Add("TabBar::active", new Block(bitmap.GetPixel(9, 0), Color.White, Color.Black, bitmap.GetPixel(0, 0)));
            Block track = new Block(/*sliceBitmap(bitmap, new Rectangle(0, 24, 50, 23))*/ bitmap.GetPixel(11, 0), Color.White, Color.Black, Color.White);
            this.Blocks.Add("track", track);
            track.Font = new Font("MS Sans Serif", 8, FontStyle.Regular, GraphicsUnit.Pixel);
            var trackSelected = new Block(bitmap.GetPixel(6, 0), bitmap.GetPixel(5, 0), Color.Transparent, Color.Black);
            trackSelected.Font = new Font("MS Sans Serif", 8);
            this.blocks.Add("track::selected", trackSelected);

            track.AlternateBackColor = bitmap.GetPixel(9, 0);
            track.Font = new Font("MS Sans Serif", 8);

            this.Blocks.Add("Divider", new Block(sliceBitmap(bitmap, new Rectangle(0, 24, 50, 23)), Color.White, Color.Black, Color.White));
            this.Blocks.Add("::selection", new Block(bitmap.GetPixel(6, 0), bitmap.GetPixel(5, 0), Color.Black, Color.White));
            this.Blocks.Add("ListView", new Block(bitmap.GetPixel(8, 0), Color.White, Color.Black, bitmap.GetPixel(8, 0)));
            this.Blocks.Add("Body", new Block(bitmap.GetPixel(0, 0), bitmap.GetPixel(1, 0), Color.Black, bitmap.GetPixel(8, 0)));
        }
        public Bitmap sliceBitmap(Bitmap src, Rectangle region)
        {
            Bitmap target = new Bitmap(region.Width, region.Height);
            Graphics g = Graphics.FromImage(src);
            Graphics targetGraphics = Graphics.FromImage(target);
            targetGraphics.DrawImage(src, 0, 0, region, GraphicsUnit.Pixel);
            return target;
        }

        public Dictionary<String, Block> Blocks
        {
            get
            {
                return this.blocks;
            }
        }
        private Dictionary<string, Block> blocks { get; set; }
    }
}
