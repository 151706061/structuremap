using System;
using System.Collections;
using System.Collections.Generic;
using StructureMap.Graph;
using StructureMap.Interceptors;
using StructureMap.Pipeline;

namespace StructureMap
{
    public class BuildSession : IBuildSession
    {
        private readonly InterceptorLibrary _interceptorLibrary;
        private readonly PipelineGraph _pipelineGraph;
        private readonly InstanceCache _cache = new InstanceCache();

        public BuildSession(PipelineGraph pipelineGraph, InterceptorLibrary interceptorLibrary)
        {
            _pipelineGraph = pipelineGraph;
            _interceptorLibrary = interceptorLibrary;
        }

        public BuildSession(PluginGraph graph)
            : this(new PipelineGraph(graph), graph.InterceptorLibrary)
        {
            
        }


        protected PipelineGraph pipelineGraph
        {
            get { return _pipelineGraph; }
        }

        #region IBuildSession Members

        public object CreateInstance(Type pluginType, string name)
        {
            Instance instance = forType(pluginType).FindInstance(name);
            if (instance == null)
            {
                throw new StructureMapException(200, name, pluginType.FullName);
            }

            return CreateInstance(pluginType, instance);
        }

        public virtual object CreateInstance(Type pluginType, Instance instance)
        {
            object result = _cache.Get(pluginType, instance);
            
            if (result == null)
            {
                result = forType(pluginType).Build(this, instance);
                _cache.Set(pluginType, instance, result);
            }

            return result;
        }

        public Array CreateInstanceArray(Type pluginType, Instance[] instances)
        {
            Array array;

            if (instances == null)
            {
                IList list = forType(pluginType).GetAllInstances(this);
                array = Array.CreateInstance(pluginType, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    array.SetValue(list[i], i);
                }
            }
            else
            {
                // TODO:  3.5, move this to an extension method of Array?
                array = Array.CreateInstance(pluginType, instances.Length);
                for (int i = 0; i < instances.Length; i++)
                {
                    Instance instance = instances[i];

                    object arrayValue = forType(pluginType).Build(this, instance);
                    array.SetValue(arrayValue, i);
                }    
            }

            return array;
        }

        public object CreateInstance(Type pluginType)
        {
            Instance instance = _pipelineGraph.GetDefault(pluginType);

            if (instance == null)
            {
                throw new StructureMapException(202, pluginType.FullName);
            }

            return CreateInstance(pluginType, instance);
        }

        public object ApplyInterception(Type pluginType, object actualValue)
        {
            return _interceptorLibrary.FindInterceptor(actualValue.GetType()).Process(actualValue);
        }

        public InstanceBuilder FindBuilderByType(Type pluginType, Type pluggedType)
        {
            return forType(pluginType).FindBuilderByType(pluggedType);
        }

        public InstanceBuilder FindBuilderByConcreteKey(Type pluginType, string concreteKey)
        {
            return forType(pluginType).FindBuilderByConcreteKey(concreteKey);
        }

        #endregion

        private IInstanceFactory forType(Type pluginType)
        {
            return _pipelineGraph.ForType(pluginType);
        }
    }
}