﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Models;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

internal delegate bool IsWeaponValidHandler(RulesetAttackMode attackMode, RulesetItem weapon,
    RulesetCharacter character);

internal static class ValidatorsWeapon
{
    internal static readonly IsWeaponValidHandler AlwaysValid = (_, _, _) => true;

    internal static IsWeaponValidHandler IsOfDamageType(string damageType)
    {
        return (mode, _, _) => mode?.EffectDescription.FindFirstDamageForm()?.DamageType == damageType;
    }

    internal static IsWeaponValidHandler IsOfWeaponType(params WeaponTypeDefinition[] weaponTypeDefinitions)
    {
        return (mode, weapon, _) => IsWeaponType(weapon ?? mode?.sourceObject as RulesetItem, weaponTypeDefinitions);
    }

    internal static bool IsMagic(RulesetAttackMode attackMode, RulesetItem weapon, RulesetCharacter _)
    {
        if (attackMode.Magical)
        {
            return true;
        }

        if (weapon == null)
        {
            return false;
        }

        return weapon.IsMagicalWeapon() || ShieldAttack.IsMagicShield(weapon);
    }

    internal static bool IsMelee([CanBeNull] ItemDefinition itemDefinition)
    {
        return itemDefinition != null &&
               (itemDefinition.WeaponDescription?.WeaponTypeDefinition.WeaponProximity ==
                   AttackProximity.Melee || itemDefinition.IsArmor /* for shields */);
    }

    internal static bool IsMelee([CanBeNull] RulesetItem weapon)
    {
        return weapon != null && IsMelee(weapon.ItemDefinition);
    }

    internal static bool IsMelee([CanBeNull] RulesetAttackMode attack)
    {
        return attack is { SourceDefinition: ItemDefinition itemDefinition } && IsMelee(itemDefinition);
    }

    internal static bool IsOneHanded(RulesetAttackMode attackMode)
    {
        if (attackMode is not { SourceDefinition: ItemDefinition weapon })
        {
            return false;
        }

        return !HasAnyWeaponTag(weapon, TagsDefinitions.WeaponTagTwoHanded);
    }

    internal static bool IsOneHanded(RulesetItem weapon)
    {
        return !HasAnyWeaponTag(weapon, TagsDefinitions.WeaponTagTwoHanded);
    }

    internal static bool IsTwoHanded([CanBeNull] RulesetItem weapon)
    {
        return HasAnyWeaponTag(weapon, TagsDefinitions.WeaponTagTwoHanded);
    }

    internal static bool IsPolearm([CanBeNull] RulesetItem weapon)
    {
        return weapon != null
               && IsPolearm(weapon.ItemDefinition);
    }

    internal static bool IsPolearm([CanBeNull] ItemDefinition weapon)
    {
        return weapon != null
               && weapon.IsWeapon
               && CustomWeaponsContext.PolearmWeaponTypes.Contains(weapon.WeaponDescription?.WeaponType);
    }

    internal static bool IsWeaponType([CanBeNull] RulesetItem item, params WeaponTypeDefinition[] weaponTypeDefinitions)
    {
        return item != null
               && item.ItemDefinition != null
               && item.ItemDefinition.IsWeapon
               && weaponTypeDefinitions.Contains(item.ItemDefinition.WeaponDescription.WeaponTypeDefinition);
    }

    internal static bool IsRanged(RulesetItem weapon)
    {
        return HasAnyWeaponTag(weapon, TagsDefinitions.WeaponTagRange, TagsDefinitions.WeaponTagThrown);
    }

#if false
    internal static bool IsRanged([CanBeNull] RulesetAttackMode attack)
    {
        return attack is { Reach: false, Ranged: true } or { Reach: false, Thrown: true };
    }
#endif

    internal static bool IsThrownWeapon([CanBeNull] RulesetItem weapon)
    {
        return weapon != null && weapon.itemDefinition.isWeapon &&
               weapon.itemDefinition.WeaponDescription.WeaponTags.Contains(TagsDefinitions.WeaponTagThrown);
    }

    //
    // UNARMED
    //

    private static bool IsUnarmedWeapon(
        [CanBeNull] RulesetAttackMode attackMode,
        RulesetItem weapon)
    {
        var item = attackMode?.SourceDefinition as ItemDefinition ?? weapon?.ItemDefinition;

        if (item != null)
        {
            return item.WeaponDescription?.WeaponTypeDefinition ==
                   DatabaseHelper.WeaponTypeDefinitions.UnarmedStrikeType;
        }

        return weapon == null;
    }

    internal static bool IsUnarmedWeapon(RulesetCharacter rulesetCharacter, RulesetAttackMode attackMode)
    {
        return rulesetCharacter is RulesetCharacterMonster || IsUnarmedWeapon(attackMode, null);
    }

    internal static bool IsUnarmedWeapon(RulesetItem weapon)
    {
        return IsUnarmedWeapon(null, weapon);
    }

    //
    // TAGS
    //

    internal static bool HasAnyWeaponTag([CanBeNull] RulesetItem item, [NotNull] params string[] tags)
    {
        return HasAnyWeaponTag(item?.ItemDefinition, tags);
    }

    internal static bool HasAnyWeaponTag(ItemDefinition item, [NotNull] params string[] tags)
    {
        var weaponTags = GetWeaponTags(item);

        return tags.Any(t => weaponTags.Contains(t));
    }

    private static List<string> GetWeaponTags([CanBeNull] ItemDefinition item)
    {
        if (item != null && item.IsWeapon)
        {
            return item.WeaponDescription.WeaponTags;
        }

        return new List<string>();
    }

#if false
    internal static bool IsReactionAttackMode(
        RulesetAttackMode attackMode,
        RulesetItem weapon,
        RulesetCharacter character)
    {
        return attackMode is {ActionType: ActionDefinitions.ActionType.Reaction};
    }

    internal static bool HasAnyTag(RulesetItem item, params string[] tags)
    {
        var tagsMap = new Dictionary<string, TagsDefinitions.Criticity>();

        item?.FillTags(tagsMap, null, true);

        return tagsMap.Keys.Any(tags.Contains);
    }
    
    private static bool HasActiveTag(RulesetAttackMode mode, RulesetItem weapon, string tag)
    {
        var hasTag = false;

        if (mode != null)
        {
            hasTag = mode.AttackTags.Contains(tag);

            if (!hasTag)
            {
                var tags = GetWeaponTags(mode.SourceDefinition as ItemDefinition);

                if (tags != null && tags.Contains(tag))
                {
                    hasTag = true;
                }
            }
    
            return hasTag;
        }
    
        if (weapon != null)
        {
            var tags = GetWeaponTags(weapon.ItemDefinition);

            if (tags != null && tags.Contains(tag))
            {
                hasTag = true;
            }
        }

        return hasTag;
    }
#endif
}
