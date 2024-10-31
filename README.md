# Ian's Blogroll

This repository contains the code that generates my [blogroll](https://ian.wold.guru/Blogroll/), which is also hosted by GitHub Pages from this repository.

I use my own static site generator [Metalsharp](https://github.com/IanWold/Metalsharp) to read a list of RSS links from a config and generate the static site. This is run as a C# Script (csx) file in an Action, which is triggered to run each day at midnight or when I push to main. This allows me to display the latest posts from my feeds at this URL, which gives me a sort of RSS reader that I can use to keep up with the blogs I follow, and you can see exactly what I'm reading on the web!
