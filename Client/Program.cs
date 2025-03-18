using Client.Network;
using Client.Form;

namespace Client
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new ChatForm());
        }
        
        // public static void ConsoleTest(string[] args)
        // {
        //     _ = NetworkManager.Instance;
        //     while (true)
        //     {
        //         
        //     }
        // }
    }
}