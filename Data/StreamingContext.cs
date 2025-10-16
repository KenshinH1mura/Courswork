using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

public class StreamingContext : DbContext
{
    public StreamingContext() : base("name=StreamingDb")
    {
        Database.SetInitializer(new StreamingDbInitializer());
    }

    public DbSet<User> Users { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Media> Media { get; set; }
    public DbSet<SubscriptionMedia> SubscriptionMedia { get; set; }
    public DbSet<View> Views { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

        modelBuilder.Entity<Subscription>()
            .HasRequired(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.user_id);

        modelBuilder.Entity<Subscription>()
            .HasRequired(s => s.SubscriptionPlan)
            .WithMany(sp => sp.Subscriptions)
            .HasForeignKey(s => s.plan_id);

        modelBuilder.Entity<Payment>()
            .HasRequired(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.user_id);

        modelBuilder.Entity<Payment>()
            .HasRequired(p => p.Subscription)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.subscription_id);

        modelBuilder.Entity<Payment>()
            .HasRequired(p => p.PaymentMethod)
            .WithMany(pm => pm.Payments)
            .HasForeignKey(p => p.method_id);

        modelBuilder.Entity<Media>()
            .HasRequired(m => m.Genre)
            .WithMany(g => g.Media)
            .HasForeignKey(m => m.genre_id);

        modelBuilder.Entity<View>()
            .HasRequired(v => v.User)
            .WithMany(u => u.Views)
            .HasForeignKey(v => v.user_id);

        modelBuilder.Entity<View>()
            .HasRequired(v => v.Media)
            .WithMany(m => m.Views)
            .HasForeignKey(v => v.media_id);

        modelBuilder.Entity<SubscriptionMedia>()
            .HasRequired(sm => sm.SubscriptionPlan)
            .WithMany(sp => sp.SubscriptionMedia)
            .HasForeignKey(sm => sm.plan_id);

        modelBuilder.Entity<SubscriptionMedia>()
            .HasRequired(sm => sm.Media)
            .WithMany(m => m.SubscriptionMedia)
            .HasForeignKey(sm => sm.media_id);
    }
}

public class StreamingDbInitializer : CreateDatabaseIfNotExists<StreamingContext>
{
    protected override void Seed(StreamingContext context)
    {
        var paymentMethods = new[]
        {
            new PaymentMethod { method_name = "Банковская карта", provider = "Visa/MasterCard", currency = "RUB" },
            new PaymentMethod { method_name = "Электронный кошелек", provider = "YooMoney", currency = "RUB" },
            new PaymentMethod { method_name = "Мобильный платеж", provider = "Tele2", currency = "RUB" }
        };
        context.PaymentMethods.AddRange(paymentMethods);

        var subscriptionPlans = new[]
        {
            new SubscriptionPlan
            {
                name = "Базовый",
                description = "Доступ к базовому контенту в стандартном качестве",
                monthly_price = 299,
                billing_cycle = "monthly"
            },
            new SubscriptionPlan
            {
                name = "Стандартный",
                description = "Доступ к большему количеству контента в HD качестве",
                monthly_price = 449,
                billing_cycle = "monthly"
            },
            new SubscriptionPlan
            {
                name = "Премиум",
                description = "Полный доступ ко всему контенту в 4K качестве",
                monthly_price = 599,
                billing_cycle = "monthly"
            }
        };
        context.SubscriptionPlans.AddRange(subscriptionPlans);

        var genres = new[]
        {
            new Genre { name = "Драма", description = "Драматические произведения", age_rating = "16+" },
            new Genre { name = "Комедия", description = "Комедийные произведения", age_rating = "12+" },
            new Genre { name = "Фантастика", description = "Научная фантастика и фэнтези", age_rating = "12+" },
            new Genre { name = "Боевик", description = "Экшн и приключения", age_rating = "16+" },
            new Genre { name = "Документальный", description = "Документальные фильмы", age_rating = "0+" }
        };
        context.Genres.AddRange(genres);

        context.SaveChanges();

        var media = new[]
        {
            new Media { title = "Интерстеллар", description = "Фантастика о космических путешествиях", genre_id = 3, release_date = new DateTime(2014, 11, 7), rating = 8.6m },
            new Media { title = "Начало", description = "Фильм о снах и реальности", genre_id = 3, release_date = new DateTime(2010, 7, 16), rating = 8.7m },
            new Media { title = "Однажды в Голливуде", description = "Комедийная драма о Голливуде", genre_id = 2, release_date = new DateTime(2019, 7, 26), rating = 7.6m },
            new Media { title = "Джентльмены", description = "Криминальная комедия", genre_id = 2, release_date = new DateTime(2019, 12, 3), rating = 8.0m },
            new Media { title = "Остров проклятых", description = "Психологический триллер", genre_id = 1, release_date = new DateTime(2010, 2, 13), rating = 8.9m },
            new Media { title = "Побег из Шоушенка", description = "Драма о надежде и свободе", genre_id = 1, release_date = new DateTime(1994, 9, 10), rating = 9.3m },
            new Media { title = "Крепкий орешек", description = "Классический боевик", genre_id = 4, release_date = new DateTime(1988, 7, 12), rating = 8.2m },
            new Media { title = "Наша планета", description = "Документальный сериал о природе", genre_id = 5, release_date = new DateTime(2019, 4, 5), rating = 9.3m }
        };
        context.Media.AddRange(media);

        context.SaveChanges();

        var subscriptionMedia = new[]
        {
            new SubscriptionMedia { plan_id = 1, media_id = 1, available_from = new DateTime(2020, 1, 1), available_to = null },
            new SubscriptionMedia { plan_id = 1, media_id = 2, available_from = new DateTime(2020, 1, 1), available_to = null },
            new SubscriptionMedia { plan_id = 1, media_id = 3, available_from = new DateTime(2020, 1, 1), available_to = null },
            new SubscriptionMedia { plan_id = 2, media_id = 4, available_from = new DateTime(2020, 1, 1), available_to = null },
            new SubscriptionMedia { plan_id = 2, media_id = 5, available_from = new DateTime(2020, 1, 1), available_to = null },
            new SubscriptionMedia { plan_id = 2, media_id = 6, available_from = new DateTime(2020, 1, 1), available_to = null },
            new SubscriptionMedia { plan_id = 3, media_id = 7, available_from = new DateTime(2020, 1, 1), available_to = null },
            new SubscriptionMedia { plan_id = 3, media_id = 8, available_from = new DateTime(2020, 1, 1), available_to = null },
            new SubscriptionMedia { plan_id = 3, media_id = 1, available_from = new DateTime(2020, 1, 1), available_to = null },
            new SubscriptionMedia { plan_id = 3, media_id = 2, available_from = new DateTime(2020, 1, 1), available_to = null }
        };
        context.SubscriptionMedia.AddRange(subscriptionMedia);

        context.SaveChanges();
    }
}