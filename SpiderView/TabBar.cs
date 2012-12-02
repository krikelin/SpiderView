using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spider
{
    public partial class TabBar : UserControl
    {
        public SpiderView SpiderView;
        public Selector Selector { get; set; }
        public Selector ActiveTabSelector { get; set; }
        public TabBar(SpiderView spiderView)
        {
            InitializeComponent();
            this.SpiderView = spiderView;
            this.Selector = spiderView.Stylesheet.Selectors["TabBar"];
            this.ActiveTabSelector = spiderView.Stylesheet.Selectors["TabBar::active"];
            
            this.Resize += TabBar_Resize;
            
        }

        void TabBar_Resize(object sender, EventArgs e)
        {
            this.Draw(this.CreateGraphics());
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Draw(e.Graphics);
        }
        public List<Tab> Tabs = new List<Tab>();
        BufferedGraphicsContext bgc = new BufferedGraphicsContext();
        public Tab ActiveTab;
        public void Draw(Graphics g)
        {
            try
            {
                BufferedGraphics graphics = bgc.Allocate(g, new Rectangle(0, 0, this.Width, this.Height));
                if(Selector.BackgroundImage != null)
                  graphics.Graphics.DrawImage(Selector.BackgroundImage, 0, 0, (int)((float)this.Width * 2), this.Height);
                int x = 0;
                foreach (Tab tab in Tabs)
                {
                    Color fgColor = Color.Black;
                    if (tab == ActiveTab)
                    {
                        fgColor = this.Selector.ForeColor;
                        graphics.Graphics.FillRectangle(new SolidBrush(ActiveTabSelector.BackColor), new Rectangle(x, 0, tab.Width, this.Height));
                    }

                    graphics.Graphics.DrawString(tab.Title, new Font("MS Sans Serif", 8), new SolidBrush(ActiveTabSelector.TextShadowColor), new Point(x + 15, 5));
                    graphics.Graphics.DrawString(tab.Title, new Font("MS Sans Serif", 8), new SolidBrush(ActiveTabSelector.ForeColor), new Point(x + 15, 4));
                    x += tab.Width;
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
