using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class View
{
    [Key]
    public int view_id { get; set; }
    public int user_id { get; set; }
    public int media_id { get; set; }
    public DateTime view_date { get; set; }
    public int watch_time { get; set; }
    public string device { get; set; }

    [ForeignKey("user_id")]
    public virtual User User { get; set; }
    [ForeignKey("media_id")]
    public virtual Media Media { get; set; }
}