/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 2010-10-27
 * Time: 19:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;


using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;
using System.Collections.Generic;

using System.Windows.Forms;
using System.Linq;

using System.Text;
using Spider.Skinning;
using Spider.Media;
namespace Spider
{
   
    /// <summary>
    /// Description of CListView.
    /// </summary>
    public class CListView : Panel
    {
        public class CListViewItem : ListViewItem
        {
            public CListView Spawn { get; set; }
            private string Title;
            public Track Track;
            public ListView.ListViewItemCollection Children { get; set; }
            public CListViewItem()
                : base()
            {
            }
            public bool Playing
            {
                get
                {
                    if(this.Track != null)
                    return this.Track.Playing;
                    return false;
                }
            }

            public CListViewItem(string Title) : base()
            {
                // TODO: Complete member initialization
                this.Title = Title;
                this.SubItems.Add(Title);
                this.Text = "";
            }

        }
        #region Properties
       
        public SortOrder Sorting { get; set; }
        public object ListViewItemSorter { get; set; }
        public ListView.SelectedIndexCollection SelectedIndices
        {
            get
            {
                return this.Spawn.SelectedIndices;
            }
        }

        public Dictionary<String, Image> EngineImages { get; set; }
        public ListView.ListViewItemCollection Items
        {
            get
            {
                return this.Spawn.Items;
            }

        }
        public bool FullRowSelect { get; set; }
        public bool OwnerDraw = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.ColumnHeaderCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [MergableProperty(false)]
        public ListView.ColumnHeaderCollection Columns
        {
            get
            {
                return this.Spawn.Columns;
            }
        }
        public event ListViewItemSelectionChangedEventHandler SelectedItemChanged;



       
        public Color HighlightColor
        {
            get;set;
        }
        public Color HighlightText
        {
            get;set;
        }

        #endregion

        #region Events

        public event EventHandler SelectedIndexChanged;
        public event ColumnClickEventHandler ColumnClick;
        public event ItemDragEventHandler ItemDrag;

        #endregion

        #region Implementions

        public void Sort()
        {
        }
        public Rectangle GetItemRect(int Id)
        {
            return new Rectangle();
        }

        #endregion
        protected override void OnSizeChanged(EventArgs e)
        {

        }
        public class ItemsAddedArgs : EventArgs
        {
            public ItemsAddedArgs(ListViewItem item)
            {
                Item = item;
            }

            public object Item { get; set; }
        }


        public bool AlternateRows { get; set; }
        public override void ResetBackColor()
        {
            base.ResetBackColor();


            foreach (ListViewItem item in Items)
            {
                if (item.Index % 2 == 1 && AlternateRows)
                {
                    item.BackColor = AlternativeColor;
                }
            }
        }
        public override void ResetForeColor()
        {
            base.ResetBackColor();

        }
        public Color AlternativeColor
        {
            get
            {
                if (AlternateRows)
                    return AlternateBlock.BackColor; //MainForm.FadeColor(-0.025f,BackColor);
                else
                    return Block.BackColor;
            }
        }
        public int Count
        {
            get
            {
                return this.Items.Count;
            }
        }
       
        public CListViewItem AddItem(CListViewItem Item)
        {
            CListViewItem item = Item;
            if (ItemAdded != null)
                ItemAdded.Invoke(this, new ItemsAddedArgs(item));

            if (item.Index % 2 == 1)
            {
                item.BackColor = AlternativeColor;
            }
            item.Spawn = this;
            this.Items.Add(item);
           
            return item;
        }
        public List<CListViewItem> SelectedItems
        {
            get
            {
                List<CListViewItem> items = new List<CListViewItem>();
                foreach (CListView.CListViewItem item in this.Items)
                {
                    if(item.Selected)
                      items.Add(item);
                }
                return items;
            }

        }

        public int ItemHeight
        {
            get
            {
                return itemHeight;
            }
            set { itemHeight = value; }
        }
        public int itemHeight = 18;
        ListView Spawn;
        public Block Block;
        public Block SelectedBlock;
        public Block AlternateBlock;
        public SectionView Section;
        public CListView(SectionView sv)
        {
            this.DoubleBuffered = true;
            this.Section = sv;
            this.DragDrop +=CListView_DragDrop;
            this.AllowDrop = true;
            this.AllowsReoreder = true;
            this.Block = (Block)this.Section.Board.Stylesheet.Blocks["track"].Clone();
            this.HeaderBlock = (Block)this.Section.Board.Stylesheet.Blocks["columnheader"].Clone();
            this.PlayingBlock = (Block)this.Section.Board.Stylesheet.Blocks["track::playing"].Clone();
            this.BackColor = this.Block.BackColor;
            this.SelectedBlock = (Block)this.Section.Board.Stylesheet.Blocks["track::selected"].Clone();
            this.AlternateBlock = (Block)this.Section.Board.Stylesheet.Blocks["track::even"].Clone();
            InitializeComponent();
            AlternateRows = true;
            this.Paint += new PaintEventHandler(CListView_Paint);
            this.FullRowSelect = true;
           // lvwColumnSorter = new MediaChrome.ListViewColumnSorter();
            EngineImages = new Dictionary<string, Image>();
           
            this.timer1 = new Timer();
            this.timer1.Tick += new EventHandler(timer1_Tick);
            this.timer1.Interval = 150;
            this.timer1.Start();
            this.MouseUp += CListView_MouseUp;
            this.MouseWheel += new MouseEventHandler(CListView_MouseWheel);
            this.OwnerDraw = true;
            this.Scroll += new ScrollEventHandler(CListView_Scroll);
            this.VScroll = true;
        }

        void CListView_MouseUp(object sender, MouseEventArgs e)
        {
            this.mouseDown = false;
        }
        public delegate void LinkClickEventHandler(object Sender, string Query, ListViewItem Item, ColumnHeader D);
        public event LinkClickEventHandler LinkClicked;
        protected override void OnMouseClick(MouseEventArgs e)
        {
           
            base.OnMouseClick(e);
            for (int i = scrollY; i < scrollY + ContainingItems; i++)
            {
                try
                {
                    ListViewItem D = Items[i];
                    int left = 0;
                    int x = 0;
                    int top = (i + scrollY + 1) * ItemHeight;
                    foreach (ColumnHeader d in Columns)
                    {

                        if (D.SubItems[i].Text.StartsWith("$") && e.X >= left && e.X <= left + d.Width && e.Y <= top && e.Y >= top + ItemHeight)
                        {
                            if (LinkClicked != null)
                                LinkClicked(this, D.SubItems[i].Text.TrimStart('$'), D, d);
                        }

                        left += d.Width;
                        x++;
                    }
                }
                catch
                {
                }
            }
        }
        public CListViewItem GetElementUnderPosition(Point pos)
        {
            for (int i = 0; i < this.Items.Count ; i++)
            {
                try
                {
                    CListViewItem D = (CListViewItem)Items[i];
                    
                    int top = (i + 1) * ItemHeight;
                    if (pos.Y > top && pos.Y < top + itemHeight)
                    {
                        return D;
                    }
                }
                catch
                {
                }
            }
            return null;
        }
        Point cursor = new Point();
        public int Diff(int x, int y) {
            return x > y ? x -y : y-x;
        }
        public List<CListViewItem> SelectedTracks
        {
            get
            {
                List<CListViewItem> items = new List<CListViewItem>();
                foreach (CListViewItem item in this.Items)
                {
                    if (item.Selected)
                        items.Add(item);
                }
                return items;
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {

            base.OnMouseMove(e);
            Cursor = Cursors.Default;
            if(mouseDown)
            if ((Diff(e.X, cursor.X) > 10 || Diff(e.Y, cursor.Y) > 10) && this.AllowsReoreder)
            {
                HoveredElement = GetElementUnderPosition(new Point(e.X, e.Y));
                StringBuilder sb = new StringBuilder();
                this.DragElements.Clear();
                foreach (CListViewItem i in SelectedTracks)
                {
                    sb.Append(i.Track.Uri);
                    DragElements.Add(i);
                }
                DataObject d = new DataObject(System.Windows.Forms.DataFormats.Text, sb);
                
                DoDragDrop(d, DragDropEffects.Copy);
            }
            else
            {
            }
           /* for (int i = scrollY; i < scrollY + ContainingItems; i++)
            {
                try
                {
                    ListViewItem D = Items[i];
                    int left = 0;
                    int x = 0;
                    int top = (i + scrollY + 1) * ItemHeight;
                    foreach (ColumnHeader d in Columns)
                    {

                        if (D.SubItems[i].Text.StartsWith("$") && e.X >= left && e.X <= left + d.Width && e.Y <= top && e.Y >= top + ItemHeight)
                        {
                            Cursor = Cursors.Hand;
                        }

                        left += d.Width;
                        x++;
                    }
                }
                catch
                {
                }
            }*/
        }
        protected override void OnScroll(ScrollEventArgs se)
        {

        }
        void CListView_Scroll(object sender, ScrollEventArgs se)
        {

            if (se.NewValue < se.OldValue)
            {
                if (ScrollY >= 0)
                {

                    ScrollY -= se.OldValue - se.NewValue;
                }
                else
                {
                    ScrollY = 0;
                }
            }
            if (se.NewValue > se.OldValue)
            {
                if (!EndOfBrowse)
                    ScrollY += se.NewValue - se.OldValue;
            }
        }
        public bool EndOfBrowse
        {
            get
            {
                if (Count - scrollY < ContainingItems)
                    return true;
                else
                    return false;
            }
        }
        public bool Overflow
        {
            get
            {
                return Count > ContainingItems;
            }
        }
        public int ContainingItems
        {
            get
            {
                int x= (int)Math.Floor((double)(this.Height / itemHeight));
                return (Count > x ? x : Count) -1;
            }
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta >= 120)
            {
                if (ScrollY >= 0)
                {
                    ScrollY -= 15;
                }
                else
                {
                    ScrollY = 0;
                }
            }
            if (e.Delta <= -120)
            {
                if (!EndOfBrowse)
                    ScrollY += 15;
                else
                    scrollY = Count - ContainingItems;
            }
            if (EndOfBrowse)
            {
                scrollY = Count - ContainingItems;
                if (ScrollEnded != null)
                    ScrollEnded(this, new EventArgs());
            }
            if (scrollY < 0)
            {
                scrollY = 0;
            }
        }
        void CListView_MouseWheel(object sender, MouseEventArgs e)
        {

        }
        public event EventHandler ScrollEnded;

        int scrollY = 0;
        public int ScrollY
        {
            get
            {
                return scrollY;
            }
            set
            {
                scrollY = value;
            }
        }
        public Block PlayingBlock;
        void DrawCore(Graphics g)
        {
            int ColumnCount = Columns.Count;
           
                int index = 0;
                g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(0, 0, this.Width, this.Height));
                for (int i = 1; i < this.Height / ItemHeight; i++)
                {
                    if (i % 2 == 1)
                    {
                        g.FillRectangle(new SolidBrush(AlternativeColor), new Rectangle(0, i * ItemHeight, this.Width, ItemHeight));

                    }
                }
                int Containing = ContainingItems;
                int CI = (Containing > Count - scrollY ? Containing : Count);
                for (int i = scrollY; i < scrollY + CI   ; i++)
                {
                    int top = (i - scrollY + 1) * ItemHeight;
                    if (i > Count)
                        continue;
                    CListViewItem Item = (CListViewItem)Items[i];
                   


                    if (Item.Selected)
                    {
                        g.FillRectangle(new SolidBrush(SelectedBlock.BackColor), new Rectangle(0, top, this.Width, ItemHeight));

                    }
                    if (Item.Playing)
                    {
                        g.FillRectangle(new SolidBrush(PlayingBlock.BackColor), new Rectangle(0, top, this.Width, ItemHeight));
                        
                    }


                    int left = 0;
                    for (var x = 0; x < ColumnCount ; x++)
                    {

                        var d = Columns[x];
                        Color FG = Item.Selected && !Item.Playing ? SelectedBlock.ForeColor : Item.Playing ? PlayingBlock.ForeColor : Block.ForeColor;
                        try
                        {
                            if (x < Item.SubItems.Count)
                            {
                                String P = Item.SubItems[x].Text;
                                float w = g.MeasureString(P, this.Font).Width;
                                if ((int)w > d.Width)
                                {
                                    StringBuilder R = new StringBuilder();

                                    foreach (Char c in P)
                                    {
                                        R.Append(c);
                                        float r  = TextRenderer.MeasureText(R.ToString(), this.Font).Width;
                                        if ((int)r > d.Width)
                                        {
                                            R.Remove(R.Length - 4, 1);
                                            P = R.ToString() + "...";
                                            break;
                                        }

                                    }
                                }
                                P = P.TrimStart('$');
                                this.Block.Stylesheet.DrawString(g, P, this.Font, new SolidBrush(FG), new Rectangle(2 + left, top + GetItemPosition(ItemHeight, 16), 300, 20));
                                left += d.Width;
                            }
                           
                        }
                        catch
                        { }
                    }
                    if (UniDraw != null)
                        UniDraw(this, g, Item);
                }
                int leftx = 0;
                g.DrawImage(HeaderBlock.BackgroundImage, new Rectangle(0, 0, this.Width, ItemHeight));
                for (var x = 0; x < ColumnCount; x++)
                {
                    var d = Columns[x];

                    g.DrawString(d.Text, HeaderBlock.Font, new SolidBrush(HeaderBlock.TextShadowColor), new Point(leftx + 2, GetItemPosition(ItemHeight, 18) + 2));
                    g.DrawString(d.Text, HeaderBlock.Font, new SolidBrush(HeaderBlock.ForeColor), new Point(leftx + 2, GetItemPosition(ItemHeight, 18) + 1));
                    leftx += d.Width;
                    // g.DrawImage(MediaChrome.Resources.columnlimiter, new Rectangle(leftx - 2, 0, 2, ItemHeight));

                }
            
           

        }
        public Block HeaderBlock;
        public int GetItemPosition(int height, int size)
        {
            int d = (int)((float)height * (((float)height - (float)size) / (float)height));
            return d / 2;
        }
        void CListView_Paint(object sender, PaintEventArgs e)
        {
            BufferedGraphics D = (new BufferedGraphicsContext()).Allocate(e.Graphics, e.ClipRectangle);
            DrawCore(D.Graphics);
            D.Render();

        }
        void CListView_DrawItem(Object sender, DrawListViewItemEventArgs e)
        {
            try
            {
                /*              BufferedGraphics D = (new BufferedGraphicsContext()).Allocate(e.Graphics, e.Item.Bounds);

                              Color F = e.Item.Selected ? HighlightColor : ((e.ItemIndex % 2 == 1) ? MainForm.ListBackground() : MainForm.AlternateRowColor());

                              D.Graphics.FillRectangle(new SolidBrush(F), e.Item.Bounds);
                              D.Render();*/
            }
            catch
            {
            }
        }
        void CListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        void CListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {

            try
            {

                BufferedGraphics D = null;
                if (e.ColumnIndex == 0)
                {
                    D = (new BufferedGraphicsContext()).Allocate(e.Graphics, new Rectangle(e.SubItem.Bounds.Left, e.SubItem.Bounds.Top, e.Item.ListView.Columns[0].Width, e.Bounds.Height));
                }
                else
                {
                    D = (new BufferedGraphicsContext()).Allocate(e.Graphics, e.SubItem.Bounds);
                }
                Color ForeColor = e.Item.Selected ? HighlightText : e.Item.ForeColor;
                if (ForeColor == e.Item.BackColor)
                {
                    //       MessageBox.Show("D");
                }
                Color BackC = e.Item.Selected ? HighlightColor : ((e.ItemIndex % 2 == 1) ? BackColor : AlternativeColor);
                if (e.SubItem.Text == "" && e.ColumnIndex == 2)
                {
                    //                   MessageBox.Show("Something difficult");
                }
                if (e.ColumnIndex == 0)
                {
                    D.Graphics.FillRectangle(new SolidBrush(BackC), e.SubItem.Bounds.Left, e.SubItem.Bounds.Top, e.Item.ListView.Columns[0].Width, e.Bounds.Height);
                }
                else
                {
                    D.Graphics.FillRectangle(new SolidBrush(BackC), e.SubItem.Bounds);
                }
                D.Graphics.DrawString(e.Item.SubItems[e.ColumnIndex].Text, e.SubItem.Font, new SolidBrush(ForeColor), e.SubItem.Bounds.Left + 1, e.SubItem.Bounds.Top + 1);
                if (UniDraw != null)
                    this.UniDraw(this, D.Graphics, e.Item);
                D.Render();
            }
            catch
            {
            }

        }

        private Timer timer1;


       // private MediaChrome.ListViewColumnSorter lvwColumnSorter;
        /*	protected override void OnColumnClick(ColumnClickEventArgs e)
            {
                /// <summary>
                /// From http://support.microsoft.com/kb/319401
                /// </summary>
                        // Determine if clicked column is already the column that is being sorted.
                if ( e.Column == lvwColumnSorter.SortColumn )
                {
                    // Reverse the current sort direction for this column.
                    if (lvwColumnSorter.Order == SortOrder.Ascending)
                    {
                        lvwColumnSorter.Order = SortOrder.Descending;
                        this.Sorting= SortOrder.Descending;
                    }
                    else
                    {
                        lvwColumnSorter.Order = SortOrder.Ascending;
                        this.Sorting= SortOrder.Ascending;
                    }
				
                }
                else
                {
                    // Set the column number that is to be sorted; default to ascending.
                    lvwColumnSorter.SortColumn = e.Column;
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
			
                // Perform the sort with these new sort options.
                this.Sort();
                foreach(ListViewItem X in this.Items)
                {
                    X.BackColor = (X.Index) % 2 == 1 && MainForm.alternating ? MainForm.AlternateRowColor() : MainForm.ListBackground();
				
                }
            }*/
        public EventHandler<ItemsAddedArgs> ItemAdded;

        private void InitializeComponent()
        {
            if (this.Spawn == null)
                this.Spawn = new ListView();
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();

            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // CListView
            // 
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CListView_MouseDown);
            
            this.ResumeLayout(false);
            this.MouseDoubleClick += CListView_MouseDoubleClick;
            this.DragOver += CListView_DragOver;
            
        }
        public event ReorderEventHandler ItemsDeleting;
        public event ReorderEventHandler ItemsDeleted;
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char)Keys.Delete)
            {
                ReorderEventArgs args = new ReorderEventArgs();
                List<int> indexes = new List<int>();
                foreach (CListViewItem item in this.SelectedItems)
                {
                    indexes.Add(this.Items.IndexOf(item));
                }
                args.Indexes = indexes.ToArray();
                if (ItemsDeleting != null)
                {
                    ItemsDeleting(this, args);

                }
                if (!args.Cancel)
                {
                    foreach (int i in indexes)
                    {
                        this.Items.RemoveAt(i);
                    }
                    if (ItemsDeleted != null)
                    {
                        ItemsDeleted(this, args);

                    }
                }

            } base.OnKeyPress(e);
        }
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Right || keyData == Keys.Down || keyData == Keys.Up ||keyData == Keys.Delete)
                return true;
            return base.IsInputKey(keyData);
        }
        
        CListViewItem HoveredElement;
        void CListView_DragOver(object sender, DragEventArgs e)
        {
            HoveredElement = GetElementUnderPosition(this.PointToClient(new Point(e.X, e.Y)));
            BufferedGraphicsContext g = new BufferedGraphicsContext();
            BufferedGraphics i = g.Allocate(this.CreateGraphics(), new Rectangle(0, 0, this.Width, this.Height));
            this.DrawCore(i.Graphics);
            
            i.Graphics.DrawLine(new Pen(Color.White), new Point(0, ItemHeight * (this.Items.IndexOf(HoveredElement) + 1)), new Point(this.Width, ItemHeight * (this.Items.IndexOf(HoveredElement) + 1)));
            i.Render();
           
                   
            if (this.AllowsReoreder || true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        public class ReorderEventArgs
        {
            public int Position { get; set; }
            public int NewPosition { get; set; }
            public List<Spider.CListView.CListViewItem> Elements { get; set; }
            public Spider.CListView.CListViewItem Context { get; set; }
            public int[] Indexes { get; set; }
            public bool Cancel { get; set; }

        }
        public delegate void ReorderEventHandler(object sender, ReorderEventArgs e);
        public event ReorderEventHandler BeforeReorder;
        public event ReorderEventHandler Reordered;
        public List<CListViewItem> DragElements = new List<CListViewItem>();
        void CListView_DragDrop(object sender, DragEventArgs e)
        {
            
            HoveredElement = GetElementUnderPosition(this.PointToClient(new Point(e.X, e.Y)));
            this.mouseDown = false;
            if (DragElements.Count > 0)
            {
                if (HoveredElement != null && AllowsReoreder)
                {

                    CListViewItem context = DragElements[0];
                    CListViewItem targetSite = HoveredElement;
                    List<int> indexes = new List<int>();
                    ReorderEventArgs args = new ReorderEventArgs();
                    args.Elements = DragElements;
                    args.Position = this.Items.IndexOf(DragElements[0]);
                    args.NewPosition = this.Items.IndexOf(HoveredElement);
                    if (BeforeReorder != null)
                        BeforeReorder(this, args);
                    if (BeforeReorder == null || !args.Cancel)
                    {


                        // Reorder the elements in reality
                        foreach (CListViewItem elm in DragElements)
                        {
                            indexes.Add(this.Items.IndexOf(elm));
                            this.Items.Remove(elm);
                        }
                        try
                        {
                            for (int i = 0; i < DragElements.Count; i++)
                            {
                                CListViewItem element = DragElements[i];
                                this.Items.Insert(i + args.NewPosition, element);
                            }
                        }
                        catch (Exception ex)
                        {
                            for (int i = 0; i < DragElements.Count; i++)
                            {
                                CListViewItem element = DragElements[i];
                                this.Items.Insert(i + args.Position, element);
                            }
                        }
                        args.Indexes = indexes.ToArray();
                        if (Reordered != null)
                            Reordered(this, args);
                    }
                }
                this.Refresh();

            }
            this.Parent.Refresh();
        }

        void CListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            for (int i = ScrollY; i < Items.Count; i++)
            {
                CListViewItem Item = (CListViewItem)Items[i];
                int y = (i - scrollY + 1) * ItemHeight;
                if (e.Y >= y && e.Y <= y + ItemHeight)
                {
                    if (Item.Track != null)
                    {
                        Item.Track.Play();
                        
                    }
                }

            }
        }

        private System.ComponentModel.IContainer components;
        public int[] GetColumnByName(string Name)
        {
            int ex = 0;
            int ey = 0;
            foreach (ColumnHeader Df in this.Columns)
            {
                if (Df.Name == Name)
                {
                    return new int[] { ex, Df.Width };
                }
                ex += Df.Width;
            }
            return new int[] { -1, -1 };
        }
        public int[] GetColumnByName(int index)
        {
            int ex = 0;
            int ey = 0;
            foreach (ColumnHeader Df in this.Columns)
            {
                if (Df.Index == index)
                {
                    return new int[] { ex, Df.Width };
                }
                ex += Df.Width;
            }
            return new int[] { -1, -1 };
        }
        public delegate void DrawItemX(object sender, Graphics g, ListViewItem Item);
        public event DrawItemX UniDraw;
        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        /*[DllImport("user32")]
        public int GetScrollInfo(IntPtr hWnd, int n, SCROLLINFO lpScrollInfo);
        public struct SCROLLINFO
        {
            int cbSize;c
            int fMask;
            int nMin;
            int nMax;
            int nPage;
            int nPos;
            int nTrackPos;
        }*/
        public void DrawImage(Graphics r, ListViewItem D, Image Graphics, String Column, bool right , int iheight )
        {
            
            Color BG = BackColor;
            if ((scrollY % 2 == 0 ||   (D.Index - scrollY) % 2 == 0))
            {
                BG = AlternativeColor;
            }
            if (D.Selected)
            {
                BG = HighlightColor;
            }

            //   int height = 10;
            Rectangle Ds = GetItemRect(D.Index);
          //  Song p = (Song)D.Tag;
            /*   int width = GetColumnByName(Column)[1];
               if (width > Graphics.Width)
               {
                   width = Graphics.Width * (height / Graphics.Height);
               }*/
            float scale = iheight > 0 ?  (float)iheight / (float)Graphics.Height : 1;
            int width = (int)((float)Graphics.Width * scale);
            int height =  (int)((float)Graphics.Height * scale) ;
            int top = (D.Index - scrollY + 1) * ItemHeight + ((ItemHeight - height ) / 2);
            int left = right ? GetColumnByName(Column)[0] + GetColumnByName(Column)[1] - width : GetColumnByName(Column)[0];
            BufferedGraphicsContext Dss = new BufferedGraphicsContext();
            BufferedGraphics R = Dss.Allocate(r, new Rectangle(left, top, width, height));
            var d = R.Graphics;

            d.FillRectangle(new SolidBrush(BG), left, top, width, height);
            d.DrawImage(Graphics, left, top, width, height);
            R.Render();
        }
        public new Color BackColor
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
        public void DrawProgressBar(Graphics d, ListViewItem D, float val, String Column)
        {
            int top = (D.Index - scrollY) * ItemHeight;

            int width = 0;
            int height = 10;
            Rectangle Ds = GetItemRect(D.Index);

            // if (val > 0)
            {
                int left = GetColumnByName(Column)[0];
                width = GetColumnByName(Column)[1];
                Color BG = this.Block.BackColor;
                Color FG = this.Block.ForeColor;

                if (D.Selected)
                {
                    BG = this.SelectedBlock.BackColor;
                    FG = this.SelectedBlock.ForeColor;
                }
                //         d.FillRectangle(new SolidBrush(BG), Ds);
                d.FillRectangle(/*new LinearGradientBrush(new Point(0, 0), new Point(100,0), BG, FG)*/new SolidBrush(FG), new Rectangle(left + 2, top + 6, (int)((val) * width) - 2, 5));
                d.DrawRectangle(new Pen(FG), new Rectangle(left, top + 4, width, 8));
                for (int i = 0; i < width / 3; i++)
                {
                    //     d.DrawLine(new Pen(FG,2), left + i * 3,Ds.Top + 2, left + i * 3, Ds.Top+height-4);
                }

            }
        }
        public bool mouseDown = false;
        private void CListView_MouseDown(object sender, MouseEventArgs e)
        {
            cursor = new Point(e.X, e.Y);
            mouseDown = true;
            try
            {
                if((Control.ModifierKeys & Keys.Control) != Keys.Control)
                foreach (ListViewItem d in Items)
                {
                    d.Selected = false;
                }
                for (int i = ScrollY; i < Items.Count; i++)
                {
                    CListViewItem Item = (CListViewItem)Items[i];
                    int y = (i - scrollY + 1) * ItemHeight;
                    if (e.Y >= y && e.Y <= y + ItemHeight)
                    {
                        Item.Selected = true;
                        this.Refresh();
                    }

                }
            }
            catch
            {
            }
        }


        public bool AllowsReoreder { get; set; }
    }
    /*
     public class TrackListView :CListView
     {
         public MultiMedia.ListViewColumnSorter Sorter;
         public TrackListView()
         {
             Sorter = new MultiMedia.ListViewColumnSorter();
             this.ListViewItemSorter = Sorter;
         }
         public void DefaultSort()
         {
             Sorter.Order = SortOrder.Ascending;
             Sorter.SortColumn=1;
             this.Sorting= SortOrder.Ascending;
			
             Sorter.SortColumn=2;
             this.Sort();
             Sorter.SortColumn=3;
             this.Sort();
         }
     }*/
}
