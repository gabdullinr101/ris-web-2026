using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

// Оглавление базы: какие таблицы видит программа и как они устроены.
// Имена таблиц, столбцов, ключей и ограничений заданы явно - те же, что в db/schema.sql (ЛР9).
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hall>(hall =>
        {
            hall.ToTable("halls");
            hall.HasKey(h => h.Id).HasName("halls_pkey");
            hall.Property(h => h.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            hall.Property(h => h.Name).HasColumnName("name").IsRequired();
            hall.HasIndex(h => h.Name).IsUnique().HasDatabaseName("halls_name_key");

            // Справочник залов - те же строки, что в db/seed.sql
            hall.HasData(
                new Hall { Id = 1, Name = "Тренажёрный зал" },
                new Hall { Id = 2, Name = "Зал единоборств" },
                new Hall { Id = 3, Name = "Зал групповых программ" });
        });

        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("users", t => t.HasCheckConstraint(
                "users_role_check", "role IN ('Admin', 'Trainer', 'Manager')"));
            user.HasKey(u => u.Id).HasName("users_pkey");
            user.Property(u => u.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            user.Property(u => u.Login).HasColumnName("login").IsRequired();
            user.Property(u => u.FullName).HasColumnName("full_name").IsRequired();
            user.Property(u => u.Role).HasColumnName("role").IsRequired();
            user.Property(u => u.PasswordHash).HasColumnName("password_hash");
            user.HasIndex(u => u.Login).IsUnique().HasDatabaseName("users_login_key");

            // Учебные учётки (пароль у всех Fitness2026!, см. README). В базе - только hash,
            // посчитанный PasswordHasher из ASP.NET Identity (PBKDF2, соль, 100 000 итераций).
            user.HasData(
                new User
                {
                    Id = 1, Login = "admin", FullName = "Анна Ковалёва", Role = UserRoles.Admin,
                    PasswordHash = "AQAAAAIAAYagAAAAEAw/EGtrNeEt1C9Uo7lWtpUMvIsHVMeTfWUsHnApVx7Mu1EXYmWv0scWDnyEmL4hvA==",
                },
                new User
                {
                    Id = 2, Login = "trainer", FullName = "Илья Соколов", Role = UserRoles.Trainer,
                    PasswordHash = "AQAAAAIAAYagAAAAEDcgI/aryuvEd4XMzMXz53H0v9Duh1xsLmHnVYGb9eYwM7lXFkh3gzPnt3L7vaLy7Q==",
                },
                new User
                {
                    Id = 3, Login = "trainer2", FullName = "Марат Хайруллин", Role = UserRoles.Trainer,
                    PasswordHash = "AQAAAAIAAYagAAAAEO39nFcWqFHojE5/9bd3ciqh887qf639LgbPU2WF+YEnHDl+0XcIg4oCKgDRy6FRQw==",
                },
                new User
                {
                    Id = 4, Login = "manager", FullName = "Ольга Петрова", Role = UserRoles.Manager,
                    PasswordHash = "AQAAAAIAAYagAAAAEDiJbFxktscOqHrVrk/CWkxC0w2J/l0JxsNBRKdueSBOAMlU6Cjs7mItGbmS61stkQ==",
                });
        });

        modelBuilder.Entity<Booking>(booking =>
        {
            booking.ToTable("bookings", t =>
            {
                t.HasCheckConstraint("bookings_status_check",
                    "status IN ('New', 'InProgress', 'Closed', 'Cancelled')");
                t.HasCheckConstraint("bookings_title_check",
                    "char_length(title) BETWEEN 5 AND 80");
                t.HasCheckConstraint("bookings_description_check",
                    "char_length(description) BETWEEN 10 AND 500");
            });
            booking.HasKey(b => b.Id).HasName("bookings_pkey");
            booking.Property(b => b.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            booking.Property(b => b.Number).HasColumnName("number").IsRequired();
            booking.Property(b => b.Title).HasColumnName("title").IsRequired();
            booking.Property(b => b.Description).HasColumnName("description").IsRequired();
            booking.Property(b => b.HallId).HasColumnName("hall_id");
            booking.Property(b => b.Status).HasColumnName("status").IsRequired().HasDefaultValue(BookingStatuses.New);
            booking.Property(b => b.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            booking.Property(b => b.CreatedByUserId).HasColumnName("created_by_user_id");
            booking.Property(b => b.AssigneeUserId).HasColumnName("assignee_user_id");

            booking.HasIndex(b => b.Number).IsUnique().HasDatabaseName("bookings_number_key");
            // зачем: фильтр списка и сводка по статусу
            booking.HasIndex(b => b.Status).HasDatabaseName("bookings_status_idx");
            // зачем: соединение с залами
            booking.HasIndex(b => b.HallId).HasDatabaseName("bookings_hall_id_idx");
            // зачем: тренер видит только свои записи
            booking.HasIndex(b => b.AssigneeUserId).HasDatabaseName("bookings_assignee_user_id_idx");
            // внешний ключ на создателя: EF всё равно заводит индекс, даём ему понятное имя
            booking.HasIndex(b => b.CreatedByUserId).HasDatabaseName("bookings_created_by_user_id_idx");

            booking.HasOne(b => b.Hall).WithMany()
                .HasForeignKey(b => b.HallId)
                .HasConstraintName("bookings_hall_id_fkey")
                .OnDelete(DeleteBehavior.Restrict);
            booking.HasOne(b => b.CreatedByUser).WithMany()
                .HasForeignKey(b => b.CreatedByUserId)
                .HasConstraintName("bookings_created_by_user_id_fkey")
                .OnDelete(DeleteBehavior.Restrict);
            booking.HasOne(b => b.AssigneeUser).WithMany()
                .HasForeignKey(b => b.AssigneeUserId)
                .HasConstraintName("bookings_assignee_user_id_fkey")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
