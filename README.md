> [!IMPORTANT]
> The following link contains a flashable 64GB version of the operating system that can be used to avoid running the setup scripts. If you flash this image, you don't have to do anything else.
> https://mega.nz/file/eslX3RpY#4oMFP1S57NJYXjUg59L6Zd5HRse-DISLrm1PmFQlxHQ  

> [!WARNING]  
> This WILL NOT work on a modified stock OS image. The modified image partitions have been SHRUNK and are not large enough to hold the files!

# XFCE Desktop Environment for RG35XX Plus and RG35XX H Handheld Consoles

This project provides a method to install the XFCE desktop environment on RG35XX Plus and RG35XX H handheld gaming consoles. It's designed to enhance the functionality of these devices by adding a versatile desktop environment.

## Installation


Follow these steps to install:

1. **Copy to SD Card**: Copy the contents of this repository to the `/Roms/APPS` directory on the internal SD card of your handheld console.
   
2. **Insert SD Card**: Place the SD card into the internal slot of the handheld.

3. **Connect to WiFi**: Ensure your device is connected to a WiFi network.

4. **Start Installation**: Navigate to the `APPS` directory in the stock UI, and select `install_xfce`.

5. **Wait**: The system will appear to freeze for a few minutes. Thats the happy path. If it doesn't appear to freeze, something went wrong. Eventually it will finish installing the components required to render text, at which point you'll be able to see what its doing.

6. **Automatic Reboot**: After the installation completes, your console will automatically reboot.

## Usage

To launch the XFCE desktop environment:

- Navigate back to the `APPS` directory and select `XFCE` to run it.

## Important Notes

- **Base OS**: This has only been tested on 35XXP-240118OS-EN64GB
- **Reboot on Completion**: The console will reboot once the installation is completed.
- **SD Card Size**: A 64GB SD card is recommended for optimal performance, though smaller sizes may work.
- **Current Limitations**: There are no battery or WiFi widgets available on the desktop currently.
- **Installation Text**: During installation, some text may display as "?????" due to the region setting, which is switched to "US" only at the end of the installation. This may be addressed in future updates.
- **Duration**: The installation process, particularly the swap file creation, can take a significant amount of time.
- **Discretion Advised**: Installing this environment may affect other functionalities of the device. No adverse effects have been observed, but installation is at the user's discretion.
- **Peripheral Support**: The environment supports mouse and keyboard, which are necessary for remapping QJoyPad.

## Contributing

Feel free to fork this project and contribute. If you have any suggestions, improvements, or want to contribute to the project, please do so through GitHub.

## Contact

For any queries or discussions regarding this project, please open an issue on this GitHub repository.
