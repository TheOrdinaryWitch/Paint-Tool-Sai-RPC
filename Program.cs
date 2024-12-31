using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using DiscordRPC;
using Microsoft.Win32;
class Program
{
    // Import the GetWindowText function from the user32.dll
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    // Import the GetForegroundWindow function from user32.dll
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetForegroundWindow();

    private static DiscordRpcClient client;
    private static string previousFileName = "";
    [STAThread]
    static void Main(string[] args)
    {
        AddToStartup();

        Thread workerThread = new Thread(() =>
        {
            while (true)
            {
                while (true)
                {
                    if (GetPid() != 0)
                    {
                        InitializeRPC();
                        break;
                    }
                    Thread.Sleep(2500);
                }

                while (true)
                {
                    if (GetPid() == 0)
                    {
                        ShutdownRPC();
                        break;
                    }
                    UpdatePresence();
                    Thread.Sleep(2500);
                }
            }
        });

        workerThread.IsBackground = true;
        workerThread.Start();

        // Keep the application alive with an infinite loop
        while (true)
        {
            Thread.Sleep(1000);
        }
    }

    private static void InitializeRPC()
    {
        // Replace "Your_Application_Client_ID" with your actual Discord App Client ID
        client = new DiscordRpcClient("DISCORD_KEY");

        // Initialize the client
        client.Initialize();

        // Set initial presence
        UpdatePresence();
    }
    private static void UpdatePresence()
    {
        string currentFileName = GetWindowsName();
        if (currentFileName != previousFileName)
        {
            previousFileName = currentFileName;
            client.SetPresence(new RichPresence()
            {
                Details = currentFileName,
                Timestamps = Timestamps.Now,
                Assets = new Assets()
                {
                    LargeImageKey = "sailogo", // Must match an asset key uploaded to your Discord App
                }
            });
        }
    }
    private static void ShutdownRPC()
    {
        // Dispose client when done to avoid memory leaks
        client.Dispose();
    }
    public static void AddToStartup()
    {
        string appName = "Pain Tool Sai 2 RPC";
        string appPath = Path.ChangeExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, ".exe");

        RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        rk.SetValue(appName, $"\"{appPath}\""); // Add quotes to handle spaces in path
    }
    public static string GetWindowsName()
    {
            Process process = Process.GetProcessById(GetPid());
            IntPtr hWnd = process.MainWindowHandle; // Get the window handle

            if (hWnd != IntPtr.Zero)
            {
                System.Text.StringBuilder windowTitle = new System.Text.StringBuilder(256); // Buffer for window title
                _ = GetWindowText(hWnd, windowTitle, windowTitle.Capacity);
                if (windowTitle.ToString().Contains(" - "))
                {
                    int index = windowTitle.ToString().LastIndexOf(" - ");
                    string withoutPrefix = windowTitle.ToString().Substring(index + 3);
                    string fileName = Path.GetFileName(withoutPrefix);
                    return "Editing: "+ fileName;
                }
                else
                {
                    return "Starting a New Canvas";
                }
            }
        return previousFileName;
    }
    private static int GetPid()
    {
        Process[] processes = Process.GetProcessesByName("sai2");
        if (processes.Length > 0)
        {
            for (int i = 0; i < processes.Length; i++)
            {
                return processes[i].Id;
            }
        }
        return 0;
    }
}