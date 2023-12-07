namespace paymentWorker.Models;

public class KafkaSchemaOrder
{
    public String Source { get; set; }
    public long Timestamp { get; set; }
    public string Type { get; set; }
    public Order Order { get; set; }
}