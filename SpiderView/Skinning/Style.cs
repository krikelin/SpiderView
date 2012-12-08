using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Spider.Skinning
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
    /// <summary>
    /// Edge
    /// </summary>
    public class Edge : Constraint
    {
        public Edge(String value)
            : base(value)
        {
        }
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
    public class Selector
    {
        public String Rule;
        public bool True;
      
    }
    [Serializable]
    public class Block : ICloneable
    {
     
        public List<Selector> Selectors = new List<Selector>();
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
        public Block()
        {
        }
        public Block(String code, Block parent)
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
        public Block(Color backColor, Color foreColor, Color textShadowColor, Color alternateColor)
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
        public Block(Image backgroundImage, Color foreColor, Color textShadowColor, Color alternateColor)
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
            Block newSelector = new Block(BackColor, ForeColor, TextShadowColor, AlternateColor);
            if (this.BackgroundImage != null)
                newSelector.BackgroundImage = this.BackgroundImage;
            newSelector.Padding = Padding;
            newSelector.Margin = Margin;
            newSelector.BackColor = BackColor;
            newSelector.ForeColor = ForeColor;
            return newSelector;
        }
    }
    public interface Style
    {
        /// <summary>
        /// Get selectors
        /// </summary>
        Dictionary<String, Block> Blocks { get; }
    }
    
}
