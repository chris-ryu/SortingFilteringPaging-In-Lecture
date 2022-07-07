using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace SortingFilteringPaging.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var appDbContext = new AppDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<AppDbContext>>()))
            {
                if (appDbContext == null || appDbContext.Employees == null)
                {
                    throw new ArgumentNullException("Null RazorPagesMovieContext");
                }

                // Look for any movies.
                if (appDbContext.Employees.Any())
                {
                    return;   // DB has been seeded
                }

                if (!appDbContext.Employees.Any())
                {
                    var filePath = Path.Combine(Environment.CurrentDirectory, "Seed", "employees.json");
                    Console.WriteLine($"filePath {filePath}");
                    var employees = JsonConvert.DeserializeObject<List<Employee>>(File.ReadAllText(filePath));
                    appDbContext.Employees.AddRange(employees);
                    appDbContext.SaveChanges();
                }
                appDbContext.SaveChanges();
            }
        }
    }
}