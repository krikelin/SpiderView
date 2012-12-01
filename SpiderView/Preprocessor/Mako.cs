using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
namespace Spider.Preprocessor
{
    /// <summary>
    /// This is an simple MakoEngine implementation for Jint.
    /// Used to minic the Spotify's view engine.
    /// 
    /// </summary>
    public class LuaMako : Preprocessor
    {
        /// <summary>
        /// Invokes an method on the script
        /// </summary>
        /// <param name="method">method name</param>
        /// <param name="args">arguments passed to the function</param>
        /// <returns></returns>
        public object Invoke(string method, params object[] args)
        {
            return this.RuntimeMachine.InvokeFunction(method, args);
        }
        /// <summary>
        /// Returns the old output
        /// </summary>
        public String OldOutput;
        /// <summary>
        /// Raises the create event handler. Useful to add features to the engine before running
        /// </summary>
        /// <param name="sender">The current instance of MakoEngine</param>
        /// <param name="e">EventArg</param>
        public delegate void CreateEventHandler(object sender, EventArgs e);
        public event CreateEventHandler Create;
        private SpiderView host;
        public LuaMako(SpiderView host)
        {
            this.host = host;
            // Initialize runtime engine. For now we use JavaScriptEngine
            RuntimeMachine = host.Scripting;

            // Set JSPython to try as default
           // JSPython = true;
            
            // Raise create event handler
            if (this.Create!=null)
            {
                this.Create(this, new EventArgs());
            }
        }
        public String Output = "";
        /// <summary>
        /// Callback where the output is thrown to, called by the parsed string
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public object __printx( string values = "")
        {
            Output += values.Replace("%BR%","\n").Replace("¤","\"");
            return true;
        }
        /// <summary>
        /// Synchronize data is called by the javascript preparser to get an ready to use JSON parsed data. If the dat can't be parsed as JSON
        /// it will be returned as an common string
        /// </summary>
        /// <param name="uri">The address to the remote information to retrieve</param>
        /// <returns></returns>
        public object synchronize_data(string uri)
        {
            // Create web request
            WebClient WC = new WebClient();
            /**
             * Try getting data. If no data was got an all, return FALSE
             * */
            try
            {
                String jsonRaw = WC.DownloadString(new Uri(uri));

                // Convert it to JSON
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(jsonRaw);
                    return xmlDoc;
                }
                catch
                {
                    return jsonRaw;
                }
#if(obsolote)
                try
                {
                    Jint.JintEngine Engine = new JintEngine();
                    Jint.Native.JsObject D = new Jint.Native.JsObject((object)jsonRaw);

                    // Try parse it as json, otherwise try as xml and if not retuurn it as an string
                    try
                    {
                        // Do not allow CLR when reading external scripts for security measurements
                        System.Web.Script.Serialization.JavaScriptSerializer d = new System.Web.Script.Serialization.JavaScriptSerializer();
                        object json = d.DeserializeObject(jsonRaw);
                        return json;
                    }
                    catch
                    {

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(jsonRaw);
                        return xmlDoc;
                    }

                   
                }
                catch
                {
                    return jsonRaw;
                }
#endif
            }
            catch
            {

                return false;
            }
        }

        /// <summary>
        /// Function to convert variable signatures to variable concations for the parser
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public string HandleToTokens(String Line, char signature)
        {
            
            Dictionary<String, Object> Variables = new Dictionary<String, Object>();
            // The index of the beginning of an varialbe statement @{
            int IndexOf = 0;
            /**
             * Iterate through all indexes of the @{ statemenet until it ends (IndexOf will return -1  becase IndexOf will gain
             * the new IndexOf with the new statement
             * */
            if(Line.Length > 0)
            while (IndexOf != -1)
            {
                IndexOf = Line.IndexOf(signature + "{", IndexOf);
                if (IndexOf == -1)
                    break;
                // Gain the index of the next occuranse of the @{ varialbe
                
                int endToken = Line.IndexOf('}', IndexOf);

                int startIndex = IndexOf + 2;

                // Get the data inside the token
                String Parseable = Line.Substring(startIndex,  endToken -  startIndex);

                // Convert the inline token to concation
                Line = Line.Replace("@{" + Parseable + "}",  "\" .. ( "  + Parseable + " ) .. \"");
                IndexOf = endToken;
               
            }
            return Line;
         

        }
        /// <summary>
        /// This function returns variable from the parser embedded in an output field, asserted with an custom sign {VARNAME}
        /// </summary>
        /// <param name="Line">The code line to execute</param>
        /// <param name="signature">The char signature</param>
        /// <returns>An list of processed variables</returns>
        public Dictionary<String, Object> GetVariables(String Line, char signature)
        {
            Dictionary<String,Object> Variables = new Dictionary<String,Object>();
            // The index of the beginning of an varialbe statement @{
            int IndexOf = 0;
            /**
             * Iterate through all indexes of the @{ statemenet until it ends (IndexOf will return -1  becase IndexOf will gain
             * the new IndexOf with the new statement
             * */
            while (IndexOf != -1)
            {
                // Gain the index of the next occuranse of the @{ varialbe
                IndexOf  = Line.IndexOf(signature+"{");
                int endToken = Line.IndexOf("}", IndexOf);

                int startIndex = IndexOf+2;

                // Get the data inside the token
                String Parseable = Line.Substring(startIndex, startIndex + endToken - 1);

                // Convert it into an variable
                String[] Result = ExecuteScalarVariable(Parseable, ':', '|', true);
                Variables.Add(Result[0], Result[1]);
            }
            return Variables;
           
        }
        /// <summary>
        /// This function executes the scalar variable, works together with GetVariable. It will also parse the inherited 
        /// codebase.
        /// </summary>
        /// <param name="Variable"></param>
        /// <param name="reflector">Reflector divides which is the conditinoal statement and the boolean output</param>
        /// <param name="divider">Boolean divider</param>
        /// <param name="vetero">Which variable beside the reflector divider should be present in fallback</param>
        /// <value>Returns an 2 field String array where {InitialVariableName,Output}</value>
        /// <returns></returns>
        public String[] ExecuteScalarVariable(String Variable,char reflector,char divider,bool vetero)
        {
            // An variable in this instruction {boolVar} : return1 | return 2
            if (Variable.Contains(reflector) && Variable.Contains(divider))
            {
                // Get the codebase
                String Codebase = Variable.Split(reflector)[0];

                // Run the codebase
                object d = RuntimeMachine.RunCode(Codebase)[0];

                // If it are an boolean decide it, otherwise return the left/right variable as fallback decided by the vetero varialbe
                if (d.GetType() == typeof(bool))
                {
                    // Get the two case output
                    String[] c = Variable.Split(reflector)[1].Split(divider);

                    // Return the decition
                    String output =  (bool)d ? c[0] : c[1];
                    return new String[] { Codebase, output };
                }
                else
                {
                    String[] c = Variable.Split(reflector)[1].Split(divider);

                    // Return the case fallback
                    String output = vetero ? c[0] : c[1];
                    return new String[] { Codebase, output };
                }
            }
           
            /**
                * Otherwise return the value of the variable asserted by the current state of the execution instance
                * */
          
            // Output data
            object _output = RuntimeMachine.RunCode("return " + Variable + ";");
            if (_output.GetType() == typeof(String))
            {
                return new String[]{Variable,(String)_output};
            }

            return new String[]{Variable,Variable};
           
            
        }
        /// <summary>
        /// The javascript will be like as python
        /// </summary>
        public bool JSPython;
        /// <summary>
        /// Instance of the Jint engine running at runtime
        /// </summary>
        public Interpreter RuntimeMachine;
        /// <summary>
        /// This function executes string in the js mako engine
        /// </summary>
        /// <param name="e"></param>
        public void Execute(string e)
        {
           
            RuntimeMachine.RunCode(e);
        }
        /// <summary>
        /// Event args for RequestOverLayEventArgs
        /// </summary>
        public class OverlayEventArgs
        {
            /// <summary>
            /// The view URI
            /// </summary>
            public string URI { get; set; }
            /// <summary>
            /// Folders for the views for each engine. You must provide it in the event attached
            /// by the MediaChrome Host (or apporiate implementation)
            /// </summary>
            public Dictionary<string, string> ViewFolders { get; set; }

            /// <summary>
            /// Gets or sets if the operation should be cancelled or not
            /// </summary>
            public bool Cancel { get; set; }
        }
        public delegate void OverlayEventHandler(object sender, OverlayEventArgs e);
        /// <summary>
        /// Occurs on request overlay
        /// </summary>
        public event OverlayEventHandler RequestOverlay;

        /// <summary>
        /// Occurs when overlay has been finished request
        /// </summary>
        public event OverlayEventHandler OverlayApplied;
        /// <summary>
        /// This function preprosses the mako layer
        /// </summary>
        /// <param name="input">The input string to parse</param>
        /// <param name="argument">The argument sent to the parser</param>
        public String Preprocess(string input, object obj )
        {string argument = "";
            bool inflate = false;
            string uri = "";
            bool onlyPreprocess = false;
            #region OverlayManager Experimental
            /****
             * STOCKHOLM 2011-07-01 14:45
             * 
             * New feature: Apply custom overlays specified by individual service:
             * Overlay are marked with <#namespace#> where "namespace" is an special
             * kind of <namespace>.xml view file inside an <extension_dir>/views/ folder
             * */
            
            // First gain attention by the event

            OverlayEventArgs args = new OverlayEventArgs(); // Create event args
            args.URI = uri;
            if (RequestOverlay != null)
                RequestOverlay(this, args);

            // if the args has retained the phase, eg. not cancelled
            if (!args.Cancel)
            {
                // Substitute the overlay placeholders with the views
                if(args.ViewFolders != null)
                foreach (KeyValuePair<string, string> overlay in args.ViewFolders)
                {
                    using (StreamReader SR = new StreamReader(overlay.Value))
                    {
                        String content = SR.ReadToEnd();
                        // Substitute overlay placeholders
                        input = input.Replace("<#" + overlay.Key.Replace(".xml","") + "#>", content);
                        SR.Close();
                    }
                }
            }

            // Remove unwanted trailings
            Regex a = new Regex(@"<\#[^\#]*\#>", RegexOptions.IgnoreCase);
            input = a.Replace(input, "");
            #endregion

            /**
             * Begin normal operation
             * */


            // Clear the output buffer
            Output = "";
            /**
             * Tell the runtime machine about the argument
             * */
            try
            {
                String[] arguments = argument.Split(':');
                RuntimeMachine.SetVariable("parameter", argument.Replace(arguments[0] + ":", "").Replace(arguments[1] + ":", ""));
                RuntimeMachine.SetVariable("service", arguments[0]);
            }
            catch { }
            /**
             * This string defines the call-stack of the query
             * This is done before any other preprocessing
             * */
            String CallStack = "";
            String[] lines = input.Split('\n');
            /**
             * Boolean indicating which parse mode the parser is in,
             * true = in executable text
             * false = in output line 
             * */
            bool parseMode = false;
            // Boolean indicating first line is parsing
            bool firstLine = true;
            /***
             * Iterate through all lines and preprocess the page.
             * If page starts with an % it will be treated as an preparser code or all content
             * inside enclosing <? ?>
             * Two string builders, the first is for the current output segment and the second for the current
             * executable segmetn
             * */
            StringBuilder outputCode =  new StringBuilder();
            StringBuilder executableSegment = new StringBuilder();

            // The buffer for the final preprocessing output
            StringBuilder finalOutput = new StringBuilder();
            // Append initial case
            outputCode.Append("");

            // Boolean startCase. true = <? false \n%
            bool startCase = false;

            // Boolean which tells if the cursor is in the preparse or output part of the buffer (inside or outside an executable segment)
            bool codeMode = false;
            for(int i=0; i < input.Length ;i++)
            {
                 // Check if at an overgoing to an code block
                if(codeMode)
                {
                    if((startCase && (input[i] == '?' ||input[i] == '%') && input[i+1] == '>') ||( input[i] == '\n' && !startCase))
                    {
                        codeMode=false;

                        // Jump forward two times if endcase is ?>
                        if(startCase)
                            i++;

                        // Get the final output
                        string codeOutput = executableSegment.ToString();
                        // If in JSPython mode, convert all row breaks to ; and other syntax elements
                        if (JSPython)
                        {
                            codeOutput = codeOutput.Replace("\n", ";");
                            
                            /**
                             * Convert statements
                             * */
                            codeOutput = codeOutput.Replace(":", "{");
                            codeOutput = codeOutput.Replace("end", "}");
                           
                            codeOutput = codeOutput.Replace("\nif ", "\nif(");
                            codeOutput = codeOutput.Replace("then:", "){");
                            codeOutput = codeOutput.Replace("do:", "){");

                            codeOutput = codeOutput.Replace("endif", "}");
                            


                        }
                        codeOutput = codeOutput.Replace("lt;", "<");
                       

                        codeOutput = codeOutput.Replace("lower than", "<");
                        codeOutput = codeOutput.Replace("lower", "<");

                        codeOutput = codeOutput.Replace("higher", ">");

                        codeOutput = codeOutput.Replace("highter than", ">");
                        codeOutput = codeOutput.Replace("gt;", ">");
                        // Append the code data to the string buffer
                        finalOutput.Append(" "+ codeOutput  + " ");
                        
                        

                        // Clear outputcode buffer
                        executableSegment = new StringBuilder();

                        continue;
                    }
                    executableSegment.Append(input[i]);
                }
                else
                {
                    // If at end, summarize the string
                    if(i == input.Length - 1)
                    {
                        // Append the last string
                        outputCode.Append(input[i]);
                        // Format output code (Replace " to ¤ and swap back on preprocessing)
                        String OutputCode = outputCode.ToString().Replace("\"", "¤").Replace("\n", "%BR%\");\n__printx(\"");
                        OutputCode = this.HandleToTokens(OutputCode.ToString(),'@');
                        finalOutput.Append("__printx(\"" + OutputCode + "\");");
                       
                    }
                    try
                    {
                        if ((((input[i] == '\n' || input[i] == ' ' || input[i] == '\t') && input[i + 1] == '%' && input[i + 2] != '>')) || (input[i] == '<' && input[i + 1] == '?'))
                        {
                            startCase = (input[i] == '<' && input[i + 1] == '?');
                            codeMode = true;

                            // Convert tokens to interpretable handles
                            String OutputCode = outputCode.ToString().Replace("\"", "¤").Replace("\n", "%BR%\");\n__printx(\"");
                            OutputCode = this.HandleToTokens(OutputCode.ToString(), '@');
                            finalOutput.Append("__printx(\"" + OutputCode + "\");");

                            // Clear the output code buffer
                            outputCode = new StringBuilder();

                            // Skip two tokens forward to not include those tokens to the code buffer
                            i += 1;
                            continue;
                        }

                    }
                    catch
                    {
                        continue;
                    }
                    outputCode.Append(input[i]);
                    
                }
                

            }            
            // if exiting in text mode, append an end of the scalar string
            if (!parseMode)
            {
                CallStack += "\");";
            }
            // Run the code
            RuntimeMachine.RegisterFunction("__printx", (MethodBase)new printx_func(__printx).Method, this);
            RuntimeMachine.RegisterFunction("synchronize_data", (MethodBase)new synchronize_func(synchronize_data).Method, this);
            CallStack = finalOutput.ToString();
           
            CallStack = CallStack.Replace("\r", "");
            if (!onlyPreprocess)
            {
                /***
                 * Try run the page. If there was error return ERROR: <error> message so the
                 * handler can choose to present it to the user
                 * */
                try
                {
                    RuntimeMachine.RunCode(CallStack);

                    /**
                     * Check if the result of the preprocessing is the same as before. If nothing
                     * has changed return NONCHANGE. This is only for rendering whole pages, not inflate.
                     * */
                    if (!inflate)
                    {
                        if (Output == OldOutput)
                            return "NONCHANGE";

                        OldOutput = Output;
                    }
                }
                catch (Exception e)
                {
                    using (System.IO.StreamReader SR = new System.IO.StreamReader("error.xml"))
                    {
                        return SR.ReadToEnd();
                    }
                    /*// clear output
                    this.Output = "";
                    // Load error page
                    using (System.IO.StreamReader SR = new System.IO.StreamReader("views\\error.xml"))
                    {
                /* string errorView = new MakoEngine().Preprocess(SR.ReadToEnd(), "", false, "", true);
                        RuntimeMachine = new JavaScriptEngine();
                        RuntimeMachine.SetFunction("__printx", new Func<String, object>(__printx));
                        RuntimeMachine.SetVariable("error", e.ToString() + "\n " );

                        RuntimeMachine.RunCode((errorView));
                    }*/
                }
                return this.Output;
            }
               
            else
            {
                return CallStack;
            }

        }
    }
    public delegate object synchronize_func(String uri);
    public delegate object printx_func(string msg = "");

}
