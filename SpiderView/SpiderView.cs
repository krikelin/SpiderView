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
    /// <summary>
    /// Clean room implementation of Spotify spider layout engine
    /// </summary>
    public partial class SpiderView : UserControl
    {
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
        public Style Stylesheet = new Style();
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
        public SpiderView()
        {
                
            this.Scripting = new Scripting.LuaInterpreter(this);
            this.Preprocessor = new Preprocessor.LuaMako(this);
            this.Scripting.RegisterFunction("refresh", GetType().GetMethod("refresh"));

            this.timer = new Timer();
            InitializeComponent();
            this.tabBar = new TabBar(this);
            this.deck = new Panel();
            this.deck.AutoScroll = true;
            tabBar.Dock = DockStyle.Top;
            tabBar.Height = 23;
            this.Controls.Add(deck);
            this.Controls.Add(tabBar);
            deck.Dock = DockStyle.Fill;
            this.tabBar.Dock = DockStyle.Top;
            this.BackColor = Stylesheet.BackColor;
            this.tabBar.TabChange += tabBar_TabChange;
            this.timer.Tick += timer_Tick;
            this.timer.Interval = 1000;
            this.Click += SpiderView_Click;
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
            Refresh(new Object());
        }
        public void Refresh(Object obj)
        {
            
#if(false)
            Template template = Template.Parse(this.template);
            String DOM = template.Render(Hash.FromAnonymousObject(obj));
#endif
            String DOM = Preprocessor.Preprocess(this.template, obj);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(DOM);
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
            
           
        }
        public void LoadNodesAgain(XmlElement element)
        {
            var sections = element.GetElementsByTagName("section");
            foreach (XmlElement _section in sections)
            {
                SectionView section = this.Sections[_section.GetAttribute("id")];
                section.Board.Children.Clear();
                section.Board.LoadNodes(_section);
                section.Board.AutoResize();
            }
            
        }
        
        public void LoadNodes(XmlElement element)
        {
            var sections = element.GetElementsByTagName("section");
            foreach (XmlElement _section in sections)
            {
                Tab tab = new Tab();
                tab.Title = _section.GetAttribute("title");
                tab.ID = _section.GetAttribute("id");
                this.tabBar.Tabs.Add(tab);
                
                Board childBoard = new Board(this);
                SectionView sv = new SectionView(childBoard, this);
                Sections.Add(tab.ID, sv);
                childBoard.LoadNodes(_section);
                childBoard.ScriptCalled += childBoard_ScriptCalled;
                childBoard.AutoResize();
                childBoard.Width = 1280;
                if(_section.HasAttribute("padding"))
                        childBoard.Padding = new Padding(_section.GetAttribute("padding"));
                this.deck.Controls.Add(sv);
                sv.Dock = DockStyle.Fill;
                

            }
            tabBar.ActiveTab = tabBar.Tabs[0];
        }

        void childBoard_ScriptCalled(object sender, Board.ScriptInvokeEventArgs e)
        {
            Scripting.InvokeFunction(e.Command, e);
        }

        private TabBar tabBar;
        private Panel deck;
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
                //  activeBoard.Board.AutoResize();
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
