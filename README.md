# New word learner  

This is a software to learn foreign languages.  

## How to use  

### config file
This file contains some specific variables for "New word learner"  
./config.json structure:  

```json
{
  "UnsplashAPIKey": "9b7218c670967d7b67b4de8fd3c0fb454a2b8e513c9e565b8d125ddb250faeee"
}
```

More information about Unsplash you could get [here](https://unsplash.com)
If you don't want to spent time to figuring out about Unsplash, 
program will work with google without UnsplashAPIKey.

### languages file
This file contains all available language in program. Also you could add yours.  
languages.json structure:

```json
[
  {
    "LanguageTitle": "English",
    "Code": "en"
  },
  {
    "LanguageTitle": "Russian",
    "Code": "ru"
  },
  {
    "LanguageTitle": "German",
    "Code": "de"
  },
  {
    "LanguageTitle": "Czech",
    "Code": "cs"
  }
  /*z
  your languages 
            where: 
                code is ISO 639-1 code,
                value is language's title
  */
]
```
Codes of languages you could get here [List of ISO 639-1 codes](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes)

### Audio
This program uses BASS audio library. 
To run program you have to download BASS library [here](http://www.un4seen.com/) under your platform and put it near exe.
