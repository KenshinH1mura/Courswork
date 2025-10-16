using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int user_id { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public int? age { get; set; }
    public string country { get; set; }
    public DateTime registration_date { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; }
    public virtual ICollection<Payment> Payments { get; set; }
    public virtual ICollection<View> Views { get; set; }
}