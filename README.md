# TerraInvicta.Diplomacy

A mod for Terra Invicta aimed at improving player experience regarding Relations and Diplomacy between Factions.

## Features

### New Diplomatic Treaties:

* `Alliance` - Agreement that makes both factions allied to each other, share intel and prevents hostile actions against
  each other.
* `Break Alliance` - Valid if in an alliance, breaks up the alliance.
* `Reset Bilateral Relations` - Agreement to restart bilateral relations between
  factions. Sets both parties hate towards each other to the default game start values.

### Changes to base game:

* In the base game, when `PlayerFaction` is far ahead, certain treaties like `Truce` or `NAP` can not be made, a
  different approach is used in this mod - treaties are available based on current "relations" and ideology of factions.

### Misc:

* Save compatible after mod removal or game update.
* Works with ongoing saves.

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

* Remove the `TerraInvicta.Diplomacy` folder in your game mod folder.
* _(optional) Any data saved by the mod can be removed by removing the folder `DiplomacyModSaves` in the save-game
  folder._

### FAQ

* Where is the `<TerraInvictaInstallFolder>` when I use Steam?
    * In `<SteamInstallFolder>\steamapps\common\Terra Invicta\`
* Will this mod ruin my Save/Is it save to remove?
    * This mod can be removed without any problems
* Where do I find the save-game folder?
    * Usually under `<MyDocuments>\My Games\TerraInvicta\Saves`
* What are the possible relations for factions?
    * Max relations are based on each factions ideology (e.g. alien friends can't be allied with alien haters). Max
      relations overview:
      ![relations_grid_ti](https://github.com/eg3r/TerraInvicta.Diplomacy/assets/41837307/8c07ba49-343c-4324-a423-2c4e26f806ca)

### Contribution

All PRs are welcome and encouraged 👍.
