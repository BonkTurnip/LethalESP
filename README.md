# LethalESP
This is a little game hacking project that I did for one of my favorite games, Lethal Company!
## Installation
1. Download/build the LethalESP.dll file
2. Use your favorite Mono Injector to inject the .dll into Lethal Company while it is running.
   Give it the following information:
   - Namespace: `LethalESP`
   - Class: `Loader`
   - Method: `Init`
5. Enjoy!
## Usage
- `insert` to toggle the ESP on/off
- `home` to rescan for grabbable items (necessary when starting a mission)
- `del` to switch between game cameras (only do this if the position of the boxes is wrong)
- `end` to switch between cameras used for the minicam
- `page down` to toggle rendering the minicam
- Enemies appear in red, scrap in green, and entrances/exits in cyan
- The ship location appears as a magenta triangle
## Building
1. Import the project into Visual Studio. I used VS2019, but it will probably work on newer versions.
2. Add Assembly-CSharp.dll, Assembly-CSharp-firstpass.dll, and Unity* from the `Lethal Company_data` directory in the `Lethal Company` install directory to the references in the project.
3. Build the project.
## Notes
- This has only been tested with a resolution of 1920x1080. You will likely have trouble with ESP alignment on other aspect ratios/resolutions.
- Works in multiplayer you will just need to use `del` to find the correct game camera and `end` to select the right camera for your minicam.
- Make sure to inject once you have loaded into a game otherwise it won't work right.
- Using this cheat **WILL** impact your game's performance
