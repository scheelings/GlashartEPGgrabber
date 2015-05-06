# Glashart EPG Grabber 
﻿## by Dennieku & jansaris
-------------------------------------------------------

The Glashart EPG Grabber has performs 2 jobs for you:
1. Generating an M3U file with all IPTV channels
   This job is executed by the program in the following parts:
   1. Downloading the TV menu web page (i.e. http://w.zt6.nl/tvmenu/index.xhtml.gz)
   2. Unzipping the index.xhtml.gz file to index.html
   3. Parsing the index.html to determine the URL of the TV script file to be downloaded (i.e. http://w.zt6.nl/tvmenu/code.js.gz?1416996339)
   4. Downloading the code.js.gz?1416996339 file
   5. Unzipping the TV script file to code.js
   6. Parsing the channels and URL's from the code.js file and generating a channels.xml file
   7. Generating the M3U file based on channels.xml
2. Generating an XMLTV file for all IPTV channels, based on Glashart EPG
   This job is executed by the program in the following parts:
   1. Downloading the EPG files for the next x days (URL is like http://w.zt6.nl/epgdata/epgdata.20141128.1.json.gz)
   2. Unzipping all downloaded EPG files to (for instance) 'epgdata.20141128.1.json'
   3. Generating the XMLTV file
Some additional features:
3. Converting a downloaded M3U file into a new m3U file according the ChannelsListFile setting


## Usage

This program is a console application which can be executed like below:
```
GlashartEPGgrabber.exe [/dl-tvmenu] [/unzip-tvmenu] [/dl-tvscript] [/unzip-tvscript] [/channels] [/m3u] [/dl-epg] [/unzip-epg] [/xmltv] [/all-m3u] [/all-xmltv] [/all] [/convert-m3u]
/dl-tvmenu			Download the TV menu web page (http://w.zt6.nl/tvmenu/index.xhtml.gz)
/unzip-tvmenu			Unzip the downloaded index.xhtml.gz TV menu web page
/dl-tvscript			Download the TV menu javascript file  (ie: http://w.zt6.nl/tvmenu/code.js.gz?1416996339)
				The URL to the TV menu script file is being determined from the TV menu web page
/unzip-tvscript			Unzip the downloaded code.js.gz TV menu script file
/channels			Generate the channels.xml file with all channels and URL's, based on the code.js file
/m3u				Generate the M3U file based on the channels.xml file
/dl-epg				Download the EPG files (ie: http://w.zt6.nl/epgdata/epgdata.20141128.1.json.gz)
/unzip-epg			Unzip all downloaded EPG files
/xmltv				Generate the XMLTV file
				Also an EPG.xml file is generated with all parsed EPG data

/all-m3u			Execute all steps needed to generate the M3U file: /dl-tvmenu /unzip-tvmenu /dl-tvscript /unzip-tvscript /channels /m3u
/all-xmltv			Execute all steps needed to generate the XMLTV file: /dl-epg /unzip-epg /xmltv
/all				Execute all steps: /dl-tvmenu /unzip-tvmenu /dl-tvscript /unzip-tvscript /channels /m3u /dl-epg /unzip-epg /xmltv

/convert-m3u			Converts a downloaded M3U file to a new M3U file
```

## Config

Configuration can be changed in GlashartEPGgrabber.exe.config

| Config | Explanation |
| ------ | ----------- |
| TvMenuURL | URL to the TV menu website on the IPTV network (default: http://w.zt6.nl/tvmenu/) |
| EpgURL | URL to the EPG folder in the website on the IPTV network (default: http://w.zt6.nl/epgdata/) |
| TvMenuFolder | Local folder to download TV menu files to (default: D:\GlashartEPGgrabber\Data\TvMenu) |
| M3UfileName | M3U file name to generate (default: D:\GlashartEPGgrabber\Data\TvMenu\glashart.m3u) |
| IgmpToUdp | Convert IGMP to UDP, so igmp:// becomes udp://@ (default: True) |
| M3U_ChannelLocationImportance | All channel URLs have a name, like HD, SD, ZTV-HD, etc. This list defines the importance of these URLs. The 1st URL name in the list is found and saved in the M3U file. When this name is not found, the 2nd in the list will be used. When nothing is found the first available URL will be used (default: ztv-hd, ztv-sd, ztv)
| ChannelsListFile | List of channels which is a filter for the M3U file. Only these channels will be saved in the M3U file. Remove this file to save all channels in the M3U file (default: D:\GlashartEPGgrabber\Data\ChannelList.txt) Format in this file: Channel number,Orininal Channel name,Optional new channel name |
| EpgNumberOfDays | Number of days to download EPG (default: 7) |
| EpgFolder | Folder to download EPG files to (default: D:\GlashartEPGgrabber\Data\EPG) |
| EpgArchiving | EPG files in the EpgFolder older than x days will be removed (default: 7) |
| XmlTvFileName | XMLTV file name to generate (default: D:\GlashartEPGgrabber\Data\guide.xml) |
| DownloadedM3UFileName | File name of downloaded M3U which will be converted to a new M3U file |

Logging can be set in log4net.config

## Linux & Mono

GlashartEPGgrabber works on linux with Mono (tested on Ubuntu 14.10 and OpenElec 5.08)

### OpenElec

To be able to run the code on OpenElec you will need to create a static compiled linux executable of the application.

1. Create a linux executable using mkbundle on your Ubuntu 14.10 machine (~repo/Linux/CreateExecutable.sh)
2. Copy the following files to your OpenElec Machine:

  * GlashartEPGgrabber (linux executable)
  * libc.so (~repo/Linux/libc.so for x64)
  * GlashartEPGgrabber.ini
