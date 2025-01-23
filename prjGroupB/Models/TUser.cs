﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjGroupB.Models;

public partial class TUser
{
    public int FUserId { get; set; }

    public int? FUserRankId { get; set; }

    public string FUserName { get; set; }

    public byte[] FUserImage { get; set; }

    public string FUserNickName { get; set; }

    public string FUserSex { get; set; }

    public DateTime? FUserBirthday { get; set; }

    public string FUserPhone { get; set; }

    public string FUserEmail { get; set; }

    public string FUserAddress { get; set; }

    public DateTime? FUserComeDate { get; set; }

    public string FUserPassword { get; set; }

    public bool? FUserNotify { get; set; }

    public bool? FUserOnLine { get; set; }

    public virtual TRank FUserRank { get; set; }

    public virtual ICollection<TAttractionComment> TAttractionComments { get; set; } = new List<TAttractionComment>();

    public virtual ICollection<TAttractionTicketOrder> TAttractionTicketOrders { get; set; } = new List<TAttractionTicketOrder>();

    public virtual ICollection<TAttractionUserFavorite> TAttractionUserFavorites { get; set; } = new List<TAttractionUserFavorite>();

    public virtual ICollection<TChatRoom> TChatRooms { get; set; } = new List<TChatRoom>();

    public virtual ICollection<TEventFavorite> TEventFavorites { get; set; } = new List<TEventFavorite>();

    public virtual ICollection<TEventLog> TEventLogs { get; set; } = new List<TEventLog>();

    public virtual ICollection<TEventRegistrationForm> TEventRegistrationForms { get; set; } = new List<TEventRegistrationForm>();

    public virtual ICollection<TFriend> TFriendFFriends { get; set; } = new List<TFriend>();

    public virtual ICollection<TFriend> TFriendFUsers { get; set; } = new List<TFriend>();

    public virtual ICollection<TMessage> TMessages { get; set; } = new List<TMessage>();

    public virtual ICollection<TOrder> TOrders { get; set; } = new List<TOrder>();

    public virtual ICollection<TPostCategory> TPostCategories { get; set; } = new List<TPostCategory>();

    public virtual ICollection<TPostComment> TPostComments { get; set; } = new List<TPostComment>();

    public virtual ICollection<TPostView> TPostViews { get; set; } = new List<TPostView>();

    public virtual ICollection<TPost> TPosts { get; set; } = new List<TPost>();

    public virtual ICollection<TProductReview> TProductReviews { get; set; } = new List<TProductReview>();

    public virtual ICollection<TProduct> TProducts { get; set; } = new List<TProduct>();

    public virtual ICollection<TShoppingCart> TShoppingCarts { get; set; } = new List<TShoppingCart>();

    public virtual ICollection<TWallet> TWallets { get; set; } = new List<TWallet>();
}