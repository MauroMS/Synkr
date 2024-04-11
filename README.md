# CloudSynkr

[![Release](https://github.com/MauroMS/Synkr/actions/workflows/release.yml/badge.svg?branch=main)](https://github.com/MauroMS/Synkr/actions/workflows/release.yml)

[![Stable Version](https://img.shields.io/github/v/tag/MauroMS/Synkr)](https://img.shields.io/github/v/tag/MauroMS/Synkr)
[![Latest Release](https://img.shields.io/github/v/release/anothrNick/github-tag-action?color=%233D9970)](https://img.shields.io/github/v/release/anothrNick/github-tag-action?color=%233D9970)

It's a small piece of software to Sync files between your device and google cloud.
I created this as the way google drive sync works doesn't suit my needs.

It uses [.net 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) and [Google apis](https://github.com/googleapis/google-api-dotnet-client)
<br>
#### Considerations

 - This app is not an active sync app, and will not keep syncing your files. You need to run it again every time you want to sync the files.
Alternatively you can create a batch to run on start/shutdown to sync the files.

- Before overriding the files in your local/cloud it will compare the last modified date of both files, in order to avoid overriding the newest version.

## Setup

### 1. Allow app to access your gdrive
Follow the steps from this [youtube video](https://www.youtube.com/watch?v=9qt2EHzYv9Q) up to when Ivan downloads the credentials file at 53 seconds.

Move this file to the root folder of the app and rename to credentials.json

Below is a sample on how the file will look like:

    {
	    "installed": {
		    "client_id": "test-test.apps.googleusercontent.com",
			"project_id": "test-test-324234234",
			"auth_uri": "https://accounts.google.com/o/oauth2/auth",
		    "token_uri": "https://oauth2.googleapis.com/token",
		    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
		    "client_secret": "THERE_WILL_BE_A_CLIENT_SECRET_HERE",
		    "redirect_uris": [
				"http://localhost"
		    ]
		}
	}


### 2. How to config the app
Once you unzip the release folder, you'll find a config file called appsettings.json, which has the section below.
 You can add as many mappings as you want.

    	"SyncBackup": {
	    	"Mappings": [
		    	{
			      "CloudFolderParentId": "",
			      "CloudFolderParentName": "",
				  "LocalFolder": "",
				  "CloudFolder": "",
				  "ActionType": 0,
				  "FilesToSync": []
				}
			]
		}
	
In order to fill CloudFolderParentId & CloudFolderParentName, you will need to go to your gdrive and create a new base folder or use an existing one, and navigate inside.

| Property name | Description |
|--|--|
| CloudFolderParentName  | Base folder name you just created/opened |
| CloudFolderParentId | Copy the Id at the end of the URL when you're inside of a folder |
| LocalFolder | Full local path to the files you want to download/upload |
| CloudFolder | Folder name you want to upload/download files on gdrive |
| ActionType | <ul><li>None = 0 --> "Disable this mapping"</li><li>Sync = 1 --> "Both Download/Upload" </li><li>DownloadOnly = 2 --> "Only Download"</li><li>UploadOnly = 3 --> "Only Upload"</li></ul>|
| FilesToSync | In case you only want to sync specific files from a folder, you need to add their name and extension here, you can select multiple files by separating them using a comma. ex: "save1.txt, save2.txt, save3.txt" <br> By populating this property, it will only sync the top folder and the selected files but not its nested folders. |
