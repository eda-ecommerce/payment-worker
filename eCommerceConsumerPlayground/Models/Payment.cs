using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECommerceConsumerPlayground.Models;

public class Payment
{
    [Key]
    public Guid UserId { get; set; }

    /// <summary>
    /// Durch [JsonIgnore] würde der Value nicht an Kafka gesendet werden
    /// </summary>
    //[JsonIgnore]
    public string Firstname { get; set; }

    /// <summary>
    /// Durch [JsonIgnore] würde der Value nicht an Kafka gesendet werden
    /// </summary>
    //[JsonIgnore]
    public string Lastname { get; set; }

    /// <summary>
    /// Bsp: DieterMücke
    /// </summary>
    public string Username { get; set; }
}
