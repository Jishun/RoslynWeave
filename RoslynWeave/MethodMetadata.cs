using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace RoslynWeave
{
    public class MethodMetadata
    {
        public MethodBase MethodBase { get; }
        public IDictionary<string, object> Parameters { get; }

        public MethodMetadata(MethodBase methodBase, params (string name, object value)[] paramters)
        {
            MethodBase = methodBase;
            Parameters = new ReadOnlyDictionary<string, object>(paramters.ToDictionary(p => p.name, p => p.value));
        }
    }

}
