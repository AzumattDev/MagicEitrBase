# Description

## A mod that will update your base Eitr by a factor of your magic skills

`Version checks with itself. If installed on the server, it will kick clients who do not have it installed.`

`This mod uses ServerSync, if installed on the server and all clients, it will sync all configs to client`

`This mod uses a file watcher. If the configuration file is not changed with BepInEx Configuration manager, but changed in the file directly on the server, upon file save, it will sync the changes to all clients.`

### Incompatible Mod Information

* This mod will likely be slightly incompatible with any mod that modifies the base eitr value.

### Math Breakdown

All math variables besides the actual level being passed in are configurable. <em>**Note:**</em> I am not responsible
for any math errors that may occur when you change the values. Learn to do math. Below is a breakdown of the stuff you
can change with the variables surrounded in {}.

```csharp
( (Math.Pow(skillLevel / {Skill Divider}, {Power Amount})) * {Skill Scalar} ) * {Final Multiplier} 
```

| `Skill Level` | `Math`                                                                 | `Result` | `With 2 Final Multiplier` |
|---------------|------------------------------------------------------------------------|----------|---------------------------|
| 0             | (Math.Pow(0 / 100.0, 2.0)) * 100 = (0 * 0) * 100                       | 0        | 0                         |
| 1             | (Math.Pow(1 / 100.0, 2.0)) * 100 = (0.01 * 0.01) * 100 = 0.01 * 100    | 1        | 2                         |
| 25            | (Math.Pow(25 / 100.0, 2.0)) * 100 = (0.25 * 0.25) * 100 = 0.0625 * 100 | 6.25     | 12.50                     |
| 50            | (Math.Pow(50 / 100.0, 2.0)) * 100 = (0.5 * 0.5) * 100 = 0.25 * 100     | 25       | 50                        |
| 75            | (Math.Pow(75 / 100.0, 2.0)) * 100 = (0.75 * 0.75) * 100 = 0.5625 * 100 | 56.25    | 112.50                    |
| 100           | (Math.Pow(100 / 100.0, 2.0)) * 100 = (1 * 1) * 100 = 1 * 100           | 100      | 200                       |

<em>**Note:**</em> The above math is for the base eitr value that will get added per level.

* The code that is surrounding the calculation is checking to see how good the player is at Elemental Magic and Blood
  Magic. It decides which of the two skills the player is better at and then finds out how much skill level that player
  has. The skill level of the highest skill is then used in the above math to determine how much eitr is added to the
  base eitr value.

`Feel free to reach out to me on discord if you need manual download assistance.`

# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>
***

| `Version` | `Update Notes`                                                                                                                                                                                                                                                                            |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 1.1.2     | - It has been requested that the Power Amount variable accept float values (to support things like 0.5). This is now possible                                                                                                                                                             |
| 1.1.1     | - Fix an issue that could cause your Eitr to be halved if your Blood Magic was above Elemental Magic. Blood magic was performing the calculation correctly and it only seemed like it was being halved.                                                                                   |
| 1.1.0     | - Add ServerSync due to it now being configurable<br/>- Allow all math variables besides the level to be configurable<br/>- Add in a version check for when it's installed on the server<br/>- The math changes basically result in additional eitr of about half of the original version |
| 1.0.0     | Initial Release                                                                                                                                                                                                                                                                           |
