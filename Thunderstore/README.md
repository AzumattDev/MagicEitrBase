# Description

## A mod that will update your base Eitr by a factor of your magic skills

This mod automatically adjusts your base Eitr (magic resource) based on your proficiency in Elemental Magic and Blood
Magic.

- **Version Enforcement:**  
  The mod performs a version check; if installed on the server, it will kick clients who do not have the mod installed.

- **Configuration Syncing:**  
  This mod uses ServerSync. When installed on both the server and all clients, all configuration options are synced to
  clients.

- **Dynamic Config Reloading:**  
  The mod uses a file watcher. Changes made directly to the configuration file on the server (outside the BepInEx
  Configuration Manager) will automatically sync with all clients upon file save.

### Incompatible Mod Information

- This mod may be slightly incompatible with any mod that also modifies the base Eitr value.
- It is incompatible with **AllTheBases**.

### Math Breakdown

All math variables besides the actual level being passed in are configurable. <em>**Note:**</em> I am not responsible
for any math errors that may occur when you change the values. Learn to do math. Below is a breakdown of the stuff you
can change with the variables surrounded in {}.

```csharp
( (Math.Pow(skillLevel / {Skill Divider}, {Power Amount})) * {Skill Scalar} ) * {Final Multiplier} 
```

Where:

- **Skill Level:** Your current level in the highest of Elemental or Blood Magic.
- **Skill Divider:** Reduces the effect of each level by dividing your skill level.
- **Power Amount:** Determines the exponent, creating a non-linear scaling effect.
- **Skill Scalar:** Scales the result of the exponentiation.
- **Final Multiplier:** Multiplies the final bonus, allowing you to adjust the overall impact.

| `Skill Level` | `Math`                                                                 | `Result` | `Result with Final Multiplier = 2 ` |
|---------------|------------------------------------------------------------------------|----------|-------------------------------------|
| 0             | (Math.Pow(0 / 100.0, 2.0)) * 100 = (0 * 0) * 100                       | 0        | 0                                   |
| 1             | (Math.Pow(1 / 100.0, 2.0)) * 100 = (0.01 * 0.01) * 100 = 0.01 * 100    | 1        | 2                                   |
| 25            | (Math.Pow(25 / 100.0, 2.0)) * 100 = (0.25 * 0.25) * 100 = 0.0625 * 100 | 6.25     | 12.50                               |
| 50            | (Math.Pow(50 / 100.0, 2.0)) * 100 = (0.5 * 0.5) * 100 = 0.25 * 100     | 25       | 50                                  |
| 75            | (Math.Pow(75 / 100.0, 2.0)) * 100 = (0.75 * 0.75) * 100 = 0.5625 * 100 | 56.25    | 112.50                              |
| 100           | (Math.Pow(100 / 100.0, 2.0)) * 100 = (1 * 1) * 100 = 1 * 100           | 100      | 200                                 |

> **Note:** The above math applies both when increasing your maximum base Eitr and when boosting your regeneration rate
> if you’ve enabled scaling based on skill.

* The code that is surrounding the calculation is checking to see how good the player is at Elemental Magic and Blood
  Magic. It decides which of the two skills the player is better at and then finds out how much skill level that player
  has. The skill level of the highest skill is then used in the above math to determine how much eitr is added to the
  base eitr value.

**Math Breakdown for Regen Bonus:**  
The bonus added to your regeneration rate is calculated with the same formula as above:

```csharp
( (Math.Pow(skillLevel / {Skill Divider}, {Power Amount})) * {Skill Scalar} ) * {Final Multiplier}
```

**However, when applied to regeneration, this bonus is further multiplied by the "Regen Bonus Multiplier" setting.**  
For example, if your bonus calculation returns 25 at level 50 and your `Regen Bonus Multiplier` is set to 0.15 (current default), the bonus added to your regeneration will be 3.75 units/second.
This ensures that while your base Eitr scales nicely with your magic skill, the regeneration bonus remains balanced even at higher levels.



---

## How It Works

1. **Base Eitr Calculation:**  
   Your highest magic skill (either Elemental or Blood Magic) is used to calculate a bonus value. This bonus is applied
   to your base Eitr and, if custom regeneration is enabled, to your regeneration rate.

2. **Regeneration Scaling:**

- **Custom Regeneration Override:**  
  When **ChangeBaseEitrRegen** is enabled, the mod completely overrides the default regeneration rate and delay with
  your configured values.
- **Skill-Based Bonus:**  
  If **ScaleBaseEitrRegenBasedOnSkill** is enabled, a bonus—calculated using your current magic level—is added to the
  base regeneration rate.
- **Linear Adjustment:**  
  The mod further adjusts the regeneration rate based on your current Eitr percentage. At low Eitr, regeneration speeds
  up (if the multiplier is above 1), while at higher Eitr levels, it slows down.

3. **Outcome:**  
   As your magic skill increases, both your base Eitr and its regeneration rate improve according to the formula. This
   means that higher-level players will naturally recover Eitr faster, reflecting their enhanced magical proficiency.

---

`Feel free to reach out to me on discord if you need manual download assistance.`

# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>




Yes, that's a great idea. Instead of rebalancing the underlying math—which would affect both your maximum base Eitr and the regeneration bonus—you can simply scale down the bonus applied to regeneration. This approach lets you keep your base Eitr values as-is while reducing the regen boost so it doesn’t become instant at higher levels.

### What You Can Do

1. **Add a New Multiplier for Regen Only:**  
   Introduce a new config entry (for example, `RegenBonusMultiplier`) that scales the bonus from your magic skill when applied to regeneration. Then, in your regeneration patch, multiply the bonus by this factor.

2. **Code Change Example:**  
   In your `PlayerUpdateStatsPatch.Prefix`, modify the line where you add the bonus to the regeneration rate. Instead of:
   ```csharp
   newRegen += PlayerGetTotalFoodValuePatch.ChangeBaseEitr();
   ```
   you could do:
   ```csharp
   newRegen += PlayerGetTotalFoodValuePatch.ChangeBaseEitr() * MagicEitrBasePlugin.RegenBonusMultiplier.Value;
   ```

3. **Config Entry Addition:**  
   In your plugin’s config section (likely under "4 - Base Eitr Regen"), add:
   ```csharp
   RegenBonusMultiplier = config("4 - Base Eitr Regen", "Regen Bonus Multiplier", 0.5f, "Multiplier for the skill-based bonus when applied to Eitr regeneration. Lower values reduce the bonus to avoid near-instant regeneration at high skill levels.");
   ```
   This new multiplier allows you to fine-tune only the regeneration bonus without affecting the base Eitr calculation.

### Updated README and Config Descriptions

#### README (Math Breakdown Section – Revised)

Replace the section explaining how the bonus is applied with something like:

> **Math Breakdown for Regen Bonus:**  
> The bonus added to your regeneration rate is calculated with:
>
> ```csharp
> ( (Math.Pow(skillLevel / {Skill Divider}, {Power Amount})) * {Skill Scalar} ) * {Final Multiplier}
> ```
>
> **However, when applied to regeneration, this bonus is further multiplied by the "Regen Bonus Multiplier" setting.**  
> For example, if your bonus calculation returns 25 at level 50 and your `Regen Bonus Multiplier` is set to 0.5, the bonus added to your regeneration will be 12.5 units/second.
>
> This ensures that while your base Eitr scales nicely with your magic skill, the regeneration bonus remains balanced even at higher levels.

#### Config Descriptions (Updated)

- **Regen Bonus Multiplier:**  
  *"Multiplier for the bonus applied to the regeneration rate based on your magic skill. Use a value less than 1 to reduce the bonus, preventing near-instant regeneration at high levels without affecting the maximum base Eitr."*

### Summary

By adding a dedicated multiplier for regeneration, you isolate the balancing for regen from the base Eitr calculation. This change is easier to control and maintains the intended balance of your base Eitr while still rewarding higher magic skill with improved regeneration—just at a more moderated rate.

Would you like a full code snippet example with these changes incorporated?