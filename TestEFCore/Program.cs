#define USE_BEGINTRANSACTION
#define EXTRA_CONTEXT
using EFGetStarted;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using TestEFCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

var host = Host.CreateDefaultBuilder(args)
    .Build();

var config = host.Services.GetRequiredService<IConfiguration>();

MainCode();

static void MainCode()
{
#if EXTRA_CONTEXT
    using var ctxExt = new BloggingContext();
    var strategy = ctxExt.Database.CreateExecutionStrategy();
    strategy.Execute(() => {
#endif

        using var ctx = new BloggingContext();

        var blobs = ctx.Blogs.Where(x => x.Url.Contains("a") && x.Posts.Count > 0).Include(x => x.Posts);

        Console.WriteLine($"Query: {blobs.ToQueryString()}");

        Console.WriteLine();
        Console.WriteLine("Results: ");

#if USE_BEGINTRANSACTION
        using (var tx = ctx.Database.BeginTransaction())
#else
    var strategy = ctx.Database.CreateExecutionStrategy();
    strategy.ExecuteInTransaction(() =>
#endif
        {
            foreach (var r in blobs)
            {
                Console.WriteLine($"{r.Url} - Entries: {r.Posts.Count}");

                foreach (var p in r.Posts)
                {
                    Console.WriteLine($"  * {p.Title} - Size = {p.ContentSize} - NumberOfReads: {p.NumberOfReads}");
                }

                r.Posts.Add(new Post
                {
                    Title = $"Title {Convert.ToBase64String(Guid.NewGuid().ToByteArray())}",
                    Content = $"Random content {Guid.NewGuid()}",
                    CreatedDate = DateTimeOffset.Now,
                    //CreatedDate = DateTimeOffset.MinValue, // <- TO SHOW THE ValueConverter USAGE
                    Author = "Admin"
                });
            }

            Console.WriteLine($"Changes: {ctx.ChangeTracker.DebugView.LongView}");
            
            ctx.SaveChanges();

#if USE_BEGINTRANSACTION
            tx.CreateSavepoint("RP1");

            ctx.Blogs.First().Url += "/2";
            ctx.SaveChanges();

            tx.RollbackToSavepoint("RP1");
            tx.ReleaseSavepoint("RP1");

            Console.WriteLine("Do you want to commit changes [Y/N] ? ");

            switch (Console.ReadKey().KeyChar)
            {
                case 'Y':
                case 'y':
                    tx.Commit();
                    break;

                default:
                    tx.Rollback();
                    break;
            }
#endif
        }
#if !USE_BEGINTRANSACTION
    , () => false);
#endif

        ctx.Entry(ctx.Blogs.First()).Reload();

        //var listAdminPosts = ctx.Blogs.Where(x => BloggingContext.CountAdminPosts(x.BlogId) > 0).Select(x => new { x.BlogId, AdminPosts = BloggingContext.CountAdminPosts(x.BlogId), hash = BloggingContext.SHA256Unicode(x.Url) });

        var listAdminPosts = from x in ctx.Blogs let ap = BloggingContext.CountAdminPosts(x.BlogId)
                             where ap > 0
                             select new { x.BlogId, AdminPosts = ap, hash = BloggingContext.SHA256ASCII(x.Url), r = EF.Functions.Random() };

        //var listAdminPosts = (from x in ctx.Blogs
        //                     let ap = BloggingContext.CountAdminPosts(x.BlogId)
        //                     where ap > 0
        //                     select new { Entity = x, AdminPosts = ap }).ToList().Select(x => new { x.Entity.BlogId, AdminPosts = x.AdminPosts, hash = BloggingContext.SHA256ASCII(x.Entity.Url) });

        foreach (var blog in listAdminPosts)
        {
            Console.WriteLine($"- {blog.BlogId,5} : admin posts = {blog.AdminPosts} - SHA256: {BitConverter.ToString(blog.hash)}");
        }

        foreach (var blog in ctx.BlogWithPostCounts)
        {
            Console.WriteLine($"-- {blog.Url} -> {blog.PostCount} posts");
        }

        ctx.IncrementNumberOfReads();

        Console.WriteLine("POSTS WITH MORE THAN 10 READERS: ");

        foreach (var p in ctx.Posts.WithReadsGreatedThen(10))
        {
            Console.WriteLine($"- {p.Title}, reads = {p.NumberOfReads}, blogURL = {p.Blog.Url}");
        }

        Console.WriteLine("POSTS WITH MORE THAN 20 READERS: ");

        foreach (var p in ctx.MostReadPosts().OrderBy(x=> x.NumberOfReads))
        {
            Console.WriteLine($"- {p.Title}, reads = {p.NumberOfReads}, blogURL = {p.Blog.Url}");
        }

        //Expression<Func<IQueryable<Post>> exp = () => ctx.Posts.Where(x => x.NumberOfReads > 1);

        IQueryable<CommentsToPosts> dd = ctx.Comments.FromSqlInterpolated($"SELECT * FROM CommentsToPosts WHERE {typeof(CommentsToPosts).GetProperty(nameof(CommentsToPosts.Author))!.GetCustomAttribute<ColumnAttribute>()!.Name} <> 'Ciao'");

        foreach (var comment in dd) { Console.WriteLine(comment.CommentId);  }


        // PARAMETRIC ORDER BY:
        var result = ctx.Posts.ParametricOrderBy(nameof(Post.NumberOfReads));

        foreach(Post post in result) { Console.WriteLine($"- {post.Title}");  }

#if EXTRA_CONTEXT
    });
#endif

}
