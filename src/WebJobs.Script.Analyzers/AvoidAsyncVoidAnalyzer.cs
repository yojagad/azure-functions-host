using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Microsoft.Azure.Functions.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AvoidAsyncVoidAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(DiagnosticDescriptors.AsyncVoidDiscouraged); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            
            // TODO: use CompilationStartAction to only register analyzer on Function methods?

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (!methodSymbol.IsFunction(context) || !methodSymbol.IsAsync)
            {
                return;
            }

            if (methodSymbol.ReturnsVoid)
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.AsyncVoidDiscouraged, methodSymbol.Locations[0]);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
