namespace ETLEngine.Interfaces
{
    /// <summary>
    /// Checks whether a <seealso cref="TIn"/> is valid or not. 
    /// </summary>
    /// <typeparam name="TIn">Type of object to validate</typeparam>
    public interface IValidator<in TIn>
    {
        public bool Validate(TIn input);
    }
}
