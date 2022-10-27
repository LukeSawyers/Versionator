namespace Versionator.Core;

public static class StringExtensions
{
    public static string ToLiteral(this string valueTextForCompiler)
    {
        return Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(valueTextForCompiler, false);
    }
}