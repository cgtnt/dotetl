namespace ETLEngine.StandardLibrary.Validators
{
    using ETLEngine.Interfaces;
    using System.Numerics;

    /// <summary>
    /// Validates that a value is within a given range.
    /// </summary>
    /// <typeparam name="TData">Type of value</typeparam>
    public class RangeValidator<TData> : IValidator<TData> 
        where TData : IComparisonOperators<TData, TData, bool>
    {
        TData min_;
        TData max_;

        /// <param name="min">Range minimum</param>
        /// <param name="max">Range maximum</param>
        /// <exception cref="ApplicationException">Thrown is <paramref name="min"/> > <paramref name="max"/></exception>
        public RangeValidator(
            TData min,
            TData max
        ) {
            if (min > max)
                throw new ApplicationException($"Argument {nameof(min)} must be smaller or equal than {nameof(max)}");

            min_ = min;
            max_ = max;
        }

        public bool Validate(TData data) => !(data > max_ || data < min_);
    }
}
