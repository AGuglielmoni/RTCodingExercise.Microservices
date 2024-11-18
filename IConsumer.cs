namespace RTCConsumer
{
    {
    public interface IRTCConsumers<in TMessage> where TMessage : class
    {
        Task Consume(ConsumeContext<TMessage> context);
    }
}
}
