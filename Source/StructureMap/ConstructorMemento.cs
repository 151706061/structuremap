using System;

namespace StructureMap
{
    public delegate PLUGINTYPE BuildObjectDelegate<PLUGINTYPE>();

    public class ConstructorMemento<PLUGINTYPE> : MemoryInstanceMemento
    {
        private BuildObjectDelegate<PLUGINTYPE> _builder;


        public ConstructorMemento()
        {
        }

        public ConstructorMemento(string instanceKey, BuildObjectDelegate<PLUGINTYPE> builder)
            : base(instanceKey, instanceKey)
        {
            _builder = builder;
        }

        public ConstructorMemento(BuildObjectDelegate<PLUGINTYPE> builder)
            : this(Guid.NewGuid().ToString(), builder)
        {
            
        }

        public override object Build(IInstanceCreator creator)
        {
            return _builder();
        }


        public BuildObjectDelegate<PLUGINTYPE> Builder
        {
            get { return _builder; }
            set { _builder = value; }
        }
    }
}
