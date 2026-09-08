using ETLEngine.Interfaces;

namespace ETLEngine.StandardLibrary
{
    /// <summary>
    /// Builds an <seealso cref="ITransformer{TDataIn, TDataOut}"/> by chaining multiple transformers, resulting in a transformation pipeline.
    /// </summary>
    /// <typeparam name="TDataIn">Type of object that the transformation will be applied to</typeparam>
    /// <typeparam name="TDataMid">Type of object that the transformer outputs after applying the transformation</typeparam>
    public class TransformationPipelineBuilder<TDataIn, TDataMid> 
    {
        private class TransformationPipeline<TDataOut>(
            ITransformer<TDataIn, TDataMid> firstTransformer,
            ITransformer<TDataMid, TDataOut> secondTransformer
        ) : ITransformer<TDataIn, TDataOut>
        {
            public TDataOut Transform(TDataIn data) => secondTransformer.Transform(
                    firstTransformer.Transform(data)
                );
        }
        private class PropertyTransformer<TProperty>(
            Func<TDataMid, TProperty> propertyGetter,
            Action<TDataMid, TProperty> propertySetter,
            ITransformer<TProperty, TProperty> rule
        ) : ITransformer<TDataMid, TDataMid>
        {
            public TDataMid Transform(TDataMid data)
            {
                var newValue = rule.Transform(propertyGetter(data));
                propertySetter(data, newValue);
                return data;
            }
        }

        private ITransformer<TDataIn, TDataMid> previousTransformer_;

        public TransformationPipelineBuilder(
            ITransformer<TDataIn, TDataMid> initialTransformer
        ) => previousTransformer_ = initialTransformer;

        /// <summary>
        /// Adds a transformer applied to the whole object.
        /// </summary>
        /// <typeparam name="TDataOut">Type of object outputted by the pipeline after applying all previous transformation steps.</typeparam>
        /// <param name="nextTransformer">The next transformer to add to the pipeline</param>
        /// <returns>Reference to this <seealso cref="TransformationPipelineBuilder{TDataIn, TDataMid}"/></returns>
        public TransformationPipelineBuilder<TDataIn, TDataOut> AddObjectTransformer<TDataOut>(
            ITransformer<TDataMid, TDataOut> nextTransformer
        ) => new TransformationPipelineBuilder<TDataIn, TDataOut>(
                new TransformationPipeline<TDataOut>(
                    previousTransformer_,
                    nextTransformer
                )
             );

        /// <summary>
        /// Adds a transformer applied to a property of the object.
        /// </summary>
        /// <typeparam name="TProperty">Type of property that the transformer should be applied to</typeparam>
        /// <param name="propertyGetter">A getter for the property to apply the transformer to</param>
        /// <param name="propertySetter">A setter for the property to apply the transformer to</param>
        /// <param name="rule">The next transformer to add to the pipeline. It will be applied to the <seealso cref="TProperty"/> property specified by the getter and setter</param>
        /// <returns>Reference to this <seealso cref="TransformationPipelineBuilder{TDataIn, TDataMid}"/></returns>
        public TransformationPipelineBuilder<TDataIn, TDataMid> AddPropertyTransformer<TProperty>(
            Func<TDataMid, TProperty> propertyGetter,
            Action<TDataMid, TProperty> propertySetter,
            ITransformer<TProperty, TProperty> rule
        ) => AddObjectTransformer(
            new PropertyTransformer<TProperty>(
                propertyGetter,
                propertySetter,
                rule
            )
        );

        /// <summary>
        /// Builds the pipeline.
        /// </summary>
        /// <returns>The resulting pipeline <seealso cref="ITransformer{TIn, TOut}"/></returns>
        public ITransformer<TDataIn, TDataMid> Build() => previousTransformer_;
    }
}
