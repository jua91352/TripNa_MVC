using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TripNa_MVC.Models;

public partial class TripNaContext : DbContext
{
    public TripNaContext()
    {
    }

    public TripNaContext(DbContextOptions<TripNaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cityarea> Cityareas { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<FavoriteSpot> FavoriteSpots { get; set; }

    public virtual DbSet<Guider> Guiders { get; set; }

    public virtual DbSet<GuiderAnswer> GuiderAnswers { get; set; }

    public virtual DbSet<Itinerary> Itineraries { get; set; }

    public virtual DbSet<ItineraryDetail> ItineraryDetails { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MemberQuestion> MemberQuestions { get; set; }

    public virtual DbSet<Orderlist> Orderlists { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<Restaurant> Restaurants { get; set; }

    public virtual DbSet<SelectGuider> SelectGuiders { get; set; }

    public virtual DbSet<Spot> Spots { get; set; }

	public virtual DbSet<OrderDetail> OrderDetail { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=TripNa;Integrated Security=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cityarea>(entity =>
        {
            entity.HasKey(e => e.City).HasName("PK__Cityarea__AEC4A06C78F55774");

            entity.ToTable("Cityarea");

            entity.Property(e => e.City).HasMaxLength(6);
            entity.Property(e => e.Area)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("PK__Coupon__384AF1DAA208AFC9");

            entity.ToTable("Coupon");

            entity.Property(e => e.CouponId).HasColumnName("CouponID");
            entity.Property(e => e.CouponCode)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ItineraryId).HasColumnName("ItineraryID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");

            entity.HasOne(d => d.Itinerary).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.ItineraryId)
                .HasConstraintName("FK_ItineraryCoupon");

            entity.HasOne(d => d.Member).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK_MemberCoupon");
        });

        modelBuilder.Entity<FavoriteSpot>(entity =>
        {
            entity.HasKey(e => e.FavoriteSpotId).HasName("PK__Favorite__C41C17BF29A994E6");

            entity.ToTable("FavoriteSpot");

            entity.Property(e => e.FavoriteSpotId).HasColumnName("FavoriteSpotID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.SpotId).HasColumnName("SpotID");

            entity.HasOne(d => d.Member).WithMany(p => p.FavoriteSpots)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberFS");

            entity.HasOne(d => d.Spot).WithMany(p => p.FavoriteSpots)
                .HasForeignKey(d => d.SpotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SpotFS");
        });

        modelBuilder.Entity<Guider>(entity =>
        {
            entity.HasKey(e => e.GuiderId).HasName("PK__Guider__164D9141616A7006");

            entity.ToTable("Guider");

            entity.Property(e => e.GuiderId).HasColumnName("GuiderID");
            entity.Property(e => e.GuiderArea).HasMaxLength(8);
            entity.Property(e => e.GuiderGender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.GuiderIntro).HasMaxLength(500);
            entity.Property(e => e.GuiderNickname).HasMaxLength(30);
        });

        modelBuilder.Entity<GuiderAnswer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__GuiderAn__D4825024D8BFD7A1");

            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.AnswerContent).HasMaxLength(200);
            entity.Property(e => e.AnswerTime).HasColumnType("datetime");
            entity.Property(e => e.GuiderId).HasColumnName("GuiderID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");

            entity.HasOne(d => d.Guider).WithMany(p => p.GuiderAnswers)
                .HasForeignKey(d => d.GuiderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AnswersGuiderID");

            entity.HasOne(d => d.Order).WithMany(p => p.GuiderAnswers)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AnswersOrderID");
        });

        modelBuilder.Entity<Itinerary>(entity =>
        {
            entity.HasKey(e => e.ItineraryId).HasName("PK__Itinerar__361216A6D6E3BCE4");

            entity.ToTable("Itinerary");

            entity.Property(e => e.ItineraryId).HasColumnName("ItineraryID");
            entity.Property(e => e.ItineraryName).HasMaxLength(24);
        });

        modelBuilder.Entity<ItineraryDetail>(entity =>
        {
            entity.HasKey(e => e.ItineraryDetailsId).HasName("PK__Itinerar__581D8EBBD842DEDA");

            entity.Property(e => e.ItineraryDetailsId).HasColumnName("ItineraryDetailsID");
            entity.Property(e => e.ItineraryId).HasColumnName("ItineraryID");
            entity.Property(e => e.SpotId).HasColumnName("SpotID");

            entity.HasOne(d => d.Itinerary).WithMany(p => p.ItineraryDetails)
                .HasForeignKey(d => d.ItineraryId)
                .HasConstraintName("FK_IntineraryID");

            entity.HasOne(d => d.Spot).WithMany(p => p.ItineraryDetails)
                .HasForeignKey(d => d.SpotId)
                .HasConstraintName("FK_IntinerarySpot");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Member__0CF04B38E6C43842");

            entity.ToTable("Member");

            entity.HasIndex(e => e.MemberEmail, "UQ__Member__3F37B77AEF984B1D").IsUnique();

            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.GuiderId).HasColumnName("GuiderID");
            entity.Property(e => e.MemberEmail).HasMaxLength(100);
            entity.Property(e => e.MemberName).HasMaxLength(30);
            entity.Property(e => e.MemberPassword).HasMaxLength(100);
            entity.Property(e => e.MemberPhone).HasMaxLength(12);

            entity.HasOne(d => d.Guider).WithMany(p => p.Members)
                .HasForeignKey(d => d.GuiderId)
                .HasConstraintName("FK_MemberGuider");
        });

        modelBuilder.Entity<MemberQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__MemberQu__0DC06F8CFB435405");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.QuestionContent).HasMaxLength(200);
            entity.Property(e => e.QuestionTime).HasColumnType("datetime");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberQuestions)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuestionsMemberID");

            entity.HasOne(d => d.Order).WithMany(p => p.MemberQuestions)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuestionsOrderID");
        });

        modelBuilder.Entity<Orderlist>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orderlis__C3905BAF18049EDF");

            entity.ToTable("Orderlist");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.CouponId).HasColumnName("CouponID");
            entity.Property(e => e.GuiderId).HasColumnName("GuiderID");
            entity.Property(e => e.ItineraryId).HasColumnName("ItineraryID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.OrderMatchStatus).HasMaxLength(6);
            entity.Property(e => e.OrderStatus).HasMaxLength(8);
            entity.Property(e => e.OrderTotalPrice).HasColumnType("money");

            entity.HasOne(d => d.Coupon).WithMany(p => p.Orderlists)
                .HasForeignKey(d => d.CouponId)
                .HasConstraintName("FK_OrderlistCoupon");

            entity.HasOne(d => d.Guider).WithMany(p => p.Orderlists)
                .HasForeignKey(d => d.GuiderId)
                .HasConstraintName("FK_GuiderOrderlist");

            entity.HasOne(d => d.Itinerary).WithMany(p => p.Orderlists)
                .HasForeignKey(d => d.ItineraryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItineraryOrderlist");

            entity.HasOne(d => d.Member).WithMany(p => p.Orderlists)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberOrderlist");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__Rating__FCCDF85CA62C9E1A");

            entity.ToTable("Rating");

            entity.Property(e => e.RatingId).HasColumnName("RatingID");
            entity.Property(e => e.GuiderId).HasColumnName("GuiderID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.RatingComment).HasMaxLength(300);

            entity.HasOne(d => d.Guider).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.GuiderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GuiderRating");

            entity.HasOne(d => d.Member).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MemberRating");

            entity.HasOne(d => d.Order).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderID");
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Restaura__3214EC27E273AE8D");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CityOrderIndex).HasComputedColumnSql("(case left([LocationName],(3)) when '基隆市' then (1) when '台北市' then (2) when '新北市' then (3) when '桃園市' then (4) when '台中市' then (5) when '彰化縣' then (6) when '南投縣' then (7) when '台南市' then (8) when '高雄市' then (9) when '屏東縣' then (10) when '宜蘭縣' then (11) when '花蓮縣' then (12) when '台東縣' then (13) else (999) end)", true);
            entity.Property(e => e.FoodType).HasMaxLength(50);
            entity.Property(e => e.LocationName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Region).HasMaxLength(50);
            entity.Property(e => e.RestaurantName).HasMaxLength(100);
        });

        modelBuilder.Entity<SelectGuider>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.MemberId, e.GuiderId }).HasName("PK__SelectGu__1349128D283E4045");

            entity.ToTable("SelectGuider");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.GuiderId).HasColumnName("GuiderID");

            entity.HasOne(d => d.Guider).WithMany(p => p.SelectGuiders)
                .HasForeignKey(d => d.GuiderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SelectGuiderGuiderID");

            entity.HasOne(d => d.Member).WithMany(p => p.SelectGuiders)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SelectGuiderMemberID");

            entity.HasOne(d => d.Order).WithMany(p => p.SelectGuiders)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SelectGuiderOrderID");
        });

        modelBuilder.Entity<Spot>(entity =>
        {
            entity.HasKey(e => e.SpotId).HasName("PK__Spot__61645FE7D70228A6");

            entity.ToTable("Spot");

            entity.Property(e => e.SpotId).HasColumnName("SpotID");
            entity.Property(e => e.SpotBrief).HasMaxLength(100);
            entity.Property(e => e.SpotCity).HasMaxLength(6);
            entity.Property(e => e.SpotIntro).HasMaxLength(300);
            entity.Property(e => e.SpotName).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
