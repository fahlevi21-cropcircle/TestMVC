
using Microsoft.EntityFrameworkCore;
using TestMVC.Models;

namespace TestMVC.Data
{
    public class MyScheduler : BackgroundService
    {
        private readonly IServiceProvider sp;
        public MyScheduler(IServiceProvider sp)
        {
            this.sp = sp;

            using var scope = sp.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var job = new Reminder
            {
                Description = "Testing",
                Active = true,
                Interval = 0,
                Schedule = DateTime.UtcNow.AddMinutes(1),
            };

            _dbContext.Add(job);
            _dbContext.SaveChanges();

            scope.Dispose();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = sp.CreateScope())
                    {
                        var _dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        var sch = await _dbContext.Reminder.FirstOrDefaultAsync(x => x.Schedule <= DateTime.UtcNow && x.Active);

                        if (sch != null)
                        {
                            Console.WriteLine("Reminder : " + sch.Description);
                            sch.Active = false;
                            await _dbContext.SaveChangesAsync();
                        }
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Job error :" + ex.Message);
                }
            }
        }
    }
}
