# CastIt
<p align="center">
  <img height="120px" src="CastIt/favicon.ico">
</p>

> A casting app and remote controller made for my personal use.
<p align="center">
  <img src="images/Promo.png">
</p>

## Windows

<p align="center">
  <img height="450" width="700" src="images/imgA.png">
</p>

<p align="center">
  <img height="450" width="700" src="images/imgB.png">    
</p>

<p align="center">
  <img height="450" width="700" src="images/imgC.png">    
</p>

Get the latest release [here](https://github.com/Wolfteam/CastIt/releases).

## Android
<p align="center">
  <img height="450" src="images/imgD.png">    
  <img height="450" src="images/imgE.png">    
  <img height="450" src="images/imgF.png">
</p>

[<img height="100" width="250" src="https://play.google.com/intl/en_us/badges/static/images/badges/en_badge_web_generic.png" />](https://play.google.com/store/apps/details?id=com.miraisoft.castit)

## Features
* Playlists
* Themes
* Drag and drop files
* And all the good stuff that comes in casting apps

## Build

### Server

Build the image locally:

```
docker build --build-arg TARGETARCH=arm64 -t castit -f CastIt.Server/Dockerfile .
```

#### Manual export / import (tarball)

Save the built image to a gzipped tarball, copy it to the target host, and load it:

```
docker save castit | gzip > ~/Development/castit.tar.gz
docker load < castit.tar.gz
```

#### Push to a registry

[`push-image.sh`](push-image.sh) builds for `linux/arm64` and pushes the image tagged with the **last commit's short hash** and `:latest` (defaults: `--image castit`, `--dockerfile CastIt.Server/Dockerfile`):

```
./push-image.sh --registry <registry-host>
```

It logs in if needed, warns on a dirty working tree, and pushes both tags. Add `--sha-only` to skip `:latest`; run `./push-image.sh --help` for all options.
