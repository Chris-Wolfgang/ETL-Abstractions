\xef\xbb\xbfnamespace Example4a_WithExtractorProgress
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
