## 📢 Latest Announcements

### `View3d.SetSectionBox` and `View.SetCropBox` have been removed. These nodes were becoming too difficult to support due to [persistent dynamo issues with bounding boxes](https://forum.dynamobim.com/t/problem-setting-crop-boxes-and-section-boxes-in-revit-2022/70045/29).

---

<h1 align="center">
  <br>
  <img src="!Documentation/Logo/RhythmLogo.png" alt="Rhythm" width="350">
  <br>
</h1>

<h3 align="center">A collection of custom nodes for <a href="http://dynamobim.org/" target="_blank">Dynamo</a>.</h4>

<p align="center">
  <a href="https://getyarn.io/yarn-clip/e2546962-768f-4e04-a4e6-5e51a3025f8d">
   <img src="https://forthebadge.com/images/badges/60-percent-of-the-time-works-every-time.svg">
  </a>
</p>

---

[![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg)](https://github.com/johnpierson/RhythmForDynamo/graphs/commit-activity)

If you appreciate the work put into this free package, please vote in support of it on Dynamo's package manager. 

## Disclaimer
This tool is not affiliated with Autodesk and was written and provided in a personal capacity by the author. This tool is provided "as is" without warranty of any kind, express or implied. Use at your own risk. By using this open-source tool, you acknowledge that the developers and contributors are not liable for any damages, losses, or issues that may arise from its use. You are responsible for ensuring compliance with all applicable laws and regulations. Remember, open-source software relies on community contributions, so functionality and security might vary. Always review, understand, and, if necessary, modify the code to suit your specific needs.

## License
[![GitHub license](https://img.shields.io/github/license/johnpierson/RhythmForDynamo)](https://github.com/johnpierson/RhythmForDynamo/blob/master/LICENSE)
Rhythm is licensed under the BSD 3 Clause License. You can use this package however you want in Dynamo. It's just limited in the way the source code may be used. A simplified version of this license is available [here](https://www.tldrlegal.com/license/bsd-3-clause-license-revised).

## Current Version
 Supported Revit versions are 2020 and up. And supported Dynamo versions are Dynamo 2.0.x and up. Rhythm is deployed from github, and auto-downloads the correct node libraries on the fly. 

## Components
Rhythm consists of several sub-libraries. These are describe a bit more in detail below.
- [Rhythm Core](https://github.com/johnpierson/RhythmForDynamo/tree/master/src/RhythmCore), General methods and helpers with no reliance on Revit.
- [Rhythm Revit](https://github.com/johnpierson/RhythmForDynamo/tree/master/src/Rhythm), All the Revit nodes that work from 2020-2025
- [Rhythm UI](https://github.com/johnpierson/RhythmForDynamo/tree/master/src/RhythmUI), Revit UI Nodes.
- [Rhythm View Extension](https://github.com/johnpierson/RhythmForDynamo/tree/master/src/RhythmViewExtension), this view extension allows for the auto-annotating of Rhythm nodes and control of the run mode when the user places `Background Document` nodes.
- [Rhythm Python](https://github.com/johnpierson/RhythmForDynamo/tree/master/RhythmPython), Python code for many of the popular Rhythm  nodes. <sub><sup>if you want to hurt my feelings and not use the Rhythm package :pleading_face: </sub></sup>

## Known Issues
- Nodes that interact with background opened documents take some more consideration. This includes running Dynamo graphs that use them in manual run mode and often times closing and opening dynamo after the process is complete. The reason this happens is Dynamo has limited document switching capability and we are exploiting this with those nodes. (Included Nodes: Applications.OpenDocumentFile, Applications.CloseDocument)

## Contributors
This package is primarily managed by the author of http://designtechunraveled.com with additional contributions from [People Like You™](https://github.com/johnpierson/RhythmForDynamo/graphs/contributors).

## Special Thanks
<img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg"/>
Thanks to Jetbrains for supplying an open-source license of Resharper for this library. More info about Jetbrains Open-Source & Resharper can be found <a href="https://www.jetbrains.com/community/opensource/?utm_campaign=opensource&utm_content=approved&utm_medium=email&utm_source=newsletter&utm_term=jblogo#support"> here</a>.

## Updates
Since there is not currently an update notification process on Dynamo's package manager, I post notifications for updates on twitter.
[![](https://img.shields.io/twitter/follow/johntpierson.svg?label=Follow&style=social)](https://twitter.com/johntpierson)

## Examples
Examples will be available on the [wiki](https://github.com/johnpierson/RhythmForDynamo/wiki) soon,

## Help improve Rhythm
If you're interested in contributing to Rhythm, just submit a [pull request](https://github.com/johnpierson/RhythmForDynamo/pulls).

## Installation
Rhythm is available through Dynamo's built-in package manager. This provides the simplest installation for most users. If you are interested in manual install, there are some fabulous resources out there describing how to build your own Dynamo package.

## Thank you!

## What is with all the removals of DYFs?!!
With Dynamo 2.0 comes a fundamental file format change that makes anything that is a DYF or DYN (the filetypes that Dynamo uses) not backward compatible. This forced me to choose between two options. Option 1: Have 2 versions of Rhythm for each major version of Dynamo, eg. “Rhythm for Dynamo 1.x” and “Rhythm for Dynamo 2.x”. Option2: Continue my long term goals of migrating Rhythm to be fully “Zero Touch” c# code. I went with option 2 as it provides a more stable product and further compatibility.

[![forthebadge](https://forthebadge.com/images/badges/made-with-crayons.svg)](https://forthebadge.com)
