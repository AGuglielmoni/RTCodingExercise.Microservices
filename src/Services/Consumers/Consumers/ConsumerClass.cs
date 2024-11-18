using MassTransit;

namespace Consumers.RTCConsumer
{
    public class RTCConsumerClass : IConsumer<RTCConsumerMessage>, IRTCConsumers<RTCConsumerMessage>
    {
        public async Task Consume(ConsumeContext<RTCConsumerMessage> context)
        {
            var message = context.Message;

            await Console.Out.WriteLineAsync($"Received message: {message.Message}");
        }

    }

    public class RTCConsumerMessage
    {
        public string Message { get; set; }
    }

}



