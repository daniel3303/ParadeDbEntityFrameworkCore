using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Equibles.ParadeDB.EntityFrameworkCore.Tests;

public class ExpressionTests {
    private static SqlExpression Stub(string token) => new StubSqlExpression(token);

    private sealed class StubSqlExpression : SqlExpression {
        private readonly string _token;
        public StubSqlExpression(string token) : base(typeof(string), new FakeTypeMapping()) {
            _token = token;
        }
        protected override void Print(ExpressionPrinter expressionPrinter) => expressionPrinter.Append(_token);
        public override bool Equals(object obj) => obj is StubSqlExpression other && other._token == _token;
        public override int GetHashCode() => _token.GetHashCode();
#if NET9_0_OR_GREATER
        public override System.Linq.Expressions.Expression Quote() => throw new NotSupportedException();
#endif
    }

    private sealed class FakeTypeMapping : RelationalTypeMapping {
        public FakeTypeMapping() : base("text", typeof(string)) { }
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters) => this;
    }

    [Fact]
    public void ModifiedQueryExpression_Equals_ReturnsTrueForSameInnerAndSuffix() {
        var inner = Stub("hello");
        var a = new ParadeDbModifiedQueryExpression(inner, "::pdb.fuzzy(2)", typeof(string), inner.TypeMapping);
        var b = new ParadeDbModifiedQueryExpression(inner, "::pdb.fuzzy(2)", typeof(string), inner.TypeMapping);

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ModifiedQueryExpression_Equals_ReturnsFalseForDifferentSuffix() {
        var inner = Stub("hello");
        var a = new ParadeDbModifiedQueryExpression(inner, "::pdb.fuzzy(2)", typeof(string), inner.TypeMapping);
        var b = new ParadeDbModifiedQueryExpression(inner, "::pdb.boost(2)", typeof(string), inner.TypeMapping);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void ModifiedQueryExpression_Equals_ReturnsFalseForUnrelatedType() {
        var inner = Stub("hello");
        var a = new ParadeDbModifiedQueryExpression(inner, "::pdb.fuzzy(2)", typeof(string), inner.TypeMapping);

        Assert.False(a.Equals("not an expression"));
    }

    [Fact]
    public void ModifiedQueryExpression_Print_EmitsInnerThenSuffix() {
        var inner = Stub("hello");
        var expr = new ParadeDbModifiedQueryExpression(inner, "::pdb.fuzzy(2)", typeof(string), inner.TypeMapping);

        var printer = new ExpressionPrinter();
        printer.Visit(expr);

        Assert.Contains("::pdb.fuzzy(2)", printer.ToString());
    }

    [Fact]
    public void NamedArgFunctionExpression_Equals_ReturnsTrueForSameStructure() {
        var arg = Stub("c");
        var named = Stub("<b>");

        var a = new ParadeDbNamedArgFunctionExpression("pdb.snippet",
            [arg], [("start_tag", named)], typeof(string), arg.TypeMapping);
        var b = new ParadeDbNamedArgFunctionExpression("pdb.snippet",
            [arg], [("start_tag", named)], typeof(string), arg.TypeMapping);

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void NamedArgFunctionExpression_Equals_ReturnsFalseForDifferentFunctionName() {
        var arg = Stub("c");
        var a = new ParadeDbNamedArgFunctionExpression("pdb.snippet", [arg], [], typeof(string), arg.TypeMapping);
        var b = new ParadeDbNamedArgFunctionExpression("pdb.snippets", [arg], [], typeof(string), arg.TypeMapping);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void NamedArgFunctionExpression_Equals_ReturnsFalseForDifferentNamedArgName() {
        var arg = Stub("c");
        var v1 = Stub("<b>");
        var a = new ParadeDbNamedArgFunctionExpression("pdb.snippet",
            [arg], [("start_tag", v1)], typeof(string), arg.TypeMapping);
        var b = new ParadeDbNamedArgFunctionExpression("pdb.snippet",
            [arg], [("end_tag", v1)], typeof(string), arg.TypeMapping);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void NamedArgFunctionExpression_Print_EmitsPositionalAndNamedArgs() {
        var positional = Stub("c");
        var named = Stub("<b>");
        var expr = new ParadeDbNamedArgFunctionExpression("pdb.snippet",
            [positional], [("start_tag", named)], typeof(string), positional.TypeMapping);

        var printer = new ExpressionPrinter();
        printer.Visit(expr);
        var output = printer.ToString();

        Assert.Contains("pdb.snippet(", output);
        Assert.Contains("start_tag => ", output);
        Assert.Contains(")", output);
    }

#if NET9_0_OR_GREATER
    [Fact]
    public void ModifiedQueryExpression_Quote_Throws() {
        var inner = Stub("hello");
        var expr = new ParadeDbModifiedQueryExpression(inner, "::pdb.fuzzy(2)", typeof(string), inner.TypeMapping);

        Assert.Throws<NotSupportedException>(() => expr.Quote());
    }

    [Fact]
    public void NamedArgFunctionExpression_Quote_Throws() {
        var arg = Stub("c");
        var expr = new ParadeDbNamedArgFunctionExpression("pdb.snippet", [arg], [], typeof(string), arg.TypeMapping);

        Assert.Throws<NotSupportedException>(() => expr.Quote());
    }
#endif
}
