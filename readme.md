PhoenixX's Economy mod 
(aka "Frontier Economy" on steam)
======================
A Steam Workshop script mod for Space Engineeers.
----------------------
This mod is an attempt to add an economic system to Space Engineers.

Space Engineers is a sandbox game about engineering, construction and maintenance 
of space works. Players build space ships and space stations of various sizes and 
utilization (civil and military), pilot ships and perform asteroid mining.

More details about the "Space Engineeers" game is available here:
[www.spaceengineersgame.com](http://www.spaceengineersgame.com/)


This mod is published and publicly available on the [Steam Community here](
http://steamcommunity.com/sharedfiles/filedetails/?id=504209260).

If you are a contributor there is some additional notes here:
[Steam Econ Guide](http://steamcommunity.com/sharedfiles/filedetails/?id=481264606&preview=true)

The offical github repository for this mod is available here:
https://github.com/jpcsupplies/Economy_mod


For more information about this mod features and roadmap check the wiki [here:](
https://github.com/jpcsupplies/Economy_mod/wiki)

Visit the Keen Software House modding forums for more detail on modding Space Engineers:
http://forums.keenswh.com/?forum=325599


Working with the code.
---------------------
A copy of the game is required to work with or develop the script mod.

Working on the github repository.
The best way to work on this mod and test in Space Engineers is to create a Symbolic 
link from your Git repository directory to the Space Engineers mods folder.
Run a command prompt as Administrator, and then run the following line.

```
mklink /J "C:\Users\%USERNAME%\AppData\Roaming\SpaceEngineers\Mods\Economy" "C:\Users\%USERNAME%\Documents\GitHub\Economy_mod\Economy"
```

The Symbolic link directory can be simply deleted through Windows Explorer like a normal directory.

The Space Engineers references can be established with the following Symbolic link.
```
mklink /J "C:\Program Files\Reference Assemblies\SpaceEngineers" "C:\Program Files (x86)\Steam\SteamApps\common\SpaceEngineers\Bin64"
```
