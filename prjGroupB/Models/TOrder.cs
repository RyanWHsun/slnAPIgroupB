﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjGroupB.Models;

public partial class TOrder
{
    public int FOrderId { get; set; }

    public int? FBuyerId { get; set; }

    public int? FOrderStatusId { get; set; }

    public DateTime? FOrderDate { get; set; }

    public string FShipAddress { get; set; }

    public string FPaymentMethod { get; set; }

    public string FExtraInfo { get; set; }

    public virtual TUser FBuyer { get; set; }

    public virtual TOrderStatus FOrderStatus { get; set; }

    public virtual ICollection<TOrderStatusHistory> TOrderStatusHistories { get; set; } = new List<TOrderStatusHistory>();

    public virtual ICollection<TOrdersDetail> TOrdersDetails { get; set; } = new List<TOrdersDetail>();
}