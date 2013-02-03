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
            var even = new Block(bitmap.GetPixel(0, 0), track.ForeColor, track.TextShadowColor, track.AlternateBackColor);
            even.Font = new Font("MS Sans Serif", 8);
            this.blocks.Add("track::even", even) ;
            track.AlternateBackColor = bitmap.GetPixel(9, 0);
            track.Font = new Font("MS Sans Serif", 8);
            this.Blocks.Add("track::playing", new Block(Color.Black, Color.LightGreen, Color.Black, Color.White));
            
            this.Blocks.Add("Divider", new Block(sliceBitmap(bitmap, new Rectangle(0, 24, 50, 23)), Color.White, Color.Black, Color.White));
            this.Blocks.Add("::selection", new Block(bitmap.GetPixel(6, 0), bitmap.GetPixel(5, 0), Color.Black, Color.White));
            var ch = new Block(sliceBitmap(bitmap,new Rectangle(0, 99, 65, 19)), bitmap.GetPixel(3, 117), bitmap.GetPixel(65, 99), bitmap.GetPixel(3, 117));
            ch.Font = new Font("MS Sans Serif", 8);
            this.Blocks.Add("columnheader", ch);
            this.blocks.Add("hr", new Block(Color.FromArgb(0, 0, 0, 0), bitmap.GetPixel(15, 1), Color.White, Color.Black));
            this.Blocks.Add("ListView", new Block(bitmap.GetPixel(8, 0), Color.White, Color.Black, bitmap.GetPixel(8, 0)));
            this.Blocks.Add("Body", new Block(bitmap.GetPixel(0, 0), bitmap.GetPixel(1, 0), Color.Black, bitmap.GetPixel(8, 0)));

            var btn = new Block(sliceBitmap(bitmap, new Rectangle(128, 1, 105, 20)), Color.Black, Color.White, Color.Black);
            var btnPressed = new Block(sliceBitmap(bitmap, new Rectangle(128, 22, 105, 20)), Color.Black, Color.White, Color.Black);
            this.blocks.Add("button", btn);
            this.blocks.Add("button:active", btnPressed);
            btn.Font = new Font("MS Sans Serif", 8);
            btnPressed.Font = new Font("MS Sans Serif", 8);
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
