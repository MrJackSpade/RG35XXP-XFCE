# XFCE Desktop Environment for RG35XXP, RG35XXH, and RG35XXSP Handhelds

This project provides a method to install the XFCE desktop environment on RG35XXP, RG35XXH, and RG35XXSP handheld gaming consoles. It's designed to enhance the functionality of these devices by adding a versatile desktop environment.

> [!IMPORTANT]
> These scripts are only known to support STOCK firmware 1.1.8.
> 
> "mod stock" is not stock. Its modified. I can only realistically test and resolve issues on unmodified firmware.

## Installation

1. Add the repository to TF1 (the internal SD card) in the directory `/Roms/APPS`. Make sure its unzipped.
2. Insert the SD card into the device, then connect the device to WiFi. Packages are not included as part of the repo so WiFi is required for installation.
3. Execute the app script `install_xfce.sh`. This process may take ~5-10 minutes, after which the console will reboot.
4. Navigate to the app `XFCE` and run it. The UI will be in Chinese and sound won't work initially.
5. Run the desktop item `Run Setup Script`. This will:
   - Correct the language
   - Update the OS
   - Fix the sound
   - Install a web browser
   - Perform other necessary configurations
6. When clicking the setup script, a popup in Chinese will appear. Select the first terminal option and then select the right option (O) to accept.
7. The setup process will take ~20 minutes. The console will reboot when complete.

> [!NOTE]
> If the screen turns off during installation, it won't turn back on unless you use a physical mouse, as the D-pad doesn't trigger screen wake.

## Usage

To launch the XFCE desktop environment:

- Navigate back to the `APPS` directory and select `XFCE` to run it.

## Additional Notes

- A virtual keyboard and a joypad-to-mouse application are preinstalled.
- You can remap keys using the joypad icon on the taskbar, but this requires a physical mouse.
- Install an adblocker in Firefox ASAP, as the device has limited memory for rendering webpages.
- YouTube videos may play better in fullscreen mode.

## Important Notes

- **Discretion Advised**: Installing this environment may affect other functionalities of the device. No adverse effects have been observed, but installation is at the user's discretion.
- **Peripheral Support**: The environment supports (not requires) mouse and keyboard, which are necessary for remapping QJoyPad.

## Contributing

Feel free to fork this project and contribute. If you have any suggestions, improvements, or want to contribute to the project, please do so through GitHub.

## Contact

For any queries or discussions regarding this project, please open an issue on this GitHub repository.
