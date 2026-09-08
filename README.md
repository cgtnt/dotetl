# ETL Engine

[![Build Status](https://github.com/cgtnt/dotetl/actions/workflows/ci.yml/badge.svg)](https://github.com/cgtnt/dotetl/actions/workflows/ci.yml)
![Code Coverage](https://raw.githubusercontent.com/cgtnt/dotetl/main/badges/coverage.svg)

A multithreaded Extract-Transform-Load engine for C#, free of third-party dependencies.

The engine is designed to process large datasets and allows users to define custom data sources, validation pipelines, transformation pipelines and loading destinations. The framework provides a range of implementations of the mentioned components as well. They can be found in the `ETLEngine.StandardLibrary` namespace.

## Usage
### Configuring the engine
Use the `EngineBuilder<TData>` or `EngineBuilder<TDataIn, TDataOut>`.

```c#
    var engine = new EngineBuilder<PersonDBExtrat, EmployeeDBImport>()
        .AddValidator(new PersonDBValidator())
        .AddTransformer(new PersonToEmployeeTransformer())
        .Build();
```

### Building composite pipelines
It is possible to build a new `IValidator<TData>` or `ITransformer<TDataIn, TDataOut>` by chaining multiple such instances, using `TransformationPipelineBuilder<TDataIn, TDataOut>` and `ValidationPipelineBuilder<TData>`.
```c#
    var compositeValidator = new ValidationPipelineBuilder<PersonDBExtract>()
        .AddObjectValidator(new PersonStep1Validator())
        .AddPropertyValidator(person => person.Age, new RangeValidator(18, 100))
        .Build();

    var compositeTransformer = new TransformerPipelineBuilder<PersonDBExtract, EmployeeDBImport>()
        .AddObjectTransformer(new PersonToEmployee1Transformer())
        .AddPropertyTransformer(
            employee => employee.Age, // getter
            (employee, value) => employee.Age = value, // setter 
            new RangeClampTransformer(18, 100))
        .Build();
```
#### Engine builder configuration options
| Method | Description |
|:---|:---|
| `WithValidator(IValidator<TIn> validator)` | Sets validatior to be applied upon to extracted items. Items that do not pass validation are dropped. |
| `WithTransformer(ITransformer<TIn, TOut> transformer)` | sets transformer to be applied to items which passed validation |
| `WithTransformationBatchSize(int batchSize)` | How many items should be transformer at a time by a single thread. Larger value reduces thread synchronization overhead. |
| `WithTransformationQueueMaxBatches(int maxBatches)` | How many batches can be in the queue of batches for transformation at a time (producers wait for space if capacity is full). Lower value reduces memory footprint. |
| `WithLoadingBatchSize(int batchSize)` | Similar. |
| `WithLoadingQueueMaxBatches(int maxBatches)` | Similar. |
| `WithExceptionsQueueMaxItems(int maxItems)` | How many exceptions can the exceptions queue hold at a time. |
| `WithMaxExtractionWorkers(int maxWorkers)` | Max number of worker threads the engine will spawn to handle extraction. |
| `WithMaxTransformationWorkers(int maxWorkers)` | Max number of worker threads the engine will spawn to handle transformation. |

### Running the engine
To run the engine (or rather the ETL pipeline it represents when built), use the 
```c# 
Process(
    ICollection<IExtractionSource<TIn>> dataExtractors,
    ILoadingDestination<TOut> outputLoader,
    ILoadingDestination<Exception> dataProcessingExceptionsLoader
)
``` 
method. It is a synchronous method, meaning it will start the worker threads, then wait for completion of the pipeline before returning. 
```c#
    List<IExtractionSource<PersonTDataExample>> inputs = new();
    for (int i = 0; i < numStreams; ++i)
    inputs.Add(
        new StreamReaderExtractionSource<PersonTDataExample>(
            new StreamReader(
                inputStreams[i]
            )
        )
    );

    var outputWriter = new StreamWriter(outputStream);
    var errorWriter = new StreamWriter(errorStream);

    StreamWriterLoadingDestination<PersonTDataExample> output = new(outputWriter);
    StreamWriterLoadingDestination<Exception> error = new(errorWriter);

    var metrics = engine.Process(inputs, output, error);
```
If processing of a single data row throws an exception, the exception will be loaded to the `dataProcessingExceptionsLoader` loader, and the engine will continue execution. If loading a batch throws an exception, the exception will again be handler similarly and the engine will continue, however the whole batch of items will be dropped, instead of just the single item.

### Metrics 
Upon completion of processing, the engine returns `EngineMetrics` with statistics of the ETL run. The object can be inspected manually or use a `IMetricsReporter`. The standard library provides the `MarkdownTableMetricsReporter` implementation.

### Using the Standard library

#### Defining data models for the default IParser (JsonParser)
Define a class, with properties which should be parsed from the data sources. You can use the `[PotentialInputKeys]` attribute to define aliases for each property, which the parser will then look for in each record. If a property is not marked by such attribute, the parser will look for a key matching the exact property name in the input data. If no alias/name is found, the property value is left at its default.
```c#
public class Person {
    [PotentialInputKeys(["name", "fullname"])]
    public string FullName { get; set; }

    [PotentialInputKeys(["age", "years_old"])]
    public int Age { get; set; }
}
```
To instantiate a `JsonParser` manually, the mapping `Dictionary<PropertyInfo, string[]>` of aliases must be provided in the constructor. Internally, when the `StreamReaderExtractionSource` instantiates the parser as the default, it uses the `ReflectionTypePropertyMappingProvider` to discover this mapping.
If implementing a custom `ISerializer`, it is an option to use the `[OutputKey]` attribute to define a custom key for the property output. However, the implemented Standard Library serializers do not respect/use this attribute.

#### Implemented IValidators and ITransformers
- `IValidator`
    - `AllowedValuesValidator`
    - `AlphanumericValidator`
    - `AlwaysValidValidator`
    - `NotNullValidator`
    - `RangeValidator`
- `ITransformer`
    - `RangeClampTransformer`
    - `StringTrimTransformer`
    
#### Utilities for writing custom IExtractionSource and ILoadingDestination 
The following interfaces and implementations (which are useful for creating custom extraction sources and loading destinations) are provided:
- `IExtractionSource`
    - `StreamReaderExtractionSource`
- `ILoadingDestination`
    - `StreamWriterLoadingDestination`
- `IParser`
    - `CsvParser`
    - `JsonParser`
- `ISerializer`
    - `CsvSerialzer`
    - `JsonSerializer`
- `IStreamReaderRecordReader`
    - `NewlineRecordReader`
- `IStreamWriterRecordWriter`
    - `DelimitedRecordWriter`