﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjGroupB.Models;

public partial class TEventRegistrationForm
{
    public int FEventRegistrationFormId { get; set; }

    public int? FEventId { get; set; }

    public int? FUserId { get; set; }

    public DateTime? FEregistrationDate { get; set; }

    public string FRegistrationStatus { get; set; }

    public virtual TEvent FEvent { get; set; }

    public virtual TUser FUser { get; set; }

    public virtual ICollection<TEventPayment> TEventPayments { get; set; } = new List<TEventPayment>();
}