Nuget: https://www.nuget.org/packages/Synergy/
# Synergy
Synergy is a Solution package consisting of many 'sub' projects. Each project has its own uses. I made a main solution for having all the projects together as i use them almost everyday.
I hope many of you will find it usefull too!

## Projects Included

* Synergy.Extensions - Contains various helper methods, including those which help to disable the annoying QuickEdit mode of windows console windows and some attributes.
* Synergy.Logging - An event based logging library which i use in almost all my projects. As a good example for the use, 
take it as you have a solution with multiple projects, u can reference this library as the logger on all projects and just use the static events to get the LogEvent messages on just a single project or a class. (getting all log message on Main.cs and handling it there)
* Synergy.Requests - An HttpClient library for sending and receiving various HTTP requests with ease. its a IDisposable with thread safety (well not much but hey it works)
