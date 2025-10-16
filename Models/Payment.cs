using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Payment
{
    [Key]
    public int payment_id { get; set; }
    public int user_id { get; set; }
    public int subscription_id { get; set; }
    public int method_id { get; set; }
    public decimal amount { get; set; }
    public DateTime payment_date { get; set; }

    [ForeignKey("user_id")]
    public virtual User User { get; set; }
    [ForeignKey("subscription_id")]
    public virtual Subscription Subscription { get; set; }
    [ForeignKey("method_id")]
    public virtual PaymentMethod PaymentMethod { get; set; }
}