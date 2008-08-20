using System;

namespace StructureMap.Pipeline
{
    public class BuildFrame
    {
        private readonly Type _requestedType;
        private readonly string _name;
        private readonly Type _concreteType;

        public BuildFrame(Type requestedType, string name, Type concreteType)
        {
            _requestedType = requestedType;
            _name = name;
            _concreteType = concreteType;
        }

        public Type RequestedType
        {
            get { return _requestedType; }
        }

        public string Name
        {
            get { return _name; }
        }

        public Type ConcreteType
        {
            get { return _concreteType; }
        }

        private BuildFrame _parent;
        private BuildFrame _next;

        internal void Attach(BuildFrame next)
        {
            _next = next;
            _next._parent = this;
        }

        internal BuildFrame Detach()
        {
            if (_parent != null) _parent._next = null;
            return _parent;
        }

        internal BuildFrame Parent
        {
            get
            {
                return _parent;
            }
        }

        public override string ToString()
        {
            return string.Format("RequestedType: {0}, Name: {1}, ConcreteType: {2}", _requestedType, _name, _concreteType);
        }

        public bool Equals(BuildFrame obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj._requestedType, _requestedType) && Equals(obj._name, _name) && Equals(obj._concreteType, _concreteType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (BuildFrame)) return false;
            return Equals((BuildFrame) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (_requestedType != null ? _requestedType.GetHashCode() : 0);
                result = (result*397) ^ (_name != null ? _name.GetHashCode() : 0);
                result = (result*397) ^ (_concreteType != null ? _concreteType.GetHashCode() : 0);
                return result;
            }
        }
    }
}