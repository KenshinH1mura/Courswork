using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Media
{
    [Key]
    public int media_id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public int genre_id { get; set; }
    public DateTime release_date { get; set; }
    public decimal? rating { get; set; }

    [ForeignKey("genre_id")]
    public virtual Genre Genre { get; set; }
    public virtual ICollection<View> Views { get; set; }
    public virtual ICollection<SubscriptionMedia> SubscriptionMedia { get; set; }
}