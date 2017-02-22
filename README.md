# PersistentKeyValueStore
This is exactly what it sounds like; a simple key-value store that gets persisted to disk.  There are a couple implementations 
of the provided IPersistentKeyValueStore interface, one of which is implemented using SQLite and a second which uses a JSON 
flat file.

I've found this mildly useful for doing things like configuration when you want to avoid app.config, or just to dump 
arbitrary, semi-structured data on disk.

# Building
This library is designed to build against .net core.

Given the recent effort in the .net core environment to [transition to the csproj project format from the previous
project.json format,](https://blogs.msdn.microsoft.com/dotnet/2016/11/16/announcing-net-core-tools-msbuild-alpha/) 
there are some restrictions on how code in this repo may be built.  This project has opted to support the new csproj 
format going forward; however, given the state of tooling at the current time what that means is 
that **building this project will require either Visual Studio 2017 RC or a version of the dotnet CLI tools greater than 1.0.0-RC3.**

# How To Use This Code
Take a look at the IPersistentKeyValueStore interface; it's pretty self explanatory.  You can create an IPersistentKeyValueStore
by instantiating a SQLiteStore or JsonStore.  You might even wrap all that up in a factory of some sort.  Good luck.
