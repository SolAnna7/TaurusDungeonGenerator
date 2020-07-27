<img src="Logo/taurus_logo_text.png" title="TaurusDungeonGenerator" alt="TaurusDungeonGenerator logo">

# TaurusDungeonGenerator

#### A graph based procedural dungeon generator for Unity

* Download Presentation
* Download Example Project
* Asset Store Link

![#f03c15](https://via.placeholder.com/15/f03c15/000000?text=+) Todo: Graphic

## Features
- Abstract graph structure definition
- Store and load structures from config with PiscesConfigLoader ![](https://via.placeholder.com/15/ff0000/000000?text=+)
- Quick layout generation (not using Unity space!)
- Reusing dungeon plans by nesting
- Main path and branch generation
- Add meta data using tags and properties
- Optional paths
- Margin between elements
- Debug view

## System Requirements

* Unity 2018.4 or later 

## Installation

- Clone into the Assets folder of your Unity project
- Download from Unity Asset Store ![](https://via.placeholder.com/15/f03c15/000000?text=+) TODO: Link
- To load the dungeon structures from config files use the PiscesConfigLoader ![](https://via.placeholder.com/15/f03c15/000000?text=+) TODO: Link

## Usage

- Create your room assets
  - Add the `Room` component to the root
  ![](https://via.placeholder.com/15/ff0000/000000?text=+) image
  - Setup your doors with `RoomConnector` component
  ![](https://via.placeholder.com/15/ff0000/000000?text=+) image
  - Collect your rooms into `RoomCollection`-s for randomized usage

- Define your dungeon structure
  - In code
  
  ![](https://via.placeholder.com/15/ff0000/000000?text=+) TODO code
  
  - Load from config files using PiscesConfigLoader ![](https://via.placeholder.com/15/ff0000/000000?text=+) TODO: Link
  
  ![](https://via.placeholder.com/15/ff0000/000000?text=+) Todo code

## Plannes features

### v0.9
 - Unity Editor extension for dungeon structure creation
### v1.0
 - Bounding box for the dungeon
 - Room repetition control
 - Path straightness/~~gayness~~ curvedness control
### ?
 - Optional handling refactor: Activate optional paths based on room tags
 - Circles in the layout
 - Variables: Define variables (like random ranges) to reuse in the structure