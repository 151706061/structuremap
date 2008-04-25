using System;
using System.Reflection;
using System.Reflection.Emit;

namespace StructureMap.Emitting.Parameters
{
    /// <summary>
    /// Implementation of ParameterEmitter for a non-primitive, non-array
    /// constructor argument
    /// </summary>
    public class ChildParameterEmitter : ParameterEmitter
    {
        protected override bool canProcess(Type parameterType)
        {
            return (!parameterType.IsPrimitive && !parameterType.IsArray);
        }

        protected override void generate(ILGenerator ilgen, ParameterInfo parameter)
        {
            Type parameterType = parameter.ParameterType;
            string parameterName = parameter.Name;

            putChildObjectOnStack(ilgen, parameterName, parameterType);
        }

        private void putChildObjectOnStack(ILGenerator ilgen, string parameterName, Type parameterType)
        {
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Ldstr, parameterName);

            ilgen.Emit(OpCodes.Ldtoken, parameterType);

            MethodInfo method = typeof (Type).GetMethod("GetTypeFromHandle");
            ilgen.Emit(OpCodes.Call, method);

            ilgen.Emit(OpCodes.Ldarg_2);

            callInstanceMemento(ilgen, "GetChild");
            cast(ilgen, parameterType);
        }

        protected override void generateSetter(ILGenerator ilgen, PropertyInfo property)
        {
            ilgen.Emit(OpCodes.Ldloc_0);

            putChildObjectOnStack(ilgen, property.Name, property.PropertyType);

            MethodInfo method = property.GetSetMethod();
            ilgen.Emit(OpCodes.Callvirt, method);
        }
    }
}