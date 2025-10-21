using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynaMock.SourceGeneration;

internal class MockableGeneratorSyntaxReceiver : ISyntaxReceiver
{
    public List<TypeDeclarationSyntax> CandidateTypes { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is TypeDeclarationSyntax typeDecl)
        {
            CandidateTypes.Add(typeDecl);
        }
    }
}