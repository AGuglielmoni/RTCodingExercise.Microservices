using System.Threading.Tasks;
using MassTransit;

namespace RTCConsumer
{
    public class RTCConsumerClass : IRTCConsumers<RTCConsumerMessage>
    {
        public async Task Consume(ConsumeContext<ConsumerMessage> context)
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



