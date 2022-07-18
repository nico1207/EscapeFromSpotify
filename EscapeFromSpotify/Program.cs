using System.Diagnostics;
using System.Drawing;
using Windows.Media.Control;
using PInvoke;

public class Program
{
    public static async Task Main(string[] args)
    {
        var sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        if (sessionManager == null)
        {
            throw new Exception("Unable to control audio.");
        }
        
        Console.WriteLine("Looking for Tarkov application...");
        var process = WaitForTarkov();
        Console.WriteLine("Found Tarkov!");
        Thread.Sleep(1000);

        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

        var placement = new User32.WINDOWPLACEMENT();
        unsafe
        {
            User32.GetWindowPlacement(process.MainWindowHandle, &placement);
        }
        
        int width = placement.rcNormalPosition.right - placement.rcNormalPosition.left;
        int height = placement.rcNormalPosition.bottom - placement.rcNormalPosition.top;
        
        Console.WriteLine($"Window size: {width}x{height}");
        if (!(width == 1920 && height == 1080) && !(width == 3840 && height == 2160))
        {
            throw new Exception("Window size is not 1920x1080 or 3840x2160.");
        }
        
        int coordMultiplier = width == 1920 ? 1 : 2;

        using var bitmap = new Bitmap(width, height);
        using var bitmapGraphics =
            Graphics.FromImage(bitmap);

        Console.WriteLine("Waiting for match to start...");

        while (true)
        {
            if (process.HasExited)
            {
                Console.WriteLine("Tarkov was closed. Waiting for it to start again...");
                process = WaitForTarkov();
                Console.WriteLine("Found Tarkov!");
                Thread.Sleep(1000);
            }
            
            User32.PrintWindow(process.MainWindowHandle, bitmapGraphics.GetHdc(), (User32.PrintWindowFlags)0x02);
            bitmapGraphics.ReleaseHdc();
            
            // Check match start screen by detecting red "match starting" box
            var topLeft = bitmap.GetPixel(863 * coordMultiplier, 582 * coordMultiplier);
            var topLeftReferenceColor = Color.FromArgb(186, 9, 9);
            var bottomRight = bitmap.GetPixel(1067 * coordMultiplier, 606 * coordMultiplier);
            var bottomRightReferenceColor = Color.FromArgb(182, 8, 9);
            
            if (Math.Abs(topLeft.R - topLeftReferenceColor.R) < 30 &&
                Math.Abs(topLeft.G - topLeftReferenceColor.G) < 30 &&
                Math.Abs(topLeft.B - topLeftReferenceColor.B) < 30 &&
                Math.Abs(bottomRight.R - bottomRightReferenceColor.R) < 30 &&
                Math.Abs(bottomRight.G - bottomRightReferenceColor.G) < 30 &&
                Math.Abs(bottomRight.B - bottomRightReferenceColor.B) < 30)
            {
                Console.WriteLine("Match is starting!");
                // Pause media
                var session = sessionManager.GetCurrentSession();
                if (session != null)
                {
                    Console.WriteLine("Pausing media.");
                    session.TryPauseAsync();
                }
                Console.WriteLine("Waiting for match to end...");
                Thread.Sleep(30000);
            }
            
            // Check match end screen
            var left = bitmap.GetPixel(850 * coordMultiplier, 25 * coordMultiplier);
            var right = bitmap.GetPixel(1074 * coordMultiplier, 38 * coordMultiplier);
            var center = bitmap.GetPixel(943 * coordMultiplier, 39 * coordMultiplier);
            
            if (left.R == 255 && left.G == 255 && left.B == 255 &&
                right.R == 255 && right.G == 255 && right.B == 255 &&
                center.R != 255 && center.G != 255 && center.B != 255)
            {
                Console.WriteLine("Match is over!");
                // Resume media
                var session = sessionManager.GetCurrentSession();
                if (session != null)
                {
                    Console.WriteLine("Resuming media.");
                    session.TryPlayAsync();
                }
                Console.WriteLine("Waiting for match to start...");
                Thread.Sleep(30000);
            }

            Thread.Sleep(2000);
        }
    }

    public static Process WaitForTarkov()
    {
        while (true)
        {
            var processes = Process.GetProcesses();
            var process = processes.FirstOrDefault(p => p.MainWindowTitle == "EscapeFromTarkov");
            if (process != null)
            {
                return process;
            }
            Thread.Sleep(1000);
        }
    }
}