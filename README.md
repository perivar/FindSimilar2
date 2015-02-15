# FindSimilar 2
## Audio Search Utility utilising Soundfingerprinting methods README

>Per Ivar Nerseth, 2015
>perivar@nerseth.com

*FindSimilar. Version 2.0.1.*
**Copyright (C) 2012-2015 Per Ivar Nerseth.**

`Usage: FindSimilar.exe <Arguments>

Arguments:
        -scandir=<scan directory path and create audio fingerprints - ignore existing files>
        -match=<path to the wave file to find matches for>
        -matchid=<database id to the wave file to find matches for>

Optional Arguments:
        -gui    <open up the Find Similar Client GUI>
        -resetdb        <clean database, used together with scandir>
        -num=<number of matches to return when querying>
        -percentage=0.x <percentage above and below duration when querying>
        -? or -help=show this usage help>`

Normal Steps are:
1. Scan Directory
FindSimilar.exe -scandir="path/to/audiosamples/dir" -resetdb

2. Optional - Scan more directories
FindSimilar.exe -scandir="path/to/another_audiosamples/dir"

3. Use either command prompt utility
FindSimilar.exe -match="path/to/audiosample.wav|mp3|flac|wma|etc"
or
FindSimilar.exe -matchid=4

4. Or use GUI client
FindSimilar.exe -gui
