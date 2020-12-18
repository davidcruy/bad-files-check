# bad-files-check
Console application that performs a bad files check with Dropbox API. This project is a temporary solution until Dropbox fixes their https://www.dropbox.com/bad_files_check.

Usage:
```
dropbox-bfc.exe bearer
```
- bearer: a Dropbox API bearer token

# How do I get a token?
At this moment I have not created a public App yet on Dropbox API. So you need to visit the Developer site yourself:
https://www.dropbox.com/developers/apps/

Create a new app for your appropriate API, at the moment I have only tested the **Dropbox API**, make sure you request **Full Dropbox** access, give it a name. And you're app is created!

Aftwerwards you can use your app to generate **access tokens** to use with this console application.