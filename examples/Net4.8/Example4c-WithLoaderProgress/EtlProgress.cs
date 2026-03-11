namespace Example4c_WithLoaderProgress
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
