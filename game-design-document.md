# The Rhythm Game

## Overview

### Theme / Setting / Genre
A rhythm game with strategic elements.

### Core gameplay mechanics brief
- Rhythmic commands
- Camp building
- Crafting and resource management

### Target platforms
- Android
- IOS
- UWP

### Monetization model
Ad driven, watch ad to get resources.
Progression enhancers via $$.

### Influences
#### Patapon
Main influencer. Hit different sounding drums in 4/4 beat to issue commands. React to boss monster prepared attacks (e.g. defend, run away) or attack enemy fortresses (with archers in towers). Commands like march, attack, defend, run away. Mainly sidescroller, but much off-meta gameplay as units can be equipped with weapons and upgraded, different unit types, ...
#### Guitar Hero
Forces you to hit notes accurately for streaks to get more points.
#### Don't Starve
Run through the overworld to collect stuff and craft items you need for progression.

### Elevator pitch
A game where the player controls a small group of units via drum beat songs to do specific tasks, like collect and fight. The player builds up a small camp which grows bigger after getting better resources.

### Project Description
Two weird little figures wake up in the middle of a field. They talk in icon speechbubbles. They want to build a cottage, but first need wood. For wood, they need an axe. For an axe, they need sticks and stones. The little one isn't motivated, so the big one starts to drum. The player has to hit the screen in correct timing a few times to motivate the little one (_march song_, tutorial).

Once he's motivated, a map view is shown and the player can select the "Fields". The level starts, a new song is learned (_gather song_). A field is shown with a few sticks and stones lying around. The formerly learned _march song_ moves the characters forward, the _gather song_ sends them to collect resources. Playing the songs correctly lets the characters collect more or move further. A level lasts for one day, so the level ends as the sun sets. Back to the camp, they can craft an axe which the smaller one is able to equip, then they rest.

On the next day, they can visit the forest to get wood (unit can chop wood if equipped with an axe with the gather song). In the forest, there are some smaller trees to chop (large ones can't be chopped), and they find another character. He joins their party after hearing their drums play a few correct beats. More characters can only join after building a cottage.

Getting better resources requires different equipment, which can be crafted using special buildings in the meta game. Collect wood and stones, craft pickaxes, get copper, build blacksmith etc...

## What sets this project apart?
- One button game
- Combination of crafting, building, rpg and rhythm game
- Procedural environments

### Core gameplay mechanics

#### Rhythmic commands
##### Details
Tapping the screen in a specific rhythm will cause the players units to execute tasks. For every task, there's a song. The songs are: march, gather, attack, defend, retreat. The player plays a song in a 4/4 beat, then waits 4 beats and the units will "sing" the song and perform their tasks. If the player misses (or fails to jump in at the right time), the units trip.
##### How it works
Playing the songs correctly fills an internal _fever_ bar. Depending on the quality of the beat hits, this bar is filled up quicker or slower. A miss resets it. If it's full, all task executions are improved (e.g. gather more, move further, etc.). To start playing a song, the player can just tap the screen once. A white border that flashes with each beat helps the player to jump in at the right time.

#### Camp building
##### Details
Camp buildings are built, used and upgraded during meta game. Resources are required to execute these actions. The longer the player plays, the higher the camp tier, the more and better resources are required. The camp can be used to house more units, upgrade them, equip them with items, craft new items, smelt ores, research advancements and recap the previous mission (e.g. what resources were gathered, how did units progress, which areas have been unlocked).
##### How it works
The camp is displayed in a bird's eye view. New buildings can be built freely by tapping onto an empty space and select the building one want to build. If there are too many buildings, the view zooms out a bit to reveal more areas (maybe scrolling around is better, have to try...). In the beginning, only the cottage can be built. Later, research hut, blacksmith, hunter's hut, kitchen, training area and more follow. The buildings can be used to improve the units' performance on the field and enable new abilities when using the different songs.

#### Crafting & Resource Management
##### Details
The first thing the player wants to craft is an axe to chop wood. To be able to craft this, they need to collect sticks and stones. Some resources require units to wield special items, e.g. chopping wood requires a unit to wield an axe. Back in the camp, resources can be used to craft items. Crafting items requires different buildings although there are some starting items that can just be crafted "at the campfire" (like the axe).
##### How it works
Resources are collected during missions with the _gather song_. When selecting a mission on the world map, all known resource types are shown so the player knows which missions he has to complete to get certain resources. Unknown ones (e.g. not yet discovered because unable to obtain with the current equipment) are only shown when the necessary equipment is available. Missions that provide new kinds of resources because of new equipment technology are highlighted.

