using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;
using System.Collections;

namespace ReflectorS
{
    public class Serializer
    {
        public int IndentSpaceCount { get; set; }
        public bool TreatStringAsEnumerable { get; set; }

        private Type[] typesInAssembly;
        private TextWriter writer;

        public Serializer()
        {
            //defaults
            IndentSpaceCount = 2;
            TreatStringAsEnumerable = false;
        }

        public void Serialize(TextWriter writer, object graph)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            this.writer = writer;
            Assembly assembly = Assembly.GetCallingAssembly();
            typesInAssembly = assembly.GetTypes();
            RecursiveSerilization(graph, 0, true);


            Console.ForegroundColor = ConsoleColor.White;
        }

        private void RecursiveSerilization(object graph, int indent, bool topLevel)
        {
            if (graph.GetType().IsPrimitive)
            {
                WriteBeginTag(graph.GetType().Name, false);
                WriteValue(graph.ToString(), ConsoleColor.Yellow);
                WriteEndTag(graph.GetType().Name);
            }
            if (graph.GetType() == typeof(string))
            {
                if (TreatStringAsEnumerable)
                {
                    WriteString(graph.ToString(), ConsoleColor.Cyan, indent + 1);
                    WriteIndent(indent);
                }
                else
                {
                    WriteValue(graph.ToString(), ConsoleColor.Cyan);
                }
            }

            foreach (Type assemblyType in typesInAssembly)
            {
                if (assemblyType.IsClass)
                {
                    if (graph.GetType() == assemblyType)
                    {
                        if (!topLevel) writer.WriteLine();
                        ProcessClass(graph, indent, assemblyType);
                    }
                }

                if (graph.GetType().IsGenericType)
                {
                    if (graph.GetType() == typeof(List<>).MakeGenericType(assemblyType))
                    {
                        ProcessList(graph, indent, assemblyType.Name);
                    }
                }
            }
        }


        //CLASS
        private void ProcessClass(object graph, int indent, Type assemblyType)
        {
            WriteIndent(indent);
            WriteBeginTag(graph.GetType().Name, /*newline*/ true);


            indent++;
            foreach (var subType in assemblyType.GetProperties())
            {
                WriteIndent(indent);
                WriteBeginTag(subType.Name, /*newline*/ false);

                var nodeType = subType.GetValue(graph).GetType();

                if (nodeType.IsPrimitive)
                {
                    WriteValue(subType.GetValue(graph).ToString(), ConsoleColor.Yellow);
                }
                else if (nodeType.Name == "String")
                {
                    if (TreatStringAsEnumerable)
                    {
                        WriteString(subType.GetValue(graph).ToString(), ConsoleColor.Cyan, indent + 1);
                        WriteIndent(indent);
                    }
                    else
                    {
                        WriteValue(subType.GetValue(graph).ToString(), ConsoleColor.Cyan);
                    }
                }
                else
                {
                    RecursiveSerilization(subType.GetValue(graph), indent + 1, false);
                    WriteIndent(indent);
                }
                WriteEndTag(subType.Name);
            }
            indent--;
            WriteIndent(indent);
            WriteEndTag(graph.GetType().Name);
        }


        //LIST
        public void ProcessList(object graph, int indent, string tag)
        {
            var enumerable = graph as IEnumerable;

            if (enumerable != null)
            {
                bool first = true;
                foreach (var listitem in enumerable)
                {
                    if (first)
                    {
                        writer.WriteLine();
                        first = false;
                    }
                    WriteIndent(indent);
                    WriteBeginTag(tag, true);
                    ProcessProperties(listitem, indent + 1);
                    WriteIndent(indent);
                    WriteEndTag(tag);
                }
            }
            else
            {
                WriteIndent(indent);
                WriteBeginTag(tag, true);
                ProcessProperties(graph, indent + 1);
                WriteIndent(indent);
                WriteEndTag(tag);
            }
        }

        //PROPERTIES
        private void ProcessProperties(object graph, int indent)
        {
            var propertyInfos = 
                graph.GetType()
                    .GetProperties()
                    .Where(
                        x => !x.GetIndexParameters().Any()
                    );

            foreach (var prop in propertyInfos)
            {
                var value = prop.GetValue(graph);
                WriteIndent(indent);
                WriteBeginTag(prop.Name, false);
                if (value.GetType().IsPrimitive)
                {
                    //PRIMITIVE
                    WriteValue(value.ToString(), ConsoleColor.Cyan);
                }
                else if(value.GetType() == typeof(string))
                {
                    //STRING
                    if (TreatStringAsEnumerable)
                    {
                        WriteString(value.ToString(), ConsoleColor.Cyan, indent + 1);
                        WriteIndent(indent);
                    }
                    else
                    {
                        WriteValue(value.ToString(), ConsoleColor.Cyan);
                    }
                }
                else
                {
                    //LIST
                    writer.WriteLine();
                    ProcessList(value, indent + 1, value.GetType().Name);
                    WriteIndent(indent);
                }
                WriteEndTag(prop.Name);
            }
        }

        #region helper methods

        private void WriteValue(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            writer.Write(value);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }

        private void WriteString(string value, ConsoleColor color, int indent)
        {
            Console.ForegroundColor = color;
            writer.WriteLine();
            foreach (var item in value)
            {
                WriteIndent(indent);
                WriteBeginTag("Char", false);
                writer.Write(item);
                WriteEndTag("Char");
            }
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }

        private void WriteIndent(int indent)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < indent * IndentSpaceCount; i++)
            {
                sb.Append(" ");
            }
            writer.Write(sb.ToString());
        }

        //test purposes
        private void WriteIndent(int indent, char c)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < indent * IndentSpaceCount; i++)
            {
                sb.Append(c);
            }
            writer.Write(sb.ToString());
        }

        private void WriteBeginTag(string tag, bool newline)
        {
            writer.Write("<" + tag + ">");
            if (newline) writer.WriteLine();
        }

        private void WriteEndTag(string tag)
        {
            writer.Write("</" + tag + ">\n");
        }

        #endregion
    }
}