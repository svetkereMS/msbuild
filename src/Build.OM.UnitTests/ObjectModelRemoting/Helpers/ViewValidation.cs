// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Build.Construction;
    using Microsoft.Build.ObjectModelRemoting;
    using Microsoft.Build.Evaluation;
    using Xunit;
    using System.Runtime.ExceptionServices;
    using System.Xml.Schema;
    using System.Collections;
    using Microsoft.Build.Framework;

    internal enum ObjectType
    {
        Real = 1,
        View = 2
    }

    internal class LinkPair<T>
    {
        public LinkPair(T view, T real)
        {
            ViewValidation.VerifyLinkedNotNull(view);
            ViewValidation.VerifyNotLinkedNotNull(real);
            this.View = view;
            this.Real = real;
        }

        public T Get(ObjectType type) => type == ObjectType.Real ? this.Real : this.View;
        public T View { get; }
        public T Real { get; }

        public void VerifySame(LinkPair<T> other)
        {
            Assert.Same((object)this.View, (object)other.View);
            Assert.Same((object)this.Real, (object)other.Real);
        }

        public void VerifySetter(string newValue, Func<T, string> getter, Action<T, string> setter)
        {
            var newValue1 = newValue.Ver(1);
            var current = getter(this.Real);
            Assert.Equal(current, getter(this.View));
            Assert.NotEqual(current, newValue);
            Assert.NotEqual(current, newValue1);

            // set via the view
            setter(this.View, newValue1);

            Assert.Equal(newValue1, getter(this.View));
            Assert.Equal(newValue1, getter(this.Real));

            // set via the real.
            setter(this.Real, newValue);

            Assert.Equal(newValue, getter(this.View));
            Assert.Equal(newValue, getter(this.Real));
        }

        public virtual void Verify()
        {
            ViewValidation.VerifyFindType(this.View, this.Real);
        }
    }

    internal static partial class ViewValidation
    {
        private static bool VerifyCheckType<T>(object view, object real, Action<T, T> elementValidator)
        {
            if (view is T viewTypedXml)
            {
                Assert.True(real is T);
                elementValidator(viewTypedXml, (T)real);
                return true;
            }
            else
            {
                Assert.False(real is T);
                return false;
            }
        }

        // "Slow" Verify, probing all known link types
        public static void VerifyFindType(object view, object real)
        {
            if (view == null && real == null) return;
            VerifyLinkedNotNull(view);
            VerifyNotLinkedNotNull(real);

            // construction
            if (VerifyCheckType<ProjectMetadataElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectChooseElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectWhenElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectOtherwiseElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectTaskElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectOutputElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectUsingTaskBodyElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectUsingTaskParameterElement>(view, real, Verify)) return;
            if (VerifyCheckType<UsingTaskParameterGroupElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectUsingTaskElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectTargetElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectRootElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectExtensionsElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectImportElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectImportGroupElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectItemDefinitionElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectItemDefinitionGroupElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectItemElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectItemGroupElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectPropertyElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectPropertyGroupElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectSdkElement>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectOnErrorElement>(view, real, Verify)) return;

            // evaluation
            if (VerifyCheckType<ProjectProperty>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectMetadata>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectItemDefinition>(view, real, Verify)) return;
            if (VerifyCheckType<ProjectItem>(view, real, Verify)) return;
            if (VerifyCheckType<Project>(view, real, Verify)) return;

            throw new NotImplementedException($"Unknown type:{view.GetType().Name}");
        }

    }
}
