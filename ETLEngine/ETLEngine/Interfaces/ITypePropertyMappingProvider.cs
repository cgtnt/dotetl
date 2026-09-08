namespace ETLEngine.Interfaces
{
    using System.Reflection;

    /// <summary>
    /// Provides a mapping from <seealso cref="PropertyInfo"/> to a <seealso cref="TMapping"/> for each property of <seealso cref="TData"/>.
    /// </summary>
    /// <typeparam name="TData">Type to be explored for properties</typeparam>
    /// <typeparam name="TMapping">Type of objects to map properties to</typeparam>
    public interface ITypePropertyMappingProvider<TData, TMapping>
    {
        Dictionary<PropertyInfo, TMapping> Discover();
    }

    /// <summary>
    /// Maps a <seealso cref="PropertyInfo"/> to a <seealso cref="TMapping"/>.
    /// </summary>
    /// <typeparam name="TMapping">Type of object to map the property to</typeparam>
    public interface IPropertyMappingStrategy<TMapping>
    {
        TMapping Map(PropertyInfo propertyInfo);
    }

    /// <summary>
    /// Specifies a list of aliases for a property. To be used by a <seealso cref="IParser{TData}"/> implementation to parse heterogenic data into a <seealso cref="TData"/>.
    /// </summary>
    /// <param name="keys">Aliases</param>
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class PotentialInputKeysAttribute(string[] keys) : System.Attribute
    {
        public string[] Keys { get; } = keys;
    }

    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class OutputKeyAttribute(string key) : System.Attribute
    {
        public string Key { get; } = key;
    }

    /// <summary>
    /// Property mapping strategy that checks for an optional attribute, then decides the mapping based on whether the attribute is present.
    /// </summary>
    /// <typeparam name="TAttribute">Attribute</typeparam>
    /// <typeparam name="TMapping">Type of object to map the property to</typeparam>
    /// <param name="mappingPresent">Mapping to apply to the property if the attribute is present</param>
    /// <param name="mappingAbsent">Mapping to apply to the property if the attribute is absent</param>
    public class OptionalAttributePropertyMappingStrategy<TAttribute, TMapping>(
        Func<TAttribute, TMapping> mappingPresent,
        Func<PropertyInfo, TMapping> mappingAbsent
    ) : IPropertyMappingStrategy<TMapping> where TAttribute : System.Attribute
    {
        public TMapping Map(PropertyInfo propertyInfo)
        {
            var mapPropertyAttribute = propertyInfo.GetCustomAttribute<TAttribute>();

            if (mapPropertyAttribute is null)
                return mappingAbsent(propertyInfo);
            else
                return mappingPresent(mapPropertyAttribute);
        }
    }

    /// <summary>
    /// An <seealso cref="OptionalAttributePropertyMappingStrategy{PotentialInputKeysAttribute, String[]}"/> that uses the <seealso cref="PotentialInputKeysAttribute"/> attribute to map properties to <seealso cref="string[]"/> aliases.
    /// </summary>
    public class PropertyAttributeInputKeysStrategy 
        : OptionalAttributePropertyMappingStrategy<PotentialInputKeysAttribute, string[]>
    {
        private static string[] AttributePresent(PotentialInputKeysAttribute attribute) => attribute.Keys;
        private static string[] AttributeAbsent(PropertyInfo propertyInfo) => [propertyInfo.Name];

        public PropertyAttributeInputKeysStrategy() : base(
            AttributePresent,
            AttributeAbsent
        ) { }
    }

    /// <summary>
    /// An <seealso cref="OptionalAttributePropertyMappingStrategy{OutputKeyAttribute, String}"/> that uses the <seealso cref="OutputKeyAttribute"/> attribute to map properties to <seealso cref="string"/> serialization keys.
    /// </summary>
    public class PropertyAttributeOutputKeyStrategy
      : OptionalAttributePropertyMappingStrategy<OutputKeyAttribute, string>
    {
        private static string AttributePresent(OutputKeyAttribute attribute) => attribute.Key;
        private static string AttributeAbsent(PropertyInfo propertyInfo) => propertyInfo.Name;

        public PropertyAttributeOutputKeyStrategy() : base(
            AttributePresent,
            AttributeAbsent
        )
        { }
    }

    /// <summary>
    /// Uses reflection to discover properties of <seealso cref="TData"/> and apply a <seealso cref="IPropertyMappingStrategy{TMapping}"/> to each property.
    /// </summary>
    /// <typeparam name="TData">Type which properties should be mapped</typeparam>
    /// <typeparam name="TMapping">Type of objects to which to map the properties</typeparam>
    /// <param name="propertyNamingStrategy">Property mapping strategy applied to each <seealso cref="PropertInfo"/> of the <seealso cref="TData"/></param>
    public class ReflectionTypePropertyMappingProvider<TData, TMapping>(
        IPropertyMappingStrategy<TMapping> propertyNamingStrategy
    ) : ITypePropertyMappingProvider<TData, TMapping>
    {
        public Dictionary<PropertyInfo, TMapping> Discover()
        {
            Dictionary<PropertyInfo, TMapping> schema = new();

            var propertyInfos = typeof(TData).GetProperties();

            foreach (var propertyInfo in propertyInfos)
                schema.Add(propertyInfo, propertyNamingStrategy.Map(propertyInfo));

            return schema;
        }
    }


}
