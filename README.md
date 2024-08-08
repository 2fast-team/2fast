# 2fast – Two Factor Authenticator Supporting TOTP

<p align="center">
	<img src="https://github.com/2fast-team/2fast/blob/master/Project2FA/Project2FA.UWP/Assets/StoreLogo.scale-400.png" width="100">
</p>
<p align="center">
	<a href="https://github.com/2fast-team/2fast/issues" target="_blank" alt="Issues">
		<img src="https://img.shields.io/github/issues/2fast-team/2fast" />
	</a>
	<a href="https://github.com/2fast-team/2fast/graphs/contributors" target="_blank" alt="Contributors">
		<img src="https://img.shields.io/github/contributors/2fast-team/2fast" />
	</a>
	<a style="text-decoration:none" href="https://github.com/2fast-team/2fast/releases">
    		<img src="https://img.shields.io/badge/Sideload-Download-purple.svg?style=flat-round" alt="Sideload link" />
	</a>
	<a href="https://github.com/2fast-team/2fast/blob/master/LICENSE" target="_blank" alt="License">
		<img src="https://img.shields.io/github/license/2fast-team/2fast" />
	</a>
</p>
<p align="center">
<a href="https://apps.microsoft.com/detail/9p9d81glh89q?cid=storebadge&mode=direct">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>
</p>

<p align="center">
other installation method (not maintained by me)
<a style="text-decoration:none" href="https://community.chocolatey.org/packages/2fast">
    		<img src="https://img.shields.io/chocolatey/dt/2fast?color=blue&label=Chocolatey" alt="Chocolatey link" />
	</a>
</p>


## About 2fast

**2fast** (acronym for **t**wo **f**actor **a**uthenticator **s**upporting **T**OTP) is a free, open source, two factor authenticator for Windows and other platforms. 2fast has the ability to store the encrypted data at a place of your choice instead of a 3rd party cloud location.

### Features

- All data is stored in a single encrypted file
	- Encrypted with AES using a password-based key by PBKDF2
	- Possibility to send the data file to your colleagues and friends to securely share logins
	- Easily backup & restore using a single file
- Everything works locally
	- No extra account, registration or login needed
	- No Internet connection needed except for WebDAV use
- Synchronisation via a WebDAV server (e.g. ownCloud or Nextcloud) as storage location
- Free for desktop OS / fee required for mobile OS
- Open Source
- Works on multiple platforms
	- Currently released for Windows 10+ (UWP) 
	- under development (with Uno platform):
		- Android
		- iOS
		- Linux
		- macOS
- Uses the widely accepted TOTP algorithm ([RFC 6238](https://tools.ietf.org/html/rfc6238))
	- Works with accounts from Google, Microsoft, Facebook, PayPal, etc.
- Build with [UNOversalTemplate](https://github.com/jp-weber/UNOversalTemplate) as template

### Screenshots

<img src="https://i.postimg.cc/FzXQpK43/2fast-screenshot-1.png" alt="2fast Screenshot Login Screen" width="470" />  <img src="https://i.postimg.cc/zv7PhDXJ/2fast-screenshot-2.png" alt="2fast Screenshot Account Screen" width="464" />

## Nominations
<a href='https://aka.ms/StoreAppAwardsWinners'><img width="720" alt="finalist_lock_up_light_on_dark_Utilities" src="https://user-images.githubusercontent.com/10946850/180846349-d0f8358d-5684-4616-92a0-5bc547087e42.png" width="250"></a>
## Wiki
See https://github.com/2fast-team/2fast/wiki

## Installation

For Windows operating systems, install safely via Microsoft Store:
<p align="center">
<a href="https://apps.microsoft.com/detail/9p9d81glh89q?cid=storebadge&mode=direct">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>
</p>

If you prefer to install via command line, ```winget``` is recommend:
```powershell
winget install 9P9D81GLH89Q
```


You can also install the app using [Chocolatey](https://community.chocolatey.org/packages/2fast) (not maintained by me):
```powershell
choco install 2fast
```

This project is not yet available in the Google Play store or the Apple app store. A mobile application is currently being worked on and will be announced when it is released in the respective stores.


## Getting Started

The following steps will help if you want to contribute or work on the application.

### Prerequirements

- [Visual Studio 2022](https://visualstudio.microsoft.com/)
	- Don't forget to select the *Mobile development with .NET* package in the installation process 
	- .NET Multi-Platform App UI development
- Windows 10, version >= `1809`, October update 2018 (for the universal Windows application)
	- The latest Windows 11 SDK is required
- Android, version >= 10.0 (for the Android application)
	- Remember to trust 3rd party Apps by enabling this in the Android developer settings if you want to build from source


### Installation

1. Clone this repository with all its submodules (HTTPS example)
```shell
git clone --recurse-submodules https://github.com/2fast-team/2fast.git
```
2. Open the `.sln` file with Visual Studio
	- The dependencies are automatically loaded by Visual Studio
3. Start working and debugging!
	- Remember to select the correct start project and -platform before hitting the play button
  (e.g. `x64` and `UWP` for the Universal Windows Application)


## Contributing

Feel free to fork the project, work in your personal branch and create a pull request. Or you can simply interact in the [issue section](https://github.com/2fast-team/2fast/issues).

**This is an open source project! Every contribution is greatly appreciated!**


## License

Distributed under the [GNU GPLv3](https://www.gnu.org/licenses/gpl-3.0.en.html) License. See `LICENSE` for more information.
A list of all used 3rd party libraries, images and information with their source and license can be found in the *Dependencies of the app* section in every installation.
Current key-icon-logo by [freepik @ flaticon](https://www.flaticon.com/de/kostenloses-icon/schlussel_2786386?related_item_id=2786386).

## Privacy

UWP: The app does not use any own telemetry functions apart from the standardised telemetry functions by Microsoft (crashes, install from which country, versions in use, custom events etc.).

## Contact

Project Link: [https://github.com/2fast-team/2fast](https://github.com/2fast-team/2fast) // [https://2fast-app.de](https://2fast-app.de)

Made with ♥ in Germany from [jp-weber](https://github.com/jp-weber) and [mhellmeier](https://github.com/mhellmeier)
