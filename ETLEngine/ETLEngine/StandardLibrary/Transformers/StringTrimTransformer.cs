using ETLEngine.Interfaces;

namespace ETLEngine.StandardLibrary.Transformers
{
    /// <summary>
    /// Trims a string.
    /// </summary>
    public class StringTrimTransformer : ITransformer<string, string>
    {
        public string Transform(string data) => data.Trim();
    }
}
