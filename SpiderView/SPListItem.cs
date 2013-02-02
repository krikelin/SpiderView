using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Spider.Skinning;

namespace Spider
{
    /// <summary>
    /// MenuItem for SPMenu
    /// </summary>
    [Serializable]
    public class SPListItem
    {
        public int AbsoluteY
        {
            get
            {
                int pos = 0;
                foreach (SPListItem item in this.parent.Items)
                {
                    if (item == this)
                        return pos;
                    pos += item.Height;
                }
                return pos;
            }
        }
        public class ListIcon
        {
            public Image Normal;
            public Image Selected;
        }
        public App AppInstance { get; set; }
        public bool Touched { get; set; }
        public Color Color
        {
            get
            {
                return this.color;
            }
            set
            {
                this.color = value;
                CustomColor = true;
            }
        }
        private Color color;

        public bool CustomColor { get; set; }
        public Uri Uri { get; set; }
        public String Text { get; set; }
        public ListIcon Icon;
        public SPListView ParentListView;
#if(False)
        public SPListItem AddItem(String text, Uri uri)
        {
            SPListItem c = new SPListItem(this.Parent);
            c.Text = text;
            c.Uri = uri;
            this.Children.Add(c);
            return c;
        }
#endif
        public String SubText { get; set; }
        private String loadedText = "";
        public SPListItem AddItem(String text, Uri uri)
        {

            
            SPListItem c = new SPListItem(this.Parent);
            c.Text = text;
            c.Uri = uri;
            this.Children.Add(c);
            return c;
        }
        public Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }
        public Block Block;
        public Block SelectedBlock;
        public int Height = 18;
        public void Draw(Graphics g, ref int level, ref int pos)
        {
            if (this.Block == null)
                return;
            Color foreColor = this.CustomColor ? this.Color : this.Block.ForeColor;
            Color intermediateColor = this.CustomColor ? this.Color : Color.Gray;
            Color backColor = this.Block.BackColor;
            if (this.Text.StartsWith("-"))
            {
                g.DrawLine(new Pen(DividerBlock.ForeColor), new Point(0, pos + (this.Height / 2)), new Point(this.ParentListView.Width, pos + (this.Height / 2)));
            }
            else if (this.Selected)
            {
                foreColor = SelectedBlock.ForeColor;
                intermediateColor = SelectedBlock.ForeColor;
                backColor = SelectedBlock.BackColor;

                g.FillRectangle(new SolidBrush(backColor), new Rectangle(0, pos, this.ParentListView.Width, this.Height));
                g.DrawString(this.Text, new Font("MS Sans Serif", 8), new SolidBrush(foreColor), new Point(level + 32, pos + 2));
                if (this.SubText != null)
                {
                    int left = level + 32 + (int)g.MeasureString(this.Text, new Font("MS Sans Serif", 8)).Width ;
                    g.DrawString(this.SubText, new Font("MS Sans Serif", 8), new SolidBrush(intermediateColor), new Point(left, pos + 3));

                }
            }
            else if (this.Text.StartsWith("#"))
            {
                foreColor = SelectedBlock.TextShadowColor;
                g.DrawString(this.Text.ToUpper().Replace("#", ""), new Font("MS Sans Serif", 8), new SolidBrush(foreColor), new Point(4, pos + 0));
                g.DrawString(this.Text.ToUpper().Replace("#", ""), new Font("MS Sans Serif", 8), new SolidBrush(Block.TextShadowColor), new Point(4, pos - 1));
            }
            else
            {
                g.DrawString(this.Text, new Font("MS Sans Serif", 8), new SolidBrush(Block.TextShadowColor), new Point(level + 32, pos + 2));
                g.DrawString(this.Text, new Font("MS Sans Serif", 8), new SolidBrush(foreColor), new Point(level + 32, pos + 3));
                if (this.SubText != null)
                {
                    int left = level + 32 + (int)g.MeasureString(this.Text, new Font("MS Sans Serif", 8)).Width;
                    g.DrawString(this.SubText, new Font("MS Sans Serif", 8), new SolidBrush(ChangeColorBrightness(intermediateColor, -0.6f)), new Point(left, pos + 2));
                    g.DrawString(this.SubText, new Font("MS Sans Serif", 8), new SolidBrush(intermediateColor), new Point(left, pos + 3));

                }
            }
            //  g.DrawString(this.Text, new Font("MS Sans Serif", 8), new SolidBrush(foreColor), new Point(level + 16, pos + 2));
            if (this.Icon != null)
            {
                g.DrawImage(this.Selected ? this.Icon.Selected : this.Icon.Normal, level + 16, pos + 1, 18, 18);
            }
            // If has subthiss create expander
            if (this.Children.Count > 0)
            {
                //Image expander = this.Expanded ? Properties.Resources.ic_expander_open : Properties.Resources.ic_expander_closed;
                // g.DrawImage(expander, level, pos, 16, 16);
            }
        }
        void instance_Loaded(object sender, EventArgs e)
        {
            this.Text = this.AppInstance.GetName();
            if (this.AppInstance.GetIcon() != null)
                this.Icon = this.AppInstance.GetIcon();
            this.SubText = this.AppInstance.GetSubName();
       
        }
        public SPListItem AddItem(String text, Uri uri, ListIcon icon)
        {
            SPListItem c = new SPListItem(this.Parent);
            c.Text = text;
            c.Uri = uri;
            c.Icon = icon;
            this.Children.Add(c);
            return c;
        }
        private SPListView parent;
        public SPListView Parent
        {
            get
            {
                return this.parent;
            }
        }
        public Boolean Selected
        {
            get;
            set;
        }
        public SPListItem GetItemByUri(Uri uri)
        {

            foreach (SPListItem item in this.Children)
            {
                if (item.Uri.ToString().Contains(uri.ToString()))
                {
                    return item;
                }
                else
                {
                    return item.GetItemByUri(uri);
                }
            }
            return null;

        }
        public bool HasUri(Uri uri, ref bool positive)
        {
            foreach (SPListItem item in this.Children)
            {
                if (item.Uri.ToString().Contains(uri.ToString()))
                {
                    positive = true;
                }
                else
                {
                    item.HasUri(uri, ref positive);
                }
            }
            return positive;

        }
        public List<SPListItem> Children { get; set; }
        public SPListItem(SPListView parent, String uri)
        {
            this.parent = parent;
            this.ParentListView = parent;
            this.Block = (Block)parent.Block.Clone();
            this.SelectedBlock = (Block)parent.SelectedBlock.Clone();
            this.Uri = new Uri(uri);
            this.Children = new List<SPListItem>();
            this.AppInstance = this.Parent.Host.LoadApp(uri.ToString());
            AppInstance.Loaded += instance_Loaded;
            this.DividerBlock = (Block)parent.stylesheet.Blocks["hr"].Clone();
            this.Text = "Loading..";
        }
        public SPListItem(SPListView parent, String uri, String title)
        {
            this.Block = (Block)parent.Block.Clone();
            this.SelectedBlock = (Block)parent.SelectedBlock.Clone();
            this.DividerBlock = (Block)parent.stylesheet.Blocks["hr"].Clone();
            this.ParentListView = parent;

            this.parent = parent;
            this.Uri = new Uri(uri);
            this.Children = new List<SPListItem>();
            
            this.Text = title;
        }
        public SPListItem(SPListView parent)
        {

            this.parent = parent;
            this.Children = new List<SPListItem>();
        }
        public SPListItem()
        {
        }
        public bool Expanded { get; set; }

        public Skinning.Block DividerBlock { get; set; }
    }
}
