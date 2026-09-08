namespace ETLEngineTests
{
    public static class StreamEqualityChecker
    {
        public static bool Equal(Stream stream1, Stream stream2)
        {
            stream1.Position = 0;
            stream2.Position = 0;

            int read1;
            int read2;

            do
            {
                read1 = stream1.ReadByte();
                read2 = stream2.ReadByte();

                if (read1 != read2)
                    return false;

            } while (read1 != -1);

            return true;
        }
    }
}
