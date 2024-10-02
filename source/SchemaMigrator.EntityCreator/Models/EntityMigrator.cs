using System.Reflection;
using Autodesk.Revit.DB.ExtensibleStorage;
using SchemaMigrator.Database;
using SchemaMigrator.Database.Enums;

namespace SchemaMigrator.EntityCreator.Models;

public static class EntityMigrator
{
    public static void Migrate(EntryKey first, EntryKey second)
    {
        var instances = Context.ActiveDocument.EnumerateInstances<FamilyInstance>().ToArray();
        using var transaction = new Transaction(Context.ActiveDocument, "Seeding database");
        transaction.Start();
        Schema schema = null;
        foreach (var instance in instances)
        {
            schema = MigrateElement(instance, first, second);
        }

        Context.ActiveDocument!.EraseSchemaAndAllEntities(schema);
        transaction.Commit();
    }

    private static Schema MigrateElement(Element element, EntryKey first, EntryKey second)
    {
        var firstConnection = new DatabaseConnection(element, first);
        var secondConnection = new DatabaseConnection(element, second);
        var firstSchema = firstConnection.Schema;
        var secondSchema = secondConnection.Schema;
        secondConnection.Save("Id", 1);
        Context.ActiveDocument!.Regenerate();
        var firstEntity = element.GetEntity(firstSchema);
        var secondEntity = element.GetEntity(secondSchema);
        var fields = firstSchema.ListFields();
        foreach (var field in fields)
        {
            var getMethod = firstEntity.GetType().GetMethod(nameof(Entity.Get), [typeof(Field)])!;
            //var methods = secondEntity.GetType().GetMethods().Where(m => m.Name == nameof(Entity.Set)).ToArray();
            var setMethod = secondEntity.GetType().GetMethods().FirstOrDefault(methodInfo =>
            {
                if (methodInfo.Name != nameof(Entity.Set)) return false;
                var parameters = methodInfo.GetParameters();
                return parameters.Length == 2 &&
                       parameters[0].ParameterType == typeof(string) &&
                       parameters[1].ParameterType.IsGenericParameter; // FieldType is generic
            })!;
            var genericSetMethod = setMethod.MakeGenericMethod(field.ValueType);
            var genericGetMethod = MakeGenericInvoker(field, getMethod);
            //var genericSetMethod = MakeGenericInvoker(field, setMethod);
            var value = genericGetMethod.Invoke(firstEntity, [field]);
            genericSetMethod.Invoke(secondEntity, [field.FieldName, value]);
        }

        var a = secondEntity.Get<string>("Name");
        var b = firstEntity.Get<string>("Name");
        element.SetEntity(secondEntity);

        return firstSchema;
    }

    private static MethodInfo MakeGenericInvoker(Field field, MethodInfo invoker)
    {
        var containerType = field.ContainerType switch
        {
            ContainerType.Simple => field.ValueType,
            ContainerType.Array => typeof(IList<>).MakeGenericType(field.ValueType),
            ContainerType.Map => typeof(IDictionary<,>).MakeGenericType(field.KeyType, field.ValueType),
            _ => throw new ArgumentOutOfRangeException()
        };

        return invoker.MakeGenericMethod(containerType);
    }
}