using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaInterface;
namespace Spider
{
    public class Preprocessor
    {
        public static String Text;
        public static void print(String p)
        {
            Text += p.Replace('¤', '\n');
            
        }
        private static Lua Lua;
        public Preprocessor()
        {
            Lua = new Lua();
            Lua.RegisterFunction("print", this, this.GetType().GetMethod("print"));
        }
        public String Preprocess(String template, String data, Object obj)
        {
            int left = 0;
            StringBuilder output = new StringBuilder();
            while (left < template.Length)
            {
                try
                {
                    String code = "";
                    String fr = "";
                    int c = template.IndexOf("{%", left);
                    if (c < 0)
                    {
                        String markup = template.Substring(left, template.Length - left);
                         output.Append("printx('" + markup.Replace('\n', '¤') + "')");
                        break;
                    }
                    var distance = (c > -1 ? c : template.Length);
                    int e = template.IndexOf("%}", c);
                    code = template.Substring(c + 2, e - c);
                    int nextClash = template.IndexOf("{%", c);
                    if (nextClash < 0)
                        nextClash = template.Length;
                    fr = template.Substring(e + 2, Diff(nextClash, e)-2);
                    output.Append("printx('" + fr.Replace('\n', '¤') + "');" + code);
                    left = e;
                }
                catch (Exception e)
                {
                    break;
                }

            }
            Lua[data] = obj;
            String code2 = output.ToString();
            Lua.DoString(code2);
            String text = Preprocessor.Text;
            Preprocessor.Text = "";
            return text;
           
        
        }
        public int Diff(int x, int y)
        {
            return x > y ? x - y : y - x;
        }
    }
    
}
