# TerraInvicta.Diplomacy

A mod for Terra Invicta aimed at improving player experience regarding Relations and Diplomacy between Factions.

## Features

* New Diplomacy/Trade treaty `Reset Bilateral Relations`. An agreement to restart bilateral relations between
  factions. Sets both parties hate towards each other to the default game start values.
* When `PlayerFaction` was far ahead, certain treaties like `Truce` or `NAP` could not be made, this was removed and a
  new system put in place to decide when those treaties are available.
* Saves data separate from game saves and thus is absolutely save game compatible after removal.

## Upcoming Features

* Improve on existing features (Ai to Ai diplomacy does not use the new features as of yet).
* Add more Diplomacy treaties, such as:
    * "Real" Alliance
    * Shared Intel Treaty
* Improve hate logic in regard to diplomacy treaties.

## Installation (Manual)

* Install the [UnityModManager](https://www.nexusmods.com/site/mods/21) for `Terra Invicta`
* Download this mod package from the files tab
* Extract all files inside the top-level folder in the ZIP to your game mod folder.
  Under: `<TerraInvictaInstallFolder>\Mods\Enabled`
* Now this folder should contain the `TerraInvicta.Diplomacy` folder

## Uninstall (Manual)

* Remove the `TerraInvicta.Diplomacy` folder in your game mods folders.
* _(optional) Any data saved by the mod can be removed by removing the folder `DiplomacyModSaves` in the save-game
  folder_

### FAQ

* Where is the `<TerraInvictaInstallFolder>` when I use Steam?
    * In `<SteamInstallFolder>\steamapps\common\Terra Invicta\`
* Will this mod ruin my Save/Is it save to remove?
    * This mod can be removed without any problems
* Where do I find the save-game folder?
    * Usually under `<MyDocuments>\My Games\TerraInvicta\Saves`

### Contribution

All PRs are welcome and encouraged 👍.
