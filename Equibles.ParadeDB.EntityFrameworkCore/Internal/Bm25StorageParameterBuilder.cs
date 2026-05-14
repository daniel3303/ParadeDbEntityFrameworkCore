using System.Text.Json.Nodes;

namespace Equibles.ParadeDB.EntityFrameworkCore.Internal;

internal static class Bm25StorageParameterBuilder
{
    public static string Serialize(JsonObject root) =>
        root.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = false });

    public static JsonObject BuildTextField(Bm25TextAttribute attr, string propertyName)
    {
        ValidateTokenizerParameters(
            propertyName,
            attr.Tokenizer,
            attr.MinGram,
            attr.MaxGram,
            attr.PrefixOnly,
            attr.RegexPattern
        );

        var node = new JsonObject();
        AddTokenizerKey(
            node,
            attr.Tokenizer,
            attr.MinGram,
            attr.MaxGram,
            attr.PrefixOnly,
            attr.RegexPattern,
            attr.Stemmer,
            attr.StopwordsLanguage
        );
        AddSharedKeys(node, attr.Fast, attr.Record, attr.Indexed, attr.Fieldnorms);
        return node;
    }

    public static JsonObject BuildNumericField(Bm25NumericAttribute attr)
    {
        var node = new JsonObject();
        if (attr.Fast)
            node["fast"] = true;
        if (!attr.Indexed)
            node["indexed"] = false;
        return node;
    }

    public static JsonObject BuildBooleanField(Bm25BooleanAttribute attr)
    {
        var node = new JsonObject();
        if (attr.Fast)
            node["fast"] = true;
        if (!attr.Indexed)
            node["indexed"] = false;
        return node;
    }

    public static JsonObject BuildDateTimeField(Bm25DateTimeAttribute attr)
    {
        var node = new JsonObject();
        if (attr.Fast)
            node["fast"] = true;
        if (!attr.Indexed)
            node["indexed"] = false;
        return node;
    }

    public static JsonObject BuildJsonField(Bm25JsonAttribute attr, string propertyName)
    {
        ValidateTokenizerParameters(
            propertyName,
            attr.Tokenizer,
            attr.MinGram,
            attr.MaxGram,
            attr.PrefixOnly,
            attr.RegexPattern
        );

        var node = new JsonObject();
        AddTokenizerKey(
            node,
            attr.Tokenizer,
            attr.MinGram,
            attr.MaxGram,
            attr.PrefixOnly,
            attr.RegexPattern,
            attr.Stemmer,
            attr.StopwordsLanguage
        );
        AddSharedKeys(node, attr.Fast, attr.Record, attr.Indexed, attr.Fieldnorms);
        if (attr.ExpandDots)
            node["expand_dots"] = true;
        return node;
    }

    private static void AddTokenizerKey(
        JsonObject node,
        Bm25Tokenizer tokenizer,
        int minGram,
        int maxGram,
        bool prefixOnly,
        string regexPattern,
        Bm25Language stemmer,
        Bm25Language stopwordsLanguage
    )
    {
        var hasAnySetting =
            tokenizer != Bm25Tokenizer.Unspecified
            || stemmer != Bm25Language.Unspecified
            || stopwordsLanguage != Bm25Language.Unspecified;
        if (!hasAnySetting)
            return;

        var effectiveTokenizer =
            tokenizer == Bm25Tokenizer.Unspecified ? Bm25Tokenizer.Default : tokenizer;

        var tok = new JsonObject { ["type"] = effectiveTokenizer.ToParadeDbString() };

        if (effectiveTokenizer == Bm25Tokenizer.Ngram)
        {
            tok["min_gram"] = minGram;
            tok["max_gram"] = maxGram;
            tok["prefix_only"] = prefixOnly;
        }
        if (effectiveTokenizer == Bm25Tokenizer.Regex)
        {
            tok["pattern"] = regexPattern;
        }
        if (stemmer != Bm25Language.Unspecified)
            tok["stemmer"] = stemmer.ToParadeDbString();
        if (stopwordsLanguage != Bm25Language.Unspecified)
            tok["stopwords_language"] = stopwordsLanguage.ToParadeDbString();

        node["tokenizer"] = tok;
    }

    private static void AddSharedKeys(
        JsonObject node,
        bool fast,
        Bm25Record record,
        bool indexed,
        bool fieldnorms
    )
    {
        if (fast)
            node["fast"] = true;
        if (record != Bm25Record.Unspecified)
            node["record"] = record.ToParadeDbString();
        if (!indexed)
            node["indexed"] = false;
        if (!fieldnorms)
            node["fieldnorms"] = false;
    }

    private static void ValidateTokenizerParameters(
        string propertyName,
        Bm25Tokenizer tokenizer,
        int minGram,
        int maxGram,
        bool prefixOnly,
        string regexPattern
    )
    {
        var hasNgramParam = minGram != 0 || maxGram != 0 || prefixOnly;
        var hasRegexParam = regexPattern is not null;

        if (hasNgramParam && tokenizer != Bm25Tokenizer.Ngram)
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}': MinGram/MaxGram/PrefixOnly require Tokenizer = Bm25Tokenizer.Ngram."
            );
        }
        if (tokenizer == Bm25Tokenizer.Ngram && (minGram == 0 || maxGram == 0))
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}': Tokenizer = Bm25Tokenizer.Ngram requires both MinGram and MaxGram (> 0)."
            );
        }
        if (hasRegexParam && tokenizer != Bm25Tokenizer.Regex)
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}': RegexPattern requires Tokenizer = Bm25Tokenizer.Regex."
            );
        }
        if (tokenizer == Bm25Tokenizer.Regex && !hasRegexParam)
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}': Tokenizer = Bm25Tokenizer.Regex requires a RegexPattern."
            );
        }
    }
}
