﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Editor.CSharp.CommentSelection;
using Microsoft.CodeAnalysis.Editor.Implementation.CommentSelection;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.CommentSelection
{
    [UseExportProvider]
    public class CSharpToggleBlockCommentCommandHandlerTests : AbstractToggleBlockCommentTestBase
    {

        [WpfFact, Trait(Traits.Feature, Traits.Features.ToggleBlockComment)]
        public void AddComment_CaretInsideToken()
        {
            var markup =
@"
class C
{
    void M()
    {
        va$$r i = 1;
    }
}";
            var expected =
@"
class C
{
    void M()
    {
        var/**/ i = 1;
    }
}";

            var expectedSelectedSpans = new[]
            {
                Span.FromBounds(46, 50)
            };
            ToggleBlockComment(markup, expected, expectedSelectedSpans);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.ToggleBlockComment)]
        public void AddComment_CaretInsideOperatorToken()
        {
            var markup = @"
class C
{
    void M()
    {
        Func<int, bool> myFunc = x =$$> x == 5;
    }
}";
            var expected =
@"
class C
{
    void M()
    {
        Func<int, bool> myFunc = x =>/**/ x == 5;
    }
}";

            var expectedSelectedSpans = new[]
            {
                Span.FromBounds(72, 76)
            };
            ToggleBlockComment(markup, expected, expectedSelectedSpans);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.ToggleBlockComment)]
        public void AddComment_CommentMarkerStringBeforeSelection()
        {
            var markup =
@"
class C
{
    void M()
    {
        string s = '/*';
        [|var j = 2;
        var k = 3;|]
    }
}";
            var expected =
@"
class C
{
    void M()
    {
        string s = '/*';
        /*var j = 2;
        var k = 3;*/
    }
}";

            var expectedSelectedSpans = new[]
            {
                Span.FromBounds(69, 103)
            };
            ToggleBlockComment(markup, expected, expectedSelectedSpans);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.ToggleBlockComment)]
        public void AddComment_DirectiveWithCommentInsideSelection()
        {
            var markup =
@"
class C
{
    void M()
    {
        [|var i = 1;
#if false
        /*var j = 2;*/
#endif
        var k = 3;|]
    }
}";
            var expected =
@"
class C
{
    void M()
    {
        /*var i = 1;
#if false
        /*var j = 2;*/
#endif
        var k = 3;*/
    }
}";

            var expectedSelectedSpans = new[]
            {
                Span.FromBounds(43, 120),
            };
            ToggleBlockComment(markup, expected, expectedSelectedSpans);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.ToggleBlockComment)]
        public void AddComment_MarkerInsideSelection()
        {
            var markup =
@"
class C
{
    void M()
    {
        [|var i = 1;
        string s = '/*';
        var k = 3;|]
    }
}";
            var expected =
@"
class C
{
    void M()
    {
        /*var i = 1;
        string s = '/*';
        var k = 3;*/
    }
}";

            var expectedSelectedSpans = new[]
            {
                Span.FromBounds(43, 103)
            };
            ToggleBlockComment(markup, expected, expectedSelectedSpans);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.ToggleBlockComment)]
        public void AddComment_CloseCommentMarkerStringInSelection()
        {
            var markup =
@"
class C
{
    void M()
    {
        [|var i = 1;
        string s = '/*';
        var k = 3;|]
    }
}";
            var expected =
@"
class C
{
    void M()
    {
        /*var i = 1;
        string s = '/*';
        var k = 3;*/
    }
}";

            var expectedSelectedSpans = new[]
            {
                Span.FromBounds(43, 103)
            };
            ToggleBlockComment(markup, expected, expectedSelectedSpans);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.ToggleBlockComment)]
        public void AddComment_CommentMarkerStringAfterSelection()
        {
            var markup =
@"
class C
{
    void M()
    {
        [|var i = 1;
        var j = 2;|]
        string s = '*/';
    }
}";
            var expected =
@"
class C
{
    void M()
    {
        /*var i = 1;
        var j = 2;*/
        string s = '*/';
    }
}";

            var expectedSelectedSpans = new[]
            {
                Span.FromBounds(43, 77)
            };
            ToggleBlockComment(markup, expected, expectedSelectedSpans);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.ToggleBlockComment)]
        public void RemoveComment_CommentMarkerStringNearSelection()
        {
            var markup =
@"
class C
{
    void M()
    {
        string s = '/*';
        [|/*var i = 1;
        var j = 2;
        var k = 3;*/|]
    }
}";
            var expected =
@"
class C
{
    void M()
    {
        string s = '/*';
        var i = 1;
        var j = 2;
        var k = 3;
    }
}";

            var expectedSelectedSpans = new[]
            {
                Span.FromBounds(69, 119)
            };
            ToggleBlockComment(markup, expected, expectedSelectedSpans);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.ToggleBlockComment)]
        public void RemoveComment_CommentMarkerStringInSelection()
        {
            var markup =
@"
class C
{
    void M()
    {
        [|/*string s = '/*';*/|]
    }
}";
            var expected =
@"
class C
{
    void M()
    {
        string s = '/*';
    }
}";

            var expectedSelectedSpans = new[]
            {
                Span.FromBounds(43, 59)
            };
            ToggleBlockComment(markup, expected, expectedSelectedSpans);
        }

        internal override ToggleBlockCommentCommandHandler GetToggleBlockCommentCommandHandler(TestWorkspace workspace)
        {
            return new CSharpToggleBlockCommentCommandHandler(
                    workspace.ExportProvider.GetExportedValue<ITextUndoHistoryRegistry>(),
                    workspace.ExportProvider.GetExportedValue<IEditorOperationsFactoryService>());
        }
    }
}
