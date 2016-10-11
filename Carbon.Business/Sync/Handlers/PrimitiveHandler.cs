using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Carbon.Business.Domain;

namespace Carbon.Business.Sync.Handlers
{    
    public abstract class PrimitiveHandler
    {
        private static Dictionary<PrimitiveType, Type> _primitivesTypeMap;

        private static readonly ConcurrentDictionary<PrimitiveType, PrimitiveHandler> _primitivesInstanceMap =
            new ConcurrentDictionary<PrimitiveType, PrimitiveHandler>();

        private static readonly object _syncRoot = new object();        

        public static PrimitiveHandler GetHandler(Primitive primitive)
        {
            EnsureTypeMap();            

            return _primitivesInstanceMap.GetOrAdd(primitive.Type, t =>
            {
                Type type;
                if (!_primitivesTypeMap.TryGetValue(t, out type))
                {
                    return null;
                }                

                var handler = (PrimitiveHandler) Activator.CreateInstance(type);
                _primitivesInstanceMap[t] = handler;
                return handler;
            });
        }                               

        public static void ApplyImmediate(IEnumerable<RawPrimitive> primitives, ProjectModel model, Func<PrimitiveContext> contextFunc)
        {            
            PrimitiveContext context = null;
            foreach (var raw in primitives)
            {                                               
                if (!raw.Type.IsDeferred())
                {
                    var primitive = (Primitive) raw;
                    var handler = GetHandler(primitive);
                    if (handler == null)
                    {
                        continue;
                    }
                    if (context == null)
                    {
                        context = contextFunc();
                    }                                        
                    handler.Apply(primitive, model, context);
                }                
            }            
        }

        public abstract void Apply(Primitive primitive, ProjectModel projectModel, PrimitiveContext context);

        private static void EnsureTypeMap()
        {
            if (_primitivesTypeMap != null)
            {
                return;
            }
            lock (_syncRoot)
            {
                if (_primitivesTypeMap == null)
                {
                    var map = new Dictionary<PrimitiveType, Type>();
                    var types = typeof (PrimitiveHandler).Assembly
                        .GetTypes()
                        .Where(t => t.GetCustomAttributes<PrimitiveHandlerAttribute>().Any());

                    foreach (var type in types)
                    {
                        var attributes = type.GetCustomAttributes<PrimitiveHandlerAttribute>();
                        foreach (var attribute in attributes)
                        {
                            map[attribute.Type] = type;
                        }
                    }

                    _primitivesTypeMap = map;
                }
            }
        }        

        public static Dictionary<PrimitiveType, Type> TypeMap
        {
            get
            {
                EnsureTypeMap();
                return _primitivesTypeMap;
            }
        }
    }

    public abstract class PrimitiveHandler<T> : PrimitiveHandler
        where T : Primitive
    {
        public sealed override void Apply(Primitive primitive, ProjectModel projectModel, PrimitiveContext context)
        {
            Apply((T)primitive, projectModel, context);
        }

        public abstract void Apply(T primitive, ProjectModel projectModel, PrimitiveContext context);
    }
}
