using System.Text.Json.Nodes;

namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// Mutable builder for boolean query clauses used within <see cref="ParadeDbJsonQuery.Boolean"/>.
/// </summary>
public sealed class ParadeDbBooleanQuery {
    internal List<ParadeDbJsonQuery> MustClauses { get; } = [];
    internal List<ParadeDbJsonQuery> ShouldClauses { get; } = [];
    internal List<ParadeDbJsonQuery> MustNotClauses { get; } = [];

    /// <summary>Adds must (AND) clauses.</summary>
    public ParadeDbBooleanQuery Must(params ParadeDbJsonQuery[] queries) {
        MustClauses.AddRange(queries);
        return this;
    }

    /// <summary>Adds should (OR) clauses.</summary>
    public ParadeDbBooleanQuery Should(params ParadeDbJsonQuery[] queries) {
        ShouldClauses.AddRange(queries);
        return this;
    }

    /// <summary>Adds must_not (NOT) clauses.</summary>
    public ParadeDbBooleanQuery MustNot(params ParadeDbJsonQuery[] queries) {
        MustNotClauses.AddRange(queries);
        return this;
    }

    internal JsonNode ToJsonNode() {
        var inner = new JsonObject();
        if (MustClauses.Count > 0) inner["must"] = ToArray(MustClauses);
        if (ShouldClauses.Count > 0) inner["should"] = ToArray(ShouldClauses);
        if (MustNotClauses.Count > 0) inner["must_not"] = ToArray(MustNotClauses);
        return new JsonObject { ["boolean"] = inner };
    }

    private static JsonArray ToArray(List<ParadeDbJsonQuery> queries) {
        var arr = new JsonArray();
        foreach (var q in queries) arr.Add(q.CloneNode());
        return arr;
    }
}
