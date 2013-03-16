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
    public partial class AppHeader : UserControl
    {
        public Style Stylesheet;
        public class Control
        {
            public Control(AppHeader host, String cssClass, Rectangle Position)
            {
                this.Stylesheet = host.Stylesheet;
                this.Position = Position;
                this.CssClass = cssClass;
            }
            public void Draw(Graphics g, int x, int y)
            {
                g.DrawImage(this.Normal.BackgroundImage, new Point(x, y));
            }
            public Spider.Skinning.Style Stylesheet;
            private String cssClass;
            public String CssClass
            {
                get
                {
                    return cssClass;
                }
                set
                {
                    cssClass = value;
                    this.Normal = Stylesheet.Blocks[cssClass];
                    this.Hover = Stylesheet.Blocks[cssClass + ""];
                    this.Active = Stylesheet.Blocks[cssClass + ":active"];
                    this.Disabled = Stylesheet.Blocks[cssClass + ":disabled"];
                }
            }
            public string Title;
            public Block Normal, Hover, Active, Disabled;
            public Rectangle Position = new Rectangle();
            public event MouseEventHandler MouseClick;
            public void OnClick(MouseEventArgs e)
            {
                if (MouseClick != null)
                {
                    MouseClick(this, e);
                }
            }
        }
        public AppHeader()
        {
            InitializeComponent();
            
        }
        public Block Background;
        public List<Control> SubControls = new List<Control>();
        public AppHeader(Style stylesheet)
        {
            InitializeComponent();
            this.Stylesheet =stylesheet;
            this.Paint += AppHead_Paint;
            this.Background = Stylesheet.Blocks["header"];
            this.MouseDown += AppHead_MouseDown;
            this.MouseClick += AppHead_MouseClick;

            Control backButton = new Control(this, "backbtn", new Rectangle(7, 28, 27, 21));
            backButton.MouseClick += backButton_MouseClick;
            Control foreButton = new Control(this, "forebtn", new Rectangle(35, 28, 27, 21));
            foreButton.MouseClick += foreButton_MouseClick;
            this.SubControls.Add(backButton);
            this.SubControls.Add(foreButton);

        }

        void foreButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (NavigationClicked != null)
            {
                NavigationClicked(this, new NavigationEventArgs() { Direction = NavigationEventArgs.NavigationDirection.Forward });
            }
        }
        public class NavigationEventArgs {
            public enum NavigationDirection
            {
                Back, Forward}
            public NavigationDirection Direction;
        }
        public delegate void NavigationButtonEventHandler(object sender, NavigationEventArgs e);
        public event NavigationButtonEventHandler NavigationClicked;

        void backButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (NavigationClicked != null)
            {
                NavigationClicked(this, new NavigationEventArgs() { Direction = NavigationEventArgs.NavigationDirection.Back });
            }
        }

        void AppHead_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (Control c in this.SubControls)
            {
                if (e.X > c.Position.X && e.X < c.Position.X + c.Position.Width &&
                    e.Y > c.Position.Y && e.Y < c.Position.Y)
                {
                    c.OnClick(e);
                }
            }
        }

        void AppHead_MouseDown(object sender, MouseEventArgs e)
        {
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {

            BufferedGraphicsContext bgc = new BufferedGraphicsContext();
            BufferedGraphics bg = bgc.Allocate(e.Graphics, e.ClipRectangle);
            Draw(bg.Graphics);
            bg.Render();
            
        }
        
        public void Draw(Graphics g)
        {
            g.DrawImage(this.Background.BackgroundImage, new Rectangle(0, 0, this.Width * 21, this.Height));
            foreach (Control c in this.SubControls)
            {
                c.Draw(g, c.Position.X, c.Position.Y);
            }
        }
        private void AppHead_Paint(object sender, PaintEventArgs e)
        {
            BufferedGraphicsContext bgc = new BufferedGraphicsContext();
            BufferedGraphics bg = bgc.Allocate(e.Graphics, e.ClipRectangle);
            Draw(bg.Graphics);
            bg.Render();
        }

        private void AppHead_Load(object sender, EventArgs e)
        {

        }
    }
}
