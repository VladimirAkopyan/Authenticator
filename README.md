# Authenticator for Windows
![Screenshot](/Images/WinOTP1.PNG)
This repository contains the official source code of WinOTP Authenticator for Windows app.
This source code is comes from original Authenticator app, which was written in 2016 and was a good quality app that fell out of support because original developer moved on or didn't have time to maintain it. 

I am taking over the project, it will be free and opensource. I am aiming to incorporate most functionality found in [WinAuth](https://github.com/winauth/winauth), which was original and excellent PowerUser's Authenticator for Windows, sadly now most of the codebase is obsolete and written for ancient version of .Net. It would have to be re-written for a modern UI stack like UWP. 
![Screenshot](/Images/WinOTP2.PNG)

## Roadmap
* Re-release the applicaiton into windows store
* Clean-up the code, update packages and remove obsolete ones. 
* UI improvements
  * Allow the user to paste QR code into the application
  * Allow user to scan QR code with camera 
  * Allow user to reveal the code of an account already stored inside the application as a string or as image
* Move the applicaiton to cross-platform support using Xamarin or Uno Platform

## Code quality
The codebase is, at the time of writing, already pretty old and could use some improvements. Still, it contains some useful pieces of code that I had to find out myself since there was not a lot to find on Google about UWP apps back then.
Be sure to grab pieces of code that you like and use them in your own app(s).

## Contributing
If you'd like to contribute, be sure to do so and create a pull request if you'd like to. I will review them in a timely fashion, and if there will be so much demand that I can't review them, I will create a github group and share control of the repository. 

## License
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  
This source code is licensed under the MIT License. Be sure to read the [LICENSE.txt](LICENSE.txt) file before using the source code.

## References: 
Reddit thread: 
* https://www.reddit.com/r/windowsphone/comments/99d5kc/replacement_for_authenticator_for_windows/

Sources and other: 
* https://bitbucket.org/Alphyraz/authenticator-for-windows/src/master/
* https://bitbucket.org/uhlik/authenticator-for-windows/src/master/
* https://www.microsoft.com/en-gb/p/authenticator-for-windows-2018/9nxn939qqx0n?rtc=1&activetab=pivot:overviewtab
