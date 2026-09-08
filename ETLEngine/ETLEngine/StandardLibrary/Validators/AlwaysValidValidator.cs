using ETLEngine.Interfaces;

namespace ETLEngine.StandardLibrary.Validators
{
    /// <summary>
    /// Always returns true.
    /// </summary>
    /// <typeparam name="TIn">Type of value</typeparam>
    public class AlwaysValidValidator<TIn> : IValidator<TIn>
    {
        public bool Validate(TIn input) => true;
    }
}
