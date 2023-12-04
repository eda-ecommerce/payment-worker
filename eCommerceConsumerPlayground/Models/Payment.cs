using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using paymentWorker.Models;

namespace ECommerceConsumerPlayground.Models;

public class Payment
{
    [Key]
    public Guid PaymentId { get; set; }

    public Guid OrderId { get; set; }
    public DateOnly? PaymentDate { get; set; }
    public DateOnly CreatedDate { get; set; }
    public Status Status { get; set; } 
    public string Type { get; set; }
}
