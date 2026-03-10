\xef\xbb\xbfnamespace Example6_ReducingDuplicateCode
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
