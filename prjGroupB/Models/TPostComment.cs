﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjGroupB.Models;

public partial class TPostComment
{
    public int FCommentId { get; set; }

    public int? FPostId { get; set; }

    public int? FUserId { get; set; }

    public string FContent { get; set; }

    public DateTime? FCreatedAt { get; set; }

    public DateTime? FUpdatedAt { get; set; }

    public int? FParentCommentId { get; set; }

    public virtual TPostComment FParentComment { get; set; }

    public virtual TPost FPost { get; set; }

    public virtual TUser FUser { get; set; }

    public virtual ICollection<TPostComment> InverseFParentComment { get; set; } = new List<TPostComment>();
}