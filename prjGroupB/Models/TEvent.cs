﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjGroupB.Models;

public partial class TEvent
{
    public int FEventId { get; set; }

    public int? FUserId { get; set; }

    public string FEventName { get; set; }

    public string FEventDescription { get; set; }

    public DateTime? FEventStartDate { get; set; }

    public DateTime? FEventEndDate { get; set; }

    public DateTime? FEventCreatedDate { get; set; }

    public DateTime? FEventUpdatedDate { get; set; }

    public string FEventUrl { get; set; }

    public bool? FEventIsActive { get; set; }

    public virtual ICollection<TEventCategoryMapping> TEventCategoryMappings { get; set; } = new List<TEventCategoryMapping>();

    public virtual ICollection<TEventContact> TEventContacts { get; set; } = new List<TEventContact>();

    public virtual ICollection<TEventFavorite> TEventFavorites { get; set; } = new List<TEventFavorite>();

    public virtual ICollection<TEventImage> TEventImages { get; set; } = new List<TEventImage>();

    public virtual ICollection<TEventLocation> TEventLocations { get; set; } = new List<TEventLocation>();

    public virtual ICollection<TEventLog> TEventLogs { get; set; } = new List<TEventLog>();

    public virtual ICollection<TEventRegistrationForm> TEventRegistrationForms { get; set; } = new List<TEventRegistrationForm>();

    public virtual ICollection<TEventSchedule> TEventSchedules { get; set; } = new List<TEventSchedule>();
}