
namespace Microsoft.Build.ObjectModelRemoting
{
    using System.Collections.Generic;
    using Microsoft.Build.Construction;

    public abstract class ProjectTaskElementLink : ProjectElementContainerLink
    {
        public abstract IDictionary<string, string> GetParameters();
        public abstract IEnumerable<KeyValuePair<string, ElementLocation>> GetParametersLocations();
        public abstract string GetParameter(string name);
        public abstract void SetParameter(string name, string unevaluatedValue);
        public abstract void RemoveParameter(string name);

        public abstract void RemoveAllParameters();
    }
}
