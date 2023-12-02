﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Structure;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Structure;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Structure;

[Trait(Traits.Feature, Traits.Features.Outlining)]
public class ArgumentListSyntaxStructureTests : AbstractCSharpSyntaxNodeStructureTests<ArgumentListSyntax>
{
    internal override AbstractSyntaxStructureProvider CreateProvider() => new ArgumentListStructureProvider();

    [Fact]
    public async Task TestInvocationExpressionSingleLine()
    {
        var code = """
            var x = M$$();
            """;

        await VerifyBlockSpansAsync(code);
    }

    [Fact]
    public async Task TestInvocationExpressionThreeLines()
    {
        var code = """
            var x = M$${|span:(
                "",
                "")|};
            """;

        await VerifyBlockSpansAsync(code,
            Region("span", CSharpStructureHelpers.Ellipsis, autoCollapse: false));
    }

    [Fact]
    public async Task TestObjectCreationSingleLine()
    {
        var code = """
            var x = new C$$();
            """;

        await VerifyBlockSpansAsync(code);
    }

    [Fact]
    public async Task TestObjectCreationThreeLines()
    {
        var code = """
            var x = new C$${|span:(
                "",
                "")|};
            """;

        await VerifyBlockSpansAsync(code,
            Region("span", CSharpStructureHelpers.Ellipsis, autoCollapse: false));
    }

    [Fact]
    public async Task TestImplicitObjectCreationSingleLine()
    {
        var code = """
            C x = new$$();
            """;

        await VerifyBlockSpansAsync(code);
    }

    [Fact]
    public async Task TestImplicitObjectCreationThreeLines()
    {
        var code = """
            C x = new$${|span:(
                "",
                "")|};
            """;

        await VerifyBlockSpansAsync(code,
            Region("span", CSharpStructureHelpers.Ellipsis, autoCollapse: false));
    }
}
