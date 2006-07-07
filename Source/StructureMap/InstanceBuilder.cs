namespace StructureMap
{
	/// <summary>
	/// Base class for creating an object instance from an InstanceMemento.  SubClasses are
	/// emitted for each concrete Plugin with constructor parameters.
	/// </summary>
	public abstract class InstanceBuilder
	{
		private InstanceManager _manager;

		public InstanceBuilder()
		{
		}

		public abstract string PluginType { get; }
		public abstract string PluggedType { get; }
		public abstract string ConcreteTypeKey { get; }
		public abstract object BuildInstance(InstanceMemento Memento);

		public void SetInstanceManager(InstanceManager Manager)
		{
			_manager = Manager;
		}

		public InstanceManager Manager
		{
			get { return _manager; }
		}
	}
}