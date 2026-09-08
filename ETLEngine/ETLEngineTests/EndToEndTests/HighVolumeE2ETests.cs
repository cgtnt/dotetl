using ETLEngine;

namespace ETLEngineTests.EndToEndTests
{
    using ETLEngine.Interfaces;
    using ETLEngine.StandardLibrary.ExtractionSources;
    using ETLEngine.StandardLibrary.LoadingDestinations;
    using ETLEngine.StandardLibrary.Parsers;
    using ETLEngineTests;
    using System.Reflection;

    file class PersonToMegaPersonTransformer : ITransformer<PersonTDataExample, MegaPersonTDataExample>
    {
        public MegaPersonTDataExample Transform(PersonTDataExample person)
        {
            return new MegaPersonTDataExample { 
                FirstName = person.FullName.Split()[0],
                Age = person.Age,
                RadioAmateurLicensed = person.Age >= 18 ? true : false
            };
        }
    }

    file class FaultyPersonToPersonTransformer : ITransformer<PersonTDataExample, PersonTDataExample>
    {
        public PersonTDataExample Transform(PersonTDataExample person)
        {
            if (person.Age == 50)
                throw new DivideByZeroException("Divided by zero!");

            return person;
        }
    }

    file class PersonToMegaPersonExpensiveTransformer : ITransformer<PersonTDataExample, MegaPersonTDataExample>
    {
        public MegaPersonTDataExample Transform(PersonTDataExample person)
        {
            Thread.SpinWait(200);
            return new MegaPersonTDataExample
            {
                FirstName = person.FullName.Split()[0],
                Age = person.Age,
                RadioAmateurLicensed = person.Age >= 18 ? true : false
            };
        }
    }

    file class PersonValidator : IValidator<PersonTDataExample>
    {
        public bool Validate(PersonTDataExample person)
        {
            bool valid = true;
            valid &= person.Age > 0;
            return valid;
        }
    }

    file static class TestHelper
    {
        public static MemoryStream Generate(long recordCount, Func<long, string> recordGenerator)
        {
            MemoryStream input = new();
            using StreamWriter writer = new(input, leaveOpen: true);

            for (long i = 0; i < recordCount; ++i)
                writer.WriteLine(recordGenerator(i));

            writer.Flush();
            input.Position = 0;

            return input;
        }

        public static int CountStreamLines(Stream stream)
        {
            stream.Position = 0;
            using StreamReader outputReader = new(stream);

            int outputCount = 0;
            while (outputReader.ReadLine() is not null)
                ++outputCount;

            return outputCount;
        }
    }

    public class HighVolumeE2ETests
    {
        [Theory]
        [InlineData(100_000, 8)]
        [InlineData(1_000_000, 8)]

        public void TestPerfectJSONNoValidation(long recordCount, int numStreams)
        {
            MemoryStream outputStream = new();
            MemoryStream errorStream = new();

            List<IExtractionSource<PersonTDataExample>> inputs = new();
            for (int i = 0; i < numStreams; ++i)
                inputs.Add(new  StreamReaderExtractionSource<PersonTDataExample>(
                    new StreamReader(
                        TestHelper.Generate(recordCount, (long i) => $"{{ \"name\": \"abcd{i}\", \"Age\": {i % 100} }}")
                    )
                ));

            var outputWriter = new StreamWriter(outputStream);
            var errorWriter = new StreamWriter(errorStream);

            StreamWriterLoadingDestination<MegaPersonTDataExample> output = new(outputWriter);
            StreamWriterLoadingDestination<Exception> error = new(errorWriter);

            var engine = new EngineBuilder<PersonTDataExample, MegaPersonTDataExample>()
                .WithTransformer(new PersonToMegaPersonTransformer())
                .Build();

            var metrics = engine.Process(inputs, output, error);

            outputWriter.Flush();
            errorWriter.Flush();

            int outputCount = TestHelper.CountStreamLines(outputStream);

            Assert.Equal(recordCount * numStreams, outputCount);

            Assert.Equal(recordCount * numStreams, metrics.PassedValidation);
            Assert.Equal(recordCount * numStreams, metrics.Transformed);
            Assert.Equal(recordCount * numStreams, metrics.Loaded);

            Assert.Equal(0, metrics.TransformationExceptions);
            Assert.Equal(0, metrics.LoadingExceptions);
            Assert.Equal(0, metrics.ExtractionExceptions);

            Assert.Equal(0, errorStream.Length);
        }

        [Theory]
        [InlineData(100_000, 8)]
        [InlineData(1_000_000, 8)]
        public void TestPerfectCSVNoValidation(long recordCount, int numStreams)
        {
            MemoryStream outputStream = new();
            MemoryStream errorStream = new();

            var inputColumnMap = new Dictionary<PropertyInfo, int>
            {
                { typeof(PersonTDataExample).GetProperty("FullName"), 0 },
                { typeof(PersonTDataExample).GetProperty("Age"), 1 }
            };

            List<IExtractionSource<PersonTDataExample>> inputs = new();
            for (int i = 0; i < numStreams; ++i)
                inputs.Add(new StreamReaderExtractionSource<PersonTDataExample>(
                    new StreamReader(
                        TestHelper.Generate(recordCount, (long i) => $"abcd{i},{i % 100}")
                    ),
                    new CsvParser<PersonTDataExample>(
                        ',',
                        inputColumnMap
                    )
                ));

            var outputWriter = new StreamWriter(outputStream);
            var errorWriter = new StreamWriter(errorStream);

            StreamWriterLoadingDestination<PersonTDataExample> output = new(outputWriter);
            StreamWriterLoadingDestination<Exception> error = new(errorWriter);

            var engine = new EngineBuilder<PersonTDataExample>()
                .Build();

            var metrics = engine.Process(inputs, output, error);

            outputWriter.Flush();
            errorWriter.Flush();

            int outputCount = TestHelper.CountStreamLines(outputStream);

            Assert.Equal(recordCount * numStreams, outputCount);

            Assert.Equal(recordCount * numStreams, metrics.PassedValidation);
            Assert.Equal(recordCount * numStreams, metrics.Transformed);
            Assert.Equal(recordCount * numStreams, metrics.Loaded);

            Assert.Equal(0, metrics.TransformationExceptions);
            Assert.Equal(0, metrics.LoadingExceptions);
            Assert.Equal(0, metrics.ExtractionExceptions);

            Assert.Equal(0, errorStream.Length);
        }

        [Theory]
        [InlineData(100_000, 8)]
        public void TestWithFiltering(long recordCount, int numStreams)
        {
            MemoryStream outputStream = new();
            MemoryStream errorStream = new();

            List<IExtractionSource<PersonTDataExample>> inputs = new();
            for (int i = 0; i < numStreams; ++i)
                inputs.Add(new StreamReaderExtractionSource<PersonTDataExample>(
                    new StreamReader(
                        TestHelper.Generate(recordCount, (long i) => $"{{ \"name\": \"abcd{i}\", \"Age\": {(i % 2 == 0 ? 0 : 5)} }}")
                    )
                ));

            var outputWriter = new StreamWriter(outputStream);
            var errorWriter = new StreamWriter(errorStream);

            StreamWriterLoadingDestination<PersonTDataExample> output = new(outputWriter);
            StreamWriterLoadingDestination<Exception> error = new(errorWriter);

            var engine = new EngineBuilder<PersonTDataExample>()
                .WithValidator(new PersonValidator())
                .Build();

            var metrics = engine.Process(inputs, output, error);

            outputWriter.Flush();
            errorWriter.Flush();

            int outputCount = TestHelper.CountStreamLines(outputStream);

            Assert.Equal(recordCount / 2 * numStreams, outputCount);

            Assert.Equal(recordCount / 2 * numStreams, metrics.PassedValidation);
            Assert.Equal(recordCount / 2 * numStreams, metrics.FailedValidation);
            Assert.Equal(recordCount / 2 * numStreams, metrics.Transformed);
            Assert.Equal(recordCount / 2 * numStreams, metrics.Loaded);

            Assert.Equal(0, metrics.TransformationExceptions);
            Assert.Equal(0, metrics.LoadingExceptions);
            Assert.Equal(0, metrics.ExtractionExceptions);

            Assert.Equal(0, errorStream.Length);
        }

        [Theory]
        [InlineData(100_000, 8)]
        public void TestExpensiveTransformation(long recordCount, int numStreams)
        {
            MemoryStream outputStream = new();
            MemoryStream errorStream = new();

            List<IExtractionSource<PersonTDataExample>> inputs = new();
            for (int i = 0; i < numStreams; ++i)
                inputs.Add(new StreamReaderExtractionSource<PersonTDataExample>(
                    new StreamReader(
                        TestHelper.Generate(recordCount, (long i) => $"{{ \"name\": \"abcd{i}\", \"Age\": {(i % 2 == 0 ? 0 : 5)} }}")
                    )
                ));

            var outputWriter = new StreamWriter(outputStream);
            var errorWriter = new StreamWriter(errorStream);

            StreamWriterLoadingDestination<MegaPersonTDataExample> output = new(outputWriter);
            StreamWriterLoadingDestination<Exception> error = new(errorWriter);

            var engine = new EngineBuilder<PersonTDataExample, MegaPersonTDataExample>()
                .WithTransformer(new PersonToMegaPersonExpensiveTransformer())
                .Build();

            var metrics = engine.Process(inputs, output, error);

            outputWriter.Flush();
            errorWriter.Flush();

            int outputCount = TestHelper.CountStreamLines(outputStream);

            Assert.Equal(recordCount * numStreams, outputCount);

            Assert.Equal(recordCount * numStreams, metrics.PassedValidation);
            Assert.Equal(recordCount * numStreams, metrics.Transformed);
            Assert.Equal(recordCount * numStreams, metrics.Loaded);

            Assert.Equal(0, metrics.TransformationExceptions);
            Assert.Equal(0, metrics.LoadingExceptions);
            Assert.Equal(0, metrics.ExtractionExceptions);

            Assert.Equal(0, errorStream.Length);
        }

        [Theory]
        [InlineData(100_000, 8)]
        public void TestTransformationException(long recordCount, int numStreams)
        {
            MemoryStream outputStream = new();
            MemoryStream errorStream = new();

            List<IExtractionSource<PersonTDataExample>> inputs = new();
            for (int i = 0; i < numStreams; ++i)
                inputs.Add(new StreamReaderExtractionSource<PersonTDataExample>(
                    new StreamReader(
                        TestHelper.Generate(recordCount, (long i) => $"{{ \"name\": \"abcd{i}\", \"Age\": {(i % 100)} }}")
                    )
                ));

            var outputWriter = new StreamWriter(outputStream);
            var errorWriter = new StreamWriter(errorStream);

            StreamWriterLoadingDestination<PersonTDataExample> output = new(outputWriter);
            StreamWriterLoadingDestination<Exception> error = new(errorWriter);

            var engine = new EngineBuilder<PersonTDataExample>()
                .WithTransformer(new FaultyPersonToPersonTransformer())
                .Build();

            var metrics = engine.Process(inputs, output, error);

            outputWriter.Flush();
            errorWriter.Flush();

            int outputCount = TestHelper.CountStreamLines(outputStream);

            Assert.Equal((recordCount - 1000) * numStreams, outputCount);

            Assert.Equal(recordCount * numStreams, metrics.PassedValidation);
            Assert.Equal((recordCount - 1000) * numStreams, metrics.Transformed);
            Assert.Equal((recordCount - 1000) * numStreams, metrics.Loaded);

            Assert.Equal(1000 * numStreams, metrics.TransformationExceptions);
            Assert.Equal(0, metrics.LoadingExceptions);
            Assert.Equal(0, metrics.ExtractionExceptions);

            Assert.Equal(1000 * numStreams, TestHelper.CountStreamLines(errorStream));
        }
    }
}
