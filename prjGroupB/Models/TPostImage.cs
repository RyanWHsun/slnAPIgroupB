﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjGroupB.Models;

public partial class TPostImage
{
    public int FImageId { get; set; }

    public int? FPostId { get; set; }

    public byte[] FImage { get; set; }

    public virtual TPost FPost { get; set; }
}