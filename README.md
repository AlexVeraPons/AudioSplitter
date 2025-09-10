# SimpleAudioSplitter

A simple **WPF desktop tool** for splitting audio files into smaller chunks using **FFmpeg**.  
Supports MP3, WAV, AAC, FLAC, OGG, and more.

## Why?
I just wanted a simple way to divide my audiobook files into smaller chuncks for my mp3 players

---

<img width="1919" height="1053" alt="image" src="https://github.com/user-attachments/assets/b1b87313-61b5-4322-9507-1c9f9856c477" />

## Features

- Split audio into chunks of **X minutes**  
- Optional **start offset**  
- Output either:
  - Next to the source file, or
  - Into a chosen folder  
- Customizable **file naming patterns** with tokens:
  - `{base}` -> original filename (without extension)  
  - `{series}` or `{series:D2}` -> detected series/volume number (e.g., Book 01)  
  - `{part}` or `{part:D3}` -> part/chunk number with padding
  - `:D3` -> means the ammount of decimals to set
- Option to **replace ID3 title metadata** with the output filename  
- Packaged with **FFmpeg** (no external install required)  

---

## How to install?

Just Install the zipped folder called AudioSplitter_NoNeedFor.NET
