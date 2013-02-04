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
using Spider.Skinning;
namespace Spider
{
    /// <summary>
    /// Clean room implementation of Spotify spider layout engine
    /// </summary>
    public partial class SpiderView : UserControl
    {
        public SpiderHost Host { get; set; }
        public Interpreter Scripting;
        public Preprocessor.Preprocessor Preprocessor;
        /// <summary>
        /// Delegator that deelgates update of view
        /// </summary>
        /// <param name="sender">The sender of the object</param>
        /// <returns></returns>
        public delegate Object UpdateView(object sender);

        /// <summary>
        /// Raised when the view has expired and is renewing
        /// </summary>
        public event UpdateView ViewUpdating;
        public SectionView activeBoard;
        public Dictionary<String, SectionView> Sections = new Dictionary<string, SectionView>();
        public PixelStyle Stylesheet = new PixelStyle();
        public int RefreshRate
        {
            get
            {
                return this.timer.Interval;
            }
            set
            {
                this.timer.Interval = value;
            }
        }
        public void refresh(Object obj)
        {
            this.Refresh(obj);
        }
        private System.Windows.Forms.Timer timer;
        public Spider.Skinning.Block Block;
        public SpiderView(SpiderHost host)
        {
            this.Host = host;
            this.Scripting = new Scripting.LuaInterpreter(this);
            this.Preprocessor = new Preprocessor.LuaMako(this);
            this.Scripting.RegisterFunction("refresh", GetType().GetMethod("refresh"), this);
            
            this.timer = new Timer();
            InitializeComponent();
            this.tabBar = new TabBar(this);
            this.deck = new Panel();
            tabBar.Dock = DockStyle.Top;
            tabBar.Height = 23;
            this.Controls.Add(deck);
            this.Controls.Add(tabBar);
            deck.Dock = DockStyle.Fill;
            this.tabBar.Dock = DockStyle.Top;
            Block = Stylesheet.Blocks["Body"];
            this.BackColor = Block.BackColor;
            this.ForeColor = Block.ForeColor;
            this.tabBar.TabChange += tabBar_TabChange;
            this.timer.Tick += timer_Tick;
            this.timer.Interval = 1000;
            this.Click += SpiderView_Click;
        }

        void SpiderView_Scroll(object sender, ScrollEventArgs e)
        {
            throw new NotImplementedException();
        }

        void deck_Scroll(object sender, ScrollEventArgs e)
        {
            
            this.Invalidate();
    
        }

        void SpiderView_Click(object sender, EventArgs e)
        {
            
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (this.ViewUpdating != null)
            {
                this.Refresh(this.ViewUpdating(this));
            }
        }

        void tabBar_TabChange(object sender, TabBar.TabChangedEventArgs e)
        {
            SectionView board = Sections[e.Tab.ID];
            this.deck.Controls.Clear();
            this.deck.Controls.Add(board);
            board.Show();
            board.Dock = DockStyle.Fill;
            activeBoard = board;
            
        }
        private String template;
        public void LoadFile(String fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                template = sr.ReadToEnd();
            }
            
        }
        public void Refresh(Object obj)
        {
            
#if(false)
            Template template = Template.Parse(this.template);
            String DOM = template.Render(Hash.FromAnonymousObject(obj));
#endif
#if(false)
            if (obj == null)
                return;
#endif
            
            String DOM = Preprocessor.Preprocess(this.template, obj);
            XmlDocument xmlDoc = new XmlDocument();
            if (DOM == "NONCHANGE" || String.IsNullOrEmpty(DOM))
                return;
            try
            {
                xmlDoc.LoadXml(DOM);
            } catch (Exception e) 
            {
                using (StreamReader sr = new StreamReader("views/error.xml"))
                {
                    String markup = sr.ReadToEnd();
                    markup = markup.Replace("${error}", e.Message);
                    xmlDoc.LoadXml(markup);
                }
            }

            XmlNodeList scripts = xmlDoc.GetElementsByTagName("script");
            foreach (XmlElement elmScript in scripts)
            {
                if (elmScript.HasAttribute("type"))
                {
                    if (elmScript.GetAttribute("type") == "text/lua")
                    {
                        if (elmScript.HasAttribute("src"))
                            Scripting.LoadFile(elmScript.GetAttribute("src"));
                        else
                            Scripting.LoadScript(elmScript.InnerText);

                    }
                }
            }
            if (this.Sections.Count > 0)
            {
                this.LoadNodesAgain(xmlDoc.DocumentElement);
            }
            else
            {
                this.LoadNodes(xmlDoc.DocumentElement);
            }
            this.tabBar.Refresh();
           
        }
        public void LoadNodesAgain(XmlElement element)
        {

            // Remove all gone sections
            var sections = element.GetElementsByTagName("section");
            for (int i = 0 ; i < Sections.Keys.Count; i++)
            {
                var key = Sections.Keys.ElementAt(i);
               bool found = false;
               foreach (XmlElement elm in sections)
               {
                   if (elm.GetAttribute("id") == Sections.Keys.ElementAt(i))
                       found = true;
               }
               if (!found)
               {
                   tabBar.Tabs.Remove(tabBar.Tabs.Find(tab => tab.ID == key));
                   SectionView childBoard = Sections[key];
                   this.deck.Controls.Remove(childBoard);
                   Sections.Remove(key);
               }


            }
            foreach (XmlElement _section in sections)
            {
                
                if(this.Sections.ContainsKey(_section.GetAttribute("id"))) {
                   
                    SectionView section = this.Sections[_section.GetAttribute("id")];
                    section.Board.Children.Clear();
                    section.Board.LoadNodes(_section);
                    section.Board.AutoResize();
                } else{
                    AddSection(_section);
                }
            }
            
        }
        public bool IsPlaylist { get; set; }
        public void AddSection(XmlElement _section) {
            Tab tab = new Tab();
                tab.Title = _section.GetAttribute("title");
                tab.ID = _section.GetAttribute("id");
                this.tabBar.Tabs.Add(tab);
                
                Board childBoard = new Board(this);
                SectionView sv = new SectionView(childBoard, this);
                childBoard.Section = sv;
                Sections.Add(tab.ID, sv);
                childBoard.LoadNodes(_section);
                childBoard.ScriptCalled += childBoard_ScriptCalled;
                childBoard.AutoResize();
                childBoard.Width = 1280;
                //if(_section.HasAttribute("padding"))
                //        childBoard.Padding = new Spider.Padding(_section.GetAttribute("padding"));
                this.deck.Controls.Add(sv);
                sv.Dock = DockStyle.Fill;
                if (_section.HasAttribute("playlist"))
                {
                    sv.Board.CustomHeight = true;
                    this.IsPlaylist = _section.GetAttribute("playlist") == "true";
                }
                if (this.IsPlaylist)
                {
                    sv.Board.MinimumSize = new Size(0, 0);
                    if (_section.HasAttribute("height"))
                    {
                        if (int.Parse(_section.GetAttribute("height")) < 1)
                        {
                            sv.Board.Hide();
                            sv.Board.Height = 0;
                        }
                        sv.Board.Height = int.Parse(_section.GetAttribute("height"));
                    }
                    else
                    {
                        sv.Board.Height = 120;
                    }
                    sv.Board.Left = 0;
                    sv.Board.Top = 0;
                    sv.Board.Width = this.Width;
                    sv.Board.Anchor |= AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    sv.ListView = new CListView(sv);
                    sv.Controls.Add(sv.ListView);
                    sv.ListView.Top = sv.Board.Height;

                    sv.ListView.Anchor |= AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    sv.ListView.Height = 1200;
                    sv.ListView.Width = this.Width;
                    sv.ListView.Columns.Add("No.", 52);
                    sv.ListView.AllowsReoreder = false;
                    sv.ListView.Columns.Add("Title", 300);
                    sv.ListView.Columns.Add("Artist", 100);
                    sv.ListView.Columns.Add("Duration", 100);
                    sv.ListView.Columns.Add("Album", 300);
                    sv.ListView.Columns.Add("User", 100);
                    sv.ListView.Columns.Add("Time", 100);
                    sv.ListView.BringToFront();
                }
        }
        public void LoadNodes(XmlElement element)
        {
            var sections = element.GetElementsByTagName("section");
            foreach (XmlElement _section in sections)
            {

                AddSection(_section);

            }
            tabBar.ActiveTab = tabBar.Tabs[0];
        }

        void childBoard_ScriptCalled(object sender, Board.ScriptInvokeEventArgs e)
        {
            Scripting.InvokeFunction(e.Command, e);
        }
        public class NavigateEventArgs
        {
            public Uri Uri;
        }
        public delegate void NavigateEventHandler(object sender, NavigateEventArgs e);
        public event NavigateEventHandler Navigate;
        public void BeginNavigate(Uri uri)
        {
            if (Navigate != null)
                Navigate(this, new NavigateEventArgs() { Uri = uri });
        }
        private TabBar tabBar;
        private Panel deck;
        public Panel Deck
        {
            get
            {
                return this.deck;
            }
        }
        private void SpiderView_Load(object sender, EventArgs e)
        {
           
        }

        void SpiderView_SizeChanged(object sender, EventArgs e)
        {
           
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (activeBoard != null)
            {
                 activeBoard.Board.AutoResize();
            }
        }
        void SpiderView_Resize(object sender, EventArgs e)
        {
           
        }

        void SpiderView_MouseMove(object sender, MouseEventArgs e)
        {
        }
    }
    
}
