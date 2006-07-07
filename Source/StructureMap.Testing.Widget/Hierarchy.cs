namespace StructureMap.Testing.Widget
{
	[PluginFamily, Pluggable("Default", "")]
	public class GrandChild
	{
		private bool _RightHanded;
		private int _BirthYear;

		public GrandChild(bool RightHanded, int BirthYear)
		{
			_BirthYear = BirthYear;
			_RightHanded = RightHanded;
		}

		public bool RightHanded
		{
			get { return _RightHanded; }
		}

		public int BirthYear
		{
			get { return _BirthYear; }
		}
	}


	[Pluggable("Leftie", "")]
	public class LeftieGrandChild : GrandChild
	{
		public LeftieGrandChild(int BirthYear) : base(false, BirthYear)
		{
		}
	}


	[PluginFamily, Pluggable("Default", "")]
	public class Child
	{
		private string _Name;
		private GrandChild _MyGrandChild;

		public Child(string Name, GrandChild MyGrandChild)
		{
			_Name = Name;
			_MyGrandChild = MyGrandChild;
		}

		public string Name
		{
			get { return _Name; }
		}

		public GrandChild MyGrandChild
		{
			get { return _MyGrandChild; }
		}
	}

	[PluginFamily, Pluggable("Default", "")]
	public class Parent
	{
		private int _Age;
		private string _EyeColor;
		private Child _MyChild;

		public Parent(int Age, string EyeColor, Child MyChild)
		{
			_Age = Age;
			_EyeColor = EyeColor;
			_MyChild = MyChild;
		}

		public int Age
		{
			get { return _Age; }
		}

		public string EyeColor
		{
			get { return _EyeColor; }
		}

		public Child MyChild
		{
			get { return _MyChild; }
		}
	}


	public class ChildLoaderTemplate : InstanceBuilder
	{
		public override string ConcreteTypeKey
		{
			get { return null; }
		}

		public override string PluggedType
		{
			get { return null; }
		}


		public override string PluginType
		{
			get { return null; }
		}


		public override object BuildInstance(InstanceMemento Memento)
		{
			return new Child(
				Memento.GetProperty("Name"),
				(GrandChild) Memento.GetChild("MyGrandChild", "StructureMap.Testing.Widget.GrandChild", this.Manager));
		}

	}

}