using Microsoft.EntityFrameworkCore;
using Splitza.Api.Models;

namespace Splitza.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BillSession> BillSessions => Set<BillSession>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<ReceiptItem> ReceiptItems => Set<ReceiptItem>();
    public DbSet<ItemAllocation> ItemAllocations => Set<ItemAllocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BillSession>(e =>
        {
            e.HasIndex(b => b.CreatedAt);
            e.Property(b => b.Currency).HasDefaultValue("ZAR");
        });

        modelBuilder.Entity<Person>(e =>
        {
            e.HasOne(p => p.BillSession)
             .WithMany(b => b.People)
             .HasForeignKey(p => p.BillSessionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReceiptItem>(e =>
        {
            e.HasOne(i => i.BillSession)
             .WithMany(b => b.Items)
             .HasForeignKey(i => i.BillSessionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemAllocation>(e =>
        {
            e.HasOne(a => a.ReceiptItem)
             .WithMany(i => i.Allocations)
             .HasForeignKey(a => a.ReceiptItemId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.Person)
             .WithMany(p => p.Allocations)
             .HasForeignKey(a => a.PersonId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed demo data
        var sessionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var aliceId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var bobId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var item1Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var item2Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var item3Id = Guid.Parse("66666666-6666-6666-6666-666666666666");

        modelBuilder.Entity<BillSession>().HasData(new BillSession
        {
            Id = sessionId,
            Name = "Demo — Harbour House",
            Currency = "ZAR",
            PaymentMode = PaymentMode.HostPays,
            Status = SessionStatus.Finalized,
            Subtotal = 520.00m,
            Vat = 0m,
            Tip = 78.00m,
            ServiceFee = 0m,
            Discount = 0m,
            Total = 598.00m,
            TipAllocation = AdjustmentAllocation.Proportional,
            CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Person>().HasData(
            new Person { Id = aliceId, BillSessionId = sessionId, Name = "Alice", IsHost = true },
            new Person { Id = bobId, BillSessionId = sessionId, Name = "Bob", IsHost = false }
        );

        modelBuilder.Entity<ReceiptItem>().HasData(
            new ReceiptItem { Id = item1Id, BillSessionId = sessionId, Name = "Grilled Kingklip", Quantity = 1, UnitPrice = 260.00m, LineTotal = 260.00m, SortOrder = 1 },
            new ReceiptItem { Id = item2Id, BillSessionId = sessionId, Name = "Calamari Starter", Quantity = 1, UnitPrice = 120.00m, LineTotal = 120.00m, SortOrder = 2 },
            new ReceiptItem { Id = item3Id, BillSessionId = sessionId, Name = "House Wines x2", Quantity = 2, UnitPrice = 70.00m, LineTotal = 140.00m, SortOrder = 3 }
        );

        modelBuilder.Entity<ItemAllocation>().HasData(
            new ItemAllocation { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), ReceiptItemId = item1Id, PersonId = aliceId, AllocationType = AllocationType.Full, Amount = 260.00m },
            new ItemAllocation { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), ReceiptItemId = item2Id, PersonId = bobId, AllocationType = AllocationType.Full, Amount = 120.00m },
            new ItemAllocation { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), ReceiptItemId = item3Id, PersonId = aliceId, AllocationType = AllocationType.EqualShare, Amount = 70.00m, QuantityPortion = 1 },
            new ItemAllocation { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), ReceiptItemId = item3Id, PersonId = bobId, AllocationType = AllocationType.EqualShare, Amount = 70.00m, QuantityPortion = 1 }
        );
    }
}
