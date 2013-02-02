using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Spider.Media;

namespace Spider
{
    /// <summary>
    /// An application inside the Spider Markup Domain
    /// </summary>
    public partial class App : UserControl
    {
        public delegate void LoadEventHandler(object sender, EventArgs e);
        public event LoadEventHandler Loaded;
        public Object Tag { get; set; }
        public SpiderHost Host { get; set; }
        public String Template { get; set; }
        public String[] Arguments;
        public App(SpiderHost host, String[] arguments)
        {
            this.Arguments = arguments;
            InitializeComponent();
            this.Host = host;
            this.spiderView = new SpiderView(host);
            //this.board =  new Board(this);
            this.Controls.Add(spiderView);
            this.spiderView.Dock = DockStyle.Fill;
            spiderView.Dock = DockStyle.Fill;
            spiderView.Navigate += spiderView_Navigate;
        }

        void spiderView_Navigate(object sender, SpiderView.NavigateEventArgs e)
        {
            Host.Navigate(e.Uri.ToString());
        }
        public void Start()
        {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync(this.Arguments);
        }
        public virtual String GetSubName()
        {
            return "";
        }

        public App()
        {
            
            InitializeComponent();
            

        }
        
        public virtual String GetName()
        {
            return "";
        }
        public virtual void LoadFinished()
        {

        }
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            spiderView.LoadFile(Template);
            spiderView.refresh(e.Result);
            if(this.Loaded != null)
                this.Loaded(this, new EventArgs());
            LoadFinished();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Loading(e.Argument);
        }

        /// <summary>
        /// What happens when the user are trying to drop on the view
        /// </summary>
        /// <param name="data"></param>
        public virtual void DropItem(IDataObject data)
        {
        }
        /// <summary>
        /// Asks for allowing drop on the current app.
        /// </summary>
        /// <param name="sObject"></param>
        public virtual bool AllowsDrop(IDataObject data)
        {
            return false;
        }
        /// <summary>
        /// On reorder
        /// </summary>
        /// <param name="oldPos"></param>
        /// <param name="count"></param>
        /// <param name="newPos"></param>
        public virtual void Reorder(int oldPos, int count, int newPos)
        {

        }
        /// <summary>
        /// Override this function to load
        /// </summary>
        /// <param name="arguments"></param>
        public virtual object Loading(object arguments)
        {
            return null;
        }
        public virtual Spider.SPListItem.ListIcon GetIcon()
        {
            return null;
        }
        /// <summary>
        /// Loads the app
        /// </summary>
        /// <param name="arguments"></param>
        public virtual void Navigate(String[] arguments)
        {
            
        }
        /// <summary>
        /// Gets the spider view
        /// </summary>
        public SpiderView Spider
        {
            get
            {
                return this.spiderView;
            }
        }
        private SpiderView spiderView;
        private void App_Load(object sender, EventArgs e)
        {
           
        }
    }
}
