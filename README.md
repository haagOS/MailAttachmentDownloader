# MailAttachmentDownloader

## Background
This project was created for my own wedding.
I wanted a service that is able to download all attachments (images) of an arbitrary email inbox in order to display them live during the party.
All guests as well as uninvited people (due to limitations because of covid) who knew the email address should be able to send images.

I tried different existing options such as [IFTTT](https://ifttt.com/) and [MS Power Automate](https://powerautomate.microsoft.com) but I could not get them to work as I wanted.

## How it works
- Opens your email inbox (root directory) via IMAP
- Reads messages one by one that are 'unseen'
- Downloads the email and the attachments
- Puts them somewhere (local file system or cloud storage)
- Marks the email as 'seen'
- Continues until there are no more 'unseen' messages

## Prerequisites
- VS 2022 with .net6
- A fresh email inbox
- IMAP access and connection data
- A host that runs the console application
- A image viewer that is able to add images to the diashow while running (e.g. [Fast Stone Image Viewer](https://www.faststone.org/FSViewerDetail.htm))

## Run locally
- Clone the repository
- fill the `MailAttachmentToLocalFolder\appsettings.json` with your own data
- run (or publish and run) the `MailAttachmentToLocalFolder` project

## Run with Azure Functions, Azure Share and a local download service
- Clone the repository
- Create an azure function and an azure file share in your Azure account
- fill the `MailAttachmentToAzureShare\settings.json` with your data
- (optional: setup Application Insights)
- publish the `MailAttachmentToAzureShare` project to your azure function
- run the `AzureFilesDownloader` project locally on any machine