# Theemin

This is a dotnet core based content rendering site.
Nothing fancy, render front-matter html or markdown content with html layouts.

Why should I use this instead of a static site generator like Hugo or Jekyll? You probably shouldn't ;)

Why is it called Theemin? Named after [Upsilon<sup>2</sup> Eridani](https://en.wikipedia.org/wiki/Upsilon2_Eridani) aka Theemin

## Front-matter format

The layout/title/variables for content is defined through yaml front-matter at the start of a file.
The content should be parseable into the `xweb.Content.PageData` class.
For example:

```yaml
---
layout: default.html 
title: Home Page
variables:
  test: content
---
your {{test}} here
```

## Templating

The templating ability is quite limited (for now), supported are variables and includes.
No regex is used to keep it performant.

Variables: `{{myVariable}}`  
Includes: `{{>template/name}}` relative to the templates folders

## Folder structure

| Folder    | Purpose                      |
|-----------|------------------------------|
| wwwroot   | Static files. ie css, images |
| content   | Site content                 |
| templates | Templates/Layout files       |

## Configuration

Via [appsettings.json](xweb/appsettings.json).

```json5
{
  "Template": {
    "Root": "templates",
    "Default": "default", // the default layout when not specified
    "DefaultExtension": "html", // extension to look for when not specified
    "Variables": { // variables available everywhere
      "SiteName": "My New Site"
    }
  },
  "Content" : {
    "Root": "content",
    "DefaultPage": "index",
    "DefaultLayout": "default"
  }
}
```

## Running

Best used in a container, a [Containerfile](Containerfile) is provided for this.

Todo: publish to dockerhub