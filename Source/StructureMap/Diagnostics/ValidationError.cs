using System;
using System.IO;
using System.Reflection;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace StructureMap.Diagnostics
{
    public class ValidationError
    {
        public ValidationError(Type pluginType, Instance instance, Exception exception, MethodInfo method)
        {
            PluginType = pluginType;
            Instance = instance;
            Exception = exception;
            MethodName = method.Name;
        }

        public Instance Instance;
        public Type PluginType;
        public Exception Exception;
        public string MethodName;

        public void Write(StringWriter writer)
        {
            string description = ((IDiagnosticInstance) Instance).CreateToken().Description;

            writer.WriteLine();
            writer.WriteLine("-----------------------------------------------------------------------------------------------------");
            writer.WriteLine("Validation Error in Method {0} of Instance {1} in PluginType {2}", MethodName, description, TypePath.GetAssemblyQualifiedName(PluginType));
            writer.WriteLine(Exception.ToString());
            writer.WriteLine();
        }
    }
}