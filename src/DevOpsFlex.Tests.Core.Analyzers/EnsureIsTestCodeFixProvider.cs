﻿namespace DevOpsFlex.Tests.Core.Analyzers
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Provides 'IsUnit' and 'IsIntegration' codefixes for tests without and Is* decorator.
    /// This is the <see cref="CodeFixProvider"/> for the <see cref="EnsureIsTestAnalyzer"/>.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EnsureIsTestCodeFixProvider)), Shared]
    public class EnsureIsTestCodeFixProvider : CodeFixProvider
    {
        private const string UnitTitle = "This is a Unit Test";
        private const string IntegrationTitle = "This is an Integration Test";
        private const string EswTestNamespace = "Esw.UnitTest.Common";

        /// <summary>
        /// A list of diagnostic IDs that this provider can provider fixes for.
        /// </summary>
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EnsureIsTestAnalyzer.DiagnosticId);

        /// <summary>
        /// Gets an optional <see cref="T:Microsoft.CodeAnalysis.CodeFixes.FixAllProvider" /> that can fix all/multiple occurrences of diagnostics fixed by this code fix provider.
        /// Return null if the provider doesn't support fix all/multiple occurrences.
        /// Otherwise, you can return any of the well known fix all providers from <see cref="T:Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders" /> or implement your own fix all provider.
        /// </summary>
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>
        /// Computes one or more fixes for the specified <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" />.
        /// </summary>
        /// <param name="context">
        /// A <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" /> containing context information about the diagnostics to fix.
        /// The context must only contain diagnostics with an <see cref="P:Microsoft.CodeAnalysis.Diagnostic.Id" /> included in the <see cref="P:Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider.FixableDiagnosticIds" /> for the current provider.
        /// </param>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(CodeAction.Create(UnitTitle, c => AddIsTestAttribute(context.Document, declaration, "IsUnit", c), UnitTitle), diagnostic);
            context.RegisterCodeFix(CodeAction.Create(IntegrationTitle, c => AddIsTestAttribute(context.Document, declaration, "IsIntegration", c), IntegrationTitle), diagnostic);
        }

        private static async Task<Document> AddIsTestAttribute(Document document, SyntaxNode node, string attribute, CancellationToken cancellationToken)
        {
            var rewriter = new IsTestRewriter(attribute);
            var newMethod = rewriter.Visit(node);

            var docRoot = await document.GetSyntaxRootAsync(cancellationToken);
            docRoot = docRoot.ReplaceNode(node, newMethod);

            var compilation = docRoot as CompilationUnitSyntax;

            if (compilation == null) return document.WithSyntaxRoot(docRoot);

            if (compilation.Usings.All(u => u.Name.ToString() != EswTestNamespace))
            {
                docRoot = compilation.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(EswTestNamespace)));
            }

            return document.WithSyntaxRoot(docRoot);
        }
    }
}