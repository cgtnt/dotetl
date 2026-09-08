using ETLEngine.Interfaces;

namespace ETLEngine.StandardLibrary
{
    /// <summary>
    /// Builds an <seealso cref="IValidator{TIn}"/> by chaining multiple validators, resulting in a validation pipeline.
    /// </summary>
    /// <typeparam name="TData">Type of object that the validation will be applied to</typeparam>
    public class ValidationPipelineBuilder<TData>
    {
        private class ValidationPipeline(
            List<Func<TData, bool>> rules
        ) : IValidator<TData>
        {
            public bool Validate(TData data)
            {
                foreach (var rule in rules)
                {
                    if (!rule(data))
                        return false;
                }

                return true;
            }
        }

        private readonly List<Func<TData, bool>> rules_ = new();

        /// <summary>
        /// Adds a validator applied to the whole object.
        /// </summary>
        /// <typeparam name="TData">Type of object that the validator will be applied to</typeparam>
        /// <param name="validator">The next validator to add to the pipeline</param>
        /// <returns>Reference to this <seealso cref="ValidationPipelineBuilder{TData}"/></returns>
        public ValidationPipelineBuilder<TData> AddObjectValidator(IValidator<TData> validator)
        {
            rules_.Add((TData data) => validator.Validate(data));
            return this;
        }

        /// <summary>
        /// Adds a validator applied to a property of the object.
        /// </summary>
        /// <typeparam name="TProperty">Type of property that the validator should be applied to</typeparam>
        /// <param name="propertyGetter">A getter for the property to apply the validator to</param>
        /// <param name="rule">The next validator to add to the pipeline. It will be applied to the <seealso cref="TProperty"/> property specified by the getter</param>
        /// <returns>Reference to this <seealso cref="TransformationPipelineBuilder{TDataIn, TDataMid}"/></returns>
        public ValidationPipelineBuilder<TData> AddPropertyValidator<TProperty>(
            Func<TData, TProperty> propertyGetter,
            IValidator<TProperty> rule
        ) {
            rules_.Add((TData data) => rule.Validate(propertyGetter(data)));
            return this;
        }

        /// <summary>
        /// Builds the pipeline.
        /// </summary>
        /// <returns>THe resulting pipeline <seealso cref="ValidationPipeline{TData}"/></returns>
        public IValidator<TData> Build() => new ValidationPipeline(rules_);
    }
}
