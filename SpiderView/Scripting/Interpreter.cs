using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spider
{
    /// <summary>
    /// Script engine for Spider applications
    /// </summary>
    public interface Interpreter
    {
        /// <summary>
        /// Invokes a function
        /// </summary>
        /// <param name="function">Function name</param>
        /// <param name="arguments">Arguments to send</param>
        void InvokeFunction(String function, params Object[] arguments);
        /// <summary>
        /// Gets the content type of the script engine
        /// </summary>
        String ContentType { get; }
        /// <summary>
        /// Loads code into memory
        /// </summary>
        /// <param name="fileName"></param>
        void LoadFile(String fileName);

        /// <summary>
        /// Load raw code
        /// </summary>
        /// <param name="code"></param>
        void LoadScript(String code);
        /// <summary>
        /// Runs the code
        /// </summary>
        /// <param name="code"></param>
        void RunCode(String code);

        /// <summary>
        /// Register a function in the cloud
        /// </summary>
        /// <param name="function"></param>
        /// <param name="func"></param>
        void PushFunction(String function, System.Reflection.MethodBase func);

        /// <summary>
        /// Gets the view the script is attached to
        /// </summary>
        SpiderView View { get; }
    }
}
