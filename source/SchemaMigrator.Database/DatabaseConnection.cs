using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Enums;
using SchemaMigrator.Database.Schemas;

namespace SchemaMigrator.Database;

public sealed class DatabaseConnection(Element element, EntryKey entryKey)
{
    public Schema Schema = entryKey switch
    {
        EntryKey.DefaultSchema => DefaultSchema.Create(),
        EntryKey.MigratedSchema => MigratedSchema.Create(),
        _ => throw new ArgumentOutOfRangeException(nameof(entryKey), entryKey, null)
    };

    private Transaction _transaction;

    /// <summary>
    ///     Begins a new transaction for database operations.
    /// </summary>
    public void BeginTransaction()
    {
        _transaction = new Transaction(element.Document, "Save data");
        _transaction.Start();
    }

    /// <summary>
    ///     Closes the connection, committing changes.
    /// </summary>
    public void Close()
    {
        if (_transaction is null) return;
        if (!_transaction.IsValidObject) throw new ObjectDisposedException(nameof(DatabaseConnection), "Attempting to close an already closed connection");

        _transaction.Commit();
        _transaction.Dispose();
    }

    /// <summary>
    ///     Saves the specified value associated with a field in the database.
    /// </summary>
    /// <typeparam name="T">The type of the value to be saved.</typeparam>
    /// <param name="field">The field name in which to save the value.</param>
    /// <param name="value">The value to be saved.</param>
    public void Save<T>(string field, T value)
    {
        if (value == null) return;
        element.SaveEntity(Schema, value, field);
    }

    /// <summary>
    /// Loads the value associated with the specified field from the database.
    /// </summary>
    /// <typeparam name="T">The type of the value to be loaded.</typeparam>
    /// <param name="field">The field name from which to load the value.</param>
    /// <returns>The value loaded from the database.</returns>
    [Pure]
    public T Load<T>(string field)
    {
        return element.LoadEntity<T>(Schema, field);
    }
}