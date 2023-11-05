using KIPFINSchedule.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Extensions.Logging;
using SerilogLoggerFactory = Serilog.Extensions.Logging.SerilogLoggerFactory;

namespace KIPFINSchedule.Database;

public sealed class AppContext : DbContext
{
    public AppContext(DbContextOptionsBuilder<AppContext> builder) : base(builder.Options) { Database.EnsureCreated(); }
    public AppContext(DbContextOptions<AppContext> options) : base(options) { Database.EnsureCreated(); }

    public DbSet<SubscriptionEntity> Subscriptions { get; set; } = null!;
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<SettingEntity> Settings { get; set; } = null!;
    public DbSet<ProfileEntity> Profiles { get; set; } = null!;

    public AppContext GetNewInstance() => new (new DbContextOptionsBuilder<AppContext>()
        .UseNpgsql(this.Database.GetConnectionString())
        .UseLoggerFactory(new SerilogLoggerFactory(new LoggerConfiguration()
            .WriteTo.File($"./logs/db/db-{DateTime.Now:yy-MM-dd hh-mm-ss}.log").CreateLogger())));
}