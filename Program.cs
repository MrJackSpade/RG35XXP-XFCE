using RG35XX.Core.Drawing;
using RG35XX.Core.Extensions;
using RG35XX.Core.Fonts;
using RG35XX.Core.GamePads;
using RG35XX.Libraries;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace RG35XX_XFCE
{
    internal partial class Program
    {

        private static bool _completed = false;

        private static readonly ConsoleRenderer _consoleRenderer = new(ConsoleFont.Px437_IBM_VGA_8x16);

        private static readonly GamePadReader _gamePadReader = new();

        private static bool _gui = false;

        private static readonly object LogLock = new();

        private static string LogFile { get; set; } = Path.Combine(AppContext.BaseDirectory, $"XFCE-Setup-{DateTime.Now.Ticks}.log");

        private static void GuiToggleThread()
        {
            while (!_completed)
            {
                GamepadKey key = _gamePadReader.WaitForInput();

                switch (key)
                {
                    case GamepadKey.A_DOWN:
                    case GamepadKey.B_DOWN:
                    case GamepadKey.X_DOWN:
                    case GamepadKey.Y_DOWN:
                    case GamepadKey.START_DOWN:
                    case GamepadKey.SELECT_DOWN:
                        _gui = !_gui;
                        _consoleRenderer.AutoFlush = !_gui;
                        break;
                }
            }
        }

        private static void GuiThread()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            string[] resourceNames = thisAssembly.GetManifestResourceNames();

            resourceNames = resourceNames.OrderBy(x => x).ToArray();

            OffsetBitmap[] offsetBitmaps = new OffsetBitmap[resourceNames.Length];

            for (int i = 0; i < offsetBitmaps.Length; i++)
            {
                string resourceName = resourceNames[i];

                using Stream stream = thisAssembly.GetManifestResourceStream(resourceName);

                Bitmap bitmap = new(stream);

                string fName = Path.GetFileNameWithoutExtension(resourceName);
                fName = fName.Split('.').Last();

                string[] parts = fName.Split('_');

                int x = int.Parse(parts[2]);
                int y = int.Parse(parts[3]);

                offsetBitmaps[i] = new OffsetBitmap
                {
                    X = x,
                    Y = y,
                    Bitmap = bitmap
                };
            }

            const int ITERATIONS_PER_SECOND = 12;
            long ticksPerIteration = Stopwatch.Frequency / ITERATIONS_PER_SECOND;

            Stopwatch stopwatch = Stopwatch.StartNew();
            long nextIterationTick = ticksPerIteration;

            int index = 0;

            _consoleRenderer.AutoFlush = false;
            Thread guiToggleThread = new(GuiToggleThread);
            guiToggleThread.Start();

            while (!_completed)
            {
                if (_gui)
                {
                    OffsetBitmap b = offsetBitmaps[index];
                    Bitmap baseImage = _consoleRenderer.Render();
                    baseImage.DrawTransparentBitmap(b.X, baseImage.Height - b.Bitmap.Height, b.Bitmap);
                    _consoleRenderer.FrameBuffer.Draw(baseImage, 0, 0);
                    index++;
                    if (index >= offsetBitmaps.Length)
                    {
                        index = 0;
                    }
                }

                // Calculate remaining time
                long remainingTicks = nextIterationTick - stopwatch.ElapsedTicks;
                if (remainingTicks > 0)
                {
                    int sleepMs = (int)(remainingTicks * 1000 / Stopwatch.Frequency);
                    if (sleepMs > 0)
                    {
                        Thread.Sleep(sleepMs);
                    }
                }

                nextIterationTick += ticksPerIteration;
            }
        }

        private static void Log(string text, Color color)
        {
            lock (LogLock)
            {
                _consoleRenderer.WriteLine(text, color);
                File.AppendAllText(LogFile, $"{text}\n");
            }
        }

        private static async Task Main(string[] args)
        {
            _consoleRenderer.Initialize(640, 480);
            _gamePadReader.Initialize();

            Thread guiThread = new(GuiThread);
            guiThread.Start();
            _gui = true;
            try
            {
                string payloadDirectoryPath = Path.Combine(AppContext.BaseDirectory, "payload");

                await Utilities.CorrectSystemTime();

                RunLogged("Updating the package list", "apt update");

                //LANGUAGE PACKS
                RunLogged("Installing english language...", "apt install language-pack-en");
                RunLogged("Setting locale...", "echo \"en_US.UTF-8 UTF-8\" > /etc/locale.gen");
                RunLogged("Generating locales...", "locale-gen en_US.UTF-8");
                RunLogged("Updating locale...", "update-locale LANG=en_US.UTF-8 LANGUAGE=en_US:en");

                //XFCE
                RunLogged("Installing Xorg and XFCE", "apt install -y xorg xfce4 qjoypad");
                RunLogged("Setting up root...", "mkdir -p /root");
                RunLogged("Prepping browser...", "rm /usr/bin/firefox");

                DirectoryInfo payloadDirectory = new(payloadDirectoryPath);

                Log($"Enumerating files in '{payloadDirectory}'", Color.White);
                foreach (FileInfo file in payloadDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    string relativePath = file.FullName[(payloadDirectory.FullName.Length + 1)..];
                    string targetPath = Path.Combine("/", relativePath);
                    FileInfo target = new(targetPath);

                    if (!target.Directory.Exists)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            target.Directory.Create();
                        }
                    }

                    Log($"Installing {file.FullName} => ", Color.White);
                    Log($"    {targetPath} ", Color.White);

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        file.MoveTo(targetPath, true);
                    }
                }

                RunLogged("Extracting firefox...", "tar -xf /usr/bin/firefox/libxul.so.tar.gz -C /usr/bin/firefox");
                RunLogged("Removing archive...", "rm /usr/bin/firefox/libxul.so.tar.gz");
                RunLogged("Configuring thunar...", "xfconf-query --channel thunar --property /misc-exec-shell-scripts-by-default --create --type bool --set true");

                RunLogged("Setting up root...", "mkdir -p /home/root");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (Directory.GetFiles("/root").Length != 0)
                    {
                        RunLogged("Copying files...", "cp -R /root/* /home/root/");
                    }
                }

                RunLogged("Setting permissions...", "chown -R root:root /home/root");
                RunLogged("Updating passwd...", "sed -i 's|root:x:0:0:root:/root:|root:x:0:0:root:/home/root:|' /etc/passwd");

                RunLogged("Installing dependencies...", "LC_ALL=C DEBIAN_FRONTEND=noninteractive apt install -y parole nano mousepad onboard lsof xterm network-manager network-manager-gnome");
                RunLogged("Cleaning Metadata...", "apt clean");

                RunLogged("Removing screensaver 1/3...", "rm /etc/xdg/autostart/xfce4-screensaver.desktop");
                RunLogged("Removing screensaver 2/3...", "rm /etc/xdg/autostart/xscreensaver.desktop");
                RunLogged("Removing screensaver 3/3...", "apt remove -y xfce4-screensaver");

                // freetype installation
                _consoleRenderer.WriteLine("Installing freetype...", Color.White);
                string freetypeSourceDir = "/home/root/setup/source";
                string freetypeTarGz = Path.Combine(freetypeSourceDir, "freetype-2.13.3.tar.gz");
                string freetypeExtractDir = Path.Combine(freetypeSourceDir, "freetype-2.13.3");

                if (File.Exists(freetypeTarGz))
                {
                    RunLogged("Extracting freetype...", $"tar -xf {freetypeTarGz} -C {freetypeSourceDir}");
                    RunLogged("Removing freetype tar.gz...", $"rm {freetypeTarGz}");
                }
                else
                {
                    _consoleRenderer.WriteLine("freetype tar.gz file not found.", Color.Red);
                }

                if (Directory.Exists(freetypeExtractDir))
                {
                    RunLogged("Building freetype...", $"make -C {freetypeExtractDir}");
                    RunLogged("Installing freetype...", $"make install -C {freetypeExtractDir}");
                }
                else
                {
                    _consoleRenderer.WriteLine("freetype source directory not found.", Color.Red);
                }

                // Remove old freetype library
                RunLogged("Removing old freetype library...", "rm /mnt/vendor/lib/libfreetype.so.6.8.0");

                // Swap file creation
                string swapFile = "/swapfile";
                if (!File.Exists(swapFile))
                {
                    _consoleRenderer.WriteLine("Creating swap file...", Color.White);
                    RunLogged("Creating swap file...", "dd if=/dev/zero of=/swapfile bs=1024K count=500");
                    RunLogged("Setting swap file permissions...", "chmod 600 /swapfile");
                    RunLogged("Setting up swap area...", "mkswap /swapfile");
                }
                else
                {
                    _consoleRenderer.WriteLine("Swap file already exists.", Color.White);
                }

                // Update alternatives for x-www-browser
                RunLogged("Updating alternatives for x-www-browser...", "update-alternatives --install /usr/bin/x-www-browser x-www-browser /usr/bin/firefox/firefox 100");
                RunLogged("Setting x-www-browser to firefox...", "update-alternatives --set x-www-browser /usr/bin/firefox/firefox");

                // Modify PulseAudio configuration
                RunLogged("Modifying PulseAudio system.pa...", "sed -i 's/load-module module-native-protocol-unix/load-module module-native-protocol-unix auth-anonymous=1/' /etc/pulse/system.pa");

                // Correct RetroArch configuration
                RunLogged("ALSA configuration...", "echo > /usr/share/alsa/alsa.conf.d/pulse.conf");

                // Create new user 'user' with home directory
                RunLogged("Creating user 'user'...", "useradd -m user");
                RunLogged("Setting password for 'user'...", "echo \"user:user\" | chpasswd");
                RunLogged("Adding 'user' to sudo group...", "usermod -aG sudo user");
                RunLogged("Creating .bashrc for 'user'...", "touch /home/user/.bashrc");
                RunLogged("Setting ownership of .bashrc...", "chown user:user /home/user/.bashrc");
                RunLogged("Setting ownership of /home/user...", "chown -R user:user /home/user");

                // Enable NetworkManager.service
                RunLogged("Enabling NetworkManager.service...", "systemctl enable NetworkManager.service");

                // Play a sound file
                RunLogged("Playing sound...", "aplay /home/root/Music/o98.wav");

                _consoleRenderer.WriteLine("Press any button to reboot...", Color.Green);
                _completed = true;
                _consoleRenderer.AutoFlush = true;
                _consoleRenderer.Flush();
                _gamePadReader.ClearBuffer();
                _gamePadReader.WaitForInput();

                _consoleRenderer.WriteLine("Setup complete. Rebooting...", Color.Green);
                RunLogged("Rebooting", "reboot -f");
                System.Environment.Exit(0);
            }
            catch (Exception e)
            {
                _consoleRenderer.WriteLine(e.Message, Color.Red);
                _consoleRenderer.WriteLine(e.StackTrace, Color.Red);

                _gamePadReader.ClearBuffer();
                _gamePadReader.WaitForInput();
                System.Environment.Exit(1);
            }
        }

        private static void RunLogged(string text, string command)
        {
            Log(text, Color.Green);
            Log(command, Color.White);

            // Properly escape the command for bash -c
            string escapedCommand = command
                .Replace("\"", "\\\"")     // Escape double quotes
                .Replace("$", "\\$");

            // Split the command into executable and arguments
            string[] commandParts = SplitCommand(command);
            string executable = commandParts[0];
            string arguments = commandParts.Length > 1 ? commandParts[1] : "";

            ProcessStartInfo startInfo = new()
            {
                FileName = "/bin/bash",
                WorkingDirectory = AppContext.BaseDirectory,
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Copy existing environment variables
            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                startInfo.Environment[env.Key.ToString()] = env.Value.ToString();
            }

            using Process process = new()
            {
                StartInfo = startInfo
            };

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    Log(args.Data, Color.Grey);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    Log(args.Data, Color.Red);
                }
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                process.Start();

                // Start asynchronous read operations
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
            }
        }

        // Helper method to split the command string
        private static string[] SplitCommand(string command)
        {
            // Use a regex to split the command into executable and arguments, respecting quotes
            string pattern = @"^(?:""([^""]+)""|'([^']+)'|(\S+))\s*(.*)$";
            Match match = Regex.Match(command, pattern);
            if (match.Success)
            {
                string executable = match.Groups[1].Value != "" ? match.Groups[1].Value :
                                    match.Groups[2].Value != "" ? match.Groups[2].Value :
                                    match.Groups[3].Value;
                string arguments = match.Groups[4].Value;
                return [executable, arguments];
            }
            else
            {
                throw new ArgumentException("Invalid command format");
            }
        }
    }
}