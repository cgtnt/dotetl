using ETLEngine;

namespace ETLEngineTests.UnitTests
{
    public class ThreadSafeBoundedQueueTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(1, 2, 3, 4, 5)]
        public void EnqueueAndDequeueSingleThread(params int[] elements) {
            int capacity = 5;
            ThreadSafeBoundedQueue<int> queue = new(capacity);

            foreach (var  element in elements)
                queue.Enqueue(element);
            
            foreach (var element in elements)
            {
                Assert.True(queue.TryDequeue(out int res));
                Assert.Equal(element, res);
            }
        }

        [Fact]
        public void TryDequeueReturnsFalseWhenCompletedAndEmpty()
        {
            int capacity = 5;
            ThreadSafeBoundedQueue<int> queue = new(capacity);

            queue.CompleteInput();
            bool success = queue.TryDequeue(out int res);

            Assert.False(success);
            Assert.Equal(default(int), res);
        }

        [Fact]
        public void EnqueueThrowsExceptionIfCompleted()
        {
            int capacity = 5;
            ThreadSafeBoundedQueue<int> queue = new(capacity);

            queue.CompleteInput();

            Assert.Throws<ApplicationException>(() => queue.Enqueue(5));
        }
    }
}
