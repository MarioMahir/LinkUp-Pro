using LinkUpPro.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUpPro.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            :base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<PostReaction> PostReactions { get; set; }

        public DbSet<Friendship> Friendships { get; set; }

        public DbSet<FriendRequest> FriendRequests { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<BattleshipGame> BattleshipGames { get; set; }

        public DbSet<Ship> Ships { get; set; }

        public DbSet<Attack> Attacks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.NormalizedEmail)
                .HasDatabaseName("EmailIndex")
                .IsUnique()
                .HasFilter("[NormalizedEmail] IS NOT NULL");

            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PostReaction>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Reactions)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PostReaction>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reactions)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PostReaction>()
                .HasIndex(r => new { r.PostId, r.UserId })
                .IsUnique();

            builder.Entity<Friendship>()
                .HasOne(f => f.UserOne)
                .WithMany()
                .HasForeignKey(f => f.UserOneId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Friendship>()
                .HasOne(f => f.UserTwo)
                .WithMany()
                .HasForeignKey(f => f.UserTwoId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Friendship>()
                .HasIndex(f => new { f.UserOneId, f.UserTwoId })
                .IsUnique();

            builder.Entity<FriendRequest>()
                .HasOne(r => r.Sender)
                .WithMany()
                .HasForeignKey(r => r.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<FriendRequest>()
                .HasOne(r => r.Receiver)
                .WithMany()
                .HasForeignKey(r => r.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<FriendRequest>()
                .HasIndex(r => r.PairKey)
                .IsUnique()
                .HasFilter("[Status] = 'Pending'");

            builder.Entity<Notification>()
                .HasOne(n => n.Recipient)
                .WithMany()
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>()
                .HasOne(n => n.Actor)
                .WithMany()
                .HasForeignKey(n => n.ActorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>()
                .HasOne(n => n.Post)
                .WithMany()
                .HasForeignKey(n => n.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>()
                .HasOne(n => n.Comment)
                .WithMany()
                .HasForeignKey(n => n.CommentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BattleshipGame>()
                .HasOne(g => g.PlayerOne)
                .WithMany()
                .HasForeignKey(g => g.PlayerOneId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BattleshipGame>()
                .HasOne(g => g.PlayerTwo)
                .WithMany()
                .HasForeignKey(g => g.PlayerTwoId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BattleshipGame>()
                .HasIndex(g => g.PairKey)
                .IsUnique()
                .HasFilter("[Status] <> 'Finished'");

            builder.Entity<BattleshipGame>()
                .Property(g => g.RowVersion)
                .IsRowVersion();

            builder.Entity<Ship>()
                .HasOne(s => s.Game)
                .WithMany(g => g.Ships)
                .HasForeignKey(s => s.GameId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Ship>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Ship>()
                .HasIndex(s => new { s.GameId, s.OwnerId, s.StartRow, s.StartCol })
                .IsUnique();

            builder.Entity<Attack>()
                .HasOne(a => a.Game)
                .WithMany(g => g.Attacks)
                .HasForeignKey(a => a.GameId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Attack>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.AttackerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Attack>()
                .HasIndex(a => new { a.GameId, a.AttackerId, a.Row, a.Col })
                .IsUnique();
        }
    }
}
