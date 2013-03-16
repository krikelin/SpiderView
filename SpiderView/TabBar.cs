using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Spider.Skinning;
namespace Spider
{
    public partial class TabBar : UserControl
    {
        public SpiderView SpiderView;
        public Block Block { get; set; }
        public Block TitleBlock;
        public Block ActiveTabBlock { get; set; }
        public TabBar(SpiderView spiderView)
        {
            InitializeComponent();
            this.SpiderView = spiderView;
            this.Block = spiderView.Stylesheet.Blocks["TabBar"];
            this.ActiveTabBlock = spiderView.Stylesheet.Blocks["TabBar::active"];
            this.TitleBlock = spiderView.Stylesheet.Blocks["TabBar::title"];
            TabDivider = spiderView.Stylesheet.Blocks["tab:divider"];
            this.MouseMove += TabBar_MouseMove;
            this.Resize += TabBar_Resize;
            this.MouseClick += TabBar_MouseClick;
            
        }

        void TabBar_MouseClick(object sender, MouseEventArgs e)
        {
            if (Titles != null)
            {
                var Graphics = this.CreateGraphics();
                // Draw titles
                float strW = 0;
                // Get width of all titles
                foreach (Link l in Titles)
                {
                    strW += this.Block.Stylesheet.MeasureString(l.Title, new System.Drawing.Font("MS Sans Serif", 8.0f, FontStyle.Bold)).Width * 1.1f;

                }
                float left = this.Width - strW - 24;
                foreach (Link l in Titles)
                {
                    float xx = this.Block.Stylesheet.MeasureString(l.Title, new System.Drawing.Font("MS Sans Serif", 8.0f, FontStyle.Bold)).Width * 1.1f;
                    if (e.X > left && e.X < left + xx)
                    {
                        SpiderView.Host.Navigate(l.Uri.ToString());
                        break;
                    }
                    left += xx;

                    left += 12;

                }

               
            }
        }

        void TabBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (Titles != null)
            {
                var Graphics = this.CreateGraphics();
                // Draw titles
                float strW = 0;
                // Get width of all titles
                bool foundLink = false;
                foreach (Link l in Titles)
                {
                    strW += this.Block.Stylesheet.MeasureString(l.Title, TitleBlock.Font).Width * 1.1f;

                }
                float left = this.Width - strW - 24;
                
                foreach (Link l in Titles)
                {
                    float xx = this.Block.Stylesheet.MeasureString(l.Title, TitleBlock.Font).Width * 1.1f;
                    if (e.X > left && e.X < left + xx)
                    {
                        foundLink = true;
                    }
                    left += xx;
                    
                    left += 12;

                }
                
                this.Cursor = foundLink ? Cursors.Hand : Cursors.Default;
            }
        }

        void TabBar_Resize(object sender, EventArgs e)
        {
            this.Draw(this.CreateGraphics());
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Draw(e.Graphics);
        }
        public class Link {
            public string Title;
            public Uri Uri;
        }
        public List<Link> Titles = new List<Link>() {  };
        public List<Tab> Tabs = new List<Tab>();
        BufferedGraphicsContext bgc = new BufferedGraphicsContext();
        public Tab ActiveTab;
        public Block TabDivider;
        public void Draw(Graphics g)
        {
            try
            {
                BufferedGraphics graphics = bgc.Allocate(g, new Rectangle(0, 0, this.Width, this.Height));
                if(Block.BackgroundImage != null)
                  graphics.Graphics.DrawImage(Block.BackgroundImage, 0, 0, (int)((float)this.Width * 2), this.Height);
                int x = 0;
                foreach (Tab tab in Tabs)
                {
                    Color fgColor = Color.Black;
                    if (tab == ActiveTab)
                    {
                        fgColor = this.Block.ForeColor;
                        graphics.Graphics.DrawImageUnscaledAndClipped(ActiveTabBlock.BackgroundImage, new Rectangle(x, 0, tab.Width +2, this.Height));
                    }
                    else
                    {
                        graphics.Graphics.DrawLine(new Pen(TabDivider.ForeColor, 1f), new Point(x+1, 1), new Point(x+1, this.Height-2));

                    }
                    float strWidth = this.Block.Stylesheet.MeasureString(tab.Title, new Font("MS Sans Serif", 8)).Width;
                    float left = x + ((float)tab.Width / 2.0f) - (strWidth / 2.0f);
                    this.Block.Stylesheet.DrawString(graphics.Graphics, tab.Title, new Font("MS Sans Serif", 8), new SolidBrush(Block.TextShadowColor), new Rectangle((int)left, 3, 320, 50));
                    this.Block.Stylesheet.DrawString(graphics.Graphics, tab.Title, new Font("MS Sans Serif", 8), new SolidBrush(Block.ForeColor), new Rectangle((int)left, 4, 320, 50));
                 
                    x += tab.Width;
                    if( tab != ActiveTab)
                        graphics.Graphics.DrawLine(new Pen(TabDivider.BackColor, 1f), new Point(x, 1), new Point(x, this.Height - 2));

                }
                if (Titles != null)
                {
                    // Draw titles
                    float strW = 0;
                    // Get width of all titles
                    foreach (Link l in Titles)
                    {
                        strW += this.Block.Stylesheet.MeasureString(l.Title, TitleBlock.Font).Width * 1.1f;

                    }
                    float left = this.Width - strW - 24;
                    int i = 0;
                    foreach (Link l in Titles)
                    {
                        float xx = this.Block.Stylesheet.MeasureString(l.Title, TitleBlock.Font).Width * 1.1f;
                        this.Block.Stylesheet.DrawString(graphics.Graphics, l.Title, TitleBlock.Font, new SolidBrush(Color.Black), new Rectangle((int)left, 5, (int)xx, 18));
                        this.Block.Stylesheet.DrawString(graphics.Graphics, l.Title, TitleBlock.Font, new SolidBrush(Color.White), new Rectangle((int)left, 4, (int)xx, 18));
                        left += xx;
                        if( i < Titles.Count -1)
                        this.Block.Stylesheet.DrawString(graphics.Graphics, "»", TitleBlock.Font, new SolidBrush(TitleBlock.AlternateForeColor), new Rectangle((int)left, 4, (int)20, 18));

                        left += 12;
                        i++;
                       

                    }
                }
                graphics.Render();
            }
            catch (Exception e)
            {
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw(e.Graphics);
        }
        public TabBar()
        {
            InitializeComponent();
        }

        private void TabBar_Load(object sender, EventArgs e)
        {

        }

        private void TabBar_MouseDown(object sender, MouseEventArgs e)
        {
            int left = 0;
            foreach (Tab tab in Tabs)
            {
                if (e.X > left && e.X < left + tab.Width)
                {
                    this.ActiveTab = tab;
                    TabChangedEventArgs args = new TabChangedEventArgs();
                    args.Tab = tab;
                    if (TabChange != null)
                    {
                        TabChange(this, args);
                    }
                    this.Draw(this.CreateGraphics());
                    break;
                    
                }
                left += tab.Width;
            }
        }
        public class TabChangedEventArgs
        {
            public Tab Tab;
        }
        public delegate void TabChangeEventHandler(object sender, TabChangedEventArgs e);
        public event TabChangeEventHandler TabChange;

        internal void Clear()
        {
            this.Tabs.Clear();
            this.Draw(this.CreateGraphics());
        }
    }
    public class Tab
    {
        public String ID;
        public String Title;
        public int Width = 120;
    }
}
