﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjGroupB.Models;

public partial class TAttractionTicket
{
    public int FAttractionTicketId { get; set; }

    public int? FAttractionId { get; set; }

    public string FTicketType { get; set; }

    public decimal? FPrice { get; set; }

    public string FDiscountInformation { get; set; }

    public DateTime? FCreatedDate { get; set; }

    public virtual TAttraction FAttraction { get; set; }

    public virtual ICollection<TAttractionTicketOrder> TAttractionTicketOrders { get; set; } = new List<TAttractionTicketOrder>();
}