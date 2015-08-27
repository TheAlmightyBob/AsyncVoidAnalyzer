using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AsyncVoidAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncVoidAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AsyncVoidAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Reliability";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeAwait, SyntaxKind.AwaitExpression);
        }

        private void AnalyzeAwait(SyntaxNodeAnalysisContext context)
        {
            SyntaxNode node = context.Node;

            // Find the first ancestor that's a method/lambda or try statement
            //
            while (node != null && !(node is MethodDeclarationSyntax) && !(node is TryStatementSyntax) && !(node is AnonymousFunctionExpressionSyntax))
            {
                node = node.Parent;
            }

            if (node == null || node is TryStatementSyntax)
            {
                // Exceptions are already being caught or we're not in a method/lambda.. either way, move along
                //
                return;
            }

            // Figure out if the method/lambda is void (we just assume it's async since it has an await...
            // ...if not, VS will already be flagging that)
            //
            bool isVoid = false;

            var method = node as MethodDeclarationSyntax;

            if (method != null)
            {
                isVoid = (method.ReturnType as PredefinedTypeSyntax)?.Keyword.Kind() == SyntaxKind.VoidKeyword;
            }
            else if (node is AnonymousFunctionExpressionSyntax)
            {
                var model = context.SemanticModel.Compilation.GetSemanticModel(node.SyntaxTree);
                var info = model.GetSymbolInfo(node);

                isVoid = (info.Symbol as IMethodSymbol)?.ReturnsVoid == true;
            }

            if (isVoid)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), context.Node.ToString()));
            }
        }
    }
}
