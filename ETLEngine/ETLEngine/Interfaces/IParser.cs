namespace ETLEngine.Interfaces
{
    /// <summary>
    /// Parses a string to a <seealso cref="TData"/>.
    /// </summary>
    /// <typeparam name="TData">Type of object to be parsed</typeparam>
    public interface IParser<out TData>
    {
        TData Parse(string str);
    }
}
