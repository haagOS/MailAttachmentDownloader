# MailAttachmentDownloader

## Background
This project was created for my own wedding.
I wanted a service that is able to download all attachments (images) of an arbitrary email inbox in order to display them live during the party.
All guests as well as uninvited people (due to limitations because of covid) who knew the email address should be able to send images.

I tried different existing options such as [IFTTT](https://ifttt.com/) and [MS Power Automate](https://powerautomate.microsoft.com) but I could not get them to work as I wanted.

## Prerequisites
- VS 2022 with .net6
- A fresh email inbox
- IMAP access and connection data
- A host that runs the console application
- A image viewer that is able to add images to the diashow while running (e.g. [Fast Stone Image Viewer](https://www.faststone.org/FSViewerDetail.htm))

## How to run
- Clone the repository
- fill the `appsettings.json` with your own data
- run (or publish and run) the `MailAttachmentToLocalFolder` project