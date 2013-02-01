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
        public App()
        {
            
            InitializeComponent();
            

        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            spiderView.LoadFile(Template);
            spiderView.refresh(e.Result);
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Loading(e.Argument);
        }
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
