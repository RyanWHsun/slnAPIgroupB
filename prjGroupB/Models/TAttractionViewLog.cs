﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjGroupB.Models;

public partial class TAttractionViewLog
{
    public int FLogId { get; set; }

    public int? FAttractionId { get; set; }

    public string FUserIp { get; set; }

    public DateTime? FViewTime { get; set; }

    public virtual TAttraction FAttraction { get; set; }
}