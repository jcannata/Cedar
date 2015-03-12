namespace Cedar.EventSourcing
{
    public interface IAllEventsPage
    {
        IStreamEvent[] Events { get; }

        string FromCheckpoint { get; }

        string IsEnd { get; }

        string NextCheckpoint { get; }

        ReadDirection ReadDirection { get; }
    }
}