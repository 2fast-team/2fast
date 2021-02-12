# 2fast – Two factor authenticator supporting TOTP
Built with [PrismLibrary/Prism](https://github.com/PrismLibrary/Prism) and for the UWP app a fork from [Template10](https://github.com/2fast-team/Template10?organization=2fast-team&organization=2fast-team)

[![Contributors][contributors-shield]][contributors-url]
[![Issues][issues-shield]][issues-url]

## About 2fast

**2fast** (acronym for **t**wo **f**actor **a**uthenticator **s**upporting **T**OTP) is the first free, open source, two factor authenticator for Windows and Android with the ability to store the sensitive data encrypted at a place of your choice instead of a 3rd party cloud location.

### Features

- Every data is stored in one single, encrypted file
	- Encrypted with AES using a password-based key by PBKDF2
	- Possibility to send the data file to your colleges and friends to share logins
	- Easy backup & restore
- Everything works local
	- No extra account for the app is needed
	- No Internet connection needed
	- but the options exists to use a WebDAV Server (ownCloud or nextCloud) [currently work in progress]
- Free
- Open Source
- Works on multiple platforms
	- Currently Windows and Android(work in progress)
- Uses the widely accepted TOTP algorithm ([RFC 6238](https://tools.ietf.org/html/rfc6238))
	- Works with accounts from Google, Microsoft, Facebook, PayPal, etc.

### Screenshots

**TBD**


## Installation

This project is not yet available in the Windows or Google Play store, you have to build it from source. Watch it!


## Getting Started

The following steps will help if you want to contribute or work on the application.

### Prerequirements

- Visual Studio
	- Don't forget to select the *Mobile development with .NET* package in the installation process 
- Windows 10, version >= `1809`, October update 2018 (for the universal Windows application)
	- The latest Windows 10 SDK is required because the target is set to this one
- Android, version >= 7.0 (for the Android application)
	* Remember to trust 3rd party Apps by enabling this in the Android developer settings if you want to build from source

### Installation

1. Clone this repository with all its submodules
```sh
git clone --recurse-submodules https://github.com/2fast-team/2fast.git
```
2. Open the `.sln` file with Visual Studio
	- Most of the important dependencies should be loaded automatically from Visual Studio
	- Install [Microsoft Store Services SDK](https://marketplace.visualstudio.com/items?itemName=AdMediator.MicrosoftStoreServicesSDK) manually
3. Start working and debugging!
	- Remember to select the correct start project and -platform before hitting the play button
	  - e.g. `x64` and `UWP` for the Universal Windows Application


## Contributing

Feel free to fork the project, work in your personal branch and create a pull request or you simple interact in the [issue section](https://github.com/2fast-team/2fast/issues).

**This is an open source project! Every contribution is greatly appreciated!**


## License

Distributed under the [GNU GPLv3](https://www.gnu.org/licenses/gpl-3.0.en.html) License. See `LICENSE` for more information.


## Contact

Project Link: [https://github.com/2fast-team/2fast](https://github.com/2fast-team/2fast) // [https://2fast-app.de](https://2fast-app.de)

Made with ♥ in Germany from [jp-weber](https://github.com/jp-weber) and [mhellmeier](https://github.com/mhellmeier)
