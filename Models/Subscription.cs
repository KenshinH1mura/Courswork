using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Subscription
{
    [Key]
    public int subscription_id { get; set; }
    public int user_id { get; set; }
    public int plan_id { get; set; }
    public string status { get; set; }
    public DateTime start_date { get; set; }

    [ForeignKey("user_id")]
    public virtual User User { get; set; }
    [ForeignKey("plan_id")]
    public virtual SubscriptionPlan SubscriptionPlan { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }
}