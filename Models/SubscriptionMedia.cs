using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class SubscriptionMedia
{
    [Key]
    public int subscription_media_id { get; set; }
    public int plan_id { get; set; }
    public int media_id { get; set; }
    public DateTime available_from { get; set; }
    public DateTime? available_to { get; set; }

    [ForeignKey("plan_id")]
    public virtual SubscriptionPlan SubscriptionPlan { get; set; }
    [ForeignKey("media_id")]
    public virtual Media Media { get; set; }
}