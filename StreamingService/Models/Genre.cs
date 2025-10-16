using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Genre
{
    [Key]
    public int genre_id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string age_rating { get; set; }

    public virtual ICollection<Media> Media { get; set; }
}