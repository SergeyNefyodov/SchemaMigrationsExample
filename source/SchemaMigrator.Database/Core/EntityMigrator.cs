using System.Reflection;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace SchemaMigrator.Database.Core;

public static class EntityMigrator
{
    public static void Migrate(Schema oldSchema, Schema newSchema)
    {
        var instances = Context.ActiveDocument.EnumerateInstances<FamilyInstance>().ToArray();
        foreach (var instance in instances)
        {
            MigrateElement(instance, oldSchema, newSchema);
        }
        // foreach (var type in types)
        // {
        //     MigrateElement(type, oldSchema, newSchema);
        // }

        Context.ActiveDocument!.EraseSchemaAndAllEntities(oldSchema);
    }

    private static void MigrateElement(Element element, Schema oldSchema, Schema newSchema)
    {
        var firstEntity = element.GetEntity(oldSchema);
        if (firstEntity is null || firstEntity.Schema is null || !firstEntity.Schema.IsValidObject)
            return;

        var secondEntity = new Entity(newSchema);
        var newFields = newSchema.ListFields();
        var firstField = newFields.First();
        var firstFieldValueType = firstField.ValueType;
        var defaultSetMethod = secondEntity.GetType().GetMethods().FirstOrDefault(methodInfo =>
        {
            if (methodInfo.Name != nameof(Entity.Set)) return false;
            var parameters = methodInfo.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType == typeof(string) &&
                   parameters[1].ParameterType.IsGenericParameter; 
        })!;
        var genericDefaultSetMethod = defaultSetMethod.MakeGenericMethod(firstFieldValueType);
        var defaultValue = Activator.CreateInstance(firstFieldValueType);
        genericDefaultSetMethod.Invoke(secondEntity, [firstField.FieldName, defaultValue]);
        element.SetEntity(secondEntity);
        element.Document.Regenerate();

        var oldFields = oldSchema.ListFields();
        foreach (var field in oldFields)
        {
            var getMethod = firstEntity.GetType().GetMethod(nameof(Entity.Get), [typeof(Field)])!;
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
            var value = genericGetMethod.Invoke(firstEntity, [field]);
            genericSetMethod.Invoke(secondEntity, [field.FieldName, value]);
        }

        var a = secondEntity.Get<string>("Name");
        var b = firstEntity.Get<string>("Name");
        element.SetEntity(secondEntity);
        element.DeleteEntity(firstEntity.Schema);
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