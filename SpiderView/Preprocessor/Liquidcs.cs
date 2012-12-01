using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spider.Preprocessor
{
    public class Liquid : Preprocessor
    {
        public SpiderView Host;
        public Liquid(SpiderView host)
        {
            this.Host = host;
        }
        public string Preprocess(string template, object code)
        {
            Template _template = Template.Parse(template);
            return _template.Render(Hash.FromAnonymousObject(code));
        }
    }
}
