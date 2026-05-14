using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.Tests;

/// <summary>
/// Verifies the CLR stub methods on <see cref="ParadeDbFunctions"/> all throw
/// when called outside of an EF Core LINQ context. This exercises the bodies
/// that the translator never touches (LINQ provider replaces the call before
/// the body runs).
/// </summary>
public class ParadeDbFunctionsTests
{
    private readonly DbFunctions _ef = EF.Functions;

    [Fact]
    public void Matches_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.Matches("col", "q"));
    }

    [Fact]
    public void MatchesAll_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesAll("col", "q"));
    }

    [Fact]
    public void MatchesPhrase_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesPhrase("col", "q"));
    }

    [Fact]
    public void MatchesPhrase_with_slop_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesPhrase("col", "q", 2));
    }

    [Fact]
    public void MatchesTerm_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesTerm("col", "q"));
    }

    [Fact]
    public void MatchesTermSet_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesTermSet("col", "a", "b"));
    }

    [Fact]
    public void MatchesFuzzy_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesFuzzy("col", "q", 2));
    }

    [Fact]
    public void MatchesFuzzy_full_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _ef.MatchesFuzzy("col", "q", 2, true, false)
        );
    }

    [Fact]
    public void MatchesAllFuzzy_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesAllFuzzy("col", "q", 2));
    }

    [Fact]
    public void MatchesAllFuzzy_full_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _ef.MatchesAllFuzzy("col", "q", 1, false, true)
        );
    }

    [Fact]
    public void MatchesTermFuzzy_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesTermFuzzy("col", "q", 1));
    }

    [Fact]
    public void MatchesTermFuzzy_full_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _ef.MatchesTermFuzzy("col", "q", 2, true, true)
        );
    }

    [Fact]
    public void MatchesBoosted_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesBoosted("col", "q", 2.0));
    }

    [Fact]
    public void MatchesAllBoosted_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesAllBoosted("col", "q", 1.5));
    }

    [Fact]
    public void MatchesFuzzyBoosted_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MatchesFuzzyBoosted("col", "q", 2, 2.0));
    }

    [Fact]
    public void MatchesAllFuzzyBoosted_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _ef.MatchesAllFuzzyBoosted("col", "q", 1, 3.0)
        );
    }

    [Fact]
    public void Score_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.Score(new object()));
    }

    [Fact]
    public void Snippet_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.Snippet("col"));
    }

    [Fact]
    public void Snippet_params_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.Snippet("col", "<b>", "</b>", 100));
    }

    [Fact]
    public void Snippets_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.Snippets("col", 15, 5, 0));
    }

    [Fact]
    public void Parse_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.Parse(new object(), "q"));
    }

    [Fact]
    public void Parse_with_options_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.Parse(new object(), "q", true, true));
    }

    [Fact]
    public void Regex_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.Regex("col", "neuro.*"));
    }

    [Fact]
    public void PhrasePrefix_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.PhrasePrefix("col", "a", "b"));
    }

    [Fact]
    public void PhrasePrefix_max_expansions_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.PhrasePrefix("col", 10, "a", "b"));
    }

    [Fact]
    public void MoreLikeThis_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MoreLikeThis(new object(), 1));
    }

    [Fact]
    public void MoreLikeThis_with_fields_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.MoreLikeThis(new object(), 1, "f"));
    }

    [Fact]
    public void JsonSearch_throws_when_called_directly()
    {
        Assert.Throws<InvalidOperationException>(() => _ef.JsonSearch(new object(), "{}"));
    }
}
