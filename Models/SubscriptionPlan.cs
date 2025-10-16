using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class SubscriptionPlan
{
    [Key]
    public int plan_id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public decimal monthly_price { get; set; }
    public string billing_cycle { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; }
    public virtual ICollection<SubscriptionMedia> SubscriptionMedia { get; set; }
}