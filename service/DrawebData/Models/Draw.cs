using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DrawebData.Models;

[Table("Draw")]
[Index("UserId", Name = "idx_draw_userid")]
public partial class Draw
{
    [Key]
    [Column("draw_id")]
    public int DrawId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("creation_date", TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    [Column("url")]
    [StringLength(2048)]
    public string Url { get; set; } = null!;

    [Column("last_update", TypeName = "datetime")]
    public DateTime? LastUpdate { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Draws")]
    public virtual User User { get; set; } = null!;
}
