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
using System.Timers;

namespace SpiderView
{
    namespace ProtocolBuffer
    {
        public class DataRecievedEventArgs
        {
            public Object data;
        }
        public delegate void DataReceivedEvent(object sender, DataRecievedEventArgs e);

        public class Message {
            public Message (MessageType type, List<Object> data) {
                this.Data = data;
                this.MessageType = type;
            }
            public MessageType MessageType {get;set;}
            public List<Object> Data;

        }
        /// <summary>
        ///  Listens to a certain protocol
        /// </summary>
       public abstract class ProtocolListener 
       {
           public Protocol Protocol {get; set;}
           public event DataReceivedEvent DataRecieved;
           public ProtocolListener(Protocol protocol) 
           {
               this.Protocol = protocol;
           }
           public abstract void StartListening();
       }
        public class Protocol
        {
           
            public event DataReceivedEvent DataRecieved;
            public void StartListening(ProtocolListener listener)
            {
                listener.DataRecieved += listener_DataRecieved;
            }

            void listener_DataRecieved(object sender, DataRecievedEventArgs e)
            {
                if (this.DataRecieved != null)
                {
                    this.DataRecieved(sender, e);
                }
            }
            public Protocol(String stylesheet)
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
                                MessageType selector = new MessageType(buffer.ToString().Trim(), block);
                                this.MessageTypes.Add(selector);
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
            public List<MessageType> MessageTypes = new List<MessageType>();
        }
        public class MessageType
        {
            public String Name {get;set;}
            public MessageType(String name, String block)
            {
                this.Name = name.Substring("Message".Length);
                String[] rules = block.Split(';');
                if (rules.Length < 1)
                {
                    Field rule = new Field(block + ";");
                    this.rules.Add(rule);
                }

                foreach (String _rule in rules)
                {
                    Field rule = new Field(_rule + ";");
                    this.rules.Add(rule);
                }
            }
            public List<Field> rules = new List<Field>();

        }
        public class Field
        {
            public enum ParserMode {
                property, value
            };
            public List<String> Properties { get; set; }
            public Field(String input)
            {
                Properties = new List<string>();
                StringBuilder valueBuffer = new StringBuilder();
                int mode = 0;

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
                        case ' ':
                            if (!inString) {
                                mode++;
                                Properties.Add(valueBuffer.ToString());
                                valueBuffer.Clear();
                            }
                            break;
                        case '}':
                        case ';':
                            if (!inString)
                            {

                                Properties.Add(valueBuffer.ToString());
                                valueBuffer.Clear();
                                i = input.Length + 1; // Break
                            }
                            break;
                        default:
                            
                            valueBuffer.Append(currentChar);
                            break;
                            
                    }
                    
                }
              


            }
           
        }
    }
}
