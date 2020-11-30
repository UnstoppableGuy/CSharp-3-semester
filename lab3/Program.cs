using System;
using System.ServiceProcess;
using System.IO;

namespace FWS
{
    static class Program
    {
        static void Main()
        {
            Service1 service = new Service1();

            try
            {
                ServiceBase.Run(service);
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exceptions.txt"), true))
                {
                    sw.WriteLine($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} Exception: {ex.Message}");
                }
            }
        }
    }
}
