\xef\xbb\xbfnamespace Example5a_ExtractorWithProgressAndCancellation
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
