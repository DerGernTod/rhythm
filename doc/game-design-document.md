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
Two drums with eyes show up on the screen, an old one (grey/white beard and eyebrows) and a young one. They talk in icon speechbubbles. The elder drum explains the young one how to make other instruments march (_march song_). The player has to play the song correctly a few times in a row. Finishing the tutorial loads the first level, where the player has to play the _march song_ multiple times to move the controlled characters through the level. 

~~They want to build a cottage, but first need wood. For wood, they need an axe. For an axe, they need sticks and stones. The little one isn't motivated, so the big one starts to drum. The player has to hit the screen in correct timing a few times to motivate the little one (_march song_, tutorial).~~

Finishing the first level triggers another tutorial: the _gather song_. Another song the elder drum character teaches the young one. When played in the fields the controlled characters will collect resources that are currently visible on the screen.

~~Once he's motivated, a map view is shown and the player can select the "Fields". The level starts, a new song is learned (_gather song_). A field is shown with a few sticks and stones lying around. The formerly learned _march song_ moves the characters forward, the _gather song_ sends them to collect resources.~~

Consecutively playing songs correctly lets the characters collect more or move further. A level lasts for one day, so the level ends as the sun sets. Finishing a level unlocks the next one.

After each level the characters return to the overworld, which shows the camp and all currently available levels. They can also see the current squad executing their walk animation on the currently selected level. In the camp, they can use the collected resources to craft tools like an axe which a character is able to equip, or later build new (instruments) characters, weapons and buildings. Also, the players can build up their squad which will be part of the next mission (level).

After finishing the first level and crafting and equipping an axe, the player can visit the next level (forest) to get wood (a unit can chop wood if equipped with an axe with the gather song). In the forest, there are some smaller trees to chop (large ones can't be chopped) ~~, and they find another character. He joins their party after hearing their drums play a few correct beats~~. With the collected wood, the player can build a cottage in the camp to house more characters. However, to build more characters, they have to build an instrument maker's hut (requires more resources).

Getting better resources requires different equipment, which can be crafted using special buildings in the meta game. Collect wood and stones, craft pickaxes, get copper, build blacksmith etc...

## What sets this project apart?
- One button game
- Combination of crafting, building, rpg and rhythm game
- Procedural environments(?)

### Core gameplay mechanics

#### Rhythmic commands
##### Details
Tapping the screen in a specific rhythm will cause the players units to execute tasks. For every task, there's a song. The songs are: march, gather, attack, defend, dodge, retreat. The player plays a song in a 4/4 beat, then waits 4 beats and the units will "sing" the song and perform their tasks. If the player misses (or fails to jump in at the right time), the units trip.

![Songs](gdd/gdd-songs.jpg)

##### How it works
Playing the songs correctly fills an internal _streak score_ bar. Depending on the quality of the beat hits, this bar is filled up quicker or slower. A miss resets it. If it's full, the _streak power_ is increased and all task executions are improved (e.g. gather more, move further, etc.) by 20% per _streak power_. To start playing a song, the player can just tap the screen once. A white border that flashes with each beat helps the player to jump in at the right time.

![Ingame](gdd/gdd-ingame.jpg)

#### Camp building
##### Details
Camp buildings are built, used and upgraded during meta game. Resources are required to execute these actions. The longer the player plays, the higher the camp tier, the more and better resources are required. The camp can be used to house more units, upgrade them, equip them with items, craft new items, smelt ores, research advancements, build new characters and recap the previous mission (e.g. what resources were gathered, how did units progress, which areas have been unlocked).
##### How it works
The camp is displayed in a bird's eye view. New buildings can be built freely by tapping onto an empty space and selecting the building one want to build. If there are too many buildings, the view zooms out a bit to reveal more areas (maybe scrolling around is better, have to try...). In the beginning, only the cottage can be built. Later, research hut, instrument builder, blacksmith, hunter's hut, kitchen, training area and more follow. The buildings can be used to improve the units' performance on the field and enable new abilities when using the different songs.

#### Crafting & Resource Management
##### Details
The first thing the player wants to craft is an axe to chop wood. To be able to craft this, they need to collect sticks and stones. Some resources require units to wield special items, e.g. chopping wood requires a unit to wield an axe. Back in the camp, resources can be used to craft items. Crafting items requires different buildings although there are some starting items that can just be crafted "at the campfire" (like the axe).
##### How it works
Resources are collected during missions with the _gather song_. When selecting a mission on the world map, all known resource types are shown so the player knows which missions he has to complete to get certain resources. Unknown ones (e.g. not yet discovered because unable to obtain with the current equipment) are only shown when the necessary equipment is available. Missions that provide new kinds of resources because of new equipment technology are highlighted.

## Story and Gameplay
### Story (Brief)
The enemy: undead EDM music instruments

### Story (Detailed)
### Gameplay (Brief)
### Gameplay (Detailed)
#### Basic interactions
Whenever there's a scene change, the screen fades to white and shows a loading spinner/progress bar. Afterwards it's faded in again to show the new scene.

#### Tutorial interactions
Tutorials are triggered whenever the player learns a new song. They are shown as a comic strip, on which the last page is zoomed in upon, followed by fade-in of some UI elements: 
* the head of the young drum (basically the drum skin)
* an indicator when to hit the drum for correctly playing a song
* a large speech bubble pointing to the elder drum, showing an animation of an idle drum silhouette

A drum starts playing (background drum), indicating the beat time, and blue bubbles show up (fading in) from the right. the player should hit the drum whenever a bubble reaches the center of the drum. Successful hits are amplified by whipping eyebrows and movement of the elder drum. A full successful song makes the black silhouette execute the command (e.g. starts with a walk animation). During these 4 strokes. The blue indicator bubbles fade to 50% and turn red, to show that now's not the time for drumming.

If a player manages to get a streak power of 3, the tutorial finishes with a "xxx song learned!" text, then triggers a level where this song is required.

#### Ingame interactions
Levels should become more difficult and require playing more different songs and having more experienced characters to be mastered.

Elements that are always part of a level:
* the selected squad
* ambient sounds
* critters/neutrals/deco objects
* effects
* backgrounds
* finish line or boss

Elements that are introduced after specific songs have been learned (and should therefore be part of the level following the song):
* March song: nothing
* Gather song: Resource deposits
* Attack song: Melee enemies
* Defend song: Ranged enemies? (TODO)
* Dodge song: Obstacles (river, falling boulders, ...)
* Retreat song: Impassable terrain (with current equipment)

##### The Squad
The player should be able to decide, which units he wants to enter the levels with. Depending on their type, they have different equipment affinity (making them better lumberjacks or better archers), and depending on their level, different health, defense, and attack stats. Finishing a level with a unit grants them experience until their XP bar is full. If it's full, the unit can be improved in the camp.

Levels should be designed so that it's an advantage to bring a nicely balanced squad, or a squad that's specialized in finishing this one level.

##### Resource deposits
The main reason why a player wants to play a certain level: possibilities to gather resources of specific types. Each level should have a chance of containing certain resources, which are spawned at random locations on the map. If the gather song is played and the player has units with the right tool type and -tier, a deposit can be damaged. Whenever the deposit loses a certain amount of health, it drops an item of it's kind. If it loses all health, it drops another item and depletes, making the gatherers search for a new target.

##### Enemies
Undead Electronic Dance Music (EDM) instruments are the big enemy in this game. Rotten DJ pults, broken subwoofers, scratched-to-death vinyls, ...
There are two main categories of enemies: bosses and minions.
###### Minions
Minions, or rather, normal level enemies, are similar to player instruments, but they can be categorized more easily. Each category has their own strengths and weaknesses.
* Swordsmen (Frontline, strong attack, medium res vs. melee, high res vs. ranged, low res vs. magic, medium hp)
* Archers (Backline, weak attack, low res, low hp)
* Mages (Backline, strong vs. high res attack, special attacks (fireball, thunderstorm, icewind), low hp)
* Heavies (Tanks/Defenders) (Frontline, medium attack, weak vs. magic, high hp)

TODO

###### Bosses
Bosses are separate levels that end when the boss has been killed. Each boss shows up in different flavors (similar to bosses in Patapon: variants with additional horns, different colors, more wings, new attacks etc.). Bosses are high HP, high damage giant enemies with powerful attacks which could decimate a whole squad pretty quickly, if the player is not careful.
Bossanimations are timed so that the player has a chance to react. If a boss is going to execute a powerful attack, it shows an anticipation animation that lasts one or two beats before actually executing the attack. The player has to learn these anticipation moves and know which song to play to minimize or negate the damage (usually dodge or defend, based on the squad and the enemies attack, or even attack and try to stun the enemy). If the player was successful, the boss might stumble and drop down, so it's vulnerable, receiving more damage from attacks.

##### Obstacles
Levels might show obstacles blocking the path for some instruments. If an obstacle shows up while a march song is being played, the squad moves until they reach the obstacle. If the player then plays a command that would force some characters to continue moving, those characters take damage. The player has to play the _dodge song_ to force the units to cross the obstacle accordingly, which could be skipping over stones to cross a river, squeezing besides a giant boulder, hopping over a gap, ...

TODO

##### Leaving a level
There should always be a possibility to leave a level. Mobile players are fast-paced and want to be able to quit on demand (without losing too much). So a level can be quit two ways: either by leaving it via the menu (or closing the app), or by playing the _retreat song_. While the first makes the player lose all progress, the _retreat song_ ends the level with all the collected experience and items.

#### Overworld interactions
The overworld is mainly an overview over all the levels. It's devided into sections, where each section represents a level. The sections are aligned on an infinitely expanding circle that grows larger the more levels there are for selection. The leftmost section is always the camp. The player can always see a single section, and the end of the previous and beginning of the next section. Navigation happens via swipe and snap. The player can swipe indefinitely and the camera will snap on the last section that's visible when the swipe velocity is under a certain threshold. Sections can also be switched by tapping the next or previous one.

![Overworld](gdd/gdd-overworld.jpg)

If the section is a level, it shows:
* name of the level
* setting background (e.g. grasslands, forest, mountains, ...)
* setting foreground (e.g. green fields, trees, hills, ...)
Moods for this are drawn from the game Reus by Abbey Games:
![Reus](https://images.igdb.com/igdb/image/upload/t_screenshot_big/zbqoot3f1uyi7zz5eteb.jpg)
* current squad in walk animation towards the level
* known and unknown resources that can be collected in this level (resources that can't be collected by the current squad are highlighted somehow (red/gray))
* possible amount of these resources (more icons means more deposits)
* highscores (collection, time)
* (while scrolling?) a "Go to beginning" button that scrolls back to the camp
* (while scrolling?) a "Go to end" button that scrolls to the newest level
* a "March" button that starts the level

If the section is the camp, it shows:
* an "Options" button on the top right
* a preview of the current state of the camp
* current squad in walk animation towards the camp
* a "Go to end" button that scrolls to the newest level
* a "March" button that enters the town

The options button shows a popup with the following possibilities:
* Beat helper volume (default: 50%)
* Beat indicator display (always show, only show if no song is active, never show)
* Delete player data (with "are you sure", returns back to main screen)
* Notification settings (which notifications do we want? TODO)
* Quit (returns back to main screen)

#### Camp interactions
Interactions on the camp screen itself:
* leave camp
* create building
* enter building
* move camera

The main interactions are with the buildings. In the beginning, the only building is a fireplace. Additional buildings can be added by tapping on a build button (which shows buildings that can and can't be built, and the required resources), tapping on a buildable building, then moving it to a free location in the camp.

* __Fireplace__
Craft simple tier1 tools, such as an axe and a pick.

* __Barracks__
Upgrade and equip units with weapons and tools. Upgrade barracks to improve all units stats.

* __Workshop__
Craft new units. Upgrade to craft more advanced ones.

* __Blacksmith__
Smelt ores and craft more advanced tools and weapons. Upgrade blacksmith to increase possibilities.

* __House(?)__
Houses characters. Upgrade to house more and increase squad size.

* __Library__
Research advancements. Researches enables special weapons, abilities or unit types, or unlock buildings and building upgrades.

* __Graveyard__
If a unit dies, its grave is shown here. Spending $$ or very rare items revives a fallen unit.

## Assets Needed
### Graphics
#### UI
* Fonts
* Popup panels
* Buttons with text
* Buttons with icons
  * Yes/No or ✓/✘
* Equipment screens
* Squad selection screens
* Crafting screens
* TODO
#### Characters
* elder drum
* young drum
* violin
* recorder
* didgeridoo
* sax
* guitar
* boss (EDM subwoofer)
* TODO
#### Items
All items are spawned by mining their deposit, so each item also needs deposit graphics.
* stone
* stick
* birch tree
* rock
* coal ore
* beech tree
* iron ore
* oak tree
* copper ore
* TODO
#### Tools
We're starting with _Axe_ and _Pick_. All tools should have an icon representation as well as an ingame representation (attached to the equipping character), or a character decides on its own how its usage of the tool should look like:
* axe
* pick
* hands (no ingame representation)
* TODO
#### Weapons
See (Tools section)[#tools]
* sword
* bow
* spear
* shield
* staff (fire)
* staff (thunder)
* staff (ice)
* TODO
#### Levels
TODO: we haven't decided yet if we want to build the levels manually or just make an alternating background. Building them manually makes it easier to introduce scripted events (like falling rocks). We definitely need backgrounds though. Each level type should be reused, so there are multiple levels for each type (e.g. multiple forests, multiple grasslands, multiple mountains, etc).
* Grasslands level backgrounds
* forest level backgrounds
* mountain level backgrounds
* TODO
#### Overworld
* Camp state preview in variable progression
* 3 different grasslands previews
* 3 different forest previews
* 3 different mountain previews
* TODO
#### Tutorials
Tutorials look comic-like with multiple panels, showing the elder and the young drum discussing new songs. While learning a song, there should be a little preview animation of a character executing the actual command when played successfully. Each song needs such a little comic:
* March
* Gather
* Attack
* Defend
* Dodge
* Retreat
#### Miscellaneous
* ingame beat indicators (drum with hit markers)

### Animations
#### Characters
* walk
* gather (depending on equipped tool: axe, pickaxe, hands)
* attack (depending on equipped weapon: shield, melee, ranged and magic)
* defend (depending on equipped weapon: shield, melee, ranged and magic)
* jump, dodge
* retreat
#### Effects
* falling rocks
* flowing water/lava
* crushing buildings
* magic attacks (heal, fire, lightning, ice)
* reached finish line
* TODO
#### Camp Buildings
* tool/weapon crafting
* build building
* create character
* research
* smelt ores
* upgrade characters
* TODO
#### Tutorials
* tutorial characters (whip with eyebrows, execute command)

### Sounds
#### Characters
Each character should have their own sound they make when the drum is hit, as well as one for each song there is, for each streak power value there is. Characters should also make sounds upon interaction with the world (being attacked, collected item, dying, ...).
* Successful drum hit
* Play song (min 6 clips per song)
* Get hit
* Die
* Collect item
* Win level
* Be upgraded
* Be picked in squad selection
#### Songs
Each song should have their own _ambient_ background, depending on current streak power.
* March
* Gather
* Attack
* Defend
* Dodge
* Retreat
#### Overworld
There should be an ambient background for the overworld, that slightly changes based on the setting of the current level section.
* Ambient (per level section)
#### UI
* Button sounds
* Window open/close
* Swipe overworld
* Equip tools/weapons 
#### Effects
* Weapon sounds (bow + flying arrow, swords hitting various things, blowtube darts, different magic wand casts/detonations)
* Resource gathering sounds (pickaxe hitting rock, axe hitting tree, ...)
#### Level Ambients
Each level type should have their own ambient sounds, e.g.
* Birds chirping (forest, fields),
* Falling rocks (volcano, mountains),
* Howling wind (cliffside, mountains),
* Waves breaking on the shore (beach, cliffside)

