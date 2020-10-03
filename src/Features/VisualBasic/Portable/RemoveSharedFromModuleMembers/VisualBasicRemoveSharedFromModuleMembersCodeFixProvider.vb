﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Collections.Immutable
Imports System.Composition
Imports System.Diagnostics.CodeAnalysis
Imports System.Threading
Imports Microsoft.CodeAnalysis.CodeActions
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Editing
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic.RemoveSharedFromModuleMembers
    <ExportCodeFixProvider(LanguageNames.VisualBasic, Name:=NameOf(VisualBasicRemoveSharedFromModuleMembersCodeFixProvider)), [Shared]>
    Friend NotInheritable Class VisualBasicRemoveSharedFromModuleMembersCodeFixProvider
        Inherits SyntaxEditorBasedCodeFixProvider

        <ImportingConstructor>
        <SuppressMessage("RoslynDiagnosticsReliability", "RS0033:Importing constructor should be [Obsolete]", Justification:="Used in test code: https://github.com/dotnet/roslyn/issues/42814")>
        Public Sub New()
        End Sub

        ' Methods in a Module cannot be declared '{0}'.
        Private Const BC30433 As String = NameOf(BC30433)

        ' Events in a Module cannot be declared '{0}'.
        Private Const BC30434 As String = NameOf(BC30434)

        ' Properties in a Module cannot be declared '{0}'.
        Private Const BC30503 As String = NameOf(BC30503)

        ' Variables in Modules cannot be declared '{0}'.
        Private Const BC30593 As String = NameOf(BC30593)

        Public Overrides ReadOnly Property FixableDiagnosticIds As ImmutableArray(Of String) = ImmutableArray.Create(
            BC30433, BC30434, BC30503, BC30593)

        Friend Overrides ReadOnly Property CodeFixCategory As CodeFixCategory = CodeFixCategory.Compile

        Public Overrides Function RegisterCodeFixesAsync(context As CodeFixContext) As Task
            context.RegisterCodeFix(
                New MyCodeAction(Function(ct) FixAsync(context.Document, context.Diagnostics(0), context.CancellationToken)),
                context.Diagnostics)
            Return Task.CompletedTask
        End Function

        Protected Overrides Function FixAllAsync(document As Document, diagnostics As ImmutableArray(Of Diagnostic), editor As SyntaxEditor, cancellationToken As CancellationToken) As Task
            For Each diagnostic In diagnostics
                Dim tokenToRemove = diagnostic.Location.FindToken(cancellationToken)
                If Not tokenToRemove.IsKind(SyntaxKind.SharedKeyword) Then
                    Continue For
                End If
                Dim node = diagnostic.Location.FindNode(cancellationToken)
                Dim newNode = GetReplacement(node, tokenToRemove)
                editor.ReplaceNode(node, newNode)
            Next
            Return Task.CompletedTask
        End Function

        Private Shared Function GetReplacement(node As SyntaxNode, tokenToRemove As SyntaxToken) As SyntaxNode
            If TypeOf node Is FieldDeclarationSyntax Then
                Dim field = DirectCast(node, FieldDeclarationSyntax)
                Return field.WithModifiers(field.Modifiers.Remove(tokenToRemove))
            ElseIf TypeOf node Is MethodBaseSyntax Then
                Dim method = DirectCast(node, MethodBaseSyntax)
                Return method.WithModifiers(method.Modifiers.Remove(tokenToRemove))
            End If
            Return node
        End Function

        Private Class MyCodeAction
            Inherits CodeAction.DocumentChangeAction

            Public Sub New(createChangedDocument As Func(Of CancellationToken, Task(Of Document)))
                MyBase.New(FeaturesResources.Remove_shared_keyword_from_module_member, createChangedDocument, FeaturesResources.Remove_shared_keyword_from_module_member)
            End Sub
        End Class
    End Class
End Namespace
