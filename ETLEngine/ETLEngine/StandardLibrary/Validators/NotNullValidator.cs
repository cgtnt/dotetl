using ETLEngine.Interfaces;

namespace ETLEngine.StandardLibrary.Validators
{
    /// <summary>
    /// Validates that a value is not null
    /// </summary>
    /// <typeparam name="TData">Type of value</typeparam>
    public class NotNullValidator<TData> 
        : IValidator<TData> where TData : class 
    {
        public bool Validate(TData data) => data is not null;
    }
}
