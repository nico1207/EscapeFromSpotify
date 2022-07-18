# Escape From Spotify
Utility app for Escape From Tarkov that automatically pauses any media once a raid starts

## Features

- Supports **any media** not just Spotify (through Windows 10 media API)
  - Examples: Youtube, Twitch, Spotify, iTunes, etc.
  - Only works on **Windows 10 and 11**
- Works by analyzing certain pixels in Tarkov's game window
  - Currently only supports **1920x1080** and **3840x2160** resolution
  - Only tested on **windowed borderless**
  - Probably only supports **English** game language
- Media will be **paused when a raid starts** (Red "GET READY!" screen)
- Media will **resume when a raid ends** ("RAID ENDED" screens)
- Only works with **PMC raids** (Scav raids do not have the "GET READY!" screen)