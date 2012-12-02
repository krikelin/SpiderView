/************
 * Simple CSS Parser for .NET and C#
 * Copyright (C) 2012-2013 Alexander Forselius <drsounds@gmail.com>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this 
 * software and associated documentation files (the "Software"), to deal in the Software 
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or 
 * substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, A
 * RISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 ****/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aleros
{
    namespace CSS
    {
        public class Parser
        {
            /// <summary>
            /// parse a rule
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            

            
        }
        public class Stylesheet
        {
            public Stylesheet(String stylesheet)
            {
                char currentChar = '\0';
                StringBuilder buffer = new StringBuilder();
                for (int i = 0, j = 0; i < stylesheet.Length; i++, j++)
                {
                    currentChar = stylesheet[i];
                    switch (currentChar)
                    {
                        case ' ':
                            continue;
                        case '{':
                            {
                                int endIndex = stylesheet.IndexOf('}', i);
                                String block = stylesheet.Substring(i, endIndex - i);
                                Selector selector = new Selector(buffer.ToString().Trim(), block);
                                this.selectors.Add(selector);
                                i = endIndex - 1;
                                buffer.Clear();
                                continue;
                            }
                        default:
                            buffer.Append(currentChar);
                            break;
                    }
                }
            }
            public List<Selector> selectors = new List<Selector>();
        }
        public class Selector
        {
            public String Name {get;set;}
            public Selector(String name, String block)
            {
                this.Name = name;
                String[] rules = block.Split(';');
                if (rules.Length < 1)
                {
                    Rule rule = new Rule(block + ";");
                    this.rules.Add(rule);
                }

                foreach (String _rule in rules)
                {
                    Rule rule = new Rule(_rule + ";");
                    this.rules.Add(rule);
                }
            }
            public List<Rule> rules = new List<Rule>();

        }
        public class Rule
        {
            public enum ParserMode {
                property, value
            };
            public Rule(String input)
            {
                StringBuilder propBuffer = new StringBuilder();
                StringBuilder valueBuffer = new StringBuilder();
                ParserMode mode = ParserMode.property;
                bool inString = false;
                char currentChar = '\0';
                for (var i = 0; i < input.Length; i++)
                {
                    currentChar = input[i];
                    switch (currentChar)
                    {
                        case '{':
                            if (!inString)
                            {
                                continue;
                            }
                            break;
                        case '"':
                        case '\'':
                            inString = !inString;
                            break;
                        case ':':
                            if (!inString)
                                mode = ParserMode.value;
                            break;
                        case '}':
                        case ';':
                            if (!inString)
                                break;
                            i = 1000; // Break
                            break;
                        default:
                            switch (mode)
                            {
                                case ParserMode.property:
                                    propBuffer.Append(currentChar);
                                    break;
                                case ParserMode.value:
                                    valueBuffer.Append(currentChar);
                                    break;
                            }
                            break;
                    }
                    
                }
                this.rule = propBuffer.ToString().Trim();
                this.value = valueBuffer.ToString().Trim().Replace("\"", "");
                /*if (input.Contains(':'))
                {
                    int assigner = input.IndexOf(':');
                    int endIndex = input.IndexOf(';', assigner);
                    String prop = input.Substring(0, assigner);
                    String val = input.Substring(assigner + 1, endIndex - assigner);
                    this.originalRule = input;
                    this.rule = prop;
                    this.value = val ;
                }
                else
                {
                    int end = input.IndexOf(';');
                    String prop = input.Substring(0, end);
                    this.originalRule = input;
                
                    this.rule = prop;
                    this.value = null;

                }*/


            }
            public String originalRule;
            public String rule;
            public String value;
            public Object objectValue
            {
                get
                {
                    return this.value;
                }
            }
        }
    }
}
