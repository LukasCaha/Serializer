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
            IndentSpaceCount = 2;
            TreatStringAsEnumerable = false;
        }

        public void Serialize(TextWriter writer, object graph)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            this.writer = writer;
            Assembly assembly = Assembly.GetCallingAssembly();
            typesInAssembly = assembly.GetTypes();
            RecursiveSerilization(graph, 0);


            Console.ForegroundColor = ConsoleColor.White;
        }

        private void RecursiveSerilization(object graph, int indent)
        {
            bool matched = false;
            foreach (Type assemblyType in typesInAssembly)
            {
                if (graph.GetType() == assemblyType)
                {
                    if (assemblyType.IsPrimitive)
                    {
                        //TODO: primitive root
                        matched = true;
                    }
                    if (assemblyType.IsClass)
                    {
                        ProcessClass(graph, indent, assemblyType, out indent);
                        matched = true;
                    }
                }
                if (graph.GetType().IsGenericType)
                {
                    //Get the collection property
                    foreach (var property in graph.GetType().GetProperties())
                    {
                        Console.WriteLine(property.Name);
                        if (property.Name == "Item")
                        {

                            /*Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(property.GetType().GetGenericTypeDefinition() + "==" + typeof(List<>));
                            Console.ForegroundColor = ConsoleColor.DarkGreen;*/
                            if (graph.GetType().GetGenericTypeDefinition() == typeof(List<>))
                            {
                                var genericTypes = graph.GetType().GetGenericTypeDefinition();

                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine(genericTypes);
                                Console.ForegroundColor = ConsoleColor.DarkGreen;

                                matched = true;
                            }

                            /*StringBuilder sbDescription = new StringBuilder();
                            StringBuilder sbAllNotes = new StringBuilder();

                            Console.WriteLine(property.PropertyType);
                            var y = (List<property.GetType()>)graph;
                            var x = Convert.ChangeType(graph, property.PropertyType);

                            foreach (var item in (IEnumerable)property.GetValue(x, null))
                            {
                                //Because props is a collection, Getting the type and property on each pass was essential
                                var propertyName = item.GetType().GetProperty("PropertyName").GetValue(item, null);
                                var newValue = item.GetType().GetProperty("NewValue").GetValue(item, null);
                                var oldValue = item.GetType().GetProperty("OldValue").GetValue(item, null);

                                sbDescription.AppendLine((propertyName != null) ? propertyName.ToString() : "" + ", ");

                                sbAllNotes.AppendLine(((propertyName != null) ? propertyName.ToString() : "")+
                                    ((newValue != null) ? newValue.ToString() : "") +
                                    ((oldValue != null) ? oldValue.ToString() : ""));
                            }*/
                        }
                    }

                    matched = true;

                    /*Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(graph.GetType().GetGenericTypeDefinition() +"=="+ typeof(List<>));
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    if (graph.GetType().GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var genericTypes = graph.GetType().GetGenericTypeDefinition().;

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(genericTypes);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;

                        matched = true;
                    }*/
                }
            }
            if (!matched)
            { 
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(graph.GetType());
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            }
        }

        private void ProcessClass(object graph, int indent, Type assemblyType, out int outIndent)
        {
            writer.Write("\n");
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
                    WriteValue(subType.GetValue(graph).ToString(), ConsoleColor.Green);
                }
                else
                {
                    RecursiveSerilization(subType.GetValue(graph), indent + 1);
                    WriteIndent(indent);
                }
                WriteEndTag(subType.Name);
            }
            indent--;
            WriteIndent(indent);
            WriteEndTag(graph.GetType().Name);

            outIndent = indent;
        }

        private void WriteValue(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            writer.Write(value);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }

        public static string GetTypeName(Type t)
        {
            if (!t.IsGenericType) return t.Name;
            //Console.WriteLine("is generic");
            if (t.IsNested && t.DeclaringType.IsGenericType) throw new NotImplementedException();
            string txt = t.Name.Substring(0, t.Name.IndexOf('`')) + "<";
            int cnt = 0;
            foreach (Type arg in t.GetGenericArguments())
            {
                if (cnt > 0) txt += ", ";
                txt += GetTypeName(arg);
                cnt++;
            }
            return txt + ">";
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

        public static bool IsIterable(Type t)
        {
            return t.IsGenericType;
        }
    }
}