using Microsoft.AspNetCore;
using Serilog;

namespace DemoWebAPI.Services
{
    public class SerilogLogger
    {
        private readonly IWebHostEnvironment env;

        public SerilogLogger(IWebHostEnvironment env)
        {
            this.env = env;
        }
        public void LogMessage(string message)
        {
            string path = env.ContentRootPath + @"/Logs/Log.txt";
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(path, rollingInterval: RollingInterval.Day)
            .CreateLogger();

            Log.Information(message);

            /*int a = 10, b = 0;
            try
            {
                Log.Debug("Dividing {A} by {B}", a, b);
                Console.WriteLine(a / b);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Something went wrong");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }*/
        }
    }
}
