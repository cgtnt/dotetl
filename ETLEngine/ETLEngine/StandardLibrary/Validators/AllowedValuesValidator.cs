using ETLEngine.Interfaces;

namespace ETLEngine.StandardLibrary.Validators
{
    /// <summary>
    /// Validates that a value is in a given set of allowed values.
    /// </summary>
    /// <typeparam name="TData">Type of value</typeparam>
    /// <param name="allowedValues">Set of allowed values</param>
    public class AllowedValuesValidator<TData>(
        params TData[] allowedValues    
    ) : IValidator<TData>
    {
        public bool Validate(TData data) => allowedValues.Contains(data);
    }
}
