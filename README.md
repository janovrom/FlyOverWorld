## FlyOverWorld
### Master thesis
The goal of my master thesis was to create visualisation of unmanned (aerial) vehicles, which are small and fast objects, and connect it to AgentFly simulator. It also tests how useful would be use of web services providing satellite imagery, heightmaps and other information like buildings and landuse.

### Used services
The work uses Mapbox to download satellite imagery and elevation data. At first, Mapzen was used to obtain vector tiles with landuse and buildings, however due to Mapzen ending its services, this was migrated to Nextzen.

### Binaries and Executables
I don't provide the simulator as its 3rd party software and I do **not** own it, though it can be obtained from [CTU](https://dspace.cvut.cz/handle/10467/68616?show=full). The older builds for different platforms can also be downloaded there, though these builds still use the old web services, so there are no buildings. Newly build executables for Windows x64 and x86 can be found [here](http://janovrom.ddns.net/janovrom/FlyOverWorld/raw/0b37d6be97784cf6b5288b37c2519219a8d5a266/Build/win-x64.zip) and [here](http://janovrom.ddns.net/janovrom/FlyOverWorld/raw/0b37d6be97784cf6b5288b37c2519219a8d5a266/Build/win-x86.zip) respectively.

<p align="center">
![](http://janovrom.ddns.net/janovrom/FlyOverWorld/raw/ee1488df3524e207e899d10752bd9dcdf5972162/Media/demo1.mp4)
<br/>
<i>Short presentation of functionality</i>
</p>