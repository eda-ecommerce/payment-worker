namespace paymentWorker.Models;

public class KafkaSchemaOrder
{
    public String Source { get; set; }
    public long Timestamp { get; set; }
    public string Operation { get; set; }
    public Order Order { get; set; }
}