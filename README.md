[![Twitter URL](https://img.shields.io/twitter/url/https/twitter.com/deanthecoder.svg?style=social&label=Follow%20%40deanthecoder)](https://twitter.com/deanthecoder)
# ZX Speculator
ZX Speculator is a ZX Spectrum 48K emulator, written in C#. It leverages the [Avalonia](https://avaloniaui.net/) library for cross-platform capabilities. Initially developed on Mac, but has seen limited testing on Windows.

![Main UI](img/MainUI.png?raw=true "Main UI")

## Features
- **Platform Independent**: Built using [Avalonia](https://avaloniaui.net/), ensuring compatibility across various platforms.
- **File Format Support**: Compatible with .z80, .bin, .scr, and .sna files.
- **Integrated Debugger**: Comes with a built-in debugger for examination of the Z80 CPU state, including step-through instruction capabilities.
- **Sound Libraries**: Utilizes [libsoundio](http://libsound.io/) on Mac and [NAudio](https://github.com/naudio/NAudio) on Windows for sound emulation.

## Development and Testing
Developed on a Mac environment, ZX Speculator is also tested on Windows, albeit with limited scope. The project passes _nearly_ all the ZEXDOC tests but does not emulate memory cycles and other aspects with 100% accuracy.

![Jetpac published by Ultimate Play the Game](img/Jetpac.png?raw=true "Jetpac")

## Getting Started
### Prerequisites
- .NET compatible IDE, such as [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio 2022](https://visualstudio.microsoft.com/vs/).
- Basic knowledge of C# and emulation concepts.

### Setup
The project is provided as C# source code:
1. Clone the repository from GitHub.
2. Open the project in your preferred IDE.
3. Build and run the application.

## Contribution and Improvements
ZX Speculator is an ongoing project and contributions are welcome. Whether it's improving emulation accuracy, testing on different platforms, or enhancing existing features, your input is valuable (although I can't always promise a fast response, as this is a side project).

## Limitations
- Emulation is not 100% accurate in terms of memory cycles and other specifics.
- Limited testing on Windows; there may be undiscovered issues - Sound support is known to be a bit sketchy.

## Future Plans
- Further testing.
- Refinement of emulation accuracy and performance.

---

Feel free to follow me on Twitter for more updates: [@deanthecoder](https://twitter.com/deanthecoder)
