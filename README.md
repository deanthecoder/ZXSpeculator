[![Twitter URL](https://img.shields.io/twitter/url/https/twitter.com/deanthecoder.svg?style=social&label=Follow%20%40deanthecoder)](https://twitter.com/deanthecoder)
# ZX Speculator
ZX Speculator is a cross-platform ZX Spectrum 48K emulator written in C#.
![Main UI](img/MainUI.png?raw=true "Main UI")

## Features
- **Cross Platform**: Built using [Avalonia](https://avaloniaui.net/), ensuring compatibility across various platforms.
- **Key Mapping**: Most keys on a modern PC keyboard are automatically mapped to the Spectrum, making it much easier to type in code.
- **File Format Support**: Compatible with .z80, .bin, .scr, .tap, and .sna files.
- **Archive Support**: Load files directly from `.zip` archives.
- **Display**: Optional CRT TV and 'Ambient Blur' effects. ![CRT](img/CRT.png)
- **Joysticks**: Kempston and Cursor joystick support.
- **Sound**: Utilizes [OpenAL](https://www.openal.org/) on Mac and Windows for sound emulation.
- **Integrated Debugger**: Includes a built-in debugger for examination of the Z80 CPU state, including:
  - Instruction stepping.
  - Breakpoints.
  - Instruction history.
- **Rollback**: Die in your favourite game? Accidentally delete a line of code? Continuous recording allows you to 'roll back' to an earlier time. (`F1` will roll back 5 seconds.)
![Rollback](img/Rollback.png)
- **Theming**: The Sinclair BASIC ROM can be customized to allow for:
  - Classic ZX Spectrum input vs a per-character typing strategy. (Courtesy of the [JGH Spectrum 48K ROM](http://mdfs.net/Software/Spectrum/Harston) by J.G.Harston)
  - Selectable colors schemes and fonts.
    - ZX Spectrum
    - BBC Micro
    - Commodore 64
![Speccy Theme](img/Theme_Speccy.png)
![BBC Micro Theme](img/Theme_BBC.png)
![Commodore 64 Theme](img/Theme_C64.png)

## Download
* Download from the [Releases](https://github.com/deanthecoder/ZXSpeculator/releases) section.
* Mac users may need to run the following command to unblock the application:<br>`xattr -d com.apple.quarantine /Applications/ZX\ Speculator.app`

## Development and Testing
Developed on a Mac environment, ZX Speculator is also tested on Windows and passes all the ZEXDOC tests and FUSE emulator tests.
![Jetpac published by Ultimate Play the Game](img/Jetpac.png?raw=true "Jetpac")

## Getting Started
### Loading Files
Common ZX Spectrum image files (.z80, .sna, etc) can be opened from the File->Open menu.

### Loading .tap Files
1. Type `Load ""` in BASIC.
2. The File->Open dialog will automatically open, allowing a .tap file to be specified.
3. Enjoy the loading experience.

### Keyboard
Move the mouse pointer to the small keyboard icon at the top-right of the screen to see a representation of the ZX Spectrum keyboard.
![Keyboard](img/Keyboard.png?raw=true "Keyboard")
Many keys on a modern keyboard are automatically mapped to their ZX Spectrum equivalent.  For example, backspace, quotes, math symbols etc.

The left shift key maps to **CAPS SHIFT** on the Spectrum, and the right shift key maps to **SYMBOL SHIFT**.

ESCape will reset the machine.

### Joystick
The emulator will mimic either a Kempston or Cursor joystick.

In both cases the keyboard arrow keys are used for direction control, and the backslash or backtick keys will 'fire'.

## Building From Source
### Prerequisites
- .NET compatible IDE, such as [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio 2022](https://visualstudio.microsoft.com/vs/).
- Basic knowledge of C# and emulation concepts.

### Setup
The project is provided as C# source code:
1. Clone the repository from GitHub.
2. Open the solution (`Speculator.sln`) in your preferred IDE.
3. Build and run the application.

## Videos
There's a [YouTube playlist](https://www.youtube.com/playlist?list=PLPA1ndSnAZTwt7cQjDNwwsPjS89Dd3yqv) showing some classic games played in the emulator.
![](img/ManicMiner.png)
![](img/ChuckieEgg.png)
![](img/BoulderDash.png)
![](img/Tapper.png)

## Experiments - Raytracer
As my other hobby is writing GLSL shaders on [ShaderToy](https://www.shadertoy.com/user/dean_the_coder) (See [here](https://github.com/deanthecoder/GLSLShaderShrinker) for my GLSL Shader Shrinker application), I thought it'd be interesting to try a 'cross over' project.

I've taken inspiration from the [Human Shader](https://humanshader.com/) project and recreated the algorithm using ZX Spectrum BASIC, using this emulator.

Here's the result:
![](Experiments/HumanShader/Pass3_AdvancedDither.png)
Earlier version, with a basic dither:
![](Experiments/HumanShader/Pass2_BasicDither.png)
First iteration: Solid blocks:
![](Experiments/HumanShader/Pass1_Rough.png)
I've included a `.sna` snapshot of the code [here](Experiments/HumanShader/HumanShader.sna).
![](Experiments/HumanShader/Code.png)

# Experiments - Conway's Game Of Life
I realized I had never written a [Conway's Game Of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life), so decided to make one using the emulator.

Performance in this BASIC version isn't great... (See [here](Experiments/GameOfLife/Conway.sna) - Requires the JGH ROM)
![](Experiments/GameOfLife/GameOfLife.png)

...so I rewrote it in C, compiled into Z80 machine code ([here](Experiments/GameOfLife/Conway.tap)).  Muuuch faster!
![](Experiments/GameOfLife/GameOfLifeC.png)

# Experiments - Retro Fire
C compiled into Z80 machine code ([here](Experiments/FireFX/fire.tap)). Too slow to call 'real time', but not bad for the Speccy.
![](Experiments/FireFX/Fire.png)

# Experiments - The Matrix
C compiled into Z80 machine code ([here](Experiments/TheMatrix/matrix.tap)). This one runs in real time, which surprised me.

I first populate the entire screen with characters (black on black), then build up a 768 element array of color values with a repeated sequence ranging from green to bright green, white to bright white.

This array is then used to set the colors on the screen, and a `memmove` command is used to scroll the buffer by one byte. This keeps the framerate up as it's only the color attributes that change - Not the characters on the screen.
![](Experiments/TheMatrix/TheMatrix.png)

# Experiments - 10PRINT
C compiled into Z80 machine code ([here](Experiments/10print/10print.tap)).

The screen is filled with forward and backslashes (Drawn with a small gap in my case), then I randomly fill areas of the screen.  Quite relaxing to watch!
![](Experiments/10print/10print.png)

# Experiments - Sandy Situation
C compiled into Z80 machine code ([here](Experiments/Sand/sand.tap)).

Iterating over a fixed number of grains, advancing each depending on what is below them. I did try a different approach of iterating through all cells in an x/y grid which worked well, but wasn't anywhere near as performant.

I'm making use of the [z88dk](https://z88dk.org/site/)'s 'chunky' pixel blitting routines too.
![](Experiments/Sand/Sand.png)

# Experiments - Twister
Pushing my graphical skills on the ZX Spectrum, resulting in the creation of a classic 'twister' effect. The code is written in C and compiled into Z80 machine code ([here](Experiments/Twister/twister.tap)) - Needed to keep the performance up.

The twister effect, a staple of early computer graphics and demoscene productions, seemed like the perfect challenge - I've always wanted to make one of these but never got round to it. Helped with details from [8bitshack](https://8bitshack.org/post/twister/).
It takes a while to complete the precaching, but I like the result.
![](Experiments/Twister/Twister.png)

# Experiments - Breakout
Two balls fighting for dominance! I saw an effect similar to this on Twitter and thought it was a great idea.
A little more of a 'demo-style' with this one - I've added a DTC logo (which I'll developed more in the future).

The code is written in C and compiled into Z80 machine code ([here](Experiments/BreakOut/breakout.tap)).
![](Experiments/BreakOut/Breakout.png)

# Experiments - One Small Step
This is based on a GLSL [shader](https://www.shadertoy.com/view/tt3yRH) I wrote a while ago. I built a library of GLSL-like functions in C to recreate the original code, then ran it over many hours.

The final quality was achieved with a Floyd-Steinberg dithering algorithm. A random dither is much easier to implement, but the result was way too noisy.

The executable is [here](Experiments/OneSmallStep/OneSmallStep.tap).
![](Experiments/OneSmallStep/OneSmallStep.png)

## Contribution and Improvements
ZX Speculator is an ongoing project and contributions are welcome. Whether it's improving emulation accuracy, testing on different platforms, or enhancing existing features, your input is valuable (although I can't always promise a fast response, as this is a side project).

## Credits
[Flux Capacitor icon](https://www.onlinewebfonts.com/icon) licensed by CC BY 4.0.

## Useful Resources
- [The Undocumented Z80 Documented](http://www.z80.info/zip/z80-documented.pdf)
- [Z80 Undocumented Instructions (World Of Spectrum)](https://worldofspectrum.org/z88forever/dn327/z80undoc.htm)
- [ClrHome's Z80 Opcode Table](https://clrhome.org/table/#%20)
- [ZX Spectrum Keyboard Cheat Sheet](http://slady.net/Sinclair-ZX-Spectrum-keyboard/)
- [Z80 Instruction Exerciser (zexall, zexdoc)](https://mdfs.net/Software/Z80/Exerciser/Spectrum/)
- [JSMoo Z80 tests](https://github.com/raddad772/jsmoo/tree/main/misc/tests/GeneratedTests/z80)
- [JGH Spectrum 48K ROM](http://mdfs.net/Software/Spectrum/Harston) by J.G.Harston.
- [z88dk C to z80 compiler](https://z88dk.org/site/)
---
Feel free to follow me on Twitter for more updates: [@deanthecoder](https://twitter.com/deanthecoder)