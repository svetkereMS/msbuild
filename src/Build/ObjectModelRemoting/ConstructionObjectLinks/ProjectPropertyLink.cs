// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;

    public abstract class ProjectPropertyLink
    {
        /// <summary>
        /// Owning project
        /// </summary>
        public abstract Project Project { get; }

        /// <summary>
        /// backing XML (for non -global properties)
        /// </summary>
        public abstract ProjectPropertyElement Xml { get; }

        //
        // Summary:
        //     Name of the property. Cannot be set.
        public abstract string Name { get; }

        /// <summary>
        /// Gets the evaluated property value.
        /// Cannot be set directly: only the unevaluated value can be set.
        /// Is never null.
        /// </summary>
        /// <remarks>
        /// Evaluated property escaped as necessary
        /// </remarks>
        public abstract string EvaluatedIncludeEscaped { get; }

        //
        // Summary:
        //     Gets or sets the unevaluated property value. Updates the evaluated value in the
        //     project, although this is not sure to be correct until re-evaluation.
        public abstract string UnevaluatedValue { get; set; }

        //
        // Summary:
        //     Whether the property originated from the environment (or the toolset)
        public abstract bool IsEnvironmentProperty { get; }

        //
        // Summary:
        //     Whether the property is a global property
        public abstract bool IsGlobalProperty { get; }

        //
        // Summary:
        //     Whether the property is a reserved property, like 'MSBuildProjectFile'.
        public abstract bool IsReservedProperty { get; }

        //
        // Summary:
        //     Any immediately previous property that was overridden by this one during evaluation.
        //     This would include all properties with the same name that lie above in the logical
        //     project file, and whose conditions evaluated to true. If there are none above
        //     this is null. If the project has not been reevaluated since the last modification
        //     this value may be incorrect.
        public abstract ProjectProperty Predecessor { get; }

        //
        // Summary:
        //     If the property originated in an imported file, returns true. If the property
        //     originates from the environment, a global property, or is a built-in property,
        //     returns false. Otherwise returns false.
        public abstract bool IsImported { get; }
    }

}
