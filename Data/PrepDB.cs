
namespace DemoWebAPI.Data
{
    public static class PrepDB
    {
        public static void PrepPopulation(IApplicationBuilder app)
        {
            using(var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>());
            }
        }

        private static void SeedData(AppDbContext? context)
        {
            if (!context.Tasks.Any())
            {
                Console.WriteLine("Populating database...");
                context.Tasks.AddRange
                (
                    new Models.Tasks()
                    {
                        Name = "Buy Groceries",
                        IsComplete = false,
                        DateAdded = DateTime.Now,
                        DateCompleted = null,
                        DateModified = null
                    },
                    new Models.Tasks()
                    {
                        Name = "Clean The House",
                        IsComplete = false,
                        DateAdded = DateTime.Now,
                        DateCompleted = null,
                        DateModified = null
                    },
                    new Models.Tasks()
                    {
                        Name = "Hang out with friends",
                        IsComplete = false,
                        DateAdded = DateTime.Now,
                        DateCompleted = null,
                        DateModified = null
                    }
                );
                context.Users.AddRange(
                    new Models.Users()
                    {
                        Name = "Daberechukwu",
                        Code = 0923
                    },
                    new Models.Users()
                    {
                        Name = "Oluebubechukwu",
                        Code = 6342
                    }
                    );
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("Data Already Exists");
            }
        }
    }
}
