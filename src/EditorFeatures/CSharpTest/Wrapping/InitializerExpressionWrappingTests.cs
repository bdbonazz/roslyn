﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Wrapping;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Wrapping
{
    [Trait(Traits.Feature, Traits.Features.CodeActionsWrapping)]
    public class InitializerExpressionWrappingTests : AbstractWrappingTests
    {
        protected override CodeRefactoringProvider CreateCodeRefactoringProvider(Workspace workspace, TestParameters parameters)
            => new CSharpWrappingCodeRefactoringProvider();

        [Fact]
        public async Task TestNoWrappingSuggestions()
        {
            await TestMissingAsync(
                """
                class C {
                    void Bar() {
                        var test = new[] [||]{ 1 };
                    }
                }
                """);
        }

        [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/59624")]
        public async Task TestNoWrappingSuggestions_TrailingComma()
        {
            await TestMissingAsync(
                """
                class C {
                    void Bar() {
                        var test = new[] [||]{ 1, };
                    }
                }
                """);
        }

        [Fact]
        public async Task TestWrappingShortInitializerExpression()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        var test = new[] [||]{ 1, 2 };
                    }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[]
                        {
                            1,
                            2
                        };
                    }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[]
                        {
                            1, 2
                        };
                    }
                }
                """);
        }

        [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/59624")]
        public async Task TestWrappingShortInitializerExpression_TrailingComma()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        var test = new[] [||]{ 1, 2, };
                    }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[]
                        {
                            1,
                            2,
                        };
                    }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[]
                        {
                            1, 2,
                        };
                    }
                }
                """);
        }

        [Fact]
        public async Task TestWrappingLongIntializerExpression()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        var test = new[] [||]{ "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog" };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[]
                        {
                            "the",
                            "quick",
                            "brown",
                            "fox",
                            "jumps",
                            "over",
                            "the",
                            "lazy",
                            "dog"
                        };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[]
                        {
                            "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog"
                        };
                     }
                }
                """
);
        }

        [Fact]
        public async Task TestWrappingMultiLineLongIntializerExpression()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        var test = new[] [||]{ "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog", "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog" };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[]
                        {
                            "the",
                            "quick",
                            "brown",
                            "fox",
                            "jumps",
                            "over",
                            "the",
                            "lazy",
                            "dog",
                            "the",
                            "quick",
                            "brown",
                            "fox",
                            "jumps",
                            "over",
                            "the",
                            "lazy",
                            "dog"
                        };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[]
                        {
                            "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog", "the", "quick", "brown", "fox",
                            "jumps", "over", "the", "lazy", "dog"
                        };
                     }
                }
                """
);
        }

        [Fact]
        public async Task TestShortInitializerExpressionRefactorings()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        var test = new[]
                        [||]{
                            1,
                            2
                        };
                    }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[] { 1, 2 };
                    }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[]
                        {
                            1, 2
                        };
                    }
                }
                """);
        }

        [Fact]
        public async Task TestLongIntializerExpressionRefactorings()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        var test = new[]
                        [||]{
                            "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog"
                        };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[]
                        {
                            "the",
                            "quick",
                            "brown",
                            "fox",
                            "jumps",
                            "over",
                            "the",
                            "lazy",
                            "dog"
                        };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new[] { "the", "quick", "brown", "fox", "jumps", "over", "the", "lazy", "dog" };
                     }
                }
                """
);
        }

        [Fact]
        public async Task TestListWrappingIntializerExpression()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        var test = new List<int> [||]{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new List<int>
                        {
                            0,
                            1,
                            2,
                            3,
                            4,
                            5,
                            6,
                            7,
                            8,
                            9
                        };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new List<int>
                        {
                            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
                        };
                     }
                }
                """
);
        }

        [Fact]
        public async Task TestWrappedListIntializerExpression()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        var test = new List<int>
                        [||]{
                            0,
                            1,
                            2,
                            3,
                            4,
                            5,
                            6,
                            7,
                            8,
                            9
                        };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new List<int>
                        {
                            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
                        };
                     }
                }
                """
);
        }

        [Fact]
        public async Task TestObjectWrappingIntializerExpression()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        var test = new List<A> [||]{ new A { B = 1, C = 1 }, new A { B = 2, C = 2 }, new A { B = 3, C = 3 } };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new List<A>
                        {
                            new A { B = 1, C = 1 },
                            new A { B = 2, C = 2 },
                            new A { B = 3, C = 3 }
                        };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new List<A>
                        {
                            new A { B = 1, C = 1 }, new A { B = 2, C = 2 }, new A { B = 3, C = 3 }
                        };
                     }
                }
                """
);
        }

        [Fact]
        public async Task TestWrappedObjectIntializerExpression()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        var test = new List<A>
                        [||]{
                            new A { B = 1, C = 1 },
                            new A { B = 2, C = 2 },
                            new A { B = 3, C = 3 }
                        };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new List<A> { new A { B = 1, C = 1 }, new A { B = 2, C = 2 }, new A { B = 3, C = 3 } };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        var test = new List<A>
                        {
                            new A { B = 1, C = 1 }, new A { B = 2, C = 2 }, new A { B = 3, C = 3 }
                        };
                     }
                }
                """
);
        }

        [Fact]
        public async Task TestReturnIntializerExpression()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        return new List<int> [||]{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        return new List<int>
                        {
                            0,
                            1,
                            2,
                            3,
                            4,
                            5,
                            6,
                            7,
                            8,
                            9
                        };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        return new List<int>
                        {
                            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
                        };
                     }
                }
                """
);
        }

        [Fact]
        public async Task TestWrappedReturnIntializerExpression()
        {
            await TestAllWrappingCasesAsync(
                """
                class C {
                    void Bar() {
                        return new List<int>
                        [||]{
                            0,
                            1,
                            2,
                            3,
                            4,
                            5,
                            6,
                            7,
                            8,
                            9
                        };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        return new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                     }
                }
                """,
                """
                class C {
                    void Bar() {
                        return new List<int>
                        {
                            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
                        };
                     }
                }
                """
);
        }

        [Fact]
        public async Task TestClassPropertyIntializerExpressionRefactorings()
        {
            await TestAllWrappingCasesAsync(
                """
                public class C {
                    public List<int> B => new List<int> [||]{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                }
                """,
                """
                public class C {
                    public List<int> B => new List<int>
                    {
                        0,
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9
                    };
                }
                """,
                """
                public class C {
                    public List<int> B => new List<int>
                    {
                        0, 1, 2, 3, 4, 5, 6, 7, 8, 9
                    };
                }
                """
);
        }

        [Fact]
        public async Task TestWrappedClassPropertyIntializerExpressionRefactorings()
        {
            await TestAllWrappingCasesAsync(
                """
                public class C {
                    public List<int> B => new List<int>
                    [||]{
                        0,
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9
                    };
                }
                """,
                """
                public class C {
                    public List<int> B => new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                }
                """,
                """
                public class C {
                    public List<int> B => new List<int>
                    {
                        0, 1, 2, 3, 4, 5, 6, 7, 8, 9
                    };
                }
                """
);
        }

        [Fact]
        public async Task TestArgumentIntializerExpressionRefactorings()
        {
            await TestAllWrappingCasesAsync(
                """
                public void F() {
                    var result = fakefunction(new List<int> [||]{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                }
                """,
                """
                public void F() {
                    var result = fakefunction(new List<int>
                    {
                        0,
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9
                    });
                }
                """,
                """
                public void F() {
                    var result = fakefunction(new List<int>
                    {
                        0, 1, 2, 3, 4, 5, 6, 7, 8, 9
                    });
                }
                """
);
        }

        [Fact]
        public async Task TestWrappedArgumentIntializerExpressionRefactorings()
        {
            await TestAllWrappingCasesAsync(
                """
                public void F() {
                    var result = fakefunction(new List<int>
                    [||]{
                        0,
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9
                    });
                }
                """,
                """
                public void F() {
                    var result = fakefunction(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
                }
                """,
                """
                public void F() {
                    var result = fakefunction(new List<int>
                    {
                        0, 1, 2, 3, 4, 5, 6, 7, 8, 9
                    });
                }
                """
);
        }

        [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/63732")]
        public async Task TestMissingStartToken()
        {
            await TestMissingAsync(
                """
                class C {
                    void Bar() {
                        var test = new[] [||] 1, 2 };
                    }
                }
                """);
        }

        [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/63732")]
        public async Task TestMissingEndToken1()
        {
            await TestMissingAsync(
                """
                class C {
                    void Bar() {
                        var test = new[] [||]{ 1, 2
                        return;
                    }
                }
                """);
        }

        [Fact, WorkItem("https://github.com/dotnet/roslyn/issues/63732")]
        public async Task TestMissingEndToken2()
        {
            await TestMissingAsync(
                """
                class C {
                    void Bar() {
                        var test = new[] [||]{ 1, 2 ;
                    }
                }
                """);
        }
    }
}
