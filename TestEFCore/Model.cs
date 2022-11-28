using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EFGetStarted
{
    public class BloggingContext : DbContext
    {
        private const string TEST_CONNECTION_STRING = 
            "Server=tcp:localhost,1433;Initial Catalog=Test;Persist Security Info=False;User ID=sa;Password=GattoCat!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";

        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!; 
        public DbSet<CommentsToPosts> Comments { get; set; } = null!;
        public DbSet<BlogWithPostCount> BlogWithPostCounts { get; set; } = null!;

        public void IncrementNumberOfReads() { this.Database.ExecuteSqlRaw("IncrementNumberOfReads"); }

        public BloggingContext()
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Pre-convention model configuration goes here
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(
                    TEST_CONNECTION_STRING,
                    options =>
                    {
                        options.EnableRetryOnFailure();
                        //options.ExecutionStrategy(...);
                    })
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .LogTo(message => Debug.WriteLine(message));

        public static int CountAdminPosts(int blobId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Post> MostReadPosts() => FromExpression(() => MostReadPosts());

        public static byte[] SHA256ASCII(string input)
        {
            return SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(input));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>().HasCheckConstraint("1", "Title like '%olo%'");

            modelBuilder.HasDbFunction(this.GetType().GetMethod(nameof(CountAdminPosts))!)
                .HasName("CountAdminPosts");

            modelBuilder.HasDbFunction(this.GetType().GetMethod(nameof(SHA256ASCII))!)
                .HasTranslation(args => new SqlFunctionExpression("HASHBYTES", new SqlExpression[]
                {
                    new SqlConstantExpression(System.Linq.Expressions.Expression.Constant("SHA2_256"), new StringTypeMapping("VARCHAR(10)", DbType.String)),
                    new SqlUnaryExpression(System.Linq.Expressions.ExpressionType.Convert, args[0], typeof(string), new StringTypeMapping("VARCHAR", DbType.String))
                }, false, new bool[] { false }, typeof(byte[]), new ByteArrayTypeMapping("VARBINARY"))
            );

            modelBuilder.HasDbFunction(this.GetType().GetMethod(nameof(MostReadPosts))!);

            modelBuilder.Entity<Post>().Property(nameof(Post.ContentSize)).HasComputedColumnSql("LEN([Content])").HasConversion(typeof(long));

            modelBuilder.Entity<BlogWithPostCount>().ToView(nameof(BlogWithPostCount)).HasKey(nameof(BlogWithPostCount.BlogId));
            modelBuilder.Entity<BlogWithPostCount>().Property(nameof(BlogWithPostCount.PostCount)).HasComputedColumnSql();

            modelBuilder.Entity<Post>().Property(nameof(Post.CreatedDate)).HasConversion<DateValueConverter>();

            modelBuilder.Entity<CommentsToPosts>().HasKey(nameof(CommentsToPosts.CommentId));
        }
    }

    class DateValueConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
    {
        public DateValueConverter() 
            : base(x => (x.Date < new DateTime(1970, 1, 1) ? new DateTimeOffset(1970, 1, 1, 0, 0, 0, x.Offset) : x), x => x)
        {
        }
    }

    public class Blog
    {
        [Key()]
        public int BlogId { get; set; }
        public string Url { get; set; } = null!;
        public string? MainAuthor3 { get; set; }

        public List<Post> Posts { get; } = new();
    }

    public class BlogWithPostCount
    {
        public int BlogId { get; set; }
        public string Url { get; set; } = null!;
        public int PostCount { get; }
    }

    public class Post
    {
        public int PostId { get; set; }
        [Column(TypeName = "VARCHAR(50)")]
        public string Title { get; set; } = null!;
        [Column(TypeName = "NVARCHAR(80)")]
        public string Author { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTimeOffset? CreatedDate { get; set; }

        public int ContentSize { get; }
        public int NumberOfReads { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; } = null!;
    }

    [Table("CommentsToPosts")]
    public class CommentsToPosts
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public string CommentContent { get; set; } = null!;
        [Column("Autore"), MaxLength(100)]
        public string Author { get; set; } = null!;
    }

    public static class PostExtensions
    {
        public static IQueryable<Post> WithReadsGreatedThen(this DbSet<Post> p, int num)
        {
            return p.FromSqlInterpolated($"PostsWithReadsGreatedThen {num}");
        }
    }
}