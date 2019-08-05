// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Evaluation.Context;
    using Microsoft.Build.Execution;
    using Microsoft.Build.ObjectModelRemoting;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Logging;
    using System.Diagnostics;

    /// <summary>
    /// The C# does not really provide a easy way to efficiently implement inheritance in cases like this
    /// for abstract classes or interface, when there is a hierarchy, it is not way to share the implementation.
    /// Like if one have IFoo and IBar : IFoo (or as we do abstractFoo, abstractBar:abstractFoo) 
    /// we can provide implementation for IFoo, but we can not use that for implementations for IBar.
    /// Since no multiple inheritance or other suitable mechanism for code share across classes is supported by C#,
    /// Instead IBar implementation should fully implement both IFoo and IBar interfaces.
    ///
    /// For construction model we do have a clear hierarchy like "Object" [: ProjectElementContainer] : ProjectElement
    /// that for the purpose of linkig is supported via ObjectLink[:ProjectElementContainer]:ProjectElementLink.
    /// Now implementation of all ProjectElementLink and ProjectElementContainer link is in fact identical, but each "ObjectLink" needs to implement it separately.
    ///
    ///
    /// This approach with extension methods helps us put all implementation in one place, and only standard copy and pace "hookup" is needed for each classes.
    /// </summary>
    internal static class InheritanceImplementationHelpers
    {
        #region ProjectElementLink implementation
        public static ProjectElementContainer GetParent(this IProjectElementLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static ProjectRootElement GetContainingProject(this IProjectElementLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static string GetElementName(this IProjectElementLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static string GetOuterElement(this IProjectElementLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static bool GetExpressedAsAttribute(this IProjectElementLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static bool SetExpressedAsAttribute(this IProjectElementLinkHelper xml, bool value)
        {
            throw new NotImplementedException();
        }
        public static ProjectElement GetPreviousSibling(this IProjectElementLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static ProjectElement GetNextSibling(this IProjectElementLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static ElementLocation GetLocation(this IProjectElementLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static void CopyFrom(this IProjectElementLinkHelper xml, ProjectElement element)
        {
            throw new NotImplementedException();
        }

        public static ProjectElement CreateNewInstance(this IProjectElementLinkHelper xml, ProjectRootElement owner)
        {
            throw new NotImplementedException();
        }

        public static ElementLocation GetAttributeLocation(this IProjectElementLinkHelper xml, string attributeName)
        {
            throw new NotImplementedException();
        }

        public static string GetAttributeValue(this IProjectElementLinkHelper xml, string attributeName, bool nullIfNotExists)
        {
            throw new NotImplementedException();
        }

        public static void SetOrRemoveAttribute(this IProjectElementLinkHelper xml, string name, string value, bool allowSettingEmptyAttributes, string reason, string param)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ProjectElementContainerLink implementation
        public static int GetCount(this IProjectElementContainerLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static ProjectElement GetFirstChild(this IProjectElementContainerLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static ProjectElement GetLastChild(this IProjectElementContainerLinkHelper xml)
        {
            throw new NotImplementedException();
        }

        public static void InsertAfterChild(this IProjectElementContainerLinkHelper xml, ProjectElement child, ProjectElement reference) { throw new NotImplementedException(); }
        public static void InsertBeforeChild(this IProjectElementContainerLinkHelper xml, ProjectElement child, ProjectElement reference)
        {
            throw new NotImplementedException();
        }
        public static void AddInitialChild(this IProjectElementContainerLinkHelper xml, ProjectElement child)
        {
            throw new NotImplementedException();
        }
        public static ProjectElementContainer DeepClone(this IProjectElementContainerLinkHelper xml, ProjectRootElement factory, ProjectElementContainer parent)
        {
            throw new NotImplementedException();
        }
        public static void RemoveChild(this IProjectElementContainerLinkHelper xml, ProjectElement child)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


    internal interface IProjectElementContainerLinkHelper
    {
    }

    internal interface IProjectElementLinkHelper
    {
    }

}
