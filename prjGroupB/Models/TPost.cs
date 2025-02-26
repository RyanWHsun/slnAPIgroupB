﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjGroupB.Models;

public partial class TPost
{
    public int FPostId { get; set; }

    public int? FUserId { get; set; }

    public string FTitle { get; set; }

    public string FContent { get; set; }

    public DateTime? FCreatedAt { get; set; }

    public DateTime? FUpdatedAt { get; set; }

    public bool? FIsPublic { get; set; }

    public int? FCategoryId { get; set; }

    public virtual TPostCategory FCategory { get; set; }

    public virtual TUser FUser { get; set; }

    public virtual ICollection<TPostAndTag> TPostAndTags { get; set; } = new List<TPostAndTag>();

    public virtual ICollection<TPostComment> TPostComments { get; set; } = new List<TPostComment>();

    public virtual ICollection<TPostImage> TPostImages { get; set; } = new List<TPostImage>();

    public virtual ICollection<TPostLike> TPostLikes { get; set; } = new List<TPostLike>();
}