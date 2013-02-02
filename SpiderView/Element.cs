using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
using System.Net;
using System.IO;
using System.Drawing.Html;
using Spider.Skinning;
using BungaSpotify09.Models;
using System.Drawing.Drawing2D;
using Spider.Media;
using System.ComponentModel;


namespace Spider
{
    
    /// <summary>
    /// Section
    /// </summary>
    public class Section
    {
        public String Title { get; set; }
        public String Name { get; set; }
        public Section(String title, String name)
        {
            this.Title = title;
            this.Name = name;
        }
        public List<Element> Children = new List<Element>();
    }
   
    /// <summary>
    /// Element
    /// </summary>
    public abstract class Element
    {
        public class DragEventArgs
        {
            public Element elm;
            public String Uri;
        }
        public delegate void DragEventHandler(object sender, DragEventArgs e);
        public event DragEventHandler BeginDrag;
        public event DragEventHandler Drop;
        public String Uri;
        private bool visible = true;
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                this.visible = value;
                this.Board.PackChildren();
            }
        }
        /// <summary>
        /// Get tracks inside
        /// </summary>
        public List<track> Tracks
        {
            get
            {
                List<track> playlist = new List<track>();
                foreach (Element e in this.Children)
                {
                    if (e.GetType() == typeof(track))
                    {
                        playlist.Add((track)e);
                    }
                    foreach (track track in e.Tracks)
                    {
                        playlist.Add(track);
                    }
                }
                return playlist;
            }
        }
        public Block Block { get; set; }
        public virtual String Text { get; set; }
        public Color BackColor
        {
            get
            {
                return this.Block.BackColor;
            }
            set
            {
                this.Block.BackColor = value;
            }
        }
        public Color ForeColor
        {
            get
            {
                return this.Block.ForeColor;
            }
            set
            {
                this.Block.ForeColor = value;
            }
        }
        public Margin Margin
        {
            get
            {
                return this.Block.Margin;
            }
        }
        public Spider.Skinning.Padding Padding
        {
            get
            {
                return this.Block.Padding;
            }
        }
        public int Flex = 0;
        public enum DockStyle {
            Left, Top, Bottom, Right
        }
        public DockStyle Dock;
        public void CheckHover(int x, int y)
        {

            Board.HoveredElement = this;
            this.OnMouseOver(x, y);
            x -= this.X;
            y -= this.Y;
            foreach (Element elm in Children)
            {

                if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                {
                    elm.CheckHover(x, y);
                }
                else
                {
                }
            }
            x += this.X;
            y += this.Y;

            
        }
        public event MouseEventHandler MouseMove;
        public virtual void OnMouseOver(int x, int y)
        {
            this.Board.HoveredElement = this;
            if (!String.IsNullOrEmpty(this.Hyperlink) && this.GetType() != typeof(track))
            {
                this.Board.foundLink = true;
            }
            if (this.MouseMove != null)
                this.MouseMove(this, new MouseEventArgs(x, y));

            x -= this.X;
            y -= this.Y;
            foreach (Element elm in this.Children)
            {
                if ((x > elm.AbsoluteLeft && x < elm.AbsoluteTop + elm.Width) && (y > elm.AbsoluteTop && y < elm.AbsoluteTop + elm.Height))
                {
                    elm.OnMouseOver(x, y);
                }
            }
            x += this.X;
            y += this.Y;
#if(DEBUG)
         //   this.mouseOver = true;
#endif
            mouseOver = true;
      //      this.Draw(Board.CreateGraphics(), ref x, ref y);
        }
        public virtual void OnMouseDown(int x, int y)
        {
            if (!String.IsNullOrEmpty(this.Hyperlink))
            {
                this.Board.foundLink = true;
                if (BeginDrag != null)
                {
                    BeginDrag(this, new DragEventArgs() { Uri = this.Hyperlink, elm = this });
                }
            }
            if (this.MouseDown != null)
                this.MouseDown(this, new MouseEventArgs(x, y));
            x -= this.X;
            y -= this.Y;
            foreach (Element elm in this.Children)
            {
                if ((x > elm.X && x < elm.Y + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                {
                    elm.OnMouseDown(x, y);
                }
            }
            x += this.X;
            y += this.Y;
#if(DEBUG)
               this.mouseOver = true;
#endif
            mouseOver = true;
            //      this.Draw(Board.CreateGraphics(), ref x, ref y);
        }
        public void CheckClick(int x, int y)
        {
            
            if(this.Hyperlink != null) {
               this.Board.SpiderView.BeginNavigate(new Uri(this.Hyperlink));
            }
            this.OnClick(x, y);
            Graphics g = this.Board.CreateGraphics();
          //  g.DrawRectangle(new Pen(Color.Green), this.X, this.Y, 20, 20);
            this.OnClick(x, y);

            if (this.ElementEventHandlers.ContainsKey("click"))
            {
                this.Board.InvokeScript(new Board.ScriptInvokeEventArgs() { Command = this.ElementEventHandlers["click"], Element = this, Event = "click", View = this.Board.SpiderView });

            }

            x -= this.X;
            y -= this.Y;
            foreach (Element elm in Children)
            {

                if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                {
                    elm.CheckClick(x, y);
                }
                else
                {

                }
            }
            x += this.X;
            y += this.Y;

                
        }
        public void CheckMouseDown(int x, int y)
        {


            this.OnMouseDown(x, y);
            Graphics g = this.Board.CreateGraphics();
            g.DrawRectangle(new Pen(Color.Green), this.X, this.Y, 20, 20);
            this.OnClick(x, y);

            if (this.ElementEventHandlers.ContainsKey("onmousedown"))
            {
                this.Board.InvokeScript(new Board.ScriptInvokeEventArgs() { Command = this.ElementEventHandlers["mousedown"], Element = this, Event = "mousedown", View = this.Board.SpiderView });

            }

            x -= this.X;
            y -= this.Y;
            foreach (Element elm in Children)
            {

                if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                {
                    elm.CheckMouseDown(x, y);
                }
                else
                {

                }
            }
            x += this.X;
            y += this.Y;


        }
       
        private Color ParseColorAttribute(String propertyName, String attribute, XmlElement elm)
        {
            if (elm.HasAttribute(attribute))
            {
                return ParseColor(elm.GetAttribute(attribute));
            }
            else
            {
                Type type = Block.GetType();
                MemberInfo member = type.GetMember(propertyName)[0];
                if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    PropertyInfo property = (PropertyInfo)member;
                    return (Color)property.GetValue(Stylesheet);
                }
            }
            if (propertyName == "ForeColor")
                return Block.ForeColor;
            else
                return Color.Transparent;
        }
        private Color ParseColor(String value)
        {
            if (value.StartsWith("@"))
            {
                Type type = Stylesheet.GetType();
                MemberInfo member = type.GetMember(value.Replace("@", "") + "Color")[0];
                if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    PropertyInfo property = (PropertyInfo)member;
                    return (Color)property.GetValue(Stylesheet);
                }
            }
            else if (value.StartsWith("#"))
            {
                
                
                  return ColorTranslator.FromHtml(value);
                
            }
            return this.Block.BackColor;
        }
        public int AbsoluteWidth = 0, AbsoluteHeight = 0;
        public String Name { get; set; }
        public Style Stylesheet = new PixelStyle();
        public Dictionary<String, String> ElementEventHandlers = new Dictionary<string, string>();
        private XmlNode node;
        public int MinHeight { get; set; }
        public int MinWidth { get; set; }
        public String Alt { get; set; }
        public String Hyperlink { get; set; }
        int flex = 0;
        public void ApplyStyle(String Block)
        {

        }
        public Element(Board Host, XmlElement node)
        {
            if (node.HasAttribute("uri"))
            {
                this.Uri = node.GetAttribute("uri");
            }
            this.Board = Host;
            this.node = node;
            this.Block = (Block)this.Board.Stylesheet.Blocks["Body"].Clone();
            this.BackColor = ParseColorAttribute("BackColor", ("bgcolor"), node);

            this.ForeColor = ParseColorAttribute("ForeColor", "color", node);
            foreach(XmlAttribute attribute in node.Attributes) 
            {
                if(attribute.Name.StartsWith("on")) {
                    this.ElementEventHandlers.Add(attribute.Name.Substring(2), node.GetAttribute(attribute.Name));
                }

            }

            if (node.HasAttribute("fgcolor"))
            {
                this.ForeColor = ColorTranslator.FromHtml(node.GetAttribute("fgcolor"));
                
            }
            if (node.HasAttribute("bgcolor"))
            {
                this.BackColor = ColorTranslator.FromHtml(node.GetAttribute("bgcolor"));
            }
            if (node.HasAttribute("onclick"))
            {
               
            }
            if (node.HasAttribute("minHeight"))
            {
                this.MinHeight = int.Parse(node.GetAttribute("minHeight"));
            }
            if (node.HasAttribute("minWidth"))
            {
                this.MinWidth = int.Parse(node.GetAttribute("minWidth"));
            }
            if (node.HasAttribute("margin"))
            {
                Block.Margin = new Margin(node.GetAttribute("margin"));
            }
            if (node.HasAttribute("flex"))
            {
                this.Flex = int.Parse(node.GetAttribute("flex"));
            }
            if (node.HasAttribute("padding"))
            {
                Block.Padding = new Skinning.Padding(node.GetAttribute("padding"));
            }
            if (node.HasAttribute("uri"))
            {
                this.Hyperlink = node.GetAttribute("uri");
            }
            if (node.HasAttribute("name"))
            {
                this.Name = node.GetAttribute("name");
            }
            if (node.HasAttribute("alt"))
            {
                this.Alt = node.GetAttribute("alt");
            }
            if (node.HasAttribute("width"))
            {
                if (node.GetAttribute("width") == "100%")
                {
                    Dock |= DockStyle.Right;
                    Width = -1;
                    //Width = Parent.Width - Margin * 2 + Parent.Padding * 2;
                }
                else
                {
                    this.AbsoluteWidth = int.Parse(node.GetAttribute("width"));
                    this.Width = this.AbsoluteWidth;
                }
            }
            else
            {
                this.AbsoluteWidth = Parent != null ? Parent.Width : Board.Width;
                this.Width = this.AbsoluteWidth;
            }
            if (node.HasAttribute("height"))
            {
                if (node.GetAttribute("height") == "100%")
                {
                    if(this.Parent != null)
                        this.AbsoluteHeight = this.Parent.Height;
                    return;
                }
                this.AbsoluteHeight = int.Parse(node.GetAttribute("height"));
                this.Height = this.AbsoluteHeight;
            }
            else
            {
                this.AbsoluteHeight = 32;
                this.Height = this.AbsoluteHeight;
            }
            if (node.HasAttribute("style"))
            {
                this.Block = new Block(node.GetAttribute("style"), Block);
            }
            this.Text = node.InnerText;
            foreach (XmlNode elm in node.ChildNodes)
            {
                if (elm.GetType() == typeof(XmlElement))
                {
                    try
                    {
                        Element _elm = (Element)Type.GetType("Spider." + elm.Name).GetConstructor(new Type[] { typeof(Board), typeof(XmlElement) }).Invoke( new Object[] {this.Board, elm});
                        this.Children.Add(_elm);
                        
                        _elm.Parent=this;
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            if (this.Children.Count > 0)
            {

                PackChildren();
            }
        }
        public Board Board { get; set; }
        public Element Parent { get; set; }
        public class MouseEventArgs {
            public MouseEventArgs(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
            public int X {get;set;}
            public int Y {get;set;}
        }
        public delegate void MouseEventHandler(object sender, MouseEventArgs e);
        public event MouseEventHandler Click;
        public event MouseEventHandler MouseDown;
        public virtual void Draw(Graphics g, ref int x, ref int y)
        {
            if (this.Stylesheet == null)
            {
                this.Stylesheet = (Parent.Stylesheet != null ? Parent.Stylesheet : Board.Stylesheet);
            }
            //g.FillRectangle(new SolidBrush(BackColor), this.X - this.Margin.Left - (this.Parent != null ? this.Parent.Padding.Left : 0), this.Y - Margin.Top - (this.Parent != null ? this.Parent.Padding.Top : 0), this.Width + Margin.Right, this.Height + Margin.Bottom);
           foreach(Element elm in this.Children)
           {
                x += elm.X;
               y +=elm.Y;
                if (elm.BackColor != null)
                {
                  //  g.FillRectangle(new SolidBrush(elm.BackColor), new Rectangle(x + elm.X , y +elm.Y + this.Padding.Top, elm.Width - this.Padding.Left * 2, elm.Height - this.Padding.Top * 2));
                }
                elm.Draw(g, ref x, ref y);
                elm.AbsoluteLeft = x;
                elm.AbsoluteTop = y;
                if (elm.GetType() == typeof(columnheader))
                {
                    Board.overflows.Add(new Spider.Board.DrawBuffer() { x = x, y = y, elm = elm });
                }
               x -= elm.X;
               y -=elm.Y;
           }
          // g.DrawRectangle(new Pen(Color.Red), new Rectangle(X, Y, Width, Height));
#if(DEBUG)
            if(mouseOver) {
        //        g.DrawRectangle(new Pen(Color.Red), new Rectangle(X, Y, Width, Height));
            }
#endif
        }
        public bool mouseOver = false;
        public void OnClick(int x, int y)
        {
            if (this.Click != null)
            {
                this.Click(this, new MouseEventArgs(x, y));
            }

        }
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual int AbsoluteLeft {get;set;}
        public virtual int AbsoluteTop { get; set; }
        public List<Element> Children = new List<Element>();
        public abstract void PackChildren();
        public abstract void BeforePackChildren();


        public virtual void CheckDoubleClick(int x, int y)
        {
          

            x -= this.X;
            y -= this.Y;
            foreach (Element elm in Children)
            {

                if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                {
                    elm.CheckDoubleClick(x, y);
                }
                else
                {

                }
            }
            x += this.X;
            y += this.Y;
        }
    }
    public class link : text
    {
        public link(Board host, XmlElement node)
            : base(host, node)
        {
        }
        
    }
    public class hr : Element
    {
        public hr(Board board, XmlElement node)
            : base(board, node)
        {
            this.Block = (Block)this.Stylesheet.Blocks["hr"].Clone();
            this.Height = 50;
        }
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            base.Draw(g, ref x, ref y);
            g.DrawLine(new Pen(Block.ForeColor, 1), new Point(0, this.Height / 2 + y), new Point(this.Board.Width, y + this.Height / 2));
        }
        public override void BeforePackChildren()
        {
            
        }
        public override void PackChildren()
        {
        }
    }
    public class text : Element
    {
        public int FontSize { get; set; }
        public override void BeforePackChildren()
        {

        }
        private String textContent;
        public String Text
        {
            get
            {
                return this.textContent;
                
            }
            set
            {
                this.textContent = value;
                this.bitmap = GenerateBitmap(); 
            }
        }
        
        public text(Board host, XmlElement node)
            : base(host, node)
        {
            Text = node.InnerText;
            if (node.HasAttribute("fontSize"))
            {
                this.Block.Font = new Font(this.Block.Font.FontFamily.Name, int.Parse(node.GetAttribute("fontSize")));
            }
            if (node.HasAttribute("bold"))
            {
                this.Block.Font = new Font(this.Block.Font, this.Block.Font.Style | FontStyle.Bold);
            }
            if (node.HasAttribute("italic"))
            {
                this.Block.Font = new Font(this.Block.Font, this.Block.Font.Style | FontStyle.Italic);
            }
        }
        private Bitmap bitmap;
        HtmlPanel htmlPanel = new HtmlPanel();
         public override void OnMouseOver(int x, int y) 
        {
            base.OnMouseOver(x, y);
           
        }
        private Bitmap GenerateBitmap()
        {
            try
            {
                Bitmap c = new Bitmap(this.Width, this.Height);

                Graphics g = Graphics.FromImage(c);
                g.FillRectangle(new SolidBrush(BackColor), new Rectangle(0, 0, Width, Height));
                int fontSize = (int)(((float)Block.Font.Size / 11) * 3);
                String html = "<font face=\"" + Block.Font.FontFamily.Name + "\" size=\"1\" color=\"" + ColorTranslator.ToHtml(Block.ForeColor) + "\">" + Text + "</font>";
                InitialContainer htmlContainer = new InitialContainer(html);
                htmlPanel.HtmlContainer.Text = html;
                //HtmlRenderer.Render(g, html, new Point(0, 0), this.Width);
                if (this.Shadow)
                    g.DrawString(Text, new Font("MS Sans Serif", 11), new SolidBrush(Block.TextShadowColor), new RectangleF(0f, -1f, (float)Width, (float)Height));
                g.DrawString(Text, new Font("MS Sans Serif", 11), new SolidBrush(ForeColor), new RectangleF(0f, -0f, (float)Width, (float)Height));

                return c;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public bool Shadow { get; set; }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="g"></param>
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            base.Draw(g, ref  x, ref y);
            //if(bitmap != null)
            //   g.DrawImage(bitmap, new Point(x, y));
            g.DrawString(Text, this.Block.Font, new SolidBrush(ForeColor), new RectangleF(x, y, (float)Width, (float)Height));
            /*label.Width = this.Width;
            label.Height = this.Height;
            label.BackColor =this.BackColor;
            label.ForeColor = this.ForeColor;
            if(this.Width > 0 && this.Height > 0) {
                Bitmap bitmap = new Bitmap(this.Width, this.Height);
                label.DrawToBitmap(bitmap, new Rectangle(0, 0, this.Width, this.Height));
                g.DrawImage(bitmap, 0, 0);
             }*/
            

        }
        public void Resize(int newX, int newY)
        {
            foreach (Element child in Children)
            {
                this.Resize(newX, newY);
            }
            if(this.Children.Count > 0)
            this.PackChildren();
        }
        public void MouseOver(int x, int y)
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        public override void PackChildren()
        {
            
        }
    }
    public class html : text
    {
        public html(Board board, XmlElement node)
            : base(board, node)
        {
        }
    }
    public class control : Element
    {
        public override void BeforePackChildren()
        {

        }
        public  override void PackChildren()
        {
        }
        public Control Control { get; set; }
        public control(Board host, XmlElement node)
            : base(host, node)
        {
            
           
        }
        private Bitmap bitmap;
        private Bitmap createBitmap()
        {
            if(Control == null)
                return null;
            bitmap = new Bitmap(Control.Width, Control.Height);
            Control.DrawToBitmap(bitmap, new Rectangle(0, 0, Control.Width, Control.Height));
            return bitmap;
        }
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            base.Draw(g, ref x, ref y);
            if (bitmap == null)
                bitmap = createBitmap();
            g.DrawImage(bitmap, x, y);
        }
        public new String Text
        {
            get
            {
                return this.Control.Text;
            }
            set
            {
                this.Control.Text = value;
                bitmap = createBitmap(); // Create new bitmap
            }
        }
        public override int AbsoluteLeft
        {
            get
            {
                return base.AbsoluteLeft;
            }
            set
            {
                base.AbsoluteLeft = value;
                if (Control != null)
                    Control.Left = value;
            }
        }
        public override int AbsoluteTop
        {
            get
            {
                return base.AbsoluteTop;
            }
            set
            {
                base.AbsoluteTop = value;
                if (Control != null)
                    Control.Top = value;
            }
        }
        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                if (value > 100)
                    value = 100;
                base.Width = value;
                if(base.Width > 0)
                if (Control != null)
                    Control.Width = value;
            }
        }
        public override int Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                if (base.Height > 0)
                base.Height = value;
                if(Control != null)
                    Control.Height = value;
            }
        }
    }
    public class columnheader : Element
    {
       
        public Dictionary<String, ColumnHeader> ColumnHeaders;
        public playlist Playlist { get; set; }
        public columnheader(Board host, XmlElement node)
            : base(host, node)
        {
            this.Block = (Block)this.Stylesheet.Blocks["columnheader"].Clone();
            this.ColumnHeaders = new Dictionary<string, ColumnHeader>();
            this.ColumnHeaders.Add("no", new ColumnHeader() { Name="", Left = 2, Width = 30 });
            this.ColumnHeaders.Add("name", new ColumnHeader() { Name="Title", Left = 30, Width = 140 });
            this.ColumnHeaders.Add("artist", new ColumnHeader() { Name="Artist", Left = 584, Width = 100 });
            this.ColumnHeaders.Add("duration", new ColumnHeader() { Name = "Duration", Left = 482, Width = 50 });
            this.ColumnHeaders.Add("popularity", new ColumnHeader() { Name = "Popularity", Left = 517, Width = 35 });
            this.ColumnHeaders.Add("album", new ColumnHeader() { Name = "Album", Left = 1025, Width = 310 });

        }
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            base.Draw(g, ref x, ref y);
            int nX = x;
            int nY = y - this.Board.Section.VerticalScroll.Value;
            if (nY < 0)
            {
                nY = this.Board.Section.VerticalScroll.Value;
            }
            else
            {
                nY = y;
            }
            g.DrawImage(this.Block.BackgroundImage, new Rectangle(nX, nY, this.Width, this.Block.BackgroundImage.Height));
          //  g.FillRectangle(new SolidBrush(Color.FromArgb(255, 166, 166, 166)), new Rectangle(nX, nY, this.Width, this.Height));
          // g.DrawString("Columnheaders is coming", Block.Font, new SolidBrush(Color.Black), new Point(x + 2, y +2));

            foreach (ColumnHeader ch in this.ColumnHeaders.Values)
            {
                g.DrawString(ch.Name, this.Block.Font, new SolidBrush(this.Block.ForeColor), new Point(nX + 20 + ch.Left, nY + 2));
            }
            
        }
        public override void PackChildren()
        {
            
        }
        public override void BeforePackChildren()
        {
            
        }

    }
    public class playlist : Element
    {
        public bool AllowsReoreder { get; set; }
        public Playlist Playlist { get; set; }
        public columnheader ColumnHeader;
        public bool HasHeaders {get;set;}
        private int trackHeight = 20;
        public override void PackChildren()
        {
            try
            {
                int i = 0;

                foreach (Element track in this.Children)
                {
                    if (!track.Visible)
                    {
                        i++;
                        continue;
                    }
                    if (track.GetType() == typeof(columnheader))
                    {
                        track.Height = trackHeight;
                        track.Width = this.Width == -1 ? this.Parent.Width : this.Width;
                        this.ColumnHeader = (columnheader)track;
                        this.ColumnHeader.Playlist = this;
                        track.X = 0;
                        track.Y = i * trackHeight;
                    }
                    if (track.GetType() == typeof(track))
                    {
                        //  if (track.GetType() != typeof(track) |)
                        //    throw new Exception("Invalid type");
                        track.Height = trackHeight;
                        track.Width = this.Width == -1 ? this.Parent.Width : this.Width;
                        ((track)track).Playlist = this;
                        ((track)track).Parent = this;
                        track.X = 0;
                        track.Y = i * trackHeight;
                        if (i % 2 == 0)
                        {
                            track.Block = this.Stylesheet.Blocks["track::even"];
                        }
                    }
                    if (track.GetType() == typeof(text))
                    {
                        track.Height = trackHeight;
                        track.Width = this.Width == -1 ? this.Parent.Width : this.Width;

                        track.X = 0;
                        track.Y = i * trackHeight;
                    }
                    i++;
                }
                if (i * trackHeight > this.Height)
                    this.Height = i * trackHeight + 25;
            }
            catch (Exception e)
            {
            }
        }
        public playlist(Board host, XmlElement node) : base (host, node)
        {
            if(node.HasAttribute("allowsReorder")) {
                this.AllowsReoreder = node.GetAttribute("allowsReorder") == "true";
            }
            

        }

        public override void BeforePackChildren()
        {
           
        }
    }
    public class ColumnHeader
    {
        public String Name;
        public int Left;
        public int Width;
        public int Index;
    }
    public class track : Element
    {
       
        public columnheader columnHeader;
        public override void BeforePackChildren()
        {

        }
        public override void CheckDoubleClick(int x, int y)
        {
            base.CheckDoubleClick(x, y);
            if (this.Track != null)
            {
                this.Track.Play();
                this.Board.SpiderView.Host.PlayContext = this.Board;
                this.Board.Invalidate(new Rectangle(X, Y, Width, Height));
            }
        }
        public playlist Playlist {get;set;}
        public Track Track { get; set; }
        public bool Selected { get; set; }
        public Block SelectedBlock;
        private XmlElement node;
        public track(Board host, XmlElement node)
            : base(host, node)
        {
            this.Visible = false;
            this.node = node;

            IMusicService ms = host.SpiderView.Host.MusicService;
            this.Visible = false;
            this.Track = new Track(ms, node.GetAttribute("uri").Split(':')[2]);
            this.Track.TrackLoaded += Track_TrackLoaded;
            this.Track.Element = this;
            this.Track.LoadAsync(this);
            
            this.Track.Name = node.GetAttribute("name");
           


            
            this.Block = (Block)this.Board.Stylesheet.Blocks["track"].Clone();
            this.Block.Font = new Font("MS Sans Serif", 11, FontStyle.Regular, GraphicsUnit.Pixel);
            this.SelectedBlock = (Block)this.Board.Stylesheet.Blocks["track::selected"].Clone();

        }

        void Track_TrackLoaded(object sender, Track.TrackLoadEventArgs e)
        {
            this.Visible = true;
            this.Board.Invalidate(new Rectangle(this.X, this.Y, this.Width, this.Height));
        }
        public override void OnMouseDown(int x, int y)
        {
            base.OnMouseDown(x, y);
            // Deselect all previous tracks
            for (int i = 0; i < Board.Tracks.Count; i++)
            {
                Board.Tracks[i].Selected = false;
            }
            this.Selected = true;
            Board.Invalidate(new Region(new Rectangle(this.X, this.Y, this.Width, this.Height)));
        }
        public override void OnMouseOver(int x, int y)
        {
            base.OnMouseOver(x, y);
        }
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            base.Draw(g, ref x, ref y);
            Color fgColor = this.Block.ForeColor;
            Color bgColor = this.Block.BackColor;
            switch (Track.Status)
            {
                case Spider.Media.Resource.State.BadKarma:
                    fgColor = Color.DarkGoldenrod;
                    break;
                case Spider.Media.Resource.State.Removed:
                case Spider.Media.Resource.State.NotAvailable:
                    fgColor = Color.FromArgb(255, Color.FromArgb(100,50,50));
                    break;
            }
            if (Track.Playing)
            {
                bgColor = Color.Black;
                fgColor = Color.LightGreen;
            }
            if (Selected)
            {
                bgColor = this.SelectedBlock.BackColor;
                fgColor = this.SelectedBlock.ForeColor;
            }
            
            if(columnHeader == null)
                columnHeader =  this.Playlist != null && this.Playlist.ColumnHeader != null ? this.Playlist.ColumnHeader : new columnheader(this.Board, node);
            
            g.FillRectangle(new SolidBrush(bgColor), new Rectangle(x, y, this.Width, this.Height));
          //  g.DrawLine(new Pen(new SolidBrush(this.Block.AlternateBackColor)), new Point(x, y + this.Height - 1), new Point(this.Width + x, this.Height - 1 + y));
            if (Track.Loaded)
            {
                //

                g.DrawString(Track.Name, this.Block.Font, new SolidBrush(fgColor), new Point(15 + x + columnHeader.ColumnHeaders["name"].Left, 1 + y));
                if(Track.Artists != null && Track.Artists.Length > 0)
                    g.DrawString(Track.Artists[0].Name, this.Block.Font, new SolidBrush(fgColor), new Point(x + columnHeader.ColumnHeaders["artist"].Left, 1 + y));
                if(Track.Album != null)
                g.DrawString(Track.Album.Name, this.Block.Font, new SolidBrush(fgColor), new Point(x + columnHeader.ColumnHeaders["album"].Left, 1 + y));
                if (columnHeader.ColumnHeaders.ContainsKey("popularity"))
                {
                    ColumnHeader cp = columnHeader.ColumnHeaders["popularity"];
                    float popularity = Track.Popularity; // Change soon
                    for (int i = 0; i < cp.Width; i+= 3)
                    {
                        Color tagColor = Color.White;
                        float progress =  ((float)popularity) / 100f;
                        if (((float)i / (float)cp.Width) > progress)
                        {
                            tagColor = Color.Gray;
                        }
                        g.FillRectangle(new SolidBrush(tagColor), new Rectangle(x + cp.Left + i, y +6, 2, 8));
                    }
                }
            }
        }

        public override void PackChildren()
        {
            
        }
    }
    public class input : control 
    {
        public input(Board host, XmlElement node)
            : base(host, node)
        {
            if (node.GetAttribute("type") == "text")
            {
                base.Control = new TextBox();
                base.Control.Tag = this;
                host.Controls.Add(Control);
                var id = node.GetAttribute("name");
                if (host.InputFields.ContainsKey(id))
                {
                    host.InputFields.Remove(id);
                }
                host.InputFields.Add(id, this);
            }
        }
    }
    public class button : control
    {
        public button(Board host, XmlElement node)
            : base(host, node)
        {
         
                base.Control = new Button();
                base.Control.Tag = this;
                host.Controls.Add(Control);
                base.Control.Text = node.InnerText;
                base.Control.MouseClick += Control_MouseClick;
                host.InputFields.Add(node.GetAttribute("name"), this);
            
        }

        void Control_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Element elm = (Element)((Control)sender).Tag;
            elm.OnClick(e.X, e.Y);

        }
    }
    public class img : Element
    {
        public override void BeforePackChildren()
        {

        }
        private Image image;
        public img(Board host, XmlElement elm)
            : base(host, elm)
        {
            if (elm.HasAttribute("src"))
            {
                this.LoadImage(elm.GetAttribute("src"));
            }
            
        }
        
        private bool hasShadow = true;
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            Rectangle Bounds = new Rectangle(x, y, this.Width, this.Height);
            base.Draw(g, ref x, ref y);
            int shadowOffset = 8;
            if (hasShadow)
            {
                // the shadow chunk
                int sQuad = 4;

                // the offset
                int sOffset = 4;

                // the shadow layer
                Bitmap shadow = Properties.Resources.shadow;
                // draw the left top corner
                g.DrawImage(shadow, new Rectangle(Bounds.Left - sQuad, Bounds.Top - sQuad, sQuad, sQuad), new Rectangle(0, 0, sQuad, sQuad), GraphicsUnit.Pixel);

                // draw the right top corner
                g.DrawImage(shadow, new Rectangle(Bounds.Left + Bounds.Width, Bounds.Top - sQuad, sQuad, sQuad), new Rectangle(shadow.Width - sQuad, 0, sQuad, sQuad), GraphicsUnit.Pixel);

                // size of vertical sides
                Size verticalSize = new Size(sQuad, Bounds.Height + shadowOffset - sQuad * 2);

                // size of horizontal sides
                Size horizontalSize = new Size(Bounds.Width + shadowOffset - sQuad * 2, sQuad);

                // draw the bottom left corner
                g.DrawImage(shadow, new Rectangle(Bounds.Left - sQuad, Bounds.Top + Bounds.Height, sQuad, sQuad), new Rectangle(0, shadow.Height - sQuad, sQuad, sQuad), GraphicsUnit.Pixel);

                // draw the bottom right corner
                g.DrawImage(shadow, new Rectangle(Bounds.Left + Bounds.Width, Bounds.Top + Bounds.Height, sQuad, sQuad), new Rectangle(shadow.Width - sOffset, shadow.Height - sQuad, sQuad, sQuad), GraphicsUnit.Pixel);


                // fill the left side
                g.DrawImage(shadow, new Rectangle(new Point(Bounds.Left - sQuad, Bounds.Top), verticalSize), new Rectangle(0, sQuad, sQuad, sQuad), GraphicsUnit.Pixel);

                // fill the right side
                g.DrawImage(shadow, new Rectangle(new Point(Bounds.Width + Bounds.Left, Bounds.Top), verticalSize), new Rectangle(shadow.Height - sQuad, sQuad, sQuad, sQuad), GraphicsUnit.Pixel);
                //  g.DrawImage(shadow,new Rectangle(Bounds.Left-shadowOffset,Bounds.Top-shadowOffset,Bounds.Width+shadowOffset+2,Bounds.Height+shadowOffset+2));

                // fill the top side

                g.DrawImage(shadow, new Rectangle(new Point(Bounds.Left, Bounds.Top - sQuad), horizontalSize), new Rectangle(sQuad, 0, sQuad, sQuad), GraphicsUnit.Pixel);

                // fill the bottom side

                g.DrawImage(shadow, new Rectangle(new Point(Bounds.Left, Bounds.Top + Bounds.Height), horizontalSize), new Rectangle(sQuad, shadow.Height - sQuad, sQuad, sQuad), GraphicsUnit.Pixel);

            }
            if(image != null)
                g.DrawImage(image, x, y, this.Width, this.Height);
        }
        public static Dictionary<String, Image> ImageCollection = new Dictionary<string, Image>();
        public void LoadImage(String url)
        {
            if (String.IsNullOrEmpty(url) || !(url.StartsWith("http://") || url.StartsWith("https://")))
                return;
            WebClient wc = new WebClient();
            if (!ImageCollection.ContainsKey(url))
            {

                wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                wc.DownloadDataAsync(new Uri(url), url);
            }
            else
            {
                this.image = ImageCollection[url];
            }
        }
        public override void PackChildren()
        {
            
        }
       
        void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            Image image = Image.FromStream(new MemoryStream(e.Result, false));
            this.image = image;
            ImageCollection[(String)e.UserState] = image;

#if(false)
            this.Width = this.image.Width;
            this.Height = this.image.Height;
#endif
        }
    }
    /// <summary>
    /// HBox
    /// </summary>
    public class hbox : Element
    {
        public override void BeforePackChildren()
        {

        }
        public hbox(Board host, XmlElement node)
            : base(host, node)
        {
            
        }
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            int spacingLeft = this.Parent != null ? this.Parent.Margin.Left : this.Board.Padding.Left;
            int spacingTop = this.Parent != null ? this.Parent.Margin.Top : this.Board.Padding.Top;
            int spacingBottom = this.Parent != null ? this.Parent.Margin.Bottom * 2 :  this.Board.Padding.Bottom*2;
            int spacingRight = this.Parent != null ? this.Parent.Margin.Right * 2 : this.Board.Padding.Right * 2;

            g.FillRectangle(new SolidBrush(BackColor), x + spacingLeft, y + spacingTop, Width - spacingRight, Height - spacingBottom);
         //   g.FillRectangle(new SolidBrush(BackColor), x, y, this.Width,this.Height);
            base.Draw(g, ref x, ref y);
        }

      

        public override void PackChildren()
        {
            int pos = 0;
            int quote = 0;
            if(this.Width > 0 && this.Children.Count > 0)
            {
                quote = this.Width / this.Children.Count;
            }
             int count_flex= 0;
            int flexible_width = 0;
            int static_width = 0;
            foreach (Element elm in this.Children)
            {
                if (elm.Flex > 0)
                {
                    count_flex += 1;
                    
                }
                else
                {
                    static_width += elm.Width;
                }
            }
            if(count_flex > 0)
            flexible_width = (this.Width - static_width) / count_flex;

            for (int i = 0; i < this.Children.Count; i++)
            {
                Element child = this.Children[i];
                child.X = child.Margin.Left + this.Padding.Left +  child.Margin.Left + pos;
                child.Y = child.Margin.Top  + this.Padding.Top + child.Margin.Top ;
                child.Width = child.AbsoluteWidth - this.Padding.Right*2 - child.Padding.Right*2;
                if (child.Width ==  -1)
                    child.Width = this.Width;
                
                child.Height = this.Height - child.Margin.Bottom*2  - this.Padding.Bottom*2;
                if (child.AbsoluteHeight > 0)
                {
                    child.Height = child.AbsoluteHeight - child.Margin.Bottom * 2 - this.Padding.Bottom;
                }
                if (child.Flex > 0)
                {
                    child.Width = flexible_width;
                    pos += flexible_width; ;
                }
                else
                {

                    pos += child.Width + child.X;
                }
               
              
                
                if(child.Children.Count > 0)
                    child.PackChildren();

              
            }
            int maxHeight = 0;
            foreach (Element c in Children)
            {
                if (c.Height > maxHeight)
                    maxHeight = c.Height;
            }
            if (maxHeight > this.Height)
            {
                this.Height = maxHeight + 0;
            }
            if (this.Height < MinHeight && MinHeight != 0)
            {
                this.Height = MinHeight;
            }
        }
    }
    public class menu : text
    {
        public Block HoverBlock;
        public Block NormalBlock;
        public menu(Board parent, XmlElement node)
            : base(parent, node)
        {

        }
       
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            if(mouseOver)
                g.FillRectangle(new SolidBrush(Color.DarkGray), new Rectangle(x, y, Width, Height));

                 
            base.Draw(g, ref x, ref y);
        }
    }
    public class divider : Element
    {
        public override void BeforePackChildren()
        {

        }
        private Image imgDivider;
        public divider(Board parent, XmlElement node)
            : base(parent, node)
        {
            this.Block = parent.Stylesheet.Blocks["Divider"];
            imgDivider = this.Block.BackgroundImage;
        }
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            g.DrawImage(imgDivider, new Rectangle(x, y, (int)((float)this.Width * 1.5), this.Height));
            g.DrawString(this.Text, Block.Font, new SolidBrush(Block.ForeColor), new Point(x, y));
            base.Draw(g, ref x, ref y);

        }

        public override void PackChildren()
        {
        }
    }

    /// <summary>
    /// Flowbox
    /// </summary>
    public class flow : Element
    {
        public override void BeforePackChildren()
        {

        }
        public flow(Board parent, XmlElement node)
            : base(parent, node)
        {
        }
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            base.Draw(g, ref x, ref y);
            
            
        }
        public override void PackChildren()
        {
            int row = 0;
            int left = Padding.Left;
            int max_height = 0;
            foreach (Element child in this.Children)
            {
                if (child.Dock == Spider.Element.DockStyle.Right)
                {
                    child.Width = this.Width - this.Padding.Right * 2 - child.Margin.Right * 2 - child.X;
                }
                if (max_height > child.Height)
                    max_height = child.Height;
                if (left + child.Width > this.Width - this.Padding.Right * 2)
                {
                    row += max_height + this.Padding.Bottom *2 + child.Margin.Bottom;
                }
                child.X = left;
                child.Y = row;
                left += child.Width + this.Padding.Left + child.Margin.Right;
                child.PackChildren();
            }
        }
    }
    /// <summary>
    /// VBox element
    /// </summary>
    public class vbox : Element
    {
        public override void BeforePackChildren()
        {

        }
        public vbox(Board parent, XmlElement node)
            : base(parent, node)
        {
        }
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            int spacingLeft = this.Parent != null ? this.Parent.Margin.Left : this.Board.Padding.Left;
            int spacingTop = this.Parent != null ? this.Parent.Margin.Top : this.Board.Padding.Top;
            int spacingBottom = this.Parent != null ? this.Parent.Margin.Bottom * 2 : this.Board.Padding.Bottom * 2;
            int spacingRight = this.Parent != null ? this.Parent.Margin.Right * 2 : this.Board.Padding.Right * 2;

            g.FillRectangle(new SolidBrush(BackColor), x + spacingLeft, y + spacingTop, Width - spacingRight, Height - spacingBottom);
            //   g.FillRectangle(new SolidBrush(BackColor), x, y, this.Width,this.Height);
            base.Draw(g, ref x, ref y);
        }


        public override void PackChildren()
        {
            int pos = 0;
            int quote = this.Width / this.Children.Count;
            int count_flex = 0;
            int flexible_height = 0;
            int static_height = 0;
            foreach (Element elm in this.Children)
            {
                if (elm.Flex > 0)
                {
                    count_flex += 1;

                }
                else
                {
                    static_height += elm.Height;
                }
            }
            if (count_flex > 0)
                flexible_height = (this.Height - static_height) / count_flex;

            for (int i = 0; i < this.Children.Count; i++)
            {
                Element child = this.Children[i];
                

                child.Y = child.Margin.Top + this.Padding.Top + pos;
                //child.Height = child.AbsoluteHeight - child.Margin.Bottom * 2 - this.Padding.Bottom * 2;
                child.Width = this.Width - child.Margin.Right * 2 - this.Padding.Right * 2;

                if (child.Flex > 0)
                {
                    child.Height = flexible_height;
                    pos += flexible_height; ;
                }
                else
                {

                    pos += child.Height + child.Y;
                }


                child.PackChildren();


                int height = 0;

            }
            if (pos > this.Height )
            {
                this.Height = pos + 50;

            }
            if (this.Height < MinHeight && MinHeight != 0)
            {
                this.Height = MinHeight;
            }
        }
    }
    /// <summary>
    /// VBox element
    /// </summary>
    public class box : Element
    {
        public override void BeforePackChildren()
        {

        }
        public box(Board parent, XmlElement node)
            : base(parent, node)
        {
        }
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            int spacingLeft = this.Parent != null ? this.Parent.Margin.Left : this.Board.Padding.Left;
            int spacingTop = this.Parent != null ? this.Parent.Margin.Top : this.Board.Padding.Top;
            int spacingBottom = this.Parent != null ? this.Parent.Margin.Bottom * 2 : this.Board.Padding.Bottom * 2;
            int spacingRight = this.Parent != null ? this.Parent.Margin.Right * 2 : this.Board.Padding.Right * 2;

            g.FillRectangle(new SolidBrush(BackColor), x + spacingLeft, y + spacingTop, Width - spacingRight, Height - spacingBottom);
            //   g.FillRectangle(new SolidBrush(BackColor), x, y, this.Width,this.Height);
            base.Draw(g, ref x, ref y);
        }


        public override void PackChildren()
        {
            int pos = 0;
            int quote = this.Width / this.Children.Count;
            int count_flex = 0;
            int flexible_height = 0;
            int static_height = 0;
            foreach (Element elm in this.Children)
            {
                if (elm.Flex > 0)
                {
                    count_flex += 1;

                }
                else
                {
                    static_height += elm.Height;
                }
            }
            if (count_flex > 0)
                flexible_height = (this.Height - static_height) / count_flex;

            for (int i = 0; i < this.Children.Count; i++)
            {
                Element child = this.Children[i];


                child.Y = child.Margin.Top + this.Padding.Top + pos;
                
                //child.Height = child.AbsoluteHeight - child.Margin.Bottom * 2 - this.Padding.Bottom * 2;
                child.Width = this.Width - child.Margin.Right * 2 - this.Padding.Right * 2;

                if (child.Flex > 0)
                {
                    child.Height = flexible_height;
                    pos += flexible_height; ;
                }
                else
                {

                    pos += child.Height;
                }

                child.PackChildren();


                int height = 0;

            }
            if (pos > this.Height)
            {
                this.Height = pos;

            }
            if (this.Height < MinHeight && MinHeight != 0)
            {
                this.Height = MinHeight;
            }
        }
    }
}
