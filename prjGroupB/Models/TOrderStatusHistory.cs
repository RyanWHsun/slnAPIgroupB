﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjGroupB.Models;

public partial class TOrderStatusHistory
{
    public int FStatusHistoryId { get; set; }

    public int FOrderStatusId { get; set; }

    public int FOrderId { get; set; }

    public string FStatusName { get; set; }

    public DateTime? FTimestamp { get; set; }

    public virtual ICollection<TOrder> TOrders { get; set; } = new List<TOrder>();
}