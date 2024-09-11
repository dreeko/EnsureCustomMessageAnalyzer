using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EnsureCustomMessageAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnsureCustomMessageAnalyzerAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "EnsureCustomMessageAnalyzer";

    private const string Category = "Naming";

    // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
    private static readonly LocalizableString Title = new LocalizableResourceString(
        nameof(Resources.AnalyzerTitle),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
        nameof(Resources.AnalyzerMessageFormat),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly LocalizableString Description = new LocalizableResourceString(
        nameof(Resources.AnalyzerDescription),
        Resources.ResourceManager,
        typeof(Resources)
    );

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        true,
        Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;

        // Check if the method being called is a Fluent Validation rule method
        if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccessExpr)
            return;

        var methodName = memberAccessExpr.Name.Identifier.Text;
        if (!IsFluentValidationRuleMethod(methodName))
            return;
        var hasCustomMessage = ContainsWithMessageCall(invocationExpr);

        if (hasCustomMessage) return;

        var diagnostic = Diagnostic.Create(
            Rule,
            memberAccessExpr.GetLocation(),
            methodName
        );
        context.ReportDiagnostic(diagnostic);
    }

    private static bool ContainsWithMessageCall(InvocationExpressionSyntax invocationExpr)
    {
        var currentExpr = invocationExpr;
        while (currentExpr != null)
        {
            if (currentExpr.Expression is MemberAccessExpressionSyntax memberAccessExpr)
            {
                if (memberAccessExpr.Name.Identifier.Text == "WithMessage") return true;

                if (memberAccessExpr.Expression is InvocationExpressionSyntax nextInvocation)
                {
                    currentExpr = nextInvocation;
                    continue;
                }
            }

            break;
        }

        return false;
    }

    private static bool IsFluentValidationRuleMethod(string methodName)
    {
        // Add more Fluent Validation rule methods as needed
        var ruleMethods = new[]
        {
            "NotEmpty",
            "NotNull",
            "Length",
            "Matches",
            "InclusiveBetween",
            "ExclusiveBetween"
        };
        return ruleMethods.Contains(methodName);
    }
}