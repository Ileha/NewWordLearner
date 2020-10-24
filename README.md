# New word learner  

This is a software to learn foreign languages.  

## How to use  

### config file
This file contains some specific variables for "New word learner"  
./config.xml structure:  

```xml
<?xml version="1.0" encoding="utf-8"?>
<config>
    <UnsplashAPIKey>your key here</UnsplashAPIKey>
</config>
```

More information about Unsplash you could get [here](https://unsplash.com)
If you don't want to spent time to figuring out about Unsplash, 
program will work with google without UnsplashAPIKey.

### languages file
This file contains all available language in program. Also you could add yours.  
languages.xml structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<languages>
    <language code="en">English</language>
    <language code="ru">Russian</language>
    <language code="de">German</language>
    <!-- 
        your languages 
            where: 
                code is ISO 639-1 code,
                value is language's title
    -->
</languages>
```
Codes of languages you could get here [List of ISO 639-1 codes](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes)

### Audio
This program uses BASS audio library. 
To run program you have to download BASS library [here](http://www.un4seen.com/) under your platform and put it near exe.
