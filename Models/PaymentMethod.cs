using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PaymentMethod
{
    [Key]
    public int method_id { get; set; }
    public string method_name { get; set; }
    public string provider { get; set; }
    public string currency { get; set; }

    public virtual ICollection<Payment> Payments { get; set; }
}