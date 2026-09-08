using ETLEngine.Interfaces;

namespace ETLEngine.StandardLibrary.Validators
{
    /// <summary>
    /// Validates that a string contains only alphanumeric characters.
    /// </summary>
    public class AlphanumericValidator : IValidator<string>
    {
        public bool Validate(string value)
        {
            foreach (char character in value)
                if (!char.IsLetterOrDigit(character))
                    return false;

            return true;
        }
    }
}
