using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Spider
{
    [Serializable]
    public class Constraint
    {
        public Constraint(String value)
        {
            if (value.Contains(','))
            {
                String[] t = value.Split(',');
                Left = int.Parse(t[0]);
                Top = int.Parse(t[1]);
                Bottom = int.Parse(t[2]);
                Right = int.Parse(t[3]);
            }
            else
            {
                int val = int.Parse(value);
                Left = val;
                Top = val;
                Right = val;
                Bottom = val;
            }
        }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }

    [Serializable]
    public class Margin : Constraint
    {
        public Margin(String value) :
            base(value)
        {

        }
    }

    [Serializable]
    public class Padding : Constraint
    {
        public Padding(String value) :
            base(value)
        {

        }
    }
    [Serializable]
    public class Selector : ICloneable
    {
        public Margin Margin { get; set; }
        public Padding Padding { get; set; }
        public Color BackColor;
        public Color ForeColor;
        public Color AlternateColor;
        public Image BackgroundImage;
        public Color TextShadowColor;
        private Font font = new Font("MS Sans Serif", 11);
        public Font Font
        {
            get
            {
                return this.font;
            }
            set
            {
                this.font = value;
            }
        }
        public Selector(String code, Selector parent)
        {
            Aleros.CSS.Selector selector = new Aleros.CSS.Selector("@internal", code);
            this.Padding = new Padding("0");
            this.Margin = new Margin("1");
            foreach (Aleros.CSS.Rule rule in selector.rules)
            {
                if (rule.rule == "background" || rule.rule == "background-color")
                {
                    this.BackColor = ColorTranslator.FromHtml(rule.value);
                }
                else
                {
                    this.BackColor = parent.BackColor;
                }
                if (rule.rule == "font-color")
                {
                        this.ForeColor = ColorTranslator.FromHtml(rule.value);
                }
                else
                {
                    this.ForeColor = parent.ForeColor;
                }
                if (rule.rule == "padding")
                {
                    this.Padding = new Padding(rule.value);
                }
                else
                {
                    this.Padding = parent.Padding;
                }
                if (rule.rule == "margin")
                {
                    this.Margin = new Margin(rule.value);
                }
                else
                {
                    this.Margin = parent.Margin;
                }

            }
            
        }
        public Selector(Color backColor, Color foreColor, Color textShadowColor, Color alternateColor)
        {
            this.Padding = new Padding("1");
            this.Margin = new Margin("1");
            this.BackColor = backColor;
            this.ForeColor = foreColor;
            this.TextShadowColor = textShadowColor;
            this.AlternateColor = alternateColor;
            this.Padding = new Padding("0");
            this.Margin = new Margin("1");
        }
        public Selector(Image backgroundImage, Color foreColor, Color textShadowColor, Color alternateColor)
        {
            this.BackgroundImage = backgroundImage;
            this.ForeColor = foreColor;
            this.TextShadowColor = textShadowColor;
            this.AlternateColor = alternateColor;
            this.Padding = new Padding("0");
            this.Margin = new Margin("1");
        }

        public object Clone()
        {
            Selector newSelector = new Selector(BackColor, ForeColor, TextShadowColor, AlternateColor);
            if (this.BackgroundImage != null)
                newSelector.BackgroundImage = this.BackgroundImage;
            newSelector.Padding = Padding;
            newSelector.Margin = Margin;
            newSelector.BackColor = BackColor;
            newSelector.ForeColor = ForeColor;
            return newSelector;
        }
    }
    [Serializable]
    [XmlRoot("skin")]
    public class Style
    {
        
        public Style()
        {

            this.Selectors = new Dictionary<string, Selector>();
            Skin = Properties.Resources.skin;
            // Partialize skin
        }
        private Bitmap skin;
        public Bitmap Skin {
            get{
                return this.skin;
            }set{
                this.skin = value;
                Slice(skin);
            }
        }
        public void Slice(Bitmap bitmap)
        { 
            // Get tab bar
            this.Selectors.Add("TabBar", new Selector(sliceBitmap(bitmap, new Rectangle(48, 1, 2, 23)), bitmap.GetPixel(2, 0), bitmap.GetPixel(4, 0), bitmap.GetPixel(0, 0)));
            this.Selectors.Add("TabBar::active", new Selector(bitmap.GetPixel(9, 0), Color.White, Color.Black, bitmap.GetPixel(0, 0)));

            this.Selectors.Add("Divider", new Selector(sliceBitmap(bitmap, new Rectangle(0, 24, 50, 23)), Color.White, Color.Black, Color.White));
            this.Selectors.Add("::selection", new Selector(bitmap.GetPixel(6, 0), bitmap.GetPixel(5, 0), Color.Black, Color.White));
            this.Selectors.Add("ListView", new Selector(bitmap.GetPixel(8, 0), Color.White, Color.Black, bitmap.GetPixel(8, 0)));
            this.Selectors.Add("Body", new Selector(bitmap.GetPixel(0, 0), bitmap.GetPixel(1, 0), Color.Black, bitmap.GetPixel(8, 0)));
        }
        public Bitmap sliceBitmap(Bitmap src, Rectangle region)
        {
            Bitmap target = new Bitmap(region.Width, region.Height);
            Graphics g = Graphics.FromImage(src);
            Graphics targetGraphics = Graphics.FromImage(target);
            targetGraphics.DrawImage(src, 0, 0, region, GraphicsUnit.Pixel);
            return target;
        }

        public Dictionary<string, Selector> Selectors { get; set; }
    }
}
