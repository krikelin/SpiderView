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


            this.OnMouseOver(x, y);
            x -= this.X;
            y -= this.Y;
            foreach (Element elm in Children)
            {
               
                if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                {
                    elm.CheckHover(x, y);
                }
            }
            x += this.X;
            y += this.Y;


        }
        public event MouseEventHandler MouseMove;
        private void OnMouseOver(int x, int y)
        {
            if (this.MouseMove != null)
                this.MouseMove(this, new MouseEventArgs(x, y));

#if(DEBUG)
         //   this.mouseOver = true;
#endif
        }
        public void CheckClick(int x, int y)
        {
            
            
            this.OnClick(x, y);
            Graphics g = this.Board.CreateGraphics();
            g.DrawRectangle(new Pen(Color.Green), this.X, this.Y, 20, 20);
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
        public String Alt { get; set; }
        public String Hyperlink { get; set; }
        int flex = 0;
        public void ApplyStyle(String Block)
        {

        }
        public Element(Board Host, XmlElement node)
        {
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
           
            if (node.HasAttribute("onclick"))
            {
               
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
            PackChildren();
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
               x -= elm.X;
               y -=elm.Y;
           }
          // g.DrawRectangle(new Pen(Color.Red), new Rectangle(X, Y, Width, Height));
#if(DEBUG)
            if(mouseOver) {
                g.DrawRectangle(new Pen(Color.Red), new Rectangle(X, Y, Width, Height));
            }
#endif
        }
        bool mouseOver = false;
        public void OnClick(int x, int y)
        {
            if (this.Click != null)
            {
                this.Click(this, new MouseEventArgs(x, y));
            }

        }
        public void MouseOver(int x, int y)
        {
            mouseOver = true;
            this.Draw(Board.CreateGraphics(), ref x, ref y);
        }
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual int AbsoluteLeft {get;set;}
        public virtual int AbsoluteTop { get; set; }
        public List<Element> Children = new List<Element>();
        public abstract void PackChildren();

    }
    public class text : Element
    {
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
        }
        private Bitmap bitmap;
        private Bitmap GenerateBitmap()
        {
            Bitmap c = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(c);
            int fontSize = (int)(((float)Block.Font.Size / 11) * 3);
            String html = "<font face=\"" + Block.Font.FontFamily.Name + "\" size=\"1\" color=\"" + ColorTranslator.ToHtml(Block.ForeColor) + "\">" + Text + "</font>";
            HtmlRenderer.Render(g, html, new Point(0, 0), this.Width);
            return c;
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="g"></param>
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            base.Draw(g, ref  x, ref y);
            if(bitmap != null)
               g.DrawImage(bitmap, new Point(x, y));
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
        private Image image;
        public img(Board host, XmlElement elm)
            : base(host, elm)
        {
            if (elm.HasAttribute("src"))
            {
                this.LoadImage(elm.GetAttribute("src"));
            }
        }
        public override void Draw(Graphics g, ref int x, ref int y)
        {
            base.Draw(g, ref x, ref y);
            if(image != null)
                g.DrawImage(image, x, y, this.Width, this.Height);
        }
        public static Dictionary<String, Image> ImageCollection = new Dictionary<string, Image>();
        public void LoadImage(String url)
        {
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
            int quote = this.Width / this.Children.Count;
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
                child.Height = this.Height - child.Margin.Bottom*2  - this.Padding.Bottom*2;
                if (child.Flex > 0)
                {
                    child.Width = flexible_width;
                    pos += flexible_width; ;
                }
                else
                {

                    pos += child.Width + child.X;
                }
               
              
                
                
                child.PackChildren();
            }
        }
    }
    public class divider : Element
    {
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
            if(count_flex > 0)
            flexible_height = (this.Height - static_height) / count_flex;

            for (int i = 0; i < this.Children.Count; i++)
            {
                Element child = this.Children[i];
                child.Y = child.Margin.Top + this.Padding.Top;
                child.Height = child.AbsoluteHeight - child.Margin.Bottom * 2 - this.Padding.Bottom * 2;
                child.Width = this.Width - child.Margin.Right * 2 - this.Padding.Right * 2;
                child.Y = pos;
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
            }
        }
    }
}
