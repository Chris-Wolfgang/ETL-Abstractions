\xef\xbb\xbfnamespace Example4b_WithTransformerProgress
{
    internal class EtlProgress
    {
        public EtlProgress(int currentCount)
        {
            CurrentCount = currentCount;
        }

        public int CurrentCount { get; }
    }
}
