using RG35XX.Core.Drawing;
using RG35XX.Core.Extensions;
using RG35XX.Core.Fonts;
using RG35XX.Libraries;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace RG35XX_XFCE
{
    internal class Program
    {
        private static readonly ConsoleRenderer _consoleRenderer = new(ConsoleFont.Px437_IBM_VGA_8x16);

        private static string LogFile { get; set; } = Path.Combine(AppContext.BaseDirectory, $"XFCE-Setup-{DateTime.Now.Ticks}.log");

        private static async Task SetInternetTimeAsync()
        {
            // Create handler that ignores SSL validation
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // Create and configure client
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

            try
            {
                // Use HEAD request to minimize data transfer
                using var request = new HttpRequestMessage(HttpMethod.Head, "http://google.com");
                using var response = await client.SendAsync(request);

                if (response.Headers.Date.HasValue)
                {
                    string dateCommand = response.Headers.Date.Value.UtcDateTime.ToString("MMddHHmmyyyy.ss");
                    RunLogged("Setting the date", $"date {dateCommand}");
                }
                else
                {
                    throw new Exception("No date header received from server");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set system time: {ex.Message}", ex);
            }
        }

        private static async Task Main(string[] args)
        {
            _consoleRenderer.Initialize(640, 480);

            GamePadReader gamePadReader = new();
            gamePadReader.Initialize();

            try
            {
                string payloadDirectoryPath = Path.Combine(AppContext.BaseDirectory, "payload");

                await SetInternetTimeAsync();

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
                RunLogged("Correcting RetroArch configuration...", "sed -i 's/^audio_driver =.*/audio_driver = \"sdl2\"/' ~/.config/retroarch/retroarch.cfg");

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
                gamePadReader.ClearBuffer();
                gamePadReader.WaitForInput();

                _consoleRenderer.WriteLine("Setup complete. Rebooting...", Color.Green);
                RunLogged("Rebooting", "reboot -f");
                System.Environment.Exit(0);
            }
            catch (Exception e)
            {
                _consoleRenderer.WriteLine(e.Message, Color.Red);
                _consoleRenderer.WriteLine(e.StackTrace, Color.Red);

                gamePadReader.ClearBuffer();
                gamePadReader.WaitForInput();
                System.Environment.Exit(1);
            }
        }

        private static readonly object LogLock = new();
        private static void Log(string text, Color color)
        {
            lock (LogLock)
            {
                _consoleRenderer.WriteLine(text, color);
                File.AppendAllText(LogFile, $"{text}\n");
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
            var pattern = @"^(?:""([^""]+)""|'([^']+)'|(\S+))\s*(.*)$";
            var match = Regex.Match(command, pattern);
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