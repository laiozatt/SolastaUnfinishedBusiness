﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace SolastaCommunityExpansion.Patches.Respec
{
    // use this patch to enable the after rest actions
    [HarmonyPatch(typeof(RulesetCharacterHero), "EnumerateAfterRestActions")]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    internal static class RulesetCharacterHero_EnumerateAfterRestActions
    {
        internal static void Postfix(RulesetCharacterHero __instance)
        {
            if (Main.Settings.EnableRespec)
            {
                var characterLevel = __instance.GetAttribute(AttributeDefinitions.CharacterLevel).CurrentValue;

                if (characterLevel > 1)
                {
                    __instance.AfterRestActions.Add(Models.LevelDownContext.RestActivityLevelDownBuilder.RestActivityLevelDown);
                }

                __instance.AfterRestActions.Add(Models.RespecContext.RestActivityRespecBuilder.RestActivityRespec);
            }
        }
    }
}
