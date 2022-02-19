# 2fast – Two Factor Authenticator Supporting TOTP

<p align="center">
	<img src="https://github.com/2fast-team/2fast/blob/master/Project2FA.UWP/Assets/StoreLogo.scale-400.png" width="100">
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

## About 2fast

**2fast** (acronym for **t**wo **f**actor **a**uthenticator **s**upporting **T**OTP) is the first free, open source, two factor authenticator for Windows and Android with the ability to store the sensitive data encrypted at a place of your choice instead of a 3rd party cloud location.

### Features

- Every data is stored in one single encrypted file
	- Encrypted with AES using a password-based key by PBKDF2
	- Possibility to send the data file to your colleges and friends to securely share logins
	- Easy backup & restore in one single file
- Everything works local
	- No extra account, registration or login needed
	- No Internet connection needed
		- Add personal WebDAV server (e.g. ownCloud or Nextcloud) as storage location [currently work in progress]
- Free
- Open Source
- Works on multiple platforms
	- Currently Windows and Android [work in progress]
- Uses the widely accepted TOTP algorithm ([RFC 6238](https://tools.ietf.org/html/rfc6238))
	- Works with accounts from Google, Microsoft, Facebook, PayPal, etc.
- Build with [PrismLibrary/Prism](https://github.com/PrismLibrary/Prism) and a fork from [Template10](https://github.com/2fast-team/Template10?organization=2fast-team&organization=2fast-team) (for the UWP app)

### Screenshots

<img src="https://i.postimg.cc/FzXQpK43/2fast-screenshot-1.png" alt="2fast Screenshot Login Screen" width="470" />  <img src="https://i.postimg.cc/zv7PhDXJ/2fast-screenshot-2.png" alt="2fast Screenshot Account Screen" width="464" />

## Installation

For Windows operating systems:

<a href='//www.microsoft.com/store/apps/9p9d81glh89q?cid=storebadge&ocid=badge'><img src='https://developer.microsoft.com/store/badges/images/English_get-it-from-MS.png' alt='English badge' width="150" /></a>

This project is not yet available in the Google Play store, you have to build it from source. Watch it!


## Getting Started

The following steps will help if you want to contribute or work on the application.

### Prerequirements

- [Visual Studio 2022](https://visualstudio.microsoft.com/)
	- Don't forget to select the *Mobile development with .NET* package in the installation process 
- Windows 10, version >= `1809`, October update 2018 (for the universal Windows application)
	- The latest Windows 10 SDK is required
- Android, version >= 7.0 (for the Android application)
	- Remember to trust 3rd party Apps by enabling this in the Android developer settings if you want to build from source

### Installation

1. Clone this repository with all its submodules (HTTPS example)
```sh
git clone --recurse-submodules https://github.com/2fast-team/2fast.git
```
2. Open the `.sln` file with Visual Studio
	- The dependencies are automatically loaded by Visual Studio
3. Start working and debugging!
	- Remember to select the correct start project and -platform before hitting the play button
	  - e.g. `x64` and `UWP` for the Universal Windows Application


## Contributing

Feel free to fork the project, work in your personal branch and create a pull request or you simple interact in the [issue section](https://github.com/2fast-team/2fast/issues).

**This is an open source project! Every contribution is greatly appreciated!**


## License

Distributed under the [GNU GPLv3](https://www.gnu.org/licenses/gpl-3.0.en.html) License. See `LICENSE` for more information.
A list of all used 3rd party libraries, images and information with their source and license can be found in the *Dependencies of the app* section in every installation.
Current key-icon-logo by [freepik @ flaticon](https://www.flaticon.com/de/kostenloses-icon/schlussel_2786386?related_item_id=2786386).

## Privacy

UWP: The app does not use any own telemetry functions apart from the standardised telemetry functions by Microsoft (crashes, install from which country, versions in use, etc.).

## Contact

Project Link: [https://github.com/2fast-team/2fast](https://github.com/2fast-team/2fast) // [https://2fast-app.de](https://2fast-app.de)

Made with ♥ in Germany from [jp-weber](https://github.com/jp-weber) and [mhellmeier](https://github.com/mhellmeier)
