using ECommerceConsumerPlayground.Models;

namespace paymentWorker.Models;

public class KafkaSchemaPayment
{
    public String Source { get; set; }
    public long Timestamp { get; set; }
    public string Type { get; set; }
    public Payment Payment { get; set; }
}