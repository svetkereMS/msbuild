// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Build.Evaluation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Evaluation.Context;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Globbing;
    using Microsoft.Build.Logging;

    /// <summary>
    /// Project implementation interface. Allows alternative <see cref="Project"> sources. (such us remote Projects)
    /// </summary>
    public abstract class ProjectLink
    {
        /// <summary>
        /// The backing Xml project.
        /// Can never be null
        /// </summary>
        /// <remarks>
        /// There is no setter here as that doesn't make sense. If you have a new ProjectRootElement, evaluate it into a new Project.
        /// </remarks>
        public abstract ProjectRootElement Xml { get; }

        /// <summary>
        /// Certain item operations split the item element in multiple elements if the include
        /// contains globs, references to items or properties, or multiple item values.
        ///
        /// The items operations that may expand item elements are:
        /// - <see cref="RemoveItem"/>
        /// - <see cref="RemoveItems"/>
        /// - <see cref="AddItem"/>
        /// - <see cref="AddItemFast"/>
        /// - <see cref="ProjectItem.ChangeItemType"/>
        /// - <see cref="ProjectItem.Rename"/>
        /// - <see cref="ProjectItem.RemoveMetadata"/>
        /// - <see cref="ProjectItem.SetMetadataValue(string,string)"/>
        /// - <see cref="ProjectItem.SetMetadataValue(string,string, bool)"/>
        /// 
        /// When this property is set to true, the previous item operations throw an <exception cref="InvalidOperationException"></exception>
        /// instead of expanding the item element. 
        /// </summary>
        public virtual bool ThrowInsteadOfSplittingItemElement { get; set; }

        /// <summary>
        /// Whether this project is dirty such that it needs reevaluation.
        /// This may be because its underlying XML has changed (either through this project or another)
        /// either the XML of the main project or an imported file; 
        /// or because its toolset may have changed.
        /// </summary>
        public abstract bool IsDirty { get; }

        /// <summary>
        /// Read only dictionary of the global properties used in the evaluation
        /// of this project.
        /// </summary>
        /// <remarks>
        /// This is the publicly exposed getter, that translates into a read-only dead IDictionary&lt;string, string&gt;.
        /// 
        /// In order to easily tell when we're dirtied, setting and removing global properties is done with 
        /// <see cref="SetGlobalProperty">SetGlobalProperty</see> and <see cref="RemoveGlobalProperty">RemoveGlobalProperty</see>.
        /// </remarks>
        public abstract IDictionary<string, string> GlobalProperties { get; }

        /// <summary>
        /// Item types in this project.
        /// This is an ordered collection.
        /// </summary>
        /// <comments>
        /// data.ItemTypes is a KeyCollection, so it doesn't need any 
        /// additional read-only protection
        /// </comments>
        public abstract ICollection<string> ItemTypes { get; }

        /// <summary>
        /// Properties in this project.
        /// Since evaluation has occurred, this is an unordered collection.
        /// </summary>
        public abstract ICollection<ProjectProperty> Properties { get; }

        /// <summary>
        /// Collection of possible values implied for properties contained in the conditions found on properties,
        /// property groups, imports, and whens.
        /// 
        /// For example, if the following conditions existed on properties in a project:
        /// 
        /// Condition="'$(Configuration)|$(Platform)' == 'Debug|x86'"
        /// Condition="'$(Configuration)' == 'Release'"
        /// 
        /// the table would be populated with
        /// 
        /// { "Configuration", { "Debug", "Release" }}
        /// { "Platform", { "x86" }}
        /// 
        /// This is used by Visual Studio to determine the configurations defined in the project.
        /// </summary>
        public virtual IDictionary<string, List<string>> ConditionedProperties { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Read-only dictionary of item definitions in this project.
        /// Keyed by item type
        /// </summary>
        public abstract IDictionary<string, ProjectItemDefinition> ItemDefinitions { get; }

        /// <summary>
        /// Items in this project, ordered within groups of item types
        /// </summary>
        public abstract ICollection<ProjectItem> Items { get; }

        /// <summary>
        /// Items in this project, ordered within groups of item types,
        /// including items whose conditions evaluated to false, or that were
        /// contained within item groups who themselves had conditioned evaluated to false.
        /// This is useful for hosts that wish to display all items, even if they might not be part 
        /// of the build in the current configuration.
        /// </summary>
        public virtual  ICollection<ProjectItem> ItemsIgnoringCondition { get { throw new NotImplementedException(); } }

        /// <summary>
        /// All the files that during evaluation contributed to this project, as ProjectRootElements,
        /// with the ProjectImportElement that caused them to be imported.
        /// This does not include projects that were never imported because a condition on an Import element was false.
        /// The outer ProjectRootElement that maps to this project itself is not included.
        /// </summary>
        /// <remarks>
        /// This can be used by the host to figure out what projects might be impacted by a change to a particular file.
        /// It could also be used, for example, to find the .user file, and use its ProjectRootElement to modify properties in it.
        /// </remarks>
        public abstract IList<ResolvedImport> Imports { get; }

        /// <summary>
        /// This list will contain duplicate imports if an import is imported multiple times. However, only the first import was used in evaluation.
        /// </summary>
        public virtual IList<ResolvedImport> ImportsIncludingDuplicates { get { return Imports; } }

        /// <summary>
        /// Targets in the project. The key to the dictionary is the target's name.
        /// Overridden targets are not included in this collection.
        /// This collection is read-only.
        /// </summary>
        public abstract IDictionary<string, ProjectTargetInstance> Targets { get; }

        /// <summary>
        /// Properties encountered during evaluation. These are read during the first evaluation pass.
        /// Unlike those returned by the Properties property, these are ordered, and includes any properties that
        /// were subsequently overridden by others with the same name. It does not include any 
        /// properties whose conditions did not evaluate to true.
        /// It does not include any properties added since the last evaluation.
        /// </summary>
        public abstract ICollection<ProjectProperty> AllEvaluatedProperties { get; }


        /// <summary>
        /// Item definition metadata encountered during evaluation. These are read during the second evaluation pass.
        /// Unlike those returned by the ItemDefinitions property, these are ordered, and include any metadata that
        /// were subsequently overridden by others with the same name and item type. It does not include any 
        /// elements whose conditions did not evaluate to true.
        /// It does not include any item definition metadata added since the last evaluation.
        /// </summary>
        public abstract ICollection<ProjectMetadata> AllEvaluatedItemDefinitionMetadata { get; }

        /// <summary>
        /// Items encountered during evaluation. These are read during the third evaluation pass.
        /// Unlike those returned by the Items property, these are ordered with respect to all other items 
        /// encountered during evaluation, not just ordered with respect to items of the same item type.
        /// In some applications, like the F# language, this complete mutual ordering is significant, and such hosts
        /// can use this property.
        /// It does not include any elements whose conditions did not evaluate to true.
        /// It does not include any items added since the last evaluation.
        /// </summary>
        public abstract ICollection<ProjectItem> AllEvaluatedItems { get; }

        /// <summary>
        /// The tools version this project was evaluated with, if any.
        /// Not necessarily the same as the tools version on the Project tag, if any;
        /// it may have been externally specified, for example with a /tv switch.
        /// The actual tools version on the Project tag, can be gotten from <see cref="Xml">Xml.ToolsVersion</see>.
        /// Cannot be changed once the project has been created.
        /// </summary>
        /// <remarks>
        /// Set by construction.
        /// </remarks>
        public abstract string ToolsVersion { get; }

        /// <summary>
        /// The sub-toolset version that, combined with the ToolsVersion, was used to determine
        /// the toolset properties for this project.  
        /// </summary>
        public abstract string SubToolsetVersion { get; }

        /// <summary>
        /// Whether ReevaluateIfNecessary is temporarily disabled.
        /// This is useful when the host expects to make a number of reads and writes 
        /// to the project, and wants to temporarily sacrifice correctness for performance.
        /// </summary>
        public virtual bool SkipEvaluation { get; set; }

        /// <summary>
        /// Whether <see cref="MarkDirty()">MarkDirty()</see> is temporarily disabled.
        /// This allows, for example, a global property to be set without the project getting
        /// marked dirty for reevaluation as a consequence.
        /// </summary>
        public virtual bool DisableMarkDirty { get; set; }

        /// <summary>
        /// This controls whether or not the building of targets/tasks is enabled for this
        /// project.  This is for security purposes in case a host wants to closely
        /// control which projects it allows to run targets/tasks.  By default, for a newly
        /// created project, we will use whatever setting is in the parent project collection.
        /// When build is disabled, the Build method on this class will fail. However if
        /// the host has already created a ProjectInstance, it can still build it. (It is 
        /// free to put a similar check around where it does this.)
        /// </summary>
        public virtual bool IsBuildEnabled { get; set; } = true;

        /// <summary>
        /// The ID of the last evaluation for this Project.
        /// A project is always evaluated upon construction and can subsequently get evaluated multiple times via
        /// <see cref="ReevaluateIfNecessary" />
        /// 
        /// It is an arbitrary number that changes when this project reevaluates.
        /// Hosts don't know whether an evaluation actually happened in an interval, but they can compare this number to
        /// their previously stored value to find out, and if so perhaps decide to update their own state.
        /// Note that the number may not increase monotonically.
        /// 
        /// This number corresponds to the <seealso cref="BuildEventContext.EvaluationId"/> and can be used to connect
        /// evaluation logging events back to the Project instance.
        /// </summary>
        public abstract int LastEvaluationId { get; }

        /// <summary>
        /// Finds all the globs specified in item includes.
        /// </summary>
        /// <example>
        /// 
        /// <code>
        ///<P>*.txt</P>
        /// 
        ///<Bar Include="bar"/> (both outside and inside project cone)
        ///<Zar Include="C:\**\*.foo"/> (both outside and inside project cone)
        ///<Foo Include="*.a;*.b" Exclude="3.a"/>
        ///<Foo Remove="2.a" />
        ///<Foo Include="**\*.b" Exclude="1.b;**\obj\*.b;**\bar\*.b"/>
        ///<Foo Include="$(P)"/> 
        ///<Foo Include="*.a;@(Bar);3.a"/> (If Bar has globs, they will have been included when querying Bar ProjectItems for globs)
        ///<Foo Include="*.cs" Exclude="@(Bar)"/>
        ///</code>
        /// 
        ///Example result: 
        ///[
        ///GlobResult(glob: "C:\**\*.foo", exclude: []),
        ///GlobResult(glob: ["*.a", "*.b"], exclude=["3.a"], remove=["2.a"]),
        ///GlobResult(glob: "**\*.b", exclude=["1.b, **\obj\*.b", **\bar\*.b"]),
        ///GlobResult(glob: "*.txt", exclude=[]),
        ///GlobResult(glob: "*.a", exclude=[]),
        ///GlobResult(glob: "*.cs", exclude=["bar"])
        ///]
        /// </example>
        /// <remarks>
        /// <see cref="GlobResult.MsBuildGlob"/> is a <see cref="IMSBuildGlob"/> that combines all globs in the include element and ignores
        /// all the fragments in the exclude attribute and all the fragments in all Remove elements that apply to the include element.
        /// 
        /// Users can construct a composite glob that incorporates all the globs in the Project:
        /// <code>
        /// var uberGlob = new CompositeGlob(project.GetAllGlobs().Select(r => r.MSBuildGlob).ToArray());
        /// uberGlob.IsMatch("foo.cs");
        /// </code>
        /// 
        /// </remarks>
        /// <returns>
        /// List of <see cref="GlobResult"/>.
        /// </returns>
        /// <param name="evaluationContext">
        ///     The evaluation context to use in case reevaluation is required.
        ///     To avoid reevaluation use <see cref="ProjectLoadSettings.RecordEvaluatedItemElements"/>
        /// </param>
        public virtual List<GlobResult> GetAllGlobs(EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        /// <summary>
        /// See <see cref="GetAllGlobs(EvaluationContext)"/>
        /// </summary>
        /// <param name="itemType">Confine search to item elements of this type</param>
        /// <param name="evaluationContext">
        ///     The evaluation context to use in case reevaluation is required.
        ///     To avoid reevaluation use <see cref="ProjectLoadSettings.RecordEvaluatedItemElements"/>
        /// </param>
        public virtual List<GlobResult> GetAllGlobs(string itemType, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        /// <summary>
        /// Finds all the item elements in the logical project with itemspecs that match the given string:
        /// - elements that would include (or exclude) the string
        /// - elements that would update the string (not yet implemented)
        /// - elements that would remove the string (not yet implemented)
        /// </summary>
        /// 
        /// <example>
        /// The following snippet shows what <c>GetItemProvenance("a.cs")</c> returns for various item elements
        /// <code>
        /// <A Include="a.cs;*.cs"/> // Occurrences:2; Operation: Include; Provenance: StringLiteral | Glob
        /// <B Include="*.cs" Exclude="a.cs"/> // Occurrences: 1; Operation: Exclude; Provenance: StringLiteral
        /// <C Include="b.cs"/> // NA
        /// <D Include="@(A)"/> // Occurrences: 2; Operation: Include; Provenance: Inconclusive (it is an indirect occurence from a referenced item)
        /// <E Include="$(P)"/> // Occurrences: 4; Operation: Include; Provenance: FromLiteral (direct reference in $P) | Glob (direct reference in $P) | Inconclusive (it is an indirect occurrence from referenced properties and items)
        /// <PropertyGroup>
        ///     <P>a.cs;*.cs;@(A)</P>
        /// </PropertyGroup>
        /// </code>
        /// 
        /// </example>
        /// 
        /// <remarks>
        /// This method and its overloads are useful for clients that need to inspect all the item elements
        /// that might refer to a specific item instance. For example, Visual Studio uses it to inspect
        /// projects with globs. Upon a file system or IDE file artifact change, VS calls this method to find all the items
        /// that might refer to the detected file change (e.g. 'which item elements refer to "Program.cs"?').
        /// It uses such information to know which elements it should edit to reflect the user or file system changes.
        /// 
        /// Literal string matching tries to first match the strings. If the check fails, it then tries to match
        /// the strings as if they represented files: it normalizes both strings as files relative to the current project directory
        ///
        /// GetItemProvenance suffers from some sources of inaccuracy:
        /// - it is performed after evaluation, thus is insensitive to item data flow when item references are present
        /// (it sees items as they are at the end of evaluation)
        /// 
        /// This API and its return types are prone to change.
        /// </remarks>
        /// 
        /// <param name="itemToMatch">The string to perform matching against</param>
        /// <param name="evaluationContext">
        ///     The evaluation context to use in case reevaluation is required.
        ///     To avoid reevaluation use <see cref="ProjectLoadSettings.RecordEvaluatedItemElements"/>
        /// </param>
        /// <returns>
        /// A list of <see cref="ProvenanceResult"/>, sorted in project evaluation order.
        /// </returns>
        public virtual List<ProvenanceResult> GetItemProvenance(string itemToMatch, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        /// <summary>
        /// See <see cref="GetItemProvenance(string, EvaluationContext)"/>
        /// </summary>
        /// <param name="itemToMatch">The string to perform matching against</param>
        /// <param name="itemType">The item type to constrain the search in</param>
        /// <param name="evaluationContext">
        ///     The evaluation context to use in case reevaluation is required.
        ///     To avoid reevaluation use <see cref="ProjectLoadSettings.RecordEvaluatedItemElements"/>
        /// </param>
        public virtual List<ProvenanceResult> GetItemProvenance(string itemToMatch, string itemType, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        /// <summary>
        /// See <see cref="GetItemProvenance(string, EvaluationContext)"/>
        /// </summary>
        /// <param name="item"> 
        /// The ProjectItem object that indicates: the itemspec to match and the item type to constrain the search in.
        /// The search is also constrained on item elements appearing before the item element that produced this <paramref name="item"/>.
        /// The element that produced this <paramref name="item"/> is included in the results.
        /// </param>
        /// <param name="evaluationContext">
        ///     The evaluation context to use in case reevaluation is required.
        ///     To avoid reevaluation use <see cref="ProjectLoadSettings.RecordEvaluatedItemElements"/>
        /// </param>
        public virtual List<ProvenanceResult> GetItemProvenance(ProjectItem item, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns an iterator over the "logical project". The logical project is defined as
        /// the unevaluated project obtained from the single MSBuild file that is the result 
        /// of inlining the text of all imports of the original MSBuild project manifest file.
        /// </summary>
        public virtual IEnumerable<ProjectElement> GetLogicalProject() { throw new NotImplementedException(); }

        /// <summary>
        /// Get any property in the project that has the specified name,
        /// otherwise returns null
        /// </summary>
        public abstract ProjectProperty GetProperty(string name);

        /// <summary>
        /// Get the unescaped value of a property in this project, or 
        /// an empty string if it does not exist.
        /// </summary>
        /// <remarks>
        /// A property with a value of empty string and no property
        /// at all are not distinguished between by this method.
        /// That makes it easier to use. To find out if a property is set at
        /// all in the project, use GetProperty(name).
        /// </remarks>
        public abstract string GetPropertyValue(string name);

        /// <summary>
        /// Set or add a property with the specified name and value.
        /// Overwrites the value of any property with the same name already in the collection if it did not originate in an imported file.
        /// If there is no such existing property, uses this heuristic:
        /// Updates the last existing property with the specified name that has no condition on itself or its property group, if any,
        /// and is in this project file rather than an imported file.
        /// Otherwise, adds a new property in the first property group without a condition, creating a property group if necessary after
        /// the last existing property group, else at the start of the project.
        /// Returns the property set.
        /// Evaluates on a best-effort basis:
        ///     -expands with all properties. Properties that are defined in the XML below the new property may be used, even though in a real evaluation they would not be.
        ///     -only this property is evaluated. Anything else that would depend on its value is not affected.
        /// This is a convenience that it is understood does not necessarily leave the project in a perfectly self consistent state until reevaluation.
        /// </summary>
        public abstract ProjectProperty SetProperty(string name, string unevaluatedValue);

        /// <summary>
        /// Change a global property after the project has been evaluated.
        /// If the value changes, this makes the project require reevaluation.
        /// If the value changes, returns true, otherwise false.
        /// </summary>
        public abstract bool SetGlobalProperty(string name, string escapedValue);

        /// <summary>
        /// Adds an item with metadata to the project.
        /// Metadata may be null, indicating no metadata.
        /// Does not modify the XML if a wildcard expression would already include the new item.
        /// Evaluates on a best-effort basis:
        ///     -expands with all items. Items that are defined in the XML below the new item may be used, even though in a real evaluation they would not be.
        ///     -only this item is evaluated. Other items that might depend on it is not affected.
        /// This is a convenience that it is understood does not necessarily leave the project in a perfectly self consistent state until reevaluation.
        /// </summary>
        public virtual IList<ProjectItem> AddItem(string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata) { throw new NotImplementedException(); }

        /// <summary>
        /// Adds an item with metadata to the project.
        /// Metadata may be null, indicating no metadata.
        /// Makes no effort to see if an existing wildcard would already match the new item, unless it is the first item in an item group.
        /// Makes no effort to locate the new item near similar items.
        /// Appends the item to the first item group that does not have a condition and has either no children or whose first child is an item of the same type.
        /// Evaluates on a best-effort basis:
        ///     -expands with all items. Items that are defined in the XML below the new item may be used, even though in a real evaluation they would not be.
        ///     -only this item is evaluated. Other items that might depend on it is not affected.
        /// This is a convenience that it is understood does not necessarily leave the project in a perfectly self consistent state until reevaluation.
        /// </summary>
        public virtual IList<ProjectItem> AddItemFast(string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata) { return AddItem(itemType, unevaluatedInclude, metadata); }

        /// <summary>
        /// All the items in the project of the specified
        /// type.
        /// If there are none, returns an empty list.
        /// Use AddItem or RemoveItem to modify items in this project.
        /// </summary>
        /// <comments>
        /// data.GetItems returns a read-only collection, so no need to re-wrap it here. 
        /// </comments>
        public abstract ICollection<ProjectItem> GetItems(string itemType);

        /// <summary>
        /// All the items in the project of the specified
        /// type, irrespective of whether the conditions on them evaluated to true.
        /// This is a read-only list: use AddItem or RemoveItem to modify items in this project.
        /// </summary>
        /// <comments>
        /// ItemDictionary[] returns a read only collection, so no need to wrap it. 
        /// </comments>
        public virtual ICollection<ProjectItem> GetItemsIgnoringCondition(string itemType) { throw new NotImplementedException(); }

        /// <summary>
        /// Returns all items that have the specified evaluated include.
        /// For example, all items that have the evaluated include "bar.cpp".
        /// Typically there will be zero or one, but sometimes there are two items with the
        /// same path and different item types, or even the same item types. This will return
        /// them all.
        /// </summary>
        /// <comments>
        /// data.GetItemsByEvaluatedInclude already returns a read-only collection, so no need
        /// to wrap it further.
        /// </comments>
        public abstract ICollection<ProjectItem> GetItemsByEvaluatedInclude(string evaluatedInclude);

        /// <summary>
        /// Removes the specified property.
        /// Property must be associated with this project.
        /// Property must not originate from an imported file.
        /// Returns true if the property was in this evaluated project, otherwise false.
        /// As a convenience, if the parent property group becomes empty, it is also removed.
        /// Updates the evaluated project, but does not affect anything else in the project until reevaluation. For example,
        /// if "p" is removed, it will be removed from the evaluated project, but "q" which is evaluated from "$(p)" will not be modified until reevaluation.
        /// This is a convenience that it is understood does not necessarily leave the project in a perfectly self consistent state.
        /// </summary>
        public abstract bool RemoveProperty(ProjectProperty property);

        /// <summary>
        /// Removes a global property.
        /// If it was set, returns true, and marks the project
        /// as requiring reevaluation.
        /// </summary>
        public abstract bool RemoveGlobalProperty(string name);

        /// <summary>
        /// Removes an item from the project.
        /// Item must be associated with this project.
        /// Item must not originate from an imported file.
        /// Returns true if the item was in this evaluated project, otherwise false.
        /// As a convenience, if the parent item group becomes empty, it is also removed.
        /// If the item originated from a wildcard or semicolon separated expression, expands that expression into multiple items first.
        /// Updates the evaluated project, but does not affect anything else in the project until reevaluation. For example,
        /// if an item of type "i" is removed, "j" which is evaluated from "@(i)" will not be modified until reevaluation.
        /// This is a convenience that it is understood does not necessarily leave the project in a perfectly self consistent state until reevaluation.
        /// </summary>
        /// <remarks>
        /// Normally this will return true, since if the item isn't in the project, it will throw.
        /// The exception is removing an item that was only in ItemsIgnoringCondition.
        /// </remarks>
        public virtual bool RemoveItem(ProjectItem item) { throw new NotImplementedException(); }

        /// <summary>
        /// Removes all the specified items from the project.
        /// Items that are not associated with this project are skipped.
        /// </summary>
        /// <remarks>
        /// Removing one item could cause the backing XML
        /// to be expanded, which could zombie (disassociate) the next item.
        /// To make this case easy for the caller, if an item
        /// is not associated with this project it is simply skipped.
        /// </remarks>
        public virtual void RemoveItems(IEnumerable<ProjectItem> items) { throw new NotImplementedException(); }

        /// <summary>
        /// Evaluates the provided string by expanding items and properties,
        /// as if it was found at the very end of the project file.
        /// This is useful for some hosts for which this kind of best-effort
        /// evaluation is sufficient.
        /// Does not expand bare metadata expressions.
        /// </summary>
        public abstract string ExpandString(string unexpandedValue);

        /// <summary>
        /// Returns an instance based on this project, but completely disconnected.
        /// This instance can be used to build independently.
        /// Before creating the instance, this will reevaluate the project if necessary, so it will not be dirty.
        /// The instance is immutable; none of the objects that form it can be modified. This makes it safe to 
        /// access concurrently from multiple threads.
        /// </summary>
        /// <param name="evaluationContext">The evaluation context to use in case reevaluation is required</param>
        /// <returns></returns>
        public virtual ProjectInstance CreateProjectInstance(ProjectInstanceSettings settings, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        /// <summary>
        /// Called to forcibly mark the project as dirty requiring reevaluation. Generally this is not necessary to set; all edits affecting
        /// this project will automatically make it dirty. However there are potential corner cases where it is necessary to mark the project dirty
        /// directly. For example, if the project has an import conditioned on a file existing on disk, and the file did not exist at
        /// evaluation time, then someone subsequently creates that file, the project cannot know that reevaluation would be productive.
        /// In such a case the host can help us by setting the dirty flag explicitly so that <see cref="ReevaluateIfNecessary">ReevaluateIfNecessary()</see>
        /// will recognize an evaluation is indeed necessary.
        /// Does not mark the underlying project file as requiring saving.
        /// </summary>
        public abstract void MarkDirty();

        /// <summary>
        /// Reevaluate the project to get it into a queryable state, if it's dirty.
        /// This incorporates all changes previously made to the backing XML by editing this project.
        /// Throws InvalidProjectFileException if the evaluation fails.
        /// </summary>
        /// <param name="evaluationContext">The <see cref="EvaluationContext"/> to use. See <see cref="EvaluationContext"/></param>
        public abstract void ReevaluateIfNecessary(EvaluationContext evaluationContext);

        /// <summary>
        /// Saves a "logical" or "preprocessed" project file, that includes all the imported 
        /// files as if they formed a single file.
        /// </summary>
        public virtual void SaveLogicalProject(TextWriter writer) { throw new NotImplementedException(); }

        /// <summary>
        /// Starts a build using this project, building the specified targets with the specified loggers.
        /// Returns true on success, false on failure.
        /// If build is disabled on this project, does not build, and returns false.
        /// Works on a privately cloned instance. To set or get
        /// virtual items for build purposes, clone an instance explicitly and build that.
        /// Does not modify the Project object.
        /// </summary>
        /// <param name="evaluationContext">The evaluation context to use in case reevaluation is required</param>
        public virtual bool Build(string[] targets, IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        /// <summary>
        /// Called by the project collection to indicate to this project that it is no longer loaded.
        /// </summary>
        public abstract void Unload();
    }
}
