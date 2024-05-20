﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This is consumed as 'generated' code in a source package and therefore requires an explicit nullable enable
#nullable enable

using System;

namespace Microsoft.CommonLanguageServerProtocol.Framework;

internal partial class TypeRef
{
    private sealed class LazyTypeRef(string typeName) : TypeRef(typeName)
    {
        private readonly object _gate = new();
        private Type? _type;

        protected override Type GetResolvedTypeCore(ITypeRefResolver resolver)
        {
            lock (_gate)
            {
                return _type ??= resolver.Resolve(this);
            }
        }
    }
}
