namespace ETLEngine.StandardLibrary.Transformers
{
    using ETLEngine.Interfaces;
    using System.Numerics;

    /// <summary>
    /// Clamps a value to a given range.
    /// </summary>
    /// <typeparam name="TData">Type of value</typeparam>
    /// <param name="min">Range minimum</param>
    /// <param name="max">Range maximum</param>
    public class RangeClampTransformer<TData> : ITransformer<TData, TData>
        where TData : IComparisonOperators<TData, TData, bool>
    {
        TData min_;
        TData max_;

        /// <param name="min">Range minimum</param>
        /// <param name="max">Range maximum</param>
        /// <exception cref="ApplicationException">Thrown is <paramref name="min"/> > <paramref name="max"/></exception>
        public RangeClampTransformer(
            TData min,
            TData max
        ) {
            if (min > max)
                throw new ApplicationException($"Argument {nameof(min)} must be smaller or equal than {nameof(max)}");

            min_ = min;
            max_ = max;
        }

        public TData Transform(TData data)
        {
            if (data > max_)
                return max_;

            if (data < min_)
                return min_;

            return data;
        }
    }
}
