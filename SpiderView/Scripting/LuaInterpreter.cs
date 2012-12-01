using LuaInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spider.Scripting
{
    /// <summary>
    /// Lua interpreter for the Spider view
    /// </summary>
    public class LuaInterpreter : Interpreter
    {
        private LuaInterface.Lua lua;
        private SpiderView host;
        public LuaInterpreter(SpiderView host)
        {
            this.host = host;
            this.lua = new LuaInterface.Lua();
        }
        public string ContentType
        {
            get { return "text/lua"; }
        }

        public void LoadFile(string fileName)
        {
            this.lua.LoadFile(fileName);
        }
        public void LoadScript(string code)
        {
            this.lua.LoadString(code, "");
        }
        public void RunCode(string code)
        {
            this.lua.DoString(code);
        }

        public void PushFunction(string function, System.Reflection.MethodBase func)
        {
            this.lua.RegisterFunction(function, null, func);
        }

        public SpiderView View
        {
            get { return this.host; }
        }

        /// <summary>
        /// Invoke the function
        /// </summary>
        /// <param name="function"></param>
        /// <param name="arguments"></param>
        public void InvokeFunction(string function, params object[] arguments)
        {
            try
            {
                LuaFunction func = this.lua.GetFunction(function);
                func.Call(arguments);
            }
            catch (Exception e)
            {

            }
        }
    }
}
