# LethalESP
This is a little game hacking project that I did for one of my favorite games, Lethal Company!
## Installation
1. Download/build the LethalESP.dll file
2. Use your favorite Mono Injector to inject the .dll into Lethal Company while it is running.
3. Enjoy!
## Controls
- `insert` to toggle the ESP on/off
- `home` to rescan for grabbable items (necessary when starting a mission)
- `del` to switch between game cameras (only do this if the position of the boxes is wrong)
## Building
1. Import the project into Visual Studio. I used VS2019, but it will probably work on newer versions.
2. Add Assembly-CSharp.dll, Assembly-CSharp-firstpass.dll, and Unity* from the `Lethal Company_data` directory in the `Lethal Company` install directory to the references in the project.
3. Build the project.
## Notes
This has only been tested on single player with a resolution of 1920x1080. You will likely have trouble with ESP alignment on other aspect ratios/resolutions.