﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Models;

namespace SolastaUnfinishedBusiness.Patches;

internal static class SlotStatusTablePatcher
{
    [HarmonyPatch(typeof(SlotStatusTable), "Bind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class Bind_Patch
    {
        private static bool UniqueLevelSlots(FeatureDefinitionCastSpell featureDefinitionCastSpell,
            RulesetSpellRepertoire rulesetSpellRepertoire)
        {
            var hero = SharedSpellsContext.GetHero(rulesetSpellRepertoire.CharacterName);

            //PATCH: displays slots on any multicaster hero so Warlocks can see their spell slots
            return featureDefinitionCastSpell.UniqueLevelSlots &&
                   (hero == null || !SharedSpellsContext.IsMulticaster(hero));
        }

        internal static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            var uniqueLevelSlotsMethod = typeof(FeatureDefinitionCastSpell).GetMethod("get_UniqueLevelSlots");
            var myUniqueLevelSlotsMethod =
                new Func<FeatureDefinitionCastSpell, RulesetSpellRepertoire, bool>(UniqueLevelSlots).Method;

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(uniqueLevelSlotsMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, myUniqueLevelSlotsMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        //PATCH: creates different slots colors and pop up messages depending on slot types
        public static void Postfix(
            SlotStatusTable __instance,
            RulesetSpellRepertoire spellRepertoire,
            int spellLevel)
        {
            // spellRepertoire is null during level up...
            if (spellRepertoire == null || spellLevel == 0)
            {
                return;
            }

            var heroWithSpellRepertoire = SharedSpellsContext.GetHero(spellRepertoire.CharacterName);

            if (heroWithSpellRepertoire is null)
            {
                return;
            }

            spellRepertoire.GetSlotsNumber(spellLevel, out var totalSlotsRemainingCount, out var totalSlotsCount);

            MulticlassGameUiContext.PaintPactSlots(
                heroWithSpellRepertoire, totalSlotsCount, totalSlotsRemainingCount, spellLevel, __instance.table, true);
        }
    }

    //PATCH: ensures slot colors are white before getting back to pool
    [HarmonyPatch(typeof(SlotStatusTable), "Unbind")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class Unbind_Patch
    {
        public static void Prefix(SlotStatusTable __instance)
        {
            MulticlassGameUiContext.PaintSlotsWhite(__instance.table);
        }
    }
}
