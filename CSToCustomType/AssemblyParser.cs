using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSToCustomType
{
    internal class AssemblyParser
    {
        public AssemblyParser(string pathToAssembly, string outputPath)
        {
            Console.WriteLine($"Parsing assembly {pathToAssembly} and generating C++ files to {outputPath}...");
            var parsedTypes = ParseAssembly(pathToAssembly);
            GenerateMarkdownFiles(parsedTypes, outputPath);
        }

        public List<ParsedType> ParseAssembly(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            var parsedTypes = new List<ParsedType>();

            foreach (var type in assembly.GetTypes())
            {
                var parsedType = new ParsedType
                {
                    Name = type.Name,
                    NamespaceName = type.Namespace,
                    Attributes = type.GetCustomAttributes().Select(a => a.GetType().Name).ToList()
                };
                Console.WriteLine($"Parsing type {type.FullName}...");

                if (type.BaseType != null && type.BaseType != typeof(object))
                {
                    parsedType.BaseClass = type.BaseType.FullName;
                }
                parsedType.Interfaces.AddRange(type.GetInterfaces().Select(i => i.FullName));

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    try
                    {
                        parsedType.Fields.Add(new ParsedField
                        {
                            Visibility = GetFieldVisibility(field),
                            Type = field.FieldType.ToString(),
                            Name = field.Name,
                            Attributes = field.GetCustomAttributes().Select(a => a.GetType().Name).ToList()
                        });
                    }
                    catch
                    {
                        // ignore fields that throw exceptions when trying to get their type
                    }

                }

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    try
                    {
                        bool isOverride = method.GetBaseDefinition() != method;
                        parsedType.Methods.Add(new ParsedMethod
                        {
                            Visibility = GetMethodVisibility(method),
                            ReturnType = method.ReturnType.ToString(),
                            Name = method.Name,
                            Parameters = method.GetParameters().Select(p => $"{p.ParameterType} {p.Name}").ToList(),
                            Attributes = method.GetCustomAttributes().Select(a => a.GetType().Name).ToList(),
                            IsOverride = isOverride
                        });
                    }
                    catch
                    {
                        // ignore methods that throw exceptions when trying to get their return type
                    }

                }

                parsedTypes.Add(parsedType);
            }
            return parsedTypes;
        }

        private static string GetFieldVisibility(FieldInfo field)
        {
            if (field.IsPublic) return "public";
            if (field.IsPrivate) return "private";
            if (field.IsFamily) return "protected";
            if (field.IsAssembly) return "internal";
            if (field.IsFamilyOrAssembly) return "protected internal";
            return "unknown";
        }

        private static string GetMethodVisibility(MethodInfo method)
        {
            if (method.IsPublic) return "public";
            if (method.IsPrivate) return "private";
            if (method.IsFamily) return "protected";
            if (method.IsAssembly) return "internal";
            if (method.IsFamilyOrAssembly) return "protected internal";
            return "unknown";
        }

        public void GenerateMarkdownFiles(List<ParsedType> parsedTypes, string outputPath)
        {
            foreach (var parsedType in parsedTypes)
            {
                string sanitizedFileName = string.Concat(parsedType.Name.Split(Path.GetInvalidFileNameChars()));
                string fileName = Path.Combine(outputPath, $"{sanitizedFileName.Replace(".", "_")}.md");

                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.WriteLine($"# {parsedType.NamespaceName}.{parsedType.Name}");
                    writer.WriteLine();

                    writer.WriteLine($"**Base Class:** `{parsedType.BaseClass ?? "None"}`  ");
                    writer.WriteLine($"**Interfaces:** {(parsedType.Interfaces.Any() ? "`" + string.Join("`, `", parsedType.Interfaces) + "`" : "None")}");
                    writer.WriteLine();

                    writer.WriteLine("## Fields");
                    if (parsedType.Fields.Any())
                    {
                        writer.WriteLine("| Attributes | Visibility | Type | Name |");
                        writer.WriteLine("|------------|------------|------------|------------|");
                        foreach (var field in parsedType.Fields)
                        {
                            string attributes = field.Attributes.Any() ? string.Join(", ", field.Attributes) : "None";
                            writer.WriteLine($"| `{attributes}` | `{field.Visibility}` | `{field.Type}` | `{field.Name}` |");
                        }
                    }
                    else
                    {
                        writer.WriteLine("_None_");
                    }
                    writer.WriteLine();

                    writer.WriteLine("## Methods");
                    if (parsedType.Methods.Any())
                    {
                        writer.WriteLine("| Attributes | Visibility | Return Type | Name | Parameters |");
                        writer.WriteLine("|------------|------------|------------|------------|------------|");
                        foreach (var method in parsedType.Methods)
                        {
                            string attributes = method.Attributes.Any() ? string.Join(", ", method.Attributes) : "None";
                            string parameters = method.Parameters.Any() ? string.Join(", ", method.Parameters) : "None";
                            writer.WriteLine($"| `{attributes}` | `{method.Visibility + (method.IsOverride ? " override" : "")}` | `{method.ReturnType}` | `{method.Name}` | `{parameters}` |");
                        }
                    }
                    else
                    {
                        writer.WriteLine("_None_");
                    }
                }
            }
        }

    }
}
