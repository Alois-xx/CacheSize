# Get and Set Windows Cache Size
This is a small utility to read and set hard file system cache size limits to measure its 
impact on system performance which can cause excessive working set trimming if the cache is too
small.

## Usage

```
CacheSize [-get] [-setmax dd] [-reset]
  -get              Print current file system cache size settings.
  -setmax dd        Set file system cache size to dd MB. May fail if value is too small.
  -reset            Disable hard cache size size limit.
```

```
c:\>cachesize -get 
Cache Size Min:            1,024 KB
Cache Size Max:   17,179,869,184 KB
Current Size  :        1,244,400 KB (according to Performance Counter (Memory/Cache Bytes))
Cache Flags   :             None

c:\>cachesize -setmax 20 
Set Max Hard Max: 20,480 KB

c:\>cachesize -get       
Cache Size Min:            1,024 KB
Cache Size Max:           20,480 KB
Current Size  :           20,400 KB (according to Performance Counter (Memory/Cache Bytes))
Cache Flags   :  MAX_HARD_ENABLE

c:\>cachesize -reset 
Resetting hard limits

c:\>cachesize -get   
Cache Size Min:            1,024 KB
Cache Size Max:           20,480 KB
Current Size  :           22,204 KB (according to Performance Counter (Memory/Cache Bytes))
Cache Flags   :             None
```