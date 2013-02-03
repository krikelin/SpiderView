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
using System.Net;
using Newtonsoft.Json.Linq;
using LuaInterface;
using Spider.Scripting;
using Newtonsoft.Json;
using Spider.Skinning;
namespace Spider
{
    public partial class Board : UserControl
    {
        
        public SectionView Section;
        public enum Mode
        {
            Normal, Error
        }
        public Mode State = Mode.Normal;
        /// <summary>
        /// Tracklist
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
        public List<track> SelectedTracks
        {
            get
            {
                List<track> playlist = new List<track>();
                foreach (track e in this.Tracks)
                {
                    if (e.Selected)
                        playlist.Add(e);
                }

                return playlist;
            }
        }
        
        #region ScriptMethods

        /// <summary>
        /// Lua delegate for simple string methods
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public delegate string lua_delegate_get_input(String input);

        /// <summary>
        /// Get input.
        /// Gets the input from the input field cache.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public String lua_get_input(String input)
        {
            if (!this.InputFields.ContainsKey(input))
                return null;
            return this.InputFields[input].Text;
        }
        public class RequestState
        {
            public HttpWebRequest Request;

            public String URL;
            public String Callback;
        }
       
        public MemoryTable getTable(JObject obj)
        {
            MemoryTable table = new MemoryTable();
            foreach (KeyValuePair<String, Newtonsoft.Json.Linq.JToken> member in obj)
            {
                if (obj[member.Key].GetType() == typeof(String))
                {
                    table[member.Key] = obj;
                } else if(obj[member.Key].GetType() == typeof(JObject)) {
                    MemoryTable _table = getTable((JObject)obj[member.Key]);
                    table[member.Key] = member.Value;
                    continue;
                }
            }
            return table;
        }
        public delegate void lua_delegate_download_http(String url, String callback);

        /// <summary>
        /// Allow the client app code to download files
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        public void lua_download_http(String url, String callback)
        {
            WebClient wc = new WebClient();
            wc.DownloadDataCompleted += wc_DownloadDataCompleted;
            wc.DownloadDataAsync(new Uri(url), new RequestState { URL = url, Callback = callback });
        }

        void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            RequestState state = (RequestState)e.UserState;

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            String str = encoding.GetString(e.Result);
            this.SpiderView.Scripting.InvokeFunction(state.Callback, str);
        }
        #endregion
        private Dictionary<String, Element> inputFields = new Dictionary<string, Element>();
        /// <summary>
        /// Input fields
        /// </summary>
        public Dictionary<String, Element> InputFields
        {
            get
            {
                return this.inputFields;
            }
        }
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
        public List<Element> Children = new List<Element>();
        public String template = "";
        public Spider.Skinning.Block Block { get; set; }
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
        public delegate void lua_delegate_send_http_request(String url, String data, String method, String userAgent, String contentType, object headers, String callback);

        /// <summary>
        /// Send http request from lua
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="callback"></param>
        public void lua_send_http_request(String url, String data, String method,  String userAgent, String contentType, object headers, String callback)
        {
            WebRequest request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            ((HttpWebRequest)request).UserAgent = userAgent;
            request.Method = method;
            request.ContentLength = data.Length;
            request.ContentType = contentType;
            Stream stream  = request.GetRequestStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(data);
            request.ContentLength = stream.Length;
            
            IAsyncResult iar = request.BeginGetRequestStream(new AsyncCallback(this.lua_web_response), new RequestState() { Request=(HttpWebRequest)request, Callback = callback, URL = url });

        }
        public void SelectNext()
        {
            track lastTrack = null;

            for (int i = 0; i < Tracks.Count; i++)
            {
                track track = this.Tracks[i];
                if (lastTrack != null)
                {
                    track.Selected = true;
                    
                    foreach(track t in this.Tracks) {
                        if (t != track)
                        {
                            t.Selected = false;
                        }
                    }
                    if (track.Y + track.Height < this.Section.VerticalScroll.Value)
                    {
                        this.Section.VerticalScroll.Value = track.Y;
                    }
                    if (track.Y + track.Height > this.Section.VerticalScroll.Value + this.Section.Height)
                    {
                        this.Section.VerticalScroll.Value = track.Y - track.Height;
                    }
                    return;
                }
                if (track.Selected)
                {
                    lastTrack = track;

                }

            }
        }
        public void SelectPrev()
        {
            track lastTrack = null;

            for (int i = Tracks.Count-1; i >= 0; i--)
            {
                track track = this.Tracks[i];
                if (lastTrack != null)
                {
                    track.Selected = true;
                    foreach (track t in this.Tracks)
                    {
                        if (t != track)
                        {
                            t.Selected = false;
                        }
                    }
                    return;
                }
                if (track.Selected)
                {
                    lastTrack = track;

                }

            }
        }
        public void lua_web_response(IAsyncResult pass)
        {
            RequestState rs = (RequestState)pass.AsyncState;  //Récupération de l'objet etat 
            HttpWebRequest req = rs.Request;
            HttpWebResponse response = (HttpWebResponse)req.EndGetResponse(pass);

            SpiderView.Scripting.InvokeFunction(rs.Callback, new { status = response.StatusDescription, code = response.StatusCode, text = "" });

        }
        public Board(SpiderView spiderView)
        {
            this.SpiderView = spiderView;
            this.Stylesheet = (this.SpiderView).Stylesheet;
            this.Click += Board_Click;
            this.MouseUp += Board_MouseUp;
            this.MouseClick += Board_MouseClick;
            this.MinimumSize = new Size(640, 480);
            this.MouseDown += Board_MouseDown;
            InitializeComponent();
            this.DragDrop += Board_DragDrop;
            this.DragOver += Board_DragOver;
            this.Paint += Board_Paint;
            this.Resize += Board_Resize;
            this.DragEnter += Board_DragEnter;
            this.Block = Stylesheet.Blocks["Body"];
            tmrDraw = new Timer();
            tmrDraw.Tick += tmrDraw_Tick;
            tmrDraw.Interval = 100;
            tmrDraw.Start();
            this.MouseMove += Board_MouseMove;
            spiderView.Scripting.RegisterFunction("getInput", new lua_delegate_get_input(lua_get_input), this); 

            // Register some other goodies here::?
            spiderView.Scripting.RegisterFunction("getWebResource", new lua_delegate_download_http(lua_download_http), this);
            spiderView.Scripting.RegisterFunction("sendToWeb", new lua_delegate_send_http_request(lua_send_http_request), this);
            spiderView.Scripting.RegisterFunction("json", new get_obj(get_json), this);
            this.MouseDoubleClick += Board_MouseDoubleClick;
            this.KeyDown += Board_KeyDown;
            Timer t = new Timer();
            t.Tick += t_Tick;

        }

        void Board_DragOver(object sender, DragEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            foreach (Element elm in Children)
            {
                if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                {

                    this.HoveredElement = elm;
                    elm.CheckHover(x, y);
                }
                else
                {
                }
            }
        }
       
        void Board_DragDrop(object sender, DragEventArgs e)
        {

            
        }

        void Board_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
            
        }

        void Board_MouseUp(object sender, MouseEventArgs e)
        {
            dragURI = null;
            try
            {

                int x = e.X;
                int y = e.Y;
                foreach (Element elm in this.Children)
                {
                    if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                    {
                        elm.CheckMouseUp(e.X, e.Y);
                    }
                }
            }
            catch (Exception ex) { }
        }
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Right || keyData == Keys.Down || keyData == Keys.Up)
                return true;
            return base.IsInputKey(keyData);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    SelectNext();
                    break;
                case Keys.Up:
                    SelectPrev();
                    break;
            }
        }
        void t_Tick(object sender, EventArgs e)
        {
           
        }

        void Board_KeyDown(object sender, KeyEventArgs e)
        {
           
        }
        public List<Element> DragElements = new List<Element>();
        public void Board_MouseDown(object sender, MouseEventArgs e)
        {
            
            if ((Control.ModifierKeys & Keys.ControlKey) == 0)
            {




                foreach (track t in Tracks)
                {
                    try
                    {
                        if (this.HoveredElement.GetType() == typeof(track))
                            if (!((track)this.HoveredElement).Selected)
                                t.Selected = false;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                
            }
            int x = e.X;
            int y = e.Y;
            try
            {
                foreach (Element elm in this.Children)
                {
                    if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                    {
                        elm.CheckMouseDown(e.X, e.Y);
                    }
                }
            }
            catch (Exception ex) { }
            Element hoveringElement = this.HoveredElement;
            if(hoveringElement != null)
            if (hoveringElement.Hyperlink != null)
            {
                dragURI = hoveringElement.Hyperlink;
                DragStartPosition = new Point(e.X, e.Y);
            }
        }
        public Point DragStartPosition;
        public String dragURI = "";
        public int Diff(int x, int y)
        {
            return x > y ? x - y : y - x;
        }
        
        /// <summary>
        /// Convert to friendly object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public MemoryTable JSONToTable(JObject jobject)
        {
            MemoryTable mt = new MemoryTable();
            foreach (KeyValuePair<String, Newtonsoft.Json.Linq.JToken> obj in jobject)
            {
                if (obj.Value.GetType() == typeof(JObject))
                {
                    MemoryTable _mt = this.JSONToTable((JObject)obj.Value);
                    mt[obj.Key] = _mt;
                }
                else
                {
                    mt[obj.Key] = obj.Value;
                }
            }
            return mt;
        }
        /// <summary>
        /// Allows app client to covert to json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public object get_json(String json)
        {
            JObject obj = JObject.Parse(json);
            return SpiderView.Scripting.TableToNative(JSONToTable(obj));
        }
        public delegate object get_obj(String str);

       

        void Board_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            try
            {
                foreach (Element elm in this.Children)
                {
                    if ((x > elm.X && x < elm.X + elm.Width) && (y > elm.Y && y < elm.Y + elm.Height))
                    {
                        elm.CheckClick(e.X, e.Y);
                    }
                }
            }
            catch (Exception ex) { }
        }

        void Board_Click(object sender, EventArgs e)
        {
            
        }
        
        public Board()
        {
            InitializeComponent();
            
            this.Paint += Board_Paint;
            this.Stylesheet = SpiderView.Stylesheet;
            this.Block = (Spider.Skinning.Block)Stylesheet.Blocks["body"].Clone();
            this.ForeColor = Block.ForeColor;
            this.ForeColor = Block.BackColor;
            tmrDraw = new Timer();
            tmrDraw.Tick += tmrDraw_Tick;
            tmrDraw.Interval = 100;
            tmrDraw.Start();
            this.MouseMove += Board_MouseMove;
            this.MouseDoubleClick += Board_MouseDoubleClick;
        }

        void Board_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
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
        }
        private List<Element> dragElements = new List<Element>();
        public bool foundLink = false;
        void Board_MouseMove(object sender, MouseEventArgs e)
        {
            if (DragStartPosition != null)
            {
                if (dragURI != null)
                {
                    if (Diff(e.X, DragStartPosition.X) > 10 || Diff(e.Y, DragStartPosition.Y) > 10)
                    {
                     
                            
                              
                                DataObject d = new DataObject(DataFormats.StringFormat, dragURI);
                                DoDragDrop(d, DragDropEffects.All);

                                dragURI = null;
                            
                        
                       
                    }
                }
            }
            this.foundLink = false;
            int x = e.X;
            int y = e.Y;
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
            this.Cursor = foundLink ? Cursors.Hand : Cursors.Default;
            this.foundLink = false;
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
            this.PackChildren();
            

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

            if (!CustomHeight)
            {
                this.Width = 1100;
                this.Height = max_height;
            }
        }
        public bool CustomHeight = false;
        public void PackChildren(bool c =true)
        {
            int row = 0;
            int left = Padding.Left;
            int max_height = 0;
            foreach (Element child in this.Children)
            {
                
               
                child.Width = child.AbsoluteWidth;
                //child.Height = child.AbsoluteHeight;
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

                child.X = this.BoxPadding != null ? this.BoxPadding.Left : 0;
                child.Y = this.BoxPadding != null ? this.BoxPadding.Top + row : row;
                left += child.Width + child.Padding.Right;
                
                row += child.Height;

                    child.PackChildren();
               
               
            }
            
           
            
        }

        public Element HoveredElement { get; set; }
        public void assignHeight(Element element)
        {
            if (element.GetType() != typeof(vbox))
                return;
            int height = 0;
            foreach (Element elm in element.Children)
            {
               
                assignHeight(elm);
                elm.PackChildren();
                height += elm.Height;
            }
            if (height > element.Height)
            {
                element.Height = height;
            }
            element.PackChildren();
        }
        public Spider.Skinning.Padding BoxPadding { get; set; }
        BufferedGraphicsContext BGC = new BufferedGraphicsContext();
        public void LoadNodes(XmlElement root)
        {
            if (root.HasAttribute("bgcolor"))
            {
                this.Block.BackColor = ColorTranslator.FromHtml(root.GetAttribute("bgcolor"));
                this.BackColor = this.Block.BackColor;
            }
            if (root.HasAttribute("fgcolor"))
            {
                this.Block.ForeColor = ColorTranslator.FromHtml(root.GetAttribute("fgcolor"));
            }
            if (root.HasAttribute("padding"))
            {
                this.BoxPadding = new Spider.Skinning.Padding(root.GetAttribute("padding"));
            }
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
            this.CreateGraphics().PageUnit = GraphicsUnit.Pixel;
        }
        public class DrawBuffer
        {
            public int x, y;
            public Element elm;
        }
        public List<DrawBuffer> overflows = new List<DrawBuffer>();
        public void Draw(Graphics g, ref int x, ref int y, Rectangle target)
        {
            if (!this.Visible)
                return;
            this.BackColor = Block.BackColor;
            try
            {
                BufferedGraphics bgc = BGC.Allocate(g, target);
                bgc.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
                bgc.Graphics.FillRegion(new SolidBrush(this.Block.BackColor), new System.Drawing.Region(new Rectangle(0, 0, this.Width, this.Height)));
                foreach (Element elm in Children)
                {

                    x += elm.X;
                    y += elm.Y;
                    if(elm.Visible)
                        elm.Draw(bgc.Graphics, ref x, ref y);
                    elm.AbsoluteTop = y;
                    elm.AbsoluteLeft = x;
                    if (elm.GetType() == typeof(columnheader))
                    {
                        overflows.Add(new DrawBuffer() { x = x, y = y, elm = elm });
                    }
                    x -= elm.X;
                    y -= elm.Y;

                }
                foreach (DrawBuffer db in overflows)
                {
                    db.elm.Draw(bgc.Graphics, ref db.x, ref db.y); 
                }
                overflows.Clear();
                
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
