﻿using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace MagicEitrBase;

[HarmonyPatch(typeof(Player), nameof(Player.Awake))]
public static class PlayerAwakePatch
{
    public static void Postfix(ref Player __instance)
    {
        UpdateBases(ref __instance);
    }

    internal static void UpdateBases(ref Player player)
    {
        if (Player.m_localPlayer == null)
            return;
        try
        {
            float eitr;
            player.GetTotalFoodValue(out float _, out float _, out eitr);
            player.SetMaxEitr(eitr, true);
        }
        catch
        {
            // ignored
        }
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
static class PlayerGetTotalFoodValuePatch
{
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldc_R4)
            {
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(PlayerGetTotalFoodValuePatch), nameof(ChangeBaseEitr)));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static float ChangeBaseEitr()
    {
        var holder = 0.0f;
        try
        {
            // If ElementalMagic skill is Higher than BloodMagic skill, use ElementalMagic skill
            if (Player.m_localPlayer.GetSkillLevel(Skills.SkillType.ElementalMagic) >
                Player.m_localPlayer.GetSkillLevel(Skills.SkillType.BloodMagic))
            {
                holder += (LevelUp((int)Player.m_localPlayer.GetSkillLevel(Skills.SkillType.ElementalMagic)) * MagicEitrBasePlugin.Final_Multiplier.Value);
            }

            if (Player.m_localPlayer.GetSkillLevel(Skills.SkillType.BloodMagic) >
                Player.m_localPlayer.GetSkillLevel(Skills.SkillType.ElementalMagic))
            {
                holder += (LevelUp((int)Player.m_localPlayer.GetSkillLevel(Skills.SkillType.BloodMagic)) * MagicEitrBasePlugin.Final_Multiplier.Value);
            }
            else if (Mathf.Abs(Player.m_localPlayer.GetSkillLevel(Skills.SkillType.BloodMagic) - Player.m_localPlayer.GetSkillLevel(Skills.SkillType.ElementalMagic)) < 0.1f)
            {
                holder += (LevelUp((int)Player.m_localPlayer.GetSkillLevel(Skills.SkillType.ElementalMagic)) * MagicEitrBasePlugin.Final_Multiplier.Value);
            }
        }
        catch
        {
            return holder;
        }

        return holder;
    }

    private static float LevelUp(int skillLevel)
    {
        return (Mathf.Pow(skillLevel / MagicEitrBasePlugin.Skill_Divider.Value, MagicEitrBasePlugin.Power_Amount.Value)) * MagicEitrBasePlugin.Skill_Scalar.Value;
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.OnSkillLevelup))]
static class PlayerGetBaseFoodHpPatch
{
    static void Prefix(Player __instance, Skills.SkillType skill, float level)
    {
        if (skill is Skills.SkillType.ElementalMagic or Skills.SkillType.BloodMagic)
        {
            PlayerAwakePatch.UpdateBases(ref __instance);
        }
    }
}

[HarmonyPatch(typeof(Skills), nameof(Skills.Awake))]
static class GameSpawnPlayerPatch
{
    static void Postfix(Game __instance)
    {
        PlayerAwakePatch.UpdateBases(ref Player.m_localPlayer);
    }
}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.TryRunCommand))]
static class TerminalTryRunCommandPatch
{
    static void Postfix(Terminal __instance, string text, bool silentFail = false, bool skipAllowedCheck = false)
    {
        if (text.ToLower().Contains("puke") || text.ToLower().Contains("skill"))
        {
            PlayerAwakePatch.UpdateBases(ref Player.m_localPlayer);
        }
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdateStats))]
static class PlayerUpdateStatsPatch
{
    private static float _m_eitrRegenTimeMultiplier;

    [HarmonyPriority(Priority.VeryLow)]
    static void Prefix(Player __instance, float dt)
    {
        if (__instance.InIntro() || __instance.IsTeleporting())
            return;

        if (MagicEitrBasePlugin.LinearRegeneration.Value == MagicEitrBasePlugin.Toggle.On && 0f < MagicEitrBasePlugin.LinearRegenerationThreshold.Value && MagicEitrBasePlugin.LinearRegenerationThreshold.Value < 1f && MagicEitrBasePlugin.LinearRegenerationMultiplier.Value > 0f && __instance.GetMaxEitr() != 0f)
        {
            if (__instance.GetEitrPercentage() < MagicEitrBasePlugin.LinearRegenerationThreshold.Value)
            {
                float t = Mathf.Clamp01(__instance.GetEitr() / (__instance.GetMaxEitr() * MagicEitrBasePlugin.LinearRegenerationThreshold.Value));
                __instance.m_eitrRegenTimer = Mathf.Lerp(MagicEitrBasePlugin.LinearRegenerationMultiplier.Value, __instance.m_staminaRegenTimeMultiplier, t);
            }
            else if (__instance.GetEitrPercentage() > MagicEitrBasePlugin.LinearRegenerationThreshold.Value)
            {
                float t = Mathf.Clamp01((__instance.GetMaxEitr() - __instance.GetEitr()) / (__instance.GetMaxEitr() * (1f - MagicEitrBasePlugin.LinearRegenerationThreshold.Value)));
                __instance.m_staminaRegenTimeMultiplier = Mathf.Lerp(1 / MagicEitrBasePlugin.LinearRegenerationMultiplier.Value, __instance.m_staminaRegenTimeMultiplier, t);
            }
        }
    }

    [HarmonyPriority(Priority.VeryHigh)]
    public static void Postfix(Player __instance)
    {
        if (__instance.m_staminaRegenTimeMultiplier != _m_eitrRegenTimeMultiplier && _m_eitrRegenTimeMultiplier != 0f)
            __instance.m_staminaRegenTimeMultiplier = _m_eitrRegenTimeMultiplier;
    }
}

