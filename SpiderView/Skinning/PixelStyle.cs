using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Aleros.CSS;
using System.IO;

namespace Spider.Skinning
{
    public class PixelStyle : Style
    {
        private Dictionary<String, Image> images;
        public Dictionary<String, Image> Images
        {
            get
            {
                return images;
            }
        }
        public PixelStyle()
        {

            this.blocks = new Dictionary<string, Block>();
            
            // Partialize skin
            this.Skin = Properties.Resources.skin;
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
#if (true)
            // Get tab bar
            var tabTitle = new Block(this, bitmap.GetPixel(1, 0), bitmap.GetPixel(1, 0), bitmap.GetPixel(4, 0), bitmap.GetPixel(1, 0));
            this.Blocks.Add("infobar::info", new Block(this, sliceBitmap(bitmap, new Rectangle(187, 98, 90 ,30)), Color.Black, Color.White, Color.Black));
            this.blocks["infobar::info"].Height = 30;
            this.blocks["infobar::info"].Padding.Left = 10;
            this.blocks["infobar::info"].Padding.Top = 10;
            this.blocks.Add(".adivider", new Block(this, bitmap.GetPixel(15, 0), bitmap.GetPixel(16, 0), bitmap.GetPixel(16, 0), bitmap.GetPixel(16, 0)));
            this.blocks.Add(".h1", new Block(this, Color.Transparent, bitmap.GetPixel(16, 0), bitmap.GetPixel(16, 0), bitmap.GetPixel(16, 0)));
            this.blocks[".h1"].Font = new Font("Helvetica", 10);
            this.Blocks[".adivider"].Font = new Font("Helvetica", 13, FontStyle.Regular, GraphicsUnit.Pixel);
            this.blocks[".adivider"].Height = 22;
            this.blocks[".adivider"].Width = -1;
            this.blocks.Add("TabBar::title", tabTitle);
            tabTitle.Font = new Font("MS Sans Serif", 8f, FontStyle.Bold);
            this.Blocks.Add("TabBar", new Block(this, sliceBitmap(bitmap, new Rectangle(99, 1, 2, 23)), bitmap.GetPixel(1, 0), bitmap.GetPixel(4, 0), bitmap.GetPixel(0, 0)));
            this.Blocks.Add("TabBar::active", new Block(this, sliceBitmap(bitmap, new Rectangle(0, 1, 39, 22)), Color.White, Color.Black, bitmap.GetPixel(0, 0)));
            Block track = new Block(/*sliceBitmap(bitmap, new Rectangle(0, 24, 50, 23))*/this, bitmap.GetPixel(11, 0), bitmap.GetPixel(21, 0), Color.Black, Color.White);
            this.Blocks.Add("track", track);
            track.Font = new Font("MS Sans Serif", 8, FontStyle.Regular, GraphicsUnit.Pixel);
            this.blocks.Add("listitem", new Block(this, bitmap.GetPixel(19, 0), bitmap.GetPixel(19, 0), bitmap.GetPixel(19, 0), bitmap.GetPixel(19, 0)));
            var trackSelected = new Block(this, bitmap.GetPixel(6, 0), bitmap.GetPixel(5, 0), Color.Transparent, Color.Black);
            trackSelected.Font = new Font("MS Sans Serif", 8);
            this.blocks.Add("track::selected", trackSelected);
            var even = new Block(this, bitmap.GetPixel(0, 0), track.ForeColor, track.TextShadowColor, track.AlternateBackColor);
            even.Font = new Font("MS Sans Serif", 8);
            this.blocks.Add("track::even", even) ;
            track.AlternateBackColor = bitmap.GetPixel(9, 0);
            track.Font = new Font("MS Sans Serif", 8);
            this.Blocks.Add("track::playing", new Block(this, Color.Black, Color.LightGreen, Color.Black, Color.White));
            this.Blocks.Add("track::unavailable", new Block(this, Color.Transparent, bitmap.GetPixel(22, 0), Color.Black, Color.DarkRed));

            this.Blocks.Add("Divider", new Block(this, sliceBitmap(bitmap, new Rectangle(0, 24, 50, 23)), Color.White, Color.Black, Color.White));
            this.Blocks.Add("::selection", new Block(this, bitmap.GetPixel(6, 0), bitmap.GetPixel(5, 0), Color.Black, Color.White));
            var ch = new Block(this, sliceBitmap(bitmap, new Rectangle(0, 99, 65, 19)), bitmap.GetPixel(3, 117), bitmap.GetPixel(65, 99), bitmap.GetPixel(3, 117));
            ch.Font = new Font("MS Sans Serif", 8);
            this.Blocks.Add("columnheader", ch);
            this.blocks.Add("hr", new Block(this, Color.FromArgb(0, 0, 0, 0), bitmap.GetPixel(15, 1), Color.White, Color.Black));
            this.Blocks.Add("ListView", new Block(this, bitmap.GetPixel(20, 0), Color.White, Color.Black, bitmap.GetPixel(8, 0)));
            this.Blocks.Add("Body", new Block(this, bitmap.GetPixel(0, 0), bitmap.GetPixel(1, 0), Color.Black, bitmap.GetPixel(8, 0)));
            this.blocks["Body"].Font = new Font("Tahoma", 8f, FontStyle.Regular);
            var btn = new Block(this, sliceBitmap(bitmap, new Rectangle(128, 1, 105, 20)), Color.Black, Color.White, Color.Black);
            var btnPressed = new Block(this, sliceBitmap(bitmap, new Rectangle(128, 22, 105, 20)), Color.Black, Color.White, Color.Black);
            this.blocks.Add("button", btn);
            this.blocks.Add(".biography", new Block(this, Color.FromArgb(255, 255, 255, 233), Color.Black, Color.White, Color.Black));

            this.blocks.Add("button:active", btnPressed);
            btn.Font = new Font("MS Sans Serif", 8);
            btnPressed.Font = new Font("MS Sans Serif", 8);

            this.blocks.Add("backbtn", new Block(this, sliceBitmap(bitmap, new Rectangle(128, 42, 27, 20)), Color.White, Color.Black, Color.White));
            this.blocks.Add("backbtn:active", new Block(this, sliceBitmap(bitmap, new Rectangle(128, 61, 27, 20)), Color.White, Color.Black, Color.White));
            this.blocks.Add("backbtn:disabled", new Block(this, sliceBitmap(bitmap, new Rectangle(128, 184, 27, 20)), Color.White, Color.Black, Color.White));
            this.blocks.Add("forebtn", new Block(this, sliceBitmap(bitmap, new Rectangle(155, 42, 27, 20)), Color.White, Color.Black, Color.White));
            this.blocks.Add("forebtn:active", new Block(this, sliceBitmap(bitmap, new Rectangle(155, 61, 27, 20)), Color.White, Color.Black, Color.White));
            this.blocks.Add("forebtn:disabled", new Block(this, sliceBitmap(bitmap, new Rectangle(155, 184, 27, 20)), Color.White, Color.Black, Color.White));
            this.blocks.Add("header", new Block(this, sliceBitmap(bitmap, new Rectangle(214, 0, 92, 55)), Color.Black, Color.White, Color.Black));
            this.blocks.Add("tab:divider", new Block(this, bitmap.GetPixel(123, 3), bitmap.GetPixel(97, 3), Color.Black, Color.Black));
            this.blocks.Add("::sub", new Block(this, bitmap.GetPixel(17, 0), bitmap.GetPixel(17, 0), bitmap.GetPixel(17, 0), bitmap.GetPixel(17, 0)));
#endif
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

        BufferedGraphicsContext bgc = new BufferedGraphicsContext();
        public void DrawString(Graphics g, string text, Font font, SolidBrush brush, Rectangle pos)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
          
        //      g.DrawString(text, font, new SolidBrush(Color.FromArgb(127, Color.Black)), new Rectangle(pos.X, pos.Y -1 , pos.Width, pos.Height));
            
#if(!Renderer)
            TextRenderer.DrawText(g, text, font, new Rectangle(pos.Left, pos.Top, pos.Width, pos.Height), brush.Color, Color.Transparent, TextFormatFlags.Left | TextFormatFlags.WordBreak);
#else

            g.DrawString( text, font, brush, pos);
#endif          
        }
        public Size MeasureString(Graphics g, string text, Font font)
        {
#if(Renderer)
            return TextRenderer.MeasureText(g, text, font, Size.Empty, TextFormatFlags.NoPadding);
#else
            return TextRenderer.MeasureText(text, font,Size.Empty, TextFormatFlags.NoPadding);
#endif
        }
    }
}
