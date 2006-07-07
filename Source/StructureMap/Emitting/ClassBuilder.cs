using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace StructureMap.Emitting
{
	/// <summary>
	/// Emits the IL for a new class Type
	/// </summary>
	public class ClassBuilder
	{
		private const TypeAttributes PUBLIC_ATTS = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

		private TypeBuilder newTypeBuilder;
		private Type superType;
		private string _ClassName;

		private ArrayList _Methods;

		public ClassBuilder(ModuleBuilder module, string ClassName) : this(module, ClassName, typeof (Object))
		{
		}

		public ClassBuilder(ModuleBuilder module, string ClassName, Type superType)
		{
			_Methods = new ArrayList();

			newTypeBuilder = module.DefineType(ClassName, PUBLIC_ATTS, superType);
			this.superType = superType;
			_ClassName = ClassName;

			this.addDefaultConstructor();
		}


		public void AddMethod(Method method)
		{
			_Methods.Add(method);
			method.Attach(newTypeBuilder);
		}


		internal void Bake()
		{
			foreach (Method method in _Methods)
			{
				method.Build();
			}

			this.newTypeBuilder.CreateType();
		}


		public string ClassName
		{
			get { return _ClassName; }
		}


		private void addDefaultConstructor()
		{
			MethodAttributes atts = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName;

			ConstructorBuilder construct = this.newTypeBuilder.DefineConstructor(atts, CallingConventions.Standard, null);

			ILGenerator ilgen = construct.GetILGenerator();

			ilgen.Emit(OpCodes.Ldarg_0);


			ConstructorInfo constructor = superType.GetConstructor(new Type[0]);
			ilgen.Emit(OpCodes.Call, constructor);
			ilgen.Emit(OpCodes.Ret);
		}


		public void AddReadonlyStringProperty(string PropertyName, string Value, bool Override)
		{
			PropertyBuilder prop = newTypeBuilder.DefineProperty(PropertyName, PropertyAttributes.HasDefault, typeof (string), null);

			MethodAttributes atts = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.Final | MethodAttributes.SpecialName;

			string _GetMethodName = "get_" + PropertyName;

			MethodBuilder methodGet = this.newTypeBuilder.DefineMethod(_GetMethodName, atts, CallingConventions.Standard, typeof (string), null);
			ILGenerator gen = methodGet.GetILGenerator();

			LocalBuilder ilReturn = gen.DeclareLocal(typeof (string));

			gen.Emit(OpCodes.Ldstr, Value);
			gen.Emit(OpCodes.Stloc_0);
			gen.Emit(OpCodes.Ldloc_0);
			gen.Emit(OpCodes.Ret);

			prop.SetGetMethod(methodGet);
		}

	}
}