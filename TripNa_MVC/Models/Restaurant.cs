using System;
using System.Collections.Generic;

namespace TripNa_MVC.Models;

public partial class Restaurant
{
    public int Id { get; set; }

    public string LocationName { get; set; } = null!;

    public string RestaurantName { get; set; } = null!;

    public string? Description { get; set; }

    public string? FoodType { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string Region { get; set; } = null!;
}
