using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Spider.Skinning
{
    public class SpotifyStyle : Style
    {
        public Bitmap SliceBitmap(Bitmap src, Rectangle region)
        {
            Bitmap target = new Bitmap(region.Width, region.Height);
            Graphics g = Graphics.FromImage(src);
            Graphics targetGraphics = Graphics.FromImage(target);
            targetGraphics.DrawImage(src, 0, 0, region, GraphicsUnit.Pixel);
            return target;
        }
        public Bitmap LoadBitmap(String fileName)
        {
            return (Bitmap)Bitmap.FromFile(fileName);
        }
        private Dictionary<String, Block> blocks = new Dictionary<string, Block>();
        private List<Selector> ParseRules(String rules)
        {
            List<Selector> sr = new List<Selector>();
            Selector currentRule = new Selector();
            StringBuilder sb = new StringBuilder();
            foreach (char c in rules)
            {
                switch (c)
                {
                    case '!':
                    case ',':
                        sr.Add(currentRule);
                        currentRule = new Selector();
                        currentRule.True = c != '!';
                        break;
                    default:
                        currentRule.Rule += c;
                        break;
                }

            }
            return sr;
        }
        private Block GetBlock(XmlElement elm)
        {
           
            String BlockName = elm.GetAttribute("ci");
            Block sel = null;
            if (!this.Blocks.ContainsKey(BlockName))
            {
                sel = new Block();
                this.Blocks.Add(BlockName, sel);
            } 
            switch (elm.Name) {
                case "color":
                    if (elm.HasAttribute("color"))
                        sel.ForeColor = ColorTranslator.FromHtml(elm.GetAttribute("color"));
                    break;
                case "bitmap":
                
                    if (elm.HasAttribute("edge") && elm.HasAttribute("file"))
                    {
                        Edge edge = new Edge(elm.GetAttribute("edge"));
                        XmlNodeList subBitmaps = elm.GetElementsByTagName("subBitmap");
                        int i = 0;
                        Bitmap bitmap = LoadBitmap(elm.GetAttribute("file"));
                        foreach (XmlElement subBitmap in subBitmaps)
                        {
                            
                            String BlockName2 = subBitmap.GetAttribute("ci") + (subBitmap.HasAttribute("Block") ?  "|" + subBitmap.GetAttribute("Block") : "");
                            Block sel2 = null;
                            if (!this.Blocks.ContainsKey(BlockName2))
                            {
                                sel2 = new Block();
                                this.Blocks.Add(BlockName2, sel2);
                            }
                            else
                            {
                                sel2 = this.Blocks[BlockName2];
                            }
                            Rectangle edge2 = new Rectangle(edge.Left, edge.Top, edge.Right, edge.Bottom);
                            if(subBitmap.HasAttribute("width")) {
                                int width = int.Parse(subBitmap.GetAttribute("width"));
                                edge2 = new Rectangle(i * width, edge.Top, width, bitmap.Height);
                            }
                            else if (subBitmap.HasAttribute("height"))
                            {
                                int height = int.Parse(subBitmap.GetAttribute("height"));
                                edge2 = new Rectangle(0, edge.Top + i *height, bitmap.Width, height);
                            }
                            sel2.BackgroundImage = SliceBitmap(bitmap, edge2);
                        }

                    }
                    
                    break;
                case "font":
                    sel.Font = new Font(elm.GetAttribute("face"), int.Parse(elm.GetAttribute("size")));
                    sel.ForeColor = ColorTranslator.FromHtml(elm.GetAttribute("color"));
                    sel.TextShadowColor = ColorTranslator.FromHtml(elm.HasAttribute("shadow_up") ? elm.GetAttribute("shadow_up") : elm.HasAttribute("shadow_down") ? elm.GetAttribute("shadow_down") : elm.HasAttribute("shadow") ? elm.GetAttribute("shadow") : "");
                    break;


            }
            return sel;
        }
        
        public SpotifyStyle(String data) : base()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(data);
            
            // Get all colors
            foreach (XmlElement color in xmlDoc.GetElementsByTagName("color"))
            {
                GetBlock(color);
            }
            foreach (XmlElement color in xmlDoc.GetElementsByTagName("font"))
            {
                GetBlock(color);
            }
            foreach (XmlElement color in xmlDoc.GetElementsByTagName("bitmap"))
            {
                GetBlock(color);
            }
        }

        public Dictionary<string, Block> Blocks
        {
            get { return blocks; }
        }


        public void DrawString(Graphics g, string text, Font font, SolidBrush brush, Rectangle pos)
        {
            throw new NotImplementedException();
        }


        public Size MeasureString(string text, Font font)
        {
            throw new NotImplementedException();
        }
    }
}
