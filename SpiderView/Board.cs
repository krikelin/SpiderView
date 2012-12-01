using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using DotLiquid;

namespace Spider
{
    public partial class Board : UserControl
    {
        public class LinkEventArgs
        {
            public Uri Link { get; set; }
        }
        public void RaiseLink(Uri uri)
        {
            if (this.LinkClicked != null)
            {
                this.LinkClicked(this, new LinkEventArgs() { Link = uri });
            }
        }
        public delegate void LinkEventHandler(object sender, LinkEventArgs e);
        public event LinkEventHandler LinkClicked;
        public SpiderView SpiderView;
        public Style Stylesheet;
        public int ScrollX { get; set; }
        public int ScrollY {get;set;}
        public Padding Padding = new Padding("0");
        public List<Element> Children = new List<Element>();
        public String template = "";

        /// <summary>
        /// Event args for scripting invokes
        /// </summary>
        public class ScriptInvokeEventArgs
        {
            /// <summary>
            /// The command to execute
            /// </summary>
            public String Command { get; set; }

            /// <summary>
            /// The element that hosted the command
            /// </summary>
            public Element Element { get; set; }

            /// <summary>
            /// The spider view that was invoked
            /// </summary>
            public SpiderView View { get; set; }

            /// <summary>
            /// The event that raised the code
            /// </summary>
            public String Event { get; set; }
        }
        /// <summary>
        /// Delegate for scripting event handlers
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Script event args</param>
        public delegate void ScriptEventHandler(object sender, ScriptInvokeEventArgs e);

        /// <summary>
        /// Occurs when a script is rised
        /// </summary>
        public event ScriptEventHandler ScriptCalled;

        /// <summary>
        /// Occurs when a script is loaded
        /// </summary>
        public event ScriptEventHandler ScriptLoaded;

        /// <summary>
        /// Invoke script message
        /// </summary>
        /// <param name="e">Script event handler</param>
        public void InvokeScript(ScriptInvokeEventArgs e)
        {
            if (ScriptCalled != null)
            {
                ScriptCalled(this, e);
            }
        }
        public Board(SpiderView spiderView)
        {
            this.SpiderView = spiderView;
            this.Stylesheet = (this.SpiderView).Stylesheet;
            this.Click += Board_Click;
            this.MouseClick += Board_MouseClick;
            InitializeComponent();

            this.Paint += Board_Paint;
            this.Resize += Board_Resize;
            
            this.ForeColor = Stylesheet.ForeColor;
            this.BackColor = Stylesheet.BackColor;
            tmrDraw = new Timer();
            tmrDraw.Tick += tmrDraw_Tick;
            tmrDraw.Interval = 100;
            tmrDraw.Start();
            this.MouseMove += Board_MouseMove;
        }

        void Board_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            foreach (Element elm in this.Children)
            {
                if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                {
                    elm.CheckClick(e.X, e.Y);
                }
            }
        }

        void Board_Click(object sender, EventArgs e)
        {
            
        }
        public Board()
        {
            InitializeComponent();
            
            this.Paint += Board_Paint;
            this.Stylesheet = SpiderView.Stylesheet;
            this.ForeColor = Stylesheet.ForeColor;
            this.BackColor = Stylesheet.BackColor;
            tmrDraw = new Timer();
            tmrDraw.Tick += tmrDraw_Tick;
            tmrDraw.Interval = 100;
            tmrDraw.Start();
            this.MouseMove += Board_MouseMove;
        }

        void Board_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            foreach (Element elm in Children)
            {
                if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                {
                    elm.CheckHover(x, y);
                }
            }
        }

        void tmrDraw_Tick(object sender, EventArgs e)
        {
           Graphics g = this.CreateGraphics();
           int x=0, y=0;
           Draw(g, ref x, ref y, new Rectangle(0, 0, Width, Height));
        }
        private Timer tmrDraw;

        void Board_Resize(object sender, EventArgs e)
        {

            

        }

        void Board_Paint(object sender, PaintEventArgs e)
        {
            int x = 0;
            int y = 0;
            Draw(e.Graphics, ref x, ref y, e.ClipRectangle);
        }
        public void AutoResize()
        {
         int max_width = 1640;
            int max_height = 0;
            foreach (Element e in this.Children)
            {
                if (e.X + e.Width > max_width)
                {
                    max_width = e.X + e.Width;
                }
                if (e.Y + e.Height > max_height)
                {
                    max_height = e.Y + e.Height;
                }
                //e.Width = this.SpiderView.Width;
                
                
            }
            this.Width = 1800;
            this.Height = max_height;
        }
        public void PackChildren()
        {
            int row = 0;
            int left = Padding.Left;
            int max_height = 0;
            foreach (Element child in this.Children)
            {
                child.Width = child.AbsoluteWidth;
                child.Height = child.AbsoluteHeight;
                if (child.Dock == Spider.Element.DockStyle.Right)
                {
                    child.Width = this.Width - Padding.Right * 2 - child.Margin.Right * 2 - child.X;
                }
               /* if (max_height > child.Height)
                    max_height = child.Height;
                if (left + child.Width > this.Width - this.Padding.Right * 2)
                {
                    
                }
                child.X = left;
                child.Y = row;*/

                child.X = 0;// this.Padding.Left;
                child.Y = row;// this.Padding.Top + row;
                left += child.Width + child.Padding.Right;
                child.PackChildren();
                row += child.Height;
            }
        }
        
        BufferedGraphicsContext BGC = new BufferedGraphicsContext();
        public void LoadNodes(XmlElement root)
        {
            foreach (XmlNode elm in root.ChildNodes)
            {
                if (elm.GetType() == typeof(XmlElement))
                {
                    try
                    {
                        Element _elm = (Element)Type.GetType("Spider." + elm.Name).GetConstructor(new Type[] { typeof(Board), typeof(XmlElement) }).Invoke(new Object[] { this, elm });
                        this.Children.Add(_elm);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            PackChildren();
        }
        private void Board_Load(object sender, EventArgs e)
        {

        }
        public void Draw(Graphics g, ref int x, ref int y, Rectangle target)
        {
            this.BackColor = Stylesheet.BackColor;
            try
            {
                BufferedGraphics bgc = BGC.Allocate(g, target);
                bgc.Graphics.FillRegion(new SolidBrush(Stylesheet.BackColor), new System.Drawing.Region(new Rectangle(0, 0, this.Width, this.Height)));
                foreach (Element elm in Children)
                {

                    x += elm.X;
                    y += elm.Y;
                    elm.Draw(bgc.Graphics, ref x, ref y);
                    x -= elm.X;
                    y -= elm.Y;

                }
                bgc.Render();
            }
            catch (System.ComponentModel.Win32Exception e)
            {

            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            int x = 0, y = 0;
            Draw(e.Graphics, ref x, ref y, e.ClipRectangle);
        }
    }
    
}
