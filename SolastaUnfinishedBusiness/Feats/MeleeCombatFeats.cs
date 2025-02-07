﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Validators;
using static RuleDefinitions;
using static FeatureDefinitionAttributeModifier;
using static RuleDefinitions.RollContext;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionActionAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAttributeModifiers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.WeaponTypeDefinitions;
using static SolastaUnfinishedBusiness.Feats.FeatHelpers;

namespace SolastaUnfinishedBusiness.Feats;

internal static class MeleeCombatFeats
{
    internal static FeatDefinition FeatFencer;

    internal static void CreateFeats([NotNull] List<FeatDefinition> feats)
    {
        var featAlwaysReady = BuildAlwaysReady();
        var featBladeMastery = BuildBladeMastery();
        var featCleavingAttack = BuildCleavingAttack();
        var featCrusherStr = BuildCrusherStr();
        var featCrusherCon = BuildCrusherCon();
        var featDefensiveDuelist = BuildDefensiveDuelist();
        var featDevastatingStrikes = BuildDevastatingStrikes();
        var featFellHanded = BuildFellHanded();
        FeatFencer = BuildFencer();
        var featHammerThePoint = BuildHammerThePoint();
        var featLongSwordFinesse = BuildLongswordFinesse();
        var featOldTacticsDex = BuildOldTacticsDex();
        var featOldTacticsStr = BuildOldTacticsStr();
        var featPiercerDex = BuildPiercerDex();
        var featPiercerStr = BuildPiercerStr();
        var featPowerAttack = BuildPowerAttack();
        var featRecklessAttack = BuildRecklessAttack();
        var featSavageAttack = BuildSavageAttack();
        var featSlasherStr = BuildSlasherStr();
        var featSlasherDex = BuildSlasherDex();
        var featSpearMastery = BuildSpearMastery();

        feats.AddRange(
            featAlwaysReady,
            featBladeMastery,
            featCleavingAttack,
            featCrusherCon,
            featCrusherStr,
            featDefensiveDuelist,
            featDevastatingStrikes,
            featFellHanded,
            FeatFencer,
            featHammerThePoint,
            featLongSwordFinesse,
            featOldTacticsDex,
            featOldTacticsStr,
            featPiercerDex,
            featPiercerStr,
            featPowerAttack,
            featRecklessAttack,
            featSavageAttack,
            featSlasherDex,
            featSlasherStr,
            featSpearMastery);

        var featGroupCrusher = GroupFeats.MakeGroup("FeatGroupCrusher", GroupFeats.Crusher,
            featCrusherStr,
            featCrusherCon);

        var featGroupOldTactics = GroupFeats.MakeGroup("FeatGroupOldTactics", GroupFeats.OldTactics,
            featOldTacticsDex,
            featOldTacticsStr);

        var featGroupSlasher = GroupFeats.MakeGroup("FeatGroupSlasher", GroupFeats.Slasher,
            featSlasherDex,
            featSlasherStr);

        GroupFeats.FeatGroupDefenseCombat.AddFeats(
            featAlwaysReady,
            featDefensiveDuelist);

        GroupFeats.FeatGroupPiercer.AddFeats(
            featPiercerDex,
            featPiercerStr);

        GroupFeats.FeatGroupUnarmoredCombat.AddFeats(
            featGroupCrusher);

        GroupFeats.MakeGroup("FeatGroupMeleeCombat", null,
            GroupFeats.FeatGroupElementalTouch,
            GroupFeats.FeatGroupPiercer,
            FeatDefinitions.DauntingPush,
            FeatDefinitions.DistractingGambit,
            FeatDefinitions.TripAttack,
            featAlwaysReady,
            featBladeMastery,
            featCleavingAttack,
            featDefensiveDuelist,
            featDevastatingStrikes,
            featFellHanded,
            FeatFencer,
            featHammerThePoint,
            featLongSwordFinesse,
            featPowerAttack,
            featRecklessAttack,
            featSavageAttack,
            featSpearMastery,
            featGroupCrusher,
            featGroupOldTactics,
            featGroupSlasher);
    }

    #region Reckless Attack

    private static FeatDefinitionWithPrerequisites BuildRecklessAttack()
    {
        return FeatDefinitionWithPrerequisitesBuilder
            .Create("FeatRecklessAttack")
            .SetGuiPresentation("RecklessAttack", Category.Action)
            .SetFeatures(ActionAffinityBarbarianRecklessAttack)
            .SetValidators(ValidatorsFeat.ValidateNotClass(CharacterClassDefinitions.Barbarian))
            .AddToDB();
    }

    #endregion

    #region Savage Attack

    private static FeatDefinition BuildSavageAttack()
    {
        return FeatDefinitionBuilder
            .Create("FeatSavageAttack")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionDieRollModifierBuilder
                    .Create("DieRollModifierFeatSavageAttackNonMagic")
                    .SetGuiPresentationNoContent(true)
                    .SetModifiers(AttackDamageValueRoll, 1, 1, 1, "Feat/&FeatSavageAttackReroll")
                    .AddToDB(),
                FeatureDefinitionDieRollModifierBuilder
                    .Create("DieRollModifierFeatSavageAttackMagic")
                    .SetGuiPresentationNoContent(true)
                    .SetModifiers(MagicDamageValueRoll, 1, 1, 1, "Feat/&FeatSavageAttackReroll")
                    .AddToDB())
            .AddToDB();
    }

    #endregion

    #region Spear Mastery

    private static FeatDefinition BuildSpearMastery()
    {
        const string NAME = "FeatSpearMastery";
        const string REACH_CONDITION = $"Condition{NAME}Reach";

        var validWeapon = ValidatorsWeapon.IsOfWeaponTypeWithoutAttackTag("Polearm", SpearType);

        var conditionFeatSpearMasteryReach = ConditionDefinitionBuilder
            .Create(REACH_CONDITION)
            .SetGuiPresentation($"Power{NAME}Reach", Category.Feature, ConditionDefinitions.ConditionGuided)
            .SetPossessive()
            .AddCustomSubFeatures(
                new IncreaseWeaponReach(1, validWeapon, ValidatorsCharacter.HasAnyOfConditions(REACH_CONDITION)))
            .AddToDB();

        var powerFeatSpearMasteryReach = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}Reach")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("SpearMasteryReach", Resources.SpearMasteryReach, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetParticleEffectParameters(SpellDefinitions.Shield)
                    .SetEffectForms(EffectFormBuilder.ConditionForm(conditionFeatSpearMasteryReach))
                    .UseQuickAnimations()
                    .Build())
            .AddToDB();

        var conditionDamage = ConditionDefinitionBuilder
            .Create($"Condition{NAME}Damage")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(
                FeatureDefinitionAdditionalDamageBuilder
                    .Create($"AdditionalDamage{NAME}")
                    .SetGuiPresentationNoContent(true)
                    .SetNotificationTag("SpearMastery")
                    .SetDamageValueDetermination(AdditionalDamageValueDetermination.SameAsBaseWeaponDie)
                    //Adding any property so that custom restricted context would trigger
                    .SetRequiredProperty(RestrictedContextRequiredProperty.Weapon)
                    .AddCustomSubFeatures(new ValidateContextInsteadOfRestrictedProperty(
                        (_, _, character, _, ranged, mode, _) =>
                            (OperationType.Set, !ranged && validWeapon(mode, null, character))))
                    .SetIgnoreCriticalDoubleDice(true)
                    .AddToDB())
            .AddToDB();

        var conditionFeatSpearMasteryCharge = ConditionDefinitionBuilder
            .Create($"Condition{NAME}Charge")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionGuided)
            .SetPossessive()
            .AddCustomSubFeatures(new CanMakeAoOOnReachEntered
            {
                AllowRange = false,
                AccountAoOImmunity = true,
                WeaponValidator = validWeapon,
                BeforeReaction = AddCondition,
                AfterReaction = RemoveCondition
            })
            .AddToDB();

        var powerFeatSpearMasteryCharge = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}Charge")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite($"Power{NAME}Charge", Resources.SpearMasteryCharge, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 1, TurnOccurenceType.StartOfTurn)
                    .SetTargetingData(Side.Ally, RangeType.Self, 1, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionFeatSpearMasteryCharge,
                                ConditionForm.ConditionOperation.Add, true)
                            .Build())
                    .UseQuickAnimations()
                    .Build())
            .AddToDB();

        return FeatDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                powerFeatSpearMasteryReach,
                powerFeatSpearMasteryCharge,
                FeatureDefinitionAttackModifierBuilder
                    .Create($"AttackModifier{NAME}")
                    .SetGuiPresentation(Category.Feature)
                    .SetAttackRollModifier(1)
                    .SetRequiredProperty(RestrictedContextRequiredProperty.MeleeWeapon)
                    .AddCustomSubFeatures(
                        new ValidateContextInsteadOfRestrictedProperty((_, _, character, _, ranged, mode, _) =>
                            (OperationType.Set, !ranged && validWeapon(mode, null, character))),
                        new UpgradeWeaponDice((_, damage) => (damage.diceNumber, DieType.D8, DieType.D10), validWeapon))
                    .AddToDB())
            .AddToDB();

        IEnumerator AddCondition(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationBattleManager manager,
            GameLocationActionManager actionManager,
            ReactionRequest request)
        {
            var rulesetCharacter = attacker.RulesetCharacter;

            rulesetCharacter.InflictCondition(
                conditionDamage.Name,
                DurationType.Round,
                0,
                TurnOccurenceType.EndOfTurn,
                AttributeDefinitions.TagEffect,
                rulesetCharacter.guid,
                rulesetCharacter.CurrentFaction.Name,
                1,
                conditionDamage.Name,
                0,
                0,
                0);

            yield break;
        }

        IEnumerator RemoveCondition(GameLocationCharacter attacker, GameLocationCharacter defender,
            GameLocationBattleManager manager, GameLocationActionManager actionManager, ReactionRequest request)
        {
            attacker.RulesetCharacter.RemoveAllConditionsOfCategoryAndType(
                AttributeDefinitions.TagEffect, conditionDamage.Name);

            yield break;
        }
    }

    #endregion

    #region Longsword Finesse

    private static FeatDefinitionWithPrerequisites BuildLongswordFinesse()
    {
        const string Name = "FeatLongswordFinesse";

        var validWeapon = ValidatorsWeapon.IsOfWeaponType(LongswordType);

        var attributeModifierArmorClass = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}ArmorClass")
            .SetGuiPresentation(Category.Feature)
            .SetModifier(AttributeModifierOperation.Additive,
                AttributeDefinitions.ArmorClass, 1)
            .SetSituationalContext(ExtraSituationalContext.HasLongswordInHands)
            .AddCustomSubFeatures(
                new AddTagToWeapon(TagsDefinitions.WeaponTagFinesse, TagsDefinitions.Criticity.Important, validWeapon))
            .AddToDB();

        return FeatDefinitionWithPrerequisitesBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                AttributeModifierCreed_Of_Misaye,
                attributeModifierArmorClass)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Dexterity, 13)
            .AddToDB();
    }

    #endregion

    #region Fencer

    private static FeatDefinition BuildFencer()
    {
        const string NAME = "FeatFencer";

        return FeatDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feat)
            .AddCustomSubFeatures(
                new AddExtraMainHandAttack(
                    ActionDefinitions.ActionType.Bonus,
                    ValidatorsCharacter.HasAttacked,
                    ValidatorsCharacter.HasFreeHandWithoutTwoHandedInMain,
                    ValidatorsCharacter.HasMeleeWeaponInMainHand))
            .AddToDB();
    }

    #endregion

    #region Defensive Duelist

    private static FeatDefinition BuildDefensiveDuelist()
    {
        const string NAME = "FeatDefensiveDuelist";

        var conditionDefensiveDuelist = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation(NAME, Category.Feat)
            .SetPossessive()
            .SetFeatures(
                FeatureDefinitionAttributeModifierBuilder
                    .Create($"AttributeModifier{NAME}")
                    .SetGuiPresentationNoContent(true)
                    .SetModifier(
                        AttributeModifierOperation.AddProficiencyBonus,
                        AttributeDefinitions.ArmorClass)
                    .AddToDB())
            .SetSpecialInterruptions(ExtraConditionInterruption.AfterWasAttacked)
            .AddToDB();

        var powerDefensiveDuelist = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}")
            .SetGuiPresentation(NAME, Category.Feat)
            .SetUsesFixed(ActivationTime.Reaction)
            .SetReactionContext(ExtraReactionContext.Custom)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(EffectFormBuilder.ConditionForm(conditionDefensiveDuelist))
                    .Build())
            .AddToDB();

        powerDefensiveDuelist.AddCustomSubFeatures(new SpiritualShieldingBlockAttack(powerDefensiveDuelist));

        return FeatDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(powerDefensiveDuelist)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Dexterity, 13)
            .AddToDB();
    }

    private class SpiritualShieldingBlockAttack(FeatureDefinitionPower powerDefensiveDuelist)
        : IAttackBeforeHitPossibleOnMeOrAlly
    {
        public IEnumerator OnAttackBeforeHitPossibleOnMeOrAlly(GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            ActionModifier actionModifier,
            RulesetAttackMode attackMode,
            RulesetEffect rulesetEffect,
            int attackRoll)
        {
            if (rulesetEffect != null &&
                rulesetEffect.EffectDescription.RangeType is not (RangeType.Touch or RangeType.MeleeHit))
            {
                yield break;
            }

            var rulesetDefender = defender.RulesetCharacter;

            if (!helper.CanReact() ||
                helper != defender ||
                !ValidatorsWeapon.IsMelee(attackMode) ||
                !ValidatorsWeapon.HasAnyWeaponTag(rulesetDefender.GetMainWeapon(), TagsDefinitions.WeaponTagFinesse))
            {
                yield break;
            }

            var totalAttack = attackRoll
                              + (attackMode?.ToHitBonus ?? rulesetEffect?.MagicAttackBonus ?? 0)
                              + actionModifier.AttackRollModifier;
            var armorClass = rulesetDefender.RefreshArmorClass(true).CurrentValue;
            var requiredACAddition = totalAttack - armorClass + 1;
            var pb = rulesetDefender.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);

            // if other actions already blocked it or if pb isn't enough
            if (requiredACAddition <= 0 || requiredACAddition > pb)
            {
                yield break;
            }

            var gameLocationActionManager =
                ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;

            if (gameLocationActionManager == null)
            {
                yield break;
            }

            var implementationManagerService =
                ServiceRepository.GetService<IRulesetImplementationService>() as RulesetImplementationManager;

            var usablePower = PowerProvider.Get(powerDefensiveDuelist, rulesetDefender);
            var actionParams =
                new CharacterActionParams(helper, ActionDefinitions.Id.PowerReaction)
                {
                    StringParameter = "DefensiveDuelist",
                    ActionModifiers = { new ActionModifier() },
                    RulesetEffect = implementationManagerService
                        .MyInstantiateEffectPower(rulesetDefender, usablePower, false),
                    UsablePower = usablePower,
                    TargetCharacters = { helper }
                };

            var count = gameLocationActionManager.PendingReactionRequestGroups.Count;

            gameLocationActionManager.ReactToUsePower(actionParams, "UsePower", helper);

            yield return battleManager.WaitForReactions(helper, gameLocationActionManager, count);
        }
    }

    #endregion

    #region Hammer the Point

    private static FeatDefinition BuildHammerThePoint()
    {
        const string Name = "FeatHammerThePoint";

        var conditionHammerThePoint = ConditionDefinitionBuilder
            .Create($"Condition{Name}HammerThePoint")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetSpecialDuration(DurationType.Round, 0, TurnOccurenceType.EndOfSourceTurn)
            .AllowMultipleInstances()
            .AddToDB();

        var additionalDamageHammerThePoint = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}HammerThePoint")
            .SetGuiPresentationNoContent(true)
            .SetAttackModeOnly()
            .AddConditionOperation(ConditionOperationDescription.ConditionOperation.Add, conditionHammerThePoint)
            .AddToDB();

        var featHammerThePoint = FeatDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(additionalDamageHammerThePoint)
            .AddToDB();

        additionalDamageHammerThePoint.AddCustomSubFeatures(
            new PhysicalAttackInitiatedByMeFeatHammerThePoint(conditionHammerThePoint, featHammerThePoint));

        return featHammerThePoint;
    }

    private sealed class PhysicalAttackInitiatedByMeFeatHammerThePoint(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ConditionDefinition conditionHammerThePoint,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        FeatDefinition featHammerThePoint)
        : IPhysicalAttackInitiatedByMe
    {
        public IEnumerator OnPhysicalAttackInitiatedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode)
        {
            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            var attackedThisTurnCount = rulesetDefender.AllConditions
                .Count(x => x.ConditionDefinition == conditionHammerThePoint);

            if (attackedThisTurnCount == 0)
            {
                yield break;
            }

            var trendInfo = new TrendInfo(
                attackedThisTurnCount, FeatureSourceType.Feat, featHammerThePoint.Name, featHammerThePoint);

            attackModifier.AttackRollModifier += attackedThisTurnCount;
            attackModifier.AttacktoHitTrends.Add(trendInfo);

            var damage = attackMode?.EffectDescription.FindFirstDamageForm();

            if (damage == null)
            {
                yield break;
            }

            damage.BonusDamage += attackedThisTurnCount;
            damage.DamageBonusTrends.Add(trendInfo);
        }
    }

    #endregion

    #region Old Tactics

    private static FeatDefinition BuildOldTacticsStr()
    {
        const string Name = "FeatOldTacticsStr";

        return FeatDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(AttributeModifierCreed_Of_Einar)
            .AddCustomSubFeatures(new ActionFinishedByEnemyOldTactics())
            .SetFeatFamily(GroupFeats.OldTactics)
            .AddToDB();
    }

    private static FeatDefinition BuildOldTacticsDex()
    {
        const string Name = "FeatOldTacticsDex";

        return FeatDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(AttributeModifierCreed_Of_Misaye)
            .AddCustomSubFeatures(new ActionFinishedByEnemyOldTactics())
            .SetFeatFamily(GroupFeats.OldTactics)
            .AddToDB();
    }

    private sealed class ActionFinishedByEnemyOldTactics : IActionFinishedByEnemy
    {
        public IEnumerator OnActionFinishedByEnemy(CharacterAction characterAction, GameLocationCharacter target)
        {
            var gameLocationActionService =
                ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;
            var gameLocationBattleService =
                ServiceRepository.GetService<IGameLocationBattleService>() as GameLocationBattleManager;

            if (gameLocationActionService == null || gameLocationBattleService is not { IsBattleInProgress: true })
            {
                yield break;
            }

            if (characterAction.ActionId != ActionDefinitions.Id.StandUp)
            {
                yield break;
            }

            if (target.IsMyTurn() ||
                !target.CanReact())
            {
                yield break;
            }

            var enemy = characterAction.ActingCharacter;

            if (!target.IsWithinRange(enemy, 1))
            {
                yield break;
            }

            var (retaliationMode, retaliationModifier) = target.GetFirstMeleeModeThatCanAttack(enemy);

            if (retaliationMode == null)
            {
                (retaliationMode, retaliationModifier) = target.GetFirstRangedModeThatCanAttack(enemy);

                if (retaliationMode == null)
                {
                    yield break;
                }
            }

            retaliationMode.AddAttackTagAsNeeded(AttacksOfOpportunity.NotAoOTag);

            var actionParams = new CharacterActionParams(target, ActionDefinitions.Id.AttackOpportunity)
            {
                StringParameter = target.Name,
                ActionModifiers = { retaliationModifier },
                AttackMode = retaliationMode,
                TargetCharacters = { enemy }
            };

            var count = gameLocationActionService.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestReactionAttack("OldTactics", actionParams);

            gameLocationActionService.AddInterruptRequest(reactionRequest);

            yield return gameLocationBattleService.WaitForReactions(target, gameLocationActionService, count);
        }
    }

    #endregion

    #region Always Ready

    private static FeatDefinition BuildAlwaysReady()
    {
        var conditionAlwaysReady = ConditionDefinitionBuilder
            .Create("ConditionAlwaysReady")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddToDB();

        var featureAlwaysReady = FeatureDefinitionBuilder
            .Create("FeatureAlwaysReady")
            .SetGuiPresentation("FeatAlwaysReady", Category.Feat)
            .AddToDB();

        featureAlwaysReady.AddCustomSubFeatures(
            new CustomBehaviorAlwaysReady(conditionAlwaysReady, featureAlwaysReady));

        return FeatDefinitionBuilder
            .Create("FeatAlwaysReady")
            .SetGuiPresentation(Category.Feat)
            .AddFeatures(featureAlwaysReady)
            .AddToDB();
    }

    private sealed class CustomBehaviorAlwaysReady(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ConditionDefinition conditionDefinition,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        FeatureDefinition featureDefinition)
        : IPhysicalAttackFinishedByMe, ICharacterTurnEndListener
    {
        public void OnCharacterTurnEnded(GameLocationCharacter locationCharacter)
        {
            var rulesetCharacter = locationCharacter.RulesetCharacter;

            if (rulesetCharacter is not { IsDeadOrDyingOrUnconscious: false } ||
                !rulesetCharacter.HasAnyConditionOfType(conditionDefinition.Name))
            {
                return;
            }

            rulesetCharacter.LogCharacterUsedFeature(featureDefinition);
            locationCharacter.ReadiedAction = ActionDefinitions.ReadyActionType.Melee;
        }

        public IEnumerator OnPhysicalAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            var rulesetCharacter = attacker.RulesetCharacter;

            if (rollOutcome is RollOutcome.Success or RollOutcome.CriticalSuccess ||
                (!ValidatorsWeapon.IsMelee(attackMode) && !ValidatorsWeapon.IsUnarmed(attackMode)))
            {
                yield break;
            }

            rulesetCharacter.InflictCondition(
                conditionDefinition.Name,
                DurationType.Round,
                0,
                TurnOccurenceType.StartOfTurn,
                AttributeDefinitions.TagEffect,
                rulesetCharacter.guid,
                rulesetCharacter.CurrentFaction.Name,
                1,
                conditionDefinition.Name,
                0,
                0,
                0);
        }
    }

    #endregion

    #region Blade Mastery

    private static FeatDefinition BuildBladeMastery()
    {
        const string NAME = "FeatBladeMastery";

        var weaponTypes = new[] { DaggerType, ShortswordType, LongswordType, ScimitarType, RapierType, GreatswordType };

        var feat = FeatDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                FeatureDefinitionAttributeModifierBuilder
                    .Create($"AttributeModifier{NAME}")
                    .SetGuiPresentation(NAME, Category.Feat)
                    .SetModifier(
                        AttributeModifierOperation.Additive,
                        AttributeDefinitions.ArmorClass, 1)
                    .SetSituationalContext(ExtraSituationalContext.HasBladeMasteryWeaponTypesInHands)
                    .AddToDB())
            .AddToDB();

        feat.AddCustomSubFeatures(
            new AttackComputeModifierFeatBladeMastery(feat, weaponTypes),
            new ModifyWeaponAttackModeTypeFilter(feat, weaponTypes));

        return feat;
    }

    private sealed class AttackComputeModifierFeatBladeMastery(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        FeatDefinition featDefinition,
        params WeaponTypeDefinition[] weaponTypeDefinition)
        : IPhysicalAttackInitiatedByMe
    {
        public IEnumerator OnPhysicalAttackInitiatedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode)
        {
            if ((action.ActionId == ActionDefinitions.Id.SwiftRetaliation ||
                 action.ActionType == ActionDefinitions.ActionType.Reaction) &&
                ValidatorsWeapon.IsOfWeaponType(weaponTypeDefinition)(attackMode, null, null))
            {
                attackModifier.attackAdvantageTrends.Add(
                    new TrendInfo(1, FeatureSourceType.Feat, featDefinition.Name, featDefinition));
            }

            yield break;
        }
    }

    #endregion

    #region Cleaving Attack

    private static FeatDefinition BuildCleavingAttack()
    {
        const string Name = "FeatCleavingAttack";

        var concentrationProvider = new StopPowerConcentrationProvider(
            Name,
            "Tooltip/&CleavingAttackConcentration",
            Sprites.GetSprite(nameof(Resources.PowerAttackConcentrationIcon), Resources.PowerAttackConcentrationIcon,
                64, 64));

        var conditionCleavingAttackFinish = ConditionDefinitionBuilder
            .Create($"Condition{Name}Finish")
            .SetGuiPresentation(Category.Condition)
            .SetPossessive()
            .SetFeatures(
                FeatureDefinitionBuilder
                    .Create($"Feature{Name}Finish")
                    .SetGuiPresentation($"Condition{Name}Finish", Category.Condition, Gui.NoLocalization)
                    .AddCustomSubFeatures(
                        ValidateAdditionalActionAttack.MeleeOnly,
                        new AddExtraMainHandAttack(ActionDefinitions.ActionType.Bonus))
                    .AddToDB())
            .AddToDB();

        var conditionCleavingAttack = ConditionDefinitionBuilder
            .Create($"Condition{Name}")
            .SetGuiPresentation(Name, Category.Feat, ConditionDefinitions.ConditionHeraldOfBattle)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddToDB();

        var powerCleavingAttack = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}")
            .SetGuiPresentation(Name, Category.Feat,
                Sprites.GetSprite(nameof(Resources.PowerAttackIcon), Resources.PowerAttackIcon, 128, 64))
            .SetUsesFixed(ActivationTime.NoCost)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetDurationData(DurationType.Permanent)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionCleavingAttack, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(
                IgnoreInvisibilityInterruptionCheck.Marker,
                new ValidatorsValidatePowerUse(
                    ValidatorsCharacter.HasNoneOfConditions(conditionCleavingAttack.Name)))
            .AddToDB();

        var powerTurnOffCleavingAttack = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}TurnOff")
            .SetGuiPresentationNoContent(true)
            .SetUsesFixed(ActivationTime.NoCost)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetDurationData(DurationType.Round, 1)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionCleavingAttack, ConditionForm.ConditionOperation.Remove)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(IgnoreInvisibilityInterruptionCheck.Marker)
            .AddToDB();

        var featCleavingAttack = FeatDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                powerCleavingAttack,
                powerTurnOffCleavingAttack,
                FeatureDefinitionBuilder
                    .Create($"Feature{Name}")
                    .SetGuiPresentationNoContent(true)
                    .AddCustomSubFeatures(new CustomBehaviorCleaving(conditionCleavingAttackFinish))
                    .AddToDB())
            .AddToDB();

        concentrationProvider.StopPower = powerTurnOffCleavingAttack;
        conditionCleavingAttack
            .AddCustomSubFeatures(
                concentrationProvider,
                new ModifyWeaponAttackModeFeatCleavingAttack(featCleavingAttack));

        return featCleavingAttack;
    }

    private sealed class CustomBehaviorCleaving(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ConditionDefinition conditionCleavingAttackFinish) : IOnReducedToZeroHpByMe, IPhysicalAttackFinishedByMe
    {
        public IEnumerator HandleReducedToZeroHpByMe(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            if (!ValidateCleavingAttack(attackMode))
            {
                yield break;
            }

            InflictCondition(attacker.RulesetCharacter);
        }

        public IEnumerator OnPhysicalAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            if (rollOutcome != RollOutcome.CriticalSuccess ||
                !ValidateCleavingAttack(attackMode))
            {
                yield break;
            }

            InflictCondition(attacker.RulesetCharacter);
        }

        private void InflictCondition(RulesetCharacter rulesetCharacter)
        {
            rulesetCharacter.InflictCondition(
                conditionCleavingAttackFinish.Name,
                DurationType.Round,
                0,
                TurnOccurenceType.EndOfTurn,
                AttributeDefinitions.TagEffect,
                rulesetCharacter.guid,
                rulesetCharacter.CurrentFaction.Name,
                1,
                conditionCleavingAttackFinish.Name,
                0,
                0,
                0);
        }
    }

    private sealed class ModifyWeaponAttackModeFeatCleavingAttack(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        FeatDefinition featDefinition)
        : IModifyWeaponAttackMode
    {
        private const int ToHit = -5;
        private const int ToDamage = +10;

        public void ModifyAttackMode(RulesetCharacter character, RulesetAttackMode attackMode)
        {
            if (!ValidateCleavingAttack(attackMode, true))
            {
                return;
            }

            attackMode.ToHitBonus += ToHit;
            attackMode.ToHitBonusTrends.Add(new TrendInfo(ToHit, FeatureSourceType.Feat,
                featDefinition.Name, featDefinition));
            var damage = attackMode.EffectDescription.FindFirstDamageForm();

            if (damage == null)
            {
                return;
            }

            damage.BonusDamage += ToDamage;
            damage.DamageBonusTrends.Add(new TrendInfo(ToDamage, FeatureSourceType.Feat,
                featDefinition.Name, featDefinition));
        }
    }

    private static bool ValidateCleavingAttack(RulesetAttackMode attackMode, bool validateHeavy = false)
    {
        return ValidatorsWeapon.IsMelee(attackMode) &&
               (!validateHeavy ||
                ValidatorsWeapon.HasAnyWeaponTag(
                    attackMode.SourceDefinition as ItemDefinition, TagsDefinitions.WeaponTagHeavy));
    }

    #endregion

    #region Crusher

    private static readonly FeatureDefinition FeatureFeatCrusher = FeatureDefinitionBuilder
        .Create("FeatureFeatCrusher")
        .SetGuiPresentationNoContent(true)
        .AddCustomSubFeatures(new PhysicalAttackFinishedByMeCrusher(
            ConditionDefinitionBuilder
                .Create("ConditionFeatCrusherCriticalHit")
                .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionDistracted)
                .SetConditionType(ConditionType.Detrimental)
                .SetFeatures(
                    FeatureDefinitionCombatAffinityBuilder
                        .Create("CombatAffinityFeatCrusher")
                        .SetGuiPresentation("ConditionFeatCrusherCriticalHit", Category.Condition, Gui.NoLocalization)
                        .SetAttackOnMeAdvantage(AdvantageType.Advantage)
                        .AddToDB())
                .AddToDB()))
        .AddToDB();

    private static FeatDefinition BuildCrusherStr()
    {
        return FeatDefinitionBuilder
            .Create("FeatCrusherStr")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                AttributeModifierCreed_Of_Einar,
                FeatureFeatCrusher,
                GameUiContext.ActionAffinityFeatCrusherToggle)
            .SetFeatFamily(GroupFeats.Crusher)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Strength, 13)
            .AddToDB();
    }

    private static FeatDefinition BuildCrusherCon()
    {
        return FeatDefinitionBuilder
            .Create("FeatCrusherCon")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                AttributeModifierCreed_Of_Arun,
                FeatureFeatCrusher,
                GameUiContext.ActionAffinityFeatCrusherToggle)
            .SetFeatFamily(GroupFeats.Crusher)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Constitution, 13)
            .AddToDB();
    }

    private sealed class PhysicalAttackFinishedByMeCrusher(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ConditionDefinition conditionDefinition)
        : IPhysicalAttackFinishedByMe
    {
        private const string SpecialFeatureName = "FeatureCrusher";

        public IEnumerator OnPhysicalAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;

            if (rollOutcome is RollOutcome.CriticalSuccess)
            {
                rulesetDefender.InflictCondition(
                    conditionDefinition.Name,
                    DurationType.Round,
                    0,
                    TurnOccurenceType.EndOfTurn,
                    AttributeDefinitions.TagEffect,
                    rulesetAttacker.guid,
                    rulesetAttacker.CurrentFaction.Name,
                    1,
                    conditionDefinition.Name,
                    0,
                    0,
                    0);
            }

            if (rollOutcome is not (RollOutcome.Success or RollOutcome.CriticalSuccess))
            {
                yield break;
            }

            if (!attacker.OncePerTurnIsValid(SpecialFeatureName) ||
                !rulesetAttacker.IsToggleEnabled((ActionDefinitions.Id)ExtraActionId.FeatCrusherToggle))
            {
                yield break;
            }

            var actionService =
                ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;

            if (actionService == null || !battleManager.IsBattleInProgress)
            {
                yield break;
            }

            if (attackMode.ranged ||
                !ValidatorsWeapon.IsOfDamageType(DamageTypeBludgeoning)(attackMode, null, null))
            {
                yield break;
            }

            var reactionParams = new CharacterActionParams(attacker, (ActionDefinitions.Id)ExtraActionId.DoNothingFree);
            var previousReactionCount = actionService.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestCustom("Crusher", reactionParams);

            actionService.AddInterruptRequest(reactionRequest);

            yield return battleManager.WaitForReactions(attacker, actionService, previousReactionCount);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            var implementationService = ServiceRepository.GetService<IRulesetImplementationService>();
            var applyFormsParams = new RulesetImplementationDefinitions.ApplyFormsParams
            {
                sourceCharacter = rulesetAttacker,
                targetCharacter = rulesetDefender,
                position = defender.LocationPosition
            };

            implementationService.ApplyEffectForms(
                [
                    EffectFormBuilder
                        .Create()
                        .SetMotionForm(MotionForm.MotionType.PushFromOrigin, 1)
                        .Build()
                ],
                applyFormsParams,
                [],
                out _,
                out _);

            attacker.UsedSpecialFeatures.TryAdd(SpecialFeatureName, 1);
        }
    }

    #endregion

    #region Devastating Strikes

    private static FeatDefinition BuildDevastatingStrikes()
    {
        const string NAME = "FeatDevastatingStrikes";

        var weaponTypes = new[] { GreatswordType, GreataxeType, MaulType };

        var conditionDevastatingStrikes = ConditionDefinitionBuilder
            .Create("ConditionDevastatingStrikes")
            .SetGuiPresentation(NAME, Category.Feat)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetSpecialInterruptions(ConditionInterruption.Attacks)
            .AddCustomSubFeatures(new ModifyDamageAffinityDevastatingStrikes())
            .AddToDB();

        var feat = FeatDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feat)
            .AddToDB();

        feat.AddCustomSubFeatures(
            new CustomBehaviorFeatDevastatingStrikes(conditionDevastatingStrikes, weaponTypes),
            new ModifyWeaponAttackModeTypeFilter(feat, weaponTypes));

        return feat;
    }

    private sealed class ModifyDamageAffinityDevastatingStrikes : IModifyDamageAffinity
    {
        public void ModifyDamageAffinity(RulesetActor defender, RulesetActor attacker, List<FeatureDefinition> features)
        {
            features.RemoveAll(x =>
                x is IDamageAffinityProvider { DamageAffinityType: DamageAffinityType.Resistance });
        }
    }

    private sealed class CustomBehaviorFeatDevastatingStrikes :
        IAttackBeforeHitConfirmedOnEnemy, IPhysicalAttackFinishedByMe
    {
        private const string DevastatingStrikesDescription = "Feat/&FeatDevastatingStrikesDescription";
        private const string DevastatingStrikesTitle = "Feat/&FeatDevastatingStrikesTitle";
        private readonly ConditionDefinition _conditionBypassResistance;
        private readonly List<WeaponTypeDefinition> _weaponTypeDefinition = [];

        public CustomBehaviorFeatDevastatingStrikes(
            ConditionDefinition conditionBypassResistance,
            params WeaponTypeDefinition[] weaponTypeDefinition)
        {
            _weaponTypeDefinition.AddRange(weaponTypeDefinition);
            _conditionBypassResistance = conditionBypassResistance;
        }

        public IEnumerator OnAttackBeforeHitConfirmedOnEnemy(GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier actionModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            RulesetEffect rulesetEffect,
            bool firstTarget,
            bool criticalHit)
        {
            if (attackMode?.sourceDefinition is not ItemDefinition { IsWeapon: true } sourceDefinition ||
                !_weaponTypeDefinition.Contains(sourceDefinition.WeaponDescription.WeaponTypeDefinition))
            {
                yield break;
            }

            var rulesetCharacter = attacker.RulesetCharacter;

            if (!criticalHit)
            {
                yield break;
            }

            rulesetCharacter.InflictCondition(
                _conditionBypassResistance.Name,
                DurationType.Round,
                0,
                TurnOccurenceType.EndOfTurn,
                AttributeDefinitions.TagEffect,
                rulesetCharacter.guid,
                rulesetCharacter.CurrentFaction.Name,
                1,
                _conditionBypassResistance.Name,
                0,
                0,
                0);
        }

        public IEnumerator OnPhysicalAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            if (attackMode == null || rollOutcome is not (RollOutcome.Success or RollOutcome.CriticalSuccess))
            {
                yield break;
            }

            var originalDamageForm = attackMode.EffectDescription.FindFirstDamageForm();

            if (originalDamageForm == null)
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;
            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false } ||
                rulesetAttacker is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            if (attackMode.sourceDefinition is not ItemDefinition { IsWeapon: true } sourceDefinition ||
                !_weaponTypeDefinition.Contains(sourceDefinition.WeaponDescription.WeaponTypeDefinition))
            {
                yield break;
            }

            var bonusDamage = 0;
            var attackModifier = action.ActionParams.ActionModifiers[0];
            var advantageType = ComputeAdvantage(attackModifier.attackAdvantageTrends);

            if (advantageType == AdvantageType.Advantage)
            {
                attacker.UsedSpecialFeatures.TryGetValue("LowestAttackRoll", out var lowestAttackRoll);

                var modifier = attackMode.ToHitBonus + attackModifier.AttackRollModifier;
                var lowOutcome = GLBM.GetAttackResult(lowestAttackRoll, modifier, rulesetDefender);

                Gui.Game.GameConsole.AttackRolled(
                    rulesetAttacker,
                    rulesetDefender,
                    attackMode.SourceDefinition,
                    lowOutcome,
                    lowestAttackRoll + modifier,
                    lowestAttackRoll,
                    modifier,
                    attackModifier.AttacktoHitTrends,
                    []);

                if (lowOutcome is RollOutcome.Success or RollOutcome.CriticalSuccess)
                {
                    var strength = rulesetAttacker.TryGetAttributeValue(AttributeDefinitions.Strength);
                    var strengthMod = AttributeDefinitions.ComputeAbilityScoreModifier(strength);
                    var dexterity = rulesetAttacker.TryGetAttributeValue(AttributeDefinitions.Dexterity);
                    var dexterityMod = AttributeDefinitions.ComputeAbilityScoreModifier(dexterity);

                    if (strengthMod > 0 || dexterityMod > 0)
                    {
                        bonusDamage = Math.Max(strengthMod, dexterityMod);
                    }
                }
            }

            if (bonusDamage == 0 && rollOutcome is not RollOutcome.CriticalSuccess)
            {
                yield break;
            }

            var rolls = new List<int>();
            var damageForm = new DamageForm
            {
                DamageType = originalDamageForm.DamageType,
                DieType = originalDamageForm.DieType,
                DiceNumber = rollOutcome == RollOutcome.CriticalSuccess ? 1 : 0,
                BonusDamage = bonusDamage
            };
            var damageRoll = rulesetAttacker.RollDamage(
                damageForm, 0, false, 0, 0, 1, false, false, false, rolls);

            rulesetAttacker.LogCharacterAffectsTarget(
                rulesetDefender,
                DevastatingStrikesTitle,
                "Feedback/&FeatFeatFellHandedDisadvantage",
                tooltipContent: DevastatingStrikesDescription);

            RulesetActor.InflictDamage(
                damageRoll,
                damageForm,
                damageForm.DamageType,
                new RulesetImplementationDefinitions.ApplyFormsParams { targetCharacter = rulesetDefender },
                rulesetDefender,
                false,
                rulesetAttacker.Guid,
                false,
                attackMode.AttackTags,
                new RollInfo(damageForm.DieType, rolls, bonusDamage),
                true,
                out _);
        }
    }

    #endregion

    #region Fell Handed

    private static FeatDefinition BuildFellHanded()
    {
        const string NAME = "FeatFellHanded";

        var weaponTypes = new[] { BattleaxeType, GreataxeType, HandaxeType, MaulType, WarhammerType };

        var fellHandedAdvantage = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}Advantage")
            .SetGuiPresentation(NAME, Category.Feat, $"Feature/&Power{NAME}AdvantageDescription", hidden: true)
            .SetUsesFixed(ActivationTime.NoCost)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Touch, 0, TargetType.IndividualsUnique)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetMotionForm(MotionForm.MotionType.FallProne)
                            .Build())
                    .Build())
            .AddToDB();

        var feat = FeatDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(fellHandedAdvantage)
            .AddToDB();

        fellHandedAdvantage.AddCustomSubFeatures(
            new PhysicalAttackFinishedByMeFeatFellHanded(fellHandedAdvantage, weaponTypes),
            new ModifyWeaponAttackModeTypeFilter(feat, weaponTypes));

        return feat;
    }

    private sealed class PhysicalAttackFinishedByMeFeatFellHanded : IPhysicalAttackFinishedByMe
    {
        private const string SuretyText = "Feedback/&FeatFeatFellHandedDisadvantage";
        private const string SuretyTitle = "Feat/&FeatFellHandedTitle";
        private const string SuretyDescription = "Feature/&PowerFeatFellHandedDisadvantageDescription";
        private readonly DamageForm _damage;
        private readonly FeatureDefinitionPower _power;
        private readonly List<WeaponTypeDefinition> _weaponTypeDefinition = [];

        public PhysicalAttackFinishedByMeFeatFellHanded(
            FeatureDefinitionPower power,
            params WeaponTypeDefinition[] weaponTypeDefinition)
        {
            _power = power;
            _weaponTypeDefinition.AddRange(weaponTypeDefinition);

            _damage = new DamageForm
            {
                DamageType = DamageTypeBludgeoning, DieType = DieType.D1, DiceNumber = 0, BonusDamage = 0
            };
        }

        public IEnumerator OnPhysicalAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            if (attackMode?.sourceDefinition is not ItemDefinition { IsWeapon: true } sourceDefinition ||
                !_weaponTypeDefinition.Contains(sourceDefinition.WeaponDescription.WeaponTypeDefinition))
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;
            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false } ||
                rulesetAttacker is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            var attackModifier = action.ActionParams.ActionModifiers[0];
            var modifier = attackMode.ToHitBonus + attackModifier.AttackRollModifier;
            var advantageType = ComputeAdvantage(attackModifier.attackAdvantageTrends);

            switch (advantageType)
            {
                case AdvantageType.Advantage when rollOutcome is RollOutcome.Success or RollOutcome.CriticalSuccess:
                    attacker.UsedSpecialFeatures.TryGetValue("LowestAttackRoll", out var lowestAttackRoll);

                    var lowOutcome = GLBM.GetAttackResult(lowestAttackRoll, modifier, rulesetDefender);

                    Gui.Game.GameConsole.AttackRolled(
                        rulesetAttacker,
                        rulesetDefender,
                        _power,
                        lowOutcome,
                        lowestAttackRoll + modifier,
                        lowestAttackRoll,
                        modifier,
                        attackModifier.AttacktoHitTrends,
                        []);

                    if (lowOutcome is RollOutcome.Success or RollOutcome.CriticalSuccess)
                    {
                        var implementationManagerService =
                            ServiceRepository.GetService<IRulesetImplementationService>() as
                                RulesetImplementationManager;

                        var usablePower = PowerProvider.Get(_power, rulesetAttacker);
                        var actionParams = new CharacterActionParams(attacker, ActionDefinitions.Id.PowerNoCost)
                        {
                            ActionModifiers = { new ActionModifier() },
                            RulesetEffect = implementationManagerService
                                //CHECK: no need for AddAsActivePowerToSource
                                .MyInstantiateEffectPower(rulesetAttacker, usablePower, false),
                            UsablePower = usablePower,
                            TargetCharacters = { defender }
                        };

                        // must enqueue actions whenever within an attack workflow otherwise game won't consume attack
                        ServiceRepository.GetService<IGameLocationActionService>()?
                            .ExecuteAction(actionParams, null, true);
                    }

                    break;
                case AdvantageType.Disadvantage when rollOutcome is RollOutcome.Failure or RollOutcome.CriticalFailure:
                    attacker.UsedSpecialFeatures.TryGetValue("LowestAttackRoll", out var highestAttackRoll);

                    var strength = rulesetAttacker.TryGetAttributeValue(AttributeDefinitions.Strength);
                    var strengthMod = AttributeDefinitions.ComputeAbilityScoreModifier(strength);

                    if (strengthMod <= 0)
                    {
                        break;
                    }

                    var higherOutcome = GLBM.GetAttackResult(highestAttackRoll, modifier, rulesetDefender);

                    if (higherOutcome is not (RollOutcome.Success or RollOutcome.CriticalSuccess))
                    {
                        break;
                    }

                    rulesetAttacker.LogCharacterAffectsTarget(rulesetDefender,
                        SuretyTitle, SuretyText, tooltipContent: SuretyDescription);

                    _damage.BonusDamage = strengthMod;
                    RulesetActor.InflictDamage(
                        strengthMod,
                        _damage,
                        DamageTypeBludgeoning,
                        new RulesetImplementationDefinitions.ApplyFormsParams { targetCharacter = rulesetDefender },
                        rulesetDefender,
                        false,
                        rulesetAttacker.Guid,
                        false,
                        attackMode.AttackTags,
                        new RollInfo(DieType.D1, [], strengthMod),
                        true,
                        out _);

                    break;
                case AdvantageType.None:
                default:
                    break;
            }
        }
    }

    #endregion

    #region Piercer

    private static readonly FeatureDefinition FeatureFeatPiercer =
        FeatureDefinitionDieRollModifierBuilder
            .Create("FeatureFeatPiercer")
            .SetGuiPresentationNoContent(true)
            .SetModifiers(AttackDamageValueRoll, 1, 1, 1, "Feat/&FeatPiercerReroll")
            .AddCustomSubFeatures(
                new CustomAdditionalDamageFeatPiercer(
                    FeatureDefinitionAdditionalDamageBuilder
                        .Create("AdditionalDamageFeatPiercer")
                        .SetGuiPresentationNoContent(true)
                        .SetNotificationTag(GroupFeats.Piercer)
                        .SetDamageValueDetermination(AdditionalDamageValueDetermination.SameAsBaseWeaponDie)
                        .SetIgnoreCriticalDoubleDice(true)
                        .AddToDB()))
            .AddToDB();

    private static FeatDefinition BuildPiercerDex()
    {
        return FeatDefinitionBuilder
            .Create("FeatPiercerDex")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                AttributeModifierCreed_Of_Misaye,
                FeatureFeatPiercer)
            .SetFeatFamily(GroupFeats.Piercer)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Dexterity, 13)
            .AddToDB();
    }

    private static FeatDefinition BuildPiercerStr()
    {
        return FeatDefinitionBuilder
            .Create("FeatPiercerStr")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                AttributeModifierCreed_Of_Einar,
                FeatureFeatPiercer)
            .SetFeatFamily(GroupFeats.Piercer)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Strength, 13)
            .AddToDB();
    }

    private sealed class CustomAdditionalDamageFeatPiercer(IAdditionalDamageProvider provider)
        : CustomAdditionalDamage(provider), IValidateDieRollModifier
    {
        public bool CanModifyRoll(RulesetCharacter character, List<FeatureDefinition> features,
            List<string> damageTypes)
        {
            return damageTypes.Contains(DamageTypePiercing);
        }

        internal override bool IsValid(
            GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode,
            bool rangedAttack,
            AdvantageType advantageType,
            List<EffectForm> actualEffectForms,
            RulesetEffect rulesetEffect,
            bool criticalHit,
            bool firstTarget,
            out CharacterActionParams reactionParams)
        {
            reactionParams = null;

            var damage = attackMode?.EffectDescription?.FindFirstDamageForm();

            return criticalHit && damage is { DamageType: DamageTypePiercing };
        }
    }

    #endregion

    #region Power Attack

    private static FeatDefinition BuildPowerAttack()
    {
        const string Name = "FeatPowerAttack";

        var concentrationProvider = new StopPowerConcentrationProvider("PowerAttack",
            "Tooltip/&PowerAttackConcentration",
            Sprites.GetSprite("PowerAttackConcentrationIcon", Resources.PowerAttackConcentrationIcon, 64, 64));

        var conditionPowerAttack = ConditionDefinitionBuilder
            .Create($"Condition{Name}")
            .SetGuiPresentation(Name, Category.Feat, ConditionDefinitions.ConditionHeraldOfBattle)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddToDB();

        var powerAttack = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}")
            .SetGuiPresentation(Name, Category.Feat,
                Sprites.GetSprite("PowerAttackIcon", Resources.PowerAttackIcon, 128, 64))
            .SetUsesFixed(ActivationTime.NoCost)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetDurationData(DurationType.Permanent)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionPowerAttack, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(
                IgnoreInvisibilityInterruptionCheck.Marker,
                new ValidatorsValidatePowerUse(ValidatorsCharacter.HasNoneOfConditions(conditionPowerAttack.Name)))
            .AddToDB();

        var powerTurnOffPowerAttack = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}TurnOff")
            .SetGuiPresentationNoContent(true)
            .SetUsesFixed(ActivationTime.NoCost)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetDurationData(DurationType.Round, 1)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionPowerAttack, ConditionForm.ConditionOperation.Remove)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(IgnoreInvisibilityInterruptionCheck.Marker)
            .AddToDB();

        var featPowerAttack = FeatDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                powerAttack,
                powerTurnOffPowerAttack)
            .AddToDB();

        concentrationProvider.StopPower = powerTurnOffPowerAttack;
        conditionPowerAttack.AddCustomSubFeatures(
            concentrationProvider,
            new ModifyWeaponAttackModeFeatPowerAttack(featPowerAttack));

        return featPowerAttack;
    }

    private sealed class ModifyWeaponAttackModeFeatPowerAttack(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        FeatDefinition featDefinition) : IModifyWeaponAttackMode
    // thrown is allowed on power attack
    //, IPhysicalAttackInitiatedByMe
    {
        private const int ToHit = 3;

        public void ModifyAttackMode(RulesetCharacter character, RulesetAttackMode attackMode)
        {
            if (!ValidatorsWeapon.IsMelee(attackMode) && !ValidatorsWeapon.IsUnarmed(attackMode))
            {
                return;
            }

            var proficiency = character.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);
            var toDamage = ToHit + proficiency;

            attackMode.ToHitBonus -= ToHit;
            attackMode.ToHitBonusTrends.Add(new TrendInfo(-ToHit, FeatureSourceType.Feat, featDefinition.Name,
                featDefinition));

            var damage = attackMode.EffectDescription?.FindFirstDamageForm();

            if (damage == null)
            {
                return;
            }

            damage.BonusDamage += toDamage;
            damage.DamageBonusTrends.Add(new TrendInfo(toDamage, FeatureSourceType.Feat, featDefinition.Name,
                featDefinition));
        }

// thrown is allowed on power attack
#if false
        // this is required to handle thrown scenarios
        public IEnumerator OnPhysicalAttackInitiatedByMe(
            GameLocationBattleManager __instance,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode)
        {
            var isMelee = ValidatorsWeapon.IsMelee(attackMode);
            var isUnarmed = ValidatorsWeapon.IsUnarmed(attackMode);
            var isPowerAttackValid = isMelee || isUnarmed;

            if (isPowerAttackValid)
            {
                yield break;
            }

            attackModifier.AttacktoHitTrends.RemoveAll(x => x.sourceName == _featDefinition.Name);
            attackMode.ToHitBonusTrends.RemoveAll(x => x.sourceName == _featDefinition.Name);
            attackMode.ToHitBonus += ToHit;

            var damageForm = attackMode.EffectDescription.FindFirstDamageForm();

            if (damageForm == null)
            {
                yield break;
            }

            var proficiency = attacker.RulesetCharacter.TryGetAttributeValue(AttributeDefinitions.ProficiencyBonus);
            var toDamage = ToHit + proficiency;

            damageForm.DamageBonusTrends.RemoveAll(x => x.sourceName == _featDefinition.Name);
            damageForm.BonusDamage -= toDamage;
        }
#endif
    }

    #endregion

    #region Slasher

    private static readonly FeatureDefinition FeatureFeatSlasher = FeatureDefinitionBuilder
        .Create("FeatureFeatSlasher")
        .SetGuiPresentationNoContent(true)
        .AddCustomSubFeatures(
            new PhysicalAttackAfterDamageFeatSlasher(
                ConditionDefinitionBuilder
                    .Create("ConditionFeatSlasherHit")
                    .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionDazzled)
                    .SetConditionType(ConditionType.Detrimental)
                    .SetPossessive()
                    .SetFeatures(
                        FeatureDefinitionMovementAffinityBuilder
                            .Create("MovementAffinityFeatSlasher")
                            .SetGuiPresentation("ConditionFeatSlasherHit", Category.Condition, Gui.NoLocalization)
                            .SetBaseSpeedAdditiveModifier(-2)
                            .AddToDB())
                    .AddToDB(),
                ConditionDefinitionBuilder
                    .Create("ConditionFeatSlasherCriticalHit")
                    .SetGuiPresentation(Category.Condition)
                    .SetConditionType(ConditionType.Detrimental)
                    .SetPossessive()
                    .SetFeatures(
                        FeatureDefinitionCombatAffinityBuilder
                            .Create("CombatAffinityFeatSlasher")
                            .SetGuiPresentation("ConditionFeatSlasherCriticalHit", Category.Condition,
                                Gui.NoLocalization)
                            .SetMyAttackAdvantage(AdvantageType.Disadvantage)
                            .AddToDB())
                    .AddToDB(),
                DamageTypeSlashing))
        .AddToDB();

    private static FeatDefinition BuildSlasherDex()
    {
        return FeatDefinitionBuilder
            .Create("FeatSlasherDex")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                AttributeModifierCreed_Of_Misaye,
                FeatureFeatSlasher)
            .SetFeatFamily(GroupFeats.Slasher)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Dexterity, 13)
            .AddToDB();
    }

    private static FeatDefinition BuildSlasherStr()
    {
        return FeatDefinitionBuilder
            .Create("FeatSlasherStr")
            .SetGuiPresentation(Category.Feat)
            .SetFeatures(
                AttributeModifierCreed_Of_Einar,
                FeatureFeatSlasher)
            .SetFeatFamily(GroupFeats.Slasher)
            .SetAbilityScorePrerequisite(AttributeDefinitions.Strength, 13)
            .AddToDB();
    }

    private sealed class PhysicalAttackAfterDamageFeatSlasher : IPhysicalAttackFinishedByMe
    {
        private readonly ConditionDefinition _conditionDefinition;
        private readonly ConditionDefinition _criticalConditionDefinition;
        private readonly string _damageType;

        internal PhysicalAttackAfterDamageFeatSlasher(
            ConditionDefinition conditionDefinition,
            ConditionDefinition criticalConditionDefinition,
            string damageType)
        {
            _conditionDefinition = conditionDefinition;
            _criticalConditionDefinition = criticalConditionDefinition;
            _damageType = damageType;
        }

        public IEnumerator OnPhysicalAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            var damage = attackMode?.EffectDescription?.FindFirstDamageForm();

            if (damage == null || damage.DamageType != _damageType)
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;
            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false } ||
                rulesetAttacker is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            if (rollOutcome is RollOutcome.Success or RollOutcome.CriticalSuccess)
            {
                rulesetDefender.InflictCondition(
                    _conditionDefinition.Name,
                    DurationType.Round,
                    0,
                    TurnOccurenceType.EndOfTurn,
                    AttributeDefinitions.TagEffect,
                    rulesetAttacker.guid,
                    rulesetAttacker.CurrentFaction.Name,
                    1,
                    _conditionDefinition.Name,
                    0,
                    0,
                    0);
            }

            if (rollOutcome is not RollOutcome.CriticalSuccess)
            {
                yield break;
            }

            rulesetDefender.InflictCondition(
                _criticalConditionDefinition.Name,
                DurationType.Round,
                0,
                TurnOccurenceType.EndOfTurn,
                AttributeDefinitions.TagEffect,
                rulesetAttacker.guid,
                rulesetAttacker.CurrentFaction.Name,
                1,
                _criticalConditionDefinition.Name,
                0,
                0,
                0);
        }
    }

    #endregion
}
