using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database.Enums;
using SchemaMigrator.Database.Schemas;

namespace SchemaMigrator.Database;

public sealed class DatabaseConnection(Element element, EntryKey entryKey)
{
    private readonly Schema _schema = entryKey switch
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
        element.SaveEntity(_schema, value, field);
    }

    public void SaveObject<T>(T value)
    {
        var objType = value.GetType();

        var properties = objType.GetProperties();
        var entity = element.GetEntity(_schema);
        if (entity is null || entity.Schema is null || !entity.IsValidObject)
        {
            entity = new Entity(_schema);
        }

        foreach (var property in properties)
        {
            var propertyName = property.Name;
            var propertyValue = property.GetValue(value);
            if (propertyValue != null)
            {
                var propertyType = property.PropertyType;
                var method = entity.GetType()
                    .GetMethods().FirstOrDefault(methodInfo =>
                    {
                        if (methodInfo.Name != nameof(Entity.Set)) return false;
                        var parameters = methodInfo.GetParameters();
                        return parameters.Length == 2 &&
                               parameters[0].ParameterType == typeof(string) &&
                               parameters[1].ParameterType.IsGenericParameter;
                    })!;

                if (propertyType.IsGenericType)
                {
                    var genericTypeDefinition = propertyType.GetGenericTypeDefinition();

                    if (genericTypeDefinition == typeof(List<>))
                    {
                        var elementType = propertyType.GetGenericArguments()[0];
                        propertyType = typeof(IList<>).MakeGenericType(elementType);
                    }
                    else if (genericTypeDefinition == typeof(Dictionary<,>))
                    {
                        var genericArgs = propertyType.GetGenericArguments();
                        var keyType = genericArgs[0];
                        var valueType = genericArgs[1];
                        propertyType = typeof(IDictionary<,>).MakeGenericType(keyType, valueType);
                    }
                }

                method.MakeGenericMethod(propertyType).Invoke(entity, [propertyName, propertyValue]);
            }
        }

        element.SetEntity(entity);
    }

    public T LoadObject<T>() where T : new()
    {
        var entity = element.GetEntity(_schema);
        var obj = new T();
        var objType = typeof(T);

        var properties = objType.GetProperties();
        var method = typeof(Entity).GetMethods().FirstOrDefault(methodInfo =>
        {
            if (methodInfo.Name != nameof(Entity.Get)) return false;
            var parameters = methodInfo.GetParameters();
            return parameters.Length == 1 &&
                   parameters[0].ParameterType == typeof(string);
        })!;

        foreach (var property in properties)
        {
            var propertyType = property.PropertyType;

            if (propertyType.IsGenericType)
            {
                if (propertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var elementType = propertyType.GetGenericArguments()[0];
                    propertyType = typeof(IList<>).MakeGenericType(elementType);
                }
                else if (propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var genericArgs = propertyType.GetGenericArguments();
                    var keyType = genericArgs[0];
                    var valueType = genericArgs[1];
                    propertyType = typeof(IDictionary<,>).MakeGenericType(keyType, valueType);
                }
            }

            var value = method
                .MakeGenericMethod(propertyType)
                .Invoke(entity, [property.Name]);

            property.SetValue(obj, value);
        }

        return obj;
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
        return element.LoadEntity<T>(_schema, field);
    }

    public static void Delete(EntryKey entryKey)
    {
        using var transaction = new Transaction(Context.ActiveDocument, "Delete data");
        transaction.Start();
        switch (entryKey)
        {
            case EntryKey.DefaultSchema:
                DefaultSchema.Delete();
                break;
            case EntryKey.MigratedSchema:
                MigratedSchema.Delete();
                break;
        }

        transaction.Commit();
    }
}