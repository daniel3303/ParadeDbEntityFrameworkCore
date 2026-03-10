namespace Equibles.ParadeDB.EntityFrameworkCore;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class Bm25IndexAttribute : Attribute {
    public string KeyField { get; }
    public string[] Columns { get; }

    public Bm25IndexAttribute(string keyField, params string[] columns) {
        KeyField = keyField;
        Columns = [keyField, ..columns];
    }
}
