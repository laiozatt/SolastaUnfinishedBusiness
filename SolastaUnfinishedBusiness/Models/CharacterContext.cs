﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Feats;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Races;
using SolastaUnfinishedBusiness.Subclasses;
using SolastaUnfinishedBusiness.Validators;
using TA;
using static RuleDefinitions;
using static FeatureDefinitionAttributeModifier;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ActionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterClassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterRaceDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterSubclassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionActionAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAttackModifiers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionFeatureSets;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionMovementAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPointPools;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionProficiencys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSenses;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.MorphotypeElementDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Models;

internal static class CharacterContext
{
    internal const int MinInitialFeats = 0;
    internal const int MaxInitialFeats = 4; // don't increase this value to avoid issue reports on crazy scenarios

    internal const int GameMaxAttribute = 15;
    internal const int GameBuyPoints = 27;

    internal const int ModMaxAttribute = 17;
    internal const int ModBuyPoints = 35;

    internal static readonly ConditionDefinition ConditionIndomitableSaving = ConditionDefinitionBuilder
        .Create("ConditionIndomitableSaving")
        .SetGuiPresentationNoContent(true)
        .SetSilent(Silent.WhenAddedOrRemoved)
        .SetSpecialInterruptions(ConditionInterruption.SavingThrow)
        .AddCustomSubFeatures(new RollSavingThrowInitiatedIndomitableSaving())
        .AddToDB();

    internal static readonly FeatureDefinitionFightingStyleChoice FightingStyleChoiceBarbarian =
        FeatureDefinitionFightingStyleChoiceBuilder
            .Create("FightingStyleChoiceBarbarian")
            .SetGuiPresentation("FighterFightingStyle", Category.Feature)
            .SetFightingStyles(
                // "BlindFighting",
                // "Crippling",
                // "Executioner",
                // "HandAndAHalf",
                // "Interception",
                // "Merciless",
                // "Pugilist",
                "TwoWeapon")
            .AddToDB();

    internal static readonly FeatureDefinitionFightingStyleChoice FightingStyleChoiceMonk =
        FeatureDefinitionFightingStyleChoiceBuilder
            .Create("FightingStyleChoiceMonk")
            .SetGuiPresentation("FighterFightingStyle", Category.Feature)
            .SetFightingStyles(
                "Archery",
                // "BlindFighting",
                // "Crippling",
                "Dueling",
                // "Executioner",
                // "Lunger",
                // "MonkShieldExpert",
                // "Pugilist",
                // "ZenArcher",
                "TwoWeapon")
            .AddToDB();

    internal static readonly FeatureDefinitionFightingStyleChoice FightingStyleChoiceRogue =
        FeatureDefinitionFightingStyleChoiceBuilder
            .Create("FightingStyleChoiceRogue")
            .SetGuiPresentation("FighterFightingStyle", Category.Feature)
            .SetFightingStyles(
                "Archery",
                // "BlindFighting",
                // "Crippling",
                "Dueling",
                // "Executioner",
                // "Lunger",
                // "Merciless",
                "TwoWeapon")
            .AddToDB();

    private static readonly FeatureDefinitionAttributeModifier AttributeModifierMonkAbundantKi =
        FeatureDefinitionAttributeModifierBuilder
            .Create("AttributeModifierMonkAbundantKi")
            .SetGuiPresentation(Category.Feature)
            .SetModifier(AttributeModifierOperation.AddHalfProficiencyBonus, AttributeDefinitions.KiPoints, 1)
            .SetSituationalContext(SituationalContext.NotWearingArmorOrShield)
            .AddToDB();

    private static readonly FeatureDefinitionAbilityCheckAffinity AbilityCheckAffinityDarknessPerceptive =
        FeatureDefinitionAbilityCheckAffinityBuilder
            .Create("AbilityCheckAffinityDarknessPerceptive")
            .SetGuiPresentation(Category.Feature)
            .BuildAndSetAffinityGroups(CharacterAbilityCheckAffinity.Advantage,
                abilityProficiencyPairs: (AttributeDefinitions.Wisdom, SkillDefinitions.Perception))
            .AddCustomSubFeatures(ValidatorsCharacter.IsUnlitOrDarkness)
            .AddToDB();

    private static readonly FeatureDefinitionCustomInvocationPool InvocationPoolMonkWeaponSpecialization =
        CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolMonkWeaponSpecialization")
            .SetGuiPresentation("InvocationPoolMonkWeaponSpecializationLearn", Category.Feature)
            .Setup(InvocationPoolTypeCustom.Pools.MonkWeaponSpecialization)
            .AddToDB();

    private static readonly FeatureDefinitionCustomInvocationPool InvocationPoolPathClawDraconicChoice =
        CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolPathClawDraconicChoice")
            .SetGuiPresentation(FeatureSetPathClawDragonAncestry.GuiPresentation)
            .Setup(InvocationPoolTypeCustom.Pools.PathClawDraconicChoice)
            .AddToDB();

    private static readonly FeatureDefinitionCustomInvocationPool InvocationPoolPathOfTheElementsElementalFuryChoice =
        CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolPathOfTheElementsElementalFuryChoice")
            .SetGuiPresentation(PathOfTheElements.FeatureSetElementalFury.GuiPresentation)
            .Setup(InvocationPoolTypeCustom.Pools.PathOfTheElementsElementalFuryChoiceChoice)
            .AddToDB();

    private static readonly FeatureDefinitionCustomInvocationPool InvocationPoolSorcererDraconicChoice =
        CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolSorcererDraconicChoice")
            .SetGuiPresentation(FeatureSetSorcererDraconicChoice.GuiPresentation)
            .Setup(InvocationPoolTypeCustom.Pools.SorcererDraconicChoice)
            .AddToDB();

    private static readonly FeatureDefinitionCustomInvocationPool InvocationPoolKindredSpiritChoice =
        CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolKindredSpiritChoice")
            .SetGuiPresentation(FeatureSetKindredSpiritChoice.GuiPresentation)
            .Setup(InvocationPoolTypeCustom.Pools.KindredSpiritChoice)
            .AddToDB();

    internal static readonly FeatureDefinitionCustomInvocationPool InvocationPoolRangerTerrainType =
        CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolRangerTerrainType")
            .SetGuiPresentation(TerrainTypeAffinityRangerNaturalExplorerChoice.GuiPresentation)
            .Setup(InvocationPoolTypeCustom.Pools.RangerTerrainTypeAffinity)
            .AddToDB();

    internal static readonly FeatureDefinitionCustomInvocationPool InvocationPoolRangerPreferredEnemy =
        CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolRangerPreferredEnemy")
            .SetGuiPresentation(AdditionalDamageRangerFavoredEnemyChoice.GuiPresentation)
            .Setup(InvocationPoolTypeCustom.Pools.RangerPreferredEnemy)
            .AddToDB();

    internal static readonly FeatureDefinitionPower FeatureDefinitionPowerHelpAction = FeatureDefinitionPowerBuilder
        .Create("PowerHelp")
        .SetGuiPresentation(Category.Feature, Sprites.GetSprite("PowerHelp", Resources.PowerHelp, 256, 128))
        .SetUsesFixed(ActivationTime.Action)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create()
                .SetDurationData(DurationType.Round, 1, TurnOccurenceType.EndOfSourceTurn)
                .SetTargetingData(Side.Enemy, RangeType.Touch, 0, TargetType.IndividualsUnique)
                .SetEffectForms(EffectFormBuilder.ConditionForm(CustomConditionsContext.Distracted))
                .Build())
        .SetUniqueInstance()
        .AddToDB();

    internal static readonly FeatureDefinitionPower PowerTeleportSummon = FeatureDefinitionPowerBuilder
        .Create("PowerTeleportSummon")
        .SetGuiPresentation(Category.Feature, DimensionDoor)
        .SetUsesFixed(ActivationTime.NoCost)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.Position)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetMotionForm(MotionForm.MotionType.TeleportToDestination)
                        .Build())
                .UseQuickAnimations()
                .Build())
        .AddCustomSubFeatures(ModifyPowerVisibility.NotInCombat, new FilterTargetingPositionPowerTeleportSummon())
        .AddToDB();

    internal static readonly FeatureDefinitionPower PowerVanishSummon = FeatureDefinitionPowerBuilder
        .Create("PowerVanishSummon")
        .SetGuiPresentation(Category.Feature, PowerSorcererCreateSpellSlot)
        .SetUsesFixed(ActivationTime.NoCost)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetKillForm(KillCondition.Always)
                        .Build())
                .UseQuickAnimations()
                .Build())
        .AddToDB();

    private static readonly FeatureDefinitionPower FeatureDefinitionPowerNatureShroud = FeatureDefinitionPowerBuilder
        .Create("PowerRangerNatureShroud")
        .SetGuiPresentation(Category.Feature, Invisibility)
        .SetUsesProficiencyBonus(ActivationTime.BonusAction)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Round, 0, TurnOccurenceType.StartOfTurn)
                .SetEffectForms(EffectFormBuilder.ConditionForm(ConditionDefinitions.ConditionInvisible))
                .SetParticleEffectParameters(PowerDruidCircleBalanceBalanceOfPower)
                .Build())
        .AddToDB();

    private static int PreviousTotalFeatsGrantedFirstLevel { get; set; } = -1;
    private static bool PreviousAlternateHuman { get; set; }

    internal static void LateLoad()
    {
        FlexibleBackgroundsContext.Load();
        FlexibleBackgroundsContext.SwitchFlexibleBackgrounds();
        FlexibleRacesContext.SwitchFlexibleRaces();
        LoadAdditionalNames();
        LoadEpicArray();
        LoadFeatsPointPools();
        LoadMonkWeaponSpecialization();
        LoadVision();
        LoadVisuals();
        BuildRogueCunningStrike();
        SwitchAsiAndFeat();
        SwitchBarbarianFightingStyle();
        SwitchDarknessPerceptive();
        SwitchDragonbornElementalBreathUsages();
        SwitchDruidKindredBeastToUseCustomInvocationPools();
        SwitchEveryFourLevelsFeats();
        SwitchEveryFourLevelsFeats(true);
        SwitchFighterWeaponSpecialization();
        SwitchFirstLevelTotalFeats();
        SwitchHelpPower();
        SwitchMonkAbundantKi();
        SwitchMonkFightingStyle();
        SwitchMonkDoNotRequireAttackActionForFlurry();
        SwitchMonkImprovedUnarmoredMovementToMoveOnTheWall();
        SwitchMonkDoNotRequireAttackActionForBonusUnarmoredAttack();
        SwitchMonkWeaponSpecialization();
        SwitchPathOfTheElementsElementalFuryToUseCustomInvocationPools();
        SwitchRangerHumanoidFavoredEnemy();
        SwitchRangerNatureShroud();
        SwitchRangerToUseCustomInvocationPools();
        SwitchRogueCunningStrike();
        SwitchRogueFightingStyle();
        SwitchRogueSteadyAim();
        SwitchRogueStrSaving();
        SwitchScimitarWeaponSpecialization();
        SwitchSubclassAncestriesToUseCustomInvocationPools(
            "PathClaw", PathClaw,
            FeatureSetPathClawDragonAncestry, InvocationPoolPathClawDraconicChoice,
            InvocationPoolTypeCustom.Pools.PathClawDraconicChoice);
        SwitchSubclassAncestriesToUseCustomInvocationPools(
            "Sorcerer", SorcerousDraconicBloodline,
            FeatureSetSorcererDraconicChoice, InvocationPoolSorcererDraconicChoice,
            InvocationPoolTypeCustom.Pools.SorcererDraconicChoice);
    }

    private static void AddNameToRace(CharacterRaceDefinition raceDefinition, string gender, string name)
    {
        var racePresentation = raceDefinition.RacePresentation;

        switch (gender)
        {
            case "Male":
                racePresentation.MaleNameOptions.Add(name);
                break;

            case "Female":
                racePresentation.FemaleNameOptions.Add(name);
                break;

            case "Sur":
                racePresentation.SurNameOptions.Add(name);
                break;
        }
    }

    private static void LoadAdditionalNames()
    {
        if (!Main.Settings.OfferAdditionalLoreFriendlyNames)
        {
            return;
        }

        var payload = Resources.Names;
        var lines = new List<string>(payload.Split([Environment.NewLine], StringSplitOptions.None));

        foreach (var line in lines)
        {
            var columns = line.Split(Separator, 3);

            if (columns.Length != 3)
            {
                Main.Error($"additional names cannot load: {line}.");

                continue;
            }

            var raceName = columns[0];
            var gender = columns[1];
            var name = columns[2];

            if (DatabaseRepository.GetDatabase<CharacterRaceDefinition>().TryGetElement(raceName, out var race))
            {
                if (race.subRaces.Count == 0)
                {
                    AddNameToRace(race, gender, name);
                }
                else
                {
                    foreach (var subRace in race.SubRaces)
                    {
                        AddNameToRace(subRace, gender, name);
                    }
                }
            }
            else
            {
                Main.Error($"additional names cannot load: {line}.");
            }
        }
    }

    private static void LoadEpicArray()
    {
        AttributeDefinitions.PredeterminedRollScores = Main.Settings.EnableEpicPointsAndArray
            ? [17, 15, 13, 12, 10, 8]
            : [15, 14, 13, 12, 10, 8];
    }

    private static void LoadFeatsPointPools()
    {
        // create feats point pools
        // +1 here as need to count the Alternate Human Feat
        for (var i = 1; i <= MaxInitialFeats + 1; i++)
        {
            var s = i.ToString();

            _ = FeatureDefinitionPointPoolBuilder
                .Create($"PointPool{i}BonusFeats")
                .SetGuiPresentation(
                    Gui.Format("Feature/&PointPoolSelectBonusFeatsTitle", s),
                    Gui.Format("Feature/&PointPoolSelectBonusFeatsDescription", s))
                .SetPool(HeroDefinitions.PointsPoolType.Feat, i)
                .AddToDB();
        }
    }

    private static void LoadMonkWeaponSpecialization()
    {
        var weaponTypeDefinitions = new List<WeaponTypeDefinition>
        {
            WeaponTypeDefinitions.BattleaxeType,
            WeaponTypeDefinitions.LightCrossbowType,
            WeaponTypeDefinitions.LongbowType,
            WeaponTypeDefinitions.LongswordType,
            WeaponTypeDefinitions.MorningstarType,
            WeaponTypeDefinitions.RapierType,
            WeaponTypeDefinitions.ScimitarType,
            WeaponTypeDefinitions.ShortbowType,
            WeaponTypeDefinitions.WarhammerType,
            CustomWeaponsContext.HandXbowWeaponType
        };

        foreach (var weaponTypeDefinition in weaponTypeDefinitions)
        {
            var weaponTypeName = weaponTypeDefinition.Name;

            var featureMonkWeaponSpecialization = FeatureDefinitionProficiencyBuilder
                .Create($"FeatureMonkWeaponSpecialization{weaponTypeName}")
                .SetGuiPresentationNoContent(true)
                .SetProficiencies(ProficiencyType.Weapon, weaponTypeName)
                .AddCustomSubFeatures(
                    new MonkWeaponSpecialization { WeaponType = weaponTypeDefinition })
                .AddToDB();

            if (!weaponTypeDefinition.IsBow && !weaponTypeDefinition.IsCrossbow)
            {
                featureMonkWeaponSpecialization.AddCustomSubFeatures(
                    new AddTagToWeapon(TagsDefinitions.WeaponTagFinesse, TagsDefinitions.Criticity.Important,
                        ValidatorsWeapon.IsOfWeaponType(weaponTypeDefinition))
                );
            }

            // ensure we get dice upgrade on these
            AttackModifierMonkMartialArtsImprovedDamage.AddCustomSubFeatures(
                new MonkWeaponSpecializationDiceUpgrade(weaponTypeDefinition));

            _ = CustomInvocationDefinitionBuilder
                .Create($"CustomInvocationMonkWeaponSpecialization{weaponTypeName}")
                .SetGuiPresentation(
                    weaponTypeDefinition.GuiPresentation.Title,
                    weaponTypeDefinition.GuiPresentation.Description,
                    CustomWeaponsContext.GetStandardWeaponOfType(weaponTypeDefinition.Name))
                .SetPoolType(InvocationPoolTypeCustom.Pools.MonkWeaponSpecialization)
                .SetGrantedFeature(featureMonkWeaponSpecialization)
                .AddCustomSubFeatures(ModifyInvocationVisibility.Marker)
                .AddToDB();
        }
    }

    private static void LoadVision()
    {
        if (Main.Settings.DisableSenseDarkVisionFromAllRaces)
        {
            foreach (var featureUnlocks in DatabaseRepository.GetDatabase<CharacterRaceDefinition>()
                         .Select(crd => crd.FeatureUnlocks))
            {
                featureUnlocks.RemoveAll(x => x.FeatureDefinition == SenseDarkvision);
                // Half-orcs have a different darkvision.
                featureUnlocks.RemoveAll(x => x.FeatureDefinition == SenseDarkvision12);
            }
        }

        // ReSharper disable once InvertIf
        if (Main.Settings.DisableSenseSuperiorDarkVisionFromAllRaces)
        {
            foreach (var featureUnlocks in DatabaseRepository.GetDatabase<CharacterRaceDefinition>()
                         .Select(crd => crd.FeatureUnlocks))
            {
                featureUnlocks.RemoveAll(x => x.FeatureDefinition == SenseSuperiorDarkvision);
            }
        }
    }

    private static void LoadVisuals()
    {
        var dbMorphotypeElementDefinition = DatabaseRepository.GetDatabase<MorphotypeElementDefinition>();

        if (Main.Settings.UnlockSkinColors)
        {
            foreach (var morphotype in dbMorphotypeElementDefinition.Where(
                         x => x.Category == MorphotypeElementDefinition.ElementCategory.Skin &&
                              x != FaceAndSkin_12 &&
                              x != FaceAndSkin_13 &&
                              x != FaceAndSkin_14 &&
                              x != FaceAndSkin_15 &&
                              x != FaceAndSkin_16 &&
                              x != FaceAndSkin_17 &&
                              x != FaceAndSkin_18))
            {
                morphotype.playerSelectable = true;
                morphotype.originAllowed = EyeColor_001.OriginAllowed;
                if (morphotype.Name.Contains("Dragonborn"))
                {
                    morphotype.GuiPresentation.sortOrder = morphotype.GuiPresentation.SortOrder + 54;
                }
            }
        }

        if (Main.Settings.UnlockGlowingColorsForAllMarksAndTattoos)
        {
            foreach (var morphotype in dbMorphotypeElementDefinition.Where(
                         x => x.Category == MorphotypeElementDefinition.ElementCategory.BodyDecorationColor &&
                              x.SubclassFilterMask == GraphicsDefinitions.MorphotypeSubclassFilterTag
                                  .SorcererManaPainter))
            {
                morphotype.subClassFilterMask = GraphicsDefinitions.MorphotypeSubclassFilterTag.All;
            }
        }

        var brightEyes = new List<MorphotypeElementDefinition>();

        Morphotypes.CreateBrightEyes(brightEyes);

        if (!Main.Settings.AddNewBrightEyeColors)
        {
            foreach (var morphotype in brightEyes)
            {
                morphotype.playerSelectable = false;
            }
        }

        var glowingEyes = new List<MorphotypeElementDefinition>();

        Morphotypes.CreateGlowingEyes(glowingEyes);

        if (!Main.Settings.UnlockGlowingEyeColors)
        {
            foreach (var morphotype in glowingEyes)
            {
                morphotype.playerSelectable = false;
            }
        }

        if (Main.Settings.UnlockEyeStyles)
        {
            foreach (var morphotype in dbMorphotypeElementDefinition.Where(x =>
                         x.Category == MorphotypeElementDefinition.ElementCategory.Eye))
            {
                morphotype.subClassFilterMask = GraphicsDefinitions.MorphotypeSubclassFilterTag.All;
                morphotype.originAllowed = EyeColor_001.OriginAllowed;
            }

            var races = DatabaseRepository.GetDatabase<CharacterRaceDefinition>();

            foreach (var race in races)
            {
                var presentation = race.racePresentation;

                if (presentation.IsAvailable(MorphotypeElementDefinition.ElementCategory.Eye))
                {
                    continue;
                }

                var all = new List<MorphotypeElementDefinition.ElementCategory>(
                    presentation.availableMorphotypeCategories) { MorphotypeElementDefinition.ElementCategory.Eye };

                presentation.availableMorphotypeCategories = all.ToArray();
            }
        }

        if (Main.Settings.UnlockAllNpcFaces)
        {
            HalfElf.RacePresentation.FemaleFaceShapeOptions.Add("FaceShape_NPC_Princess");
            HalfElf.RacePresentation.MaleFaceShapeOptions.Add("FaceShape_HalfElf_NPC_Bartender");
            Human.RacePresentation.MaleFaceShapeOptions.Add("FaceShape_NPC_TavernGuy");
            Human.RacePresentation.MaleFaceShapeOptions.Add("FaceShape_NPC_TomWorker");

            foreach (var morphotype in dbMorphotypeElementDefinition.Where(x =>
                         x.Category == MorphotypeElementDefinition.ElementCategory.FaceShape &&
                         x != FaceShape_NPC_Aksha))
            {
                morphotype.playerSelectable = true;
            }
        }

        if (Main.Settings.AllowBeardlessDwarves)
        {
            Dwarf.RacePresentation.needBeard = false;
            DwarfHill.RacePresentation.needBeard = false;
            DwarfSnow.RacePresentation.needBeard = false;
            Dwarf.RacePresentation.MaleBeardShapeOptions.Add(BeardShape_None.Name);
        }

        if (Main.Settings.UnlockMarkAndTattoosForAllCharacters)
        {
            foreach (var morphotype in dbMorphotypeElementDefinition.Where(x =>
                         x.Category == MorphotypeElementDefinition.ElementCategory.BodyDecoration))
            {
                morphotype.subClassFilterMask = GraphicsDefinitions.MorphotypeSubclassFilterTag.All;
            }
        }

        if (!Main.Settings.AllowUnmarkedSorcerers)
        {
            return;
        }

        SorcerousDraconicBloodline.morphotypeSubclassFilterTag = GraphicsDefinitions.MorphotypeSubclassFilterTag
            .Default;
        SorcerousManaPainter.morphotypeSubclassFilterTag = GraphicsDefinitions.MorphotypeSubclassFilterTag
            .Default;
        SorcerousChildRift.morphotypeSubclassFilterTag = GraphicsDefinitions.MorphotypeSubclassFilterTag
            .Default;
        SorcerousHauntedSoul.morphotypeSubclassFilterTag = GraphicsDefinitions.MorphotypeSubclassFilterTag
            .Default;
    }

    internal static void SwitchAsiAndFeat()
    {
        FeatureSetAbilityScoreChoice.mode = Main.Settings.EnablesAsiAndFeat
            ? FeatureDefinitionFeatureSet.FeatureSetMode.Union
            : FeatureDefinitionFeatureSet.FeatureSetMode.Exclusion;
    }

    internal static void SwitchBarbarianFightingStyle()
    {
        if (Main.Settings.EnableBarbarianFightingStyle)
        {
            Barbarian.FeatureUnlocks.TryAdd(
                new FeatureUnlockByLevel(FightingStyleChoiceBarbarian, 2));
        }
        else
        {
            Barbarian.FeatureUnlocks
                .RemoveAll(x => x.level == 2 &&
                                x.FeatureDefinition == FightingStyleChoiceBarbarian);
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            Barbarian.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }

    internal static void SwitchDragonbornElementalBreathUsages()
    {
        var powers = DatabaseRepository.GetDatabase<FeatureDefinitionPower>()
            .Where(x => x.Name.StartsWith("PowerDragonbornBreathWeapon"));

        foreach (var power in powers)
        {
            if (Main.Settings.ChangeDragonbornElementalBreathUsages)
            {
                power.usesAbilityScoreName = AttributeDefinitions.Constitution;
                power.usesDetermination = UsesDetermination.AbilityBonusPlusFixed;
                power.fixedUsesPerRecharge = 0;
            }
            else
            {
                power.usesAbilityScoreName = AttributeDefinitions.Charisma;
                power.usesDetermination = UsesDetermination.Fixed;
                power.fixedUsesPerRecharge = 1;
            }
        }
    }

    private static void SwitchDruidKindredBeastToUseCustomInvocationPools()
    {
        var kindredSpirits = FeatureSetKindredSpiritChoice.FeatureSet;

        var kindredSpiritsSprites = new Dictionary<string, byte[]>
        {
            { "PowerKindredSpiritApe", Resources.SpiritApe },
            { "PowerKindredSpiritBear", Resources.SpiritBear },
            { "PowerKindredSpiritEagle", Resources.SpiritEagle },
            { "PowerKindredSpiritSpider", Resources.SpiritSpider },
            { "PowerKindredSpiritViper", Resources.SpiritViper },
            { "PowerKindredSpiritWolf", Resources.SpiritWolf }
        };

        foreach (var featureDefinitionPower in kindredSpirits.OfType<FeatureDefinitionPower>())
        {
            var monsterName = featureDefinitionPower.EffectDescription.EffectForms[0].SummonForm.MonsterDefinitionName;
            var monsterDefinition = GetDefinition<MonsterDefinition>(monsterName);
            var guiPresentation = monsterDefinition.GuiPresentation;
            var powerName = featureDefinitionPower.Name;
            var sprite = kindredSpiritsSprites.TryGetValue(powerName, out var resource)
                ? Sprites.GetSprite(powerName, resource, 128)
                : monsterDefinition.GuiPresentation.SpriteReference;

            _ = CustomInvocationDefinitionBuilder
                .Create($"CustomInvocation{monsterName}")
                .SetGuiPresentation(guiPresentation.Title, guiPresentation.Description, sprite)
                .SetPoolType(InvocationPoolTypeCustom.Pools.KindredSpiritChoice)
                .SetGrantedFeature(featureDefinitionPower)
                .AddCustomSubFeatures(ModifyInvocationVisibility.Marker)
                .AddToDB();
        }

        // replace the original features with custom invocation pools

        if (!Main.Settings.ImproveLevelUpFeaturesSelection)
        {
            return;
        }

        var replacedFeatures = CircleKindred.FeatureUnlocks
            .Select(x => x.FeatureDefinition == FeatureSetKindredSpiritChoice
                ? new FeatureUnlockByLevel(InvocationPoolKindredSpiritChoice, x.Level)
                : x)
            .ToList();

        CircleKindred.FeatureUnlocks.SetRange(replacedFeatures);
    }

    internal static void SwitchEveryFourLevelsFeats(bool isMiddle = false)
    {
        var levels = isMiddle ? new[] { 6, 14 } : [2, 10, 18];
        var dbCharacterClassDefinition = DatabaseRepository.GetDatabase<CharacterClassDefinition>();
        var pointPool1BonusFeats = GetDefinition<FeatureDefinitionPointPool>("PointPool1BonusFeats");
        var pointPool2BonusFeats = GetDefinition<FeatureDefinitionPointPool>("PointPool2BonusFeats");
        var enable = isMiddle
            ? Main.Settings.EnableFeatsAtEveryFourLevelsMiddle
            : Main.Settings.EnableFeatsAtEveryFourLevels;

        foreach (var characterClassDefinition in dbCharacterClassDefinition)
        {
            foreach (var level in levels)
            {
                var featureUnlockPointPool1 = new FeatureUnlockByLevel(pointPool1BonusFeats, level);
                var featureUnlockPointPool2 = new FeatureUnlockByLevel(pointPool2BonusFeats, level);

                if (enable)
                {
                    characterClassDefinition.FeatureUnlocks.Add(ShouldBe2Points()
                        ? featureUnlockPointPool2
                        : featureUnlockPointPool1);
                }
                else
                {
                    if (ShouldBe2Points())
                    {
                        characterClassDefinition.FeatureUnlocks.RemoveAll(x =>
                            x.FeatureDefinition == pointPool2BonusFeats && x.level == level);
                    }
                    else
                    {
                        characterClassDefinition.FeatureUnlocks.RemoveAll(x =>
                            x.FeatureDefinition == pointPool1BonusFeats && x.level == level);
                    }
                }

                continue;

                bool ShouldBe2Points()
                {
                    return (characterClassDefinition == Rogue && level is 10 && !isMiddle) ||
                           (characterClassDefinition == Fighter && level is 6 or 14 && isMiddle);
                }
            }

            if (Main.Settings.EnableSortingFutureFeatures)
            {
                characterClassDefinition.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
            }
        }
    }

    internal static void SwitchFighterWeaponSpecialization()
    {
        var levels = new[] { 8, 16 };

        if (Main.Settings.EnableFighterWeaponSpecialization)
        {
            foreach (var level in levels)
            {
                Fighter.FeatureUnlocks.TryAdd(
                    new FeatureUnlockByLevel(MartialWeaponMaster.InvocationPoolSpecialization, level));
            }
        }
        else
        {
            foreach (var level in levels)
            {
                Fighter.FeatureUnlocks
                    .RemoveAll(x => x.level == level &&
                                    x.FeatureDefinition == MartialWeaponMaster.InvocationPoolSpecialization);
            }
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            Fighter.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }

    internal static void SwitchFirstLevelTotalFeats()
    {
        if (PreviousTotalFeatsGrantedFirstLevel > -1)
        {
            UnloadRacesLevel1Feats(PreviousTotalFeatsGrantedFirstLevel, PreviousAlternateHuman);
        }

        PreviousTotalFeatsGrantedFirstLevel = Main.Settings.TotalFeatsGrantedFirstLevel;
        PreviousAlternateHuman = Main.Settings.EnableAlternateHuman;
        LoadRacesLevel1Feats(Main.Settings.TotalFeatsGrantedFirstLevel, Main.Settings.EnableAlternateHuman);
    }

    internal static void SwitchHelpPower()
    {
        var dbCharacterRaceDefinition = DatabaseRepository.GetDatabase<CharacterRaceDefinition>();
        var subRaces = dbCharacterRaceDefinition
            .SelectMany(x => x.SubRaces);
        var races = dbCharacterRaceDefinition
            .Where(x => !subRaces.Contains(x));

        if (Main.Settings.AddHelpActionToAllRaces)
        {
            foreach (var characterRaceDefinition in races
                         .Where(a => !a.FeatureUnlocks.Exists(x =>
                             x.Level == 1 && x.FeatureDefinition == FeatureDefinitionPowerHelpAction)))
            {
                characterRaceDefinition.FeatureUnlocks.Add(
                    new FeatureUnlockByLevel(FeatureDefinitionPowerHelpAction, 1));
            }
        }
        else
        {
            foreach (var characterRaceDefinition in races
                         .Where(a => a.FeatureUnlocks.Exists(x =>
                             x.Level == 1 && x.FeatureDefinition == FeatureDefinitionPowerHelpAction)))
            {
                characterRaceDefinition.FeatureUnlocks.RemoveAll(x =>
                    x.Level == 1 && x.FeatureDefinition == FeatureDefinitionPowerHelpAction);
            }
        }
    }

    internal static void SwitchDarknessPerceptive()
    {
        var races = new List<CharacterRaceDefinition>
        {
            RaceKoboldBuilder.SubraceDarkKobold,
            SubraceDarkelfBuilder.SubraceDarkelf,
            SubraceGrayDwarfBuilder.SubraceGrayDwarf
        };

        if (Main.Settings.AddDarknessPerceptiveToDarkRaces)
        {
            foreach (var characterRaceDefinition in races
                         .Where(a => !a.FeatureUnlocks.Exists(x =>
                             x.Level == 1 && x.FeatureDefinition == AbilityCheckAffinityDarknessPerceptive)))
            {
                characterRaceDefinition.FeatureUnlocks.Add(
                    new FeatureUnlockByLevel(AbilityCheckAffinityDarknessPerceptive, 1));
            }
        }
        else
        {
            foreach (var characterRaceDefinition in races
                         .Where(a => a.FeatureUnlocks.Exists(x =>
                             x.Level == 1 && x.FeatureDefinition == AbilityCheckAffinityDarknessPerceptive)))
            {
                characterRaceDefinition.FeatureUnlocks.RemoveAll(x =>
                    x.Level == 1 && x.FeatureDefinition == AbilityCheckAffinityDarknessPerceptive);
            }
        }
    }

    internal static void SwitchMonkAbundantKi()
    {
        if (Main.Settings.EnableMonkAbundantKi)
        {
            Monk.FeatureUnlocks.TryAdd(
                new FeatureUnlockByLevel(AttributeModifierMonkAbundantKi, 2));
        }
        else
        {
            Monk.FeatureUnlocks
                .RemoveAll(x => x.level == 2 &&
                                x.FeatureDefinition == AttributeModifierMonkAbundantKi);
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            Monk.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }

    internal static void SwitchMonkFightingStyle()
    {
        if (Main.Settings.EnableMonkFightingStyle)
        {
            Monk.FeatureUnlocks.TryAdd(
                new FeatureUnlockByLevel(FightingStyleChoiceMonk, 2));
        }
        else
        {
            Monk.FeatureUnlocks
                .RemoveAll(x => x.level == 2 &&
                                x.FeatureDefinition == FightingStyleChoiceMonk);
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            Monk.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }

    internal static void SwitchMonkDoNotRequireAttackActionForBonusUnarmoredAttack()
    {
        if (Main.Settings.EnableMonkDoNotRequireAttackActionForBonusUnarmoredAttack)
        {
            PowerMonkMartialArts.GuiPresentation.description =
                "Feature/&AttackModifierMonkMartialArtsUnarmedStrikeBonusDescription";
            PowerMonkMartialArts.GuiPresentation.title =
                "Feature/&AttackModifierMonkMartialArtsUnarmedStrikeBonusTitle";
            PowerMonkMartialArts.GuiPresentation.hidden = true;
            PowerMonkMartialArts.activationTime = ActivationTime.NoCost;
        }
        else
        {
            PowerMonkMartialArts.GuiPresentation.description = "Action/&MartialArtsDescription";
            PowerMonkMartialArts.GuiPresentation.title = "Action/&MartialArtsTitle";
            PowerMonkMartialArts.GuiPresentation.hidden = false;
            PowerMonkMartialArts.activationTime = ActivationTime.OnAttackHitMartialArts;
        }

        if (Main.Settings.EnableMonkDoNotRequireAttackActionForBonusUnarmoredAttack)
        {
            Monk.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }

    internal static void SwitchMonkDoNotRequireAttackActionForFlurry()
    {
        if (Main.Settings.EnableMonkDoNotRequireAttackActionForFlurry)
        {
            FeatureSetMonkFlurryOfBlows.GuiPresentation.description =
                "Feature/&FeatureSetAlternateMonkFlurryOfBlowsDescription";
            FeatureSetMonkFlurryOfBlows.GuiPresentation.title =
                "Feature/&FeatureSetAlternateMonkFlurryOfBlowsTitle";
            WayOfTheTempest.FeatureSetTempestFury.GuiPresentation.description =
                "Feature/&FeatureSetWayOfTheTempestAlternateTempestFuryDescription";
            WayOfTheTempest.FeatureSetTempestFury.GuiPresentation.title =
                "Feature/&FeatureSetWayOfTheTempestAlternateTempestFuryTitle";
        }
        else
        {
            FeatureSetMonkFlurryOfBlows.GuiPresentation.description = "Feature/&FeatureSetMonkFlurryOfBlowsDescription";
            FeatureSetMonkFlurryOfBlows.GuiPresentation.title = "Feature/&FeatureSetMonkFlurryOfBlowsTitle";
            WayOfTheTempest.FeatureSetTempestFury.GuiPresentation.description =
                "Feature/&FeatureSetWayOfTheTempestTempestFuryDescription";
            WayOfTheTempest.FeatureSetTempestFury.GuiPresentation.title =
                "Feature/&FeatureSetWayOfTheTempestTempestFuryTitle";
        }
    }

    internal static void SwitchMonkImprovedUnarmoredMovementToMoveOnTheWall()
    {
        if (Main.Settings.EnableMonkImprovedUnarmoredMovementToMoveOnTheWall)
        {
            MovementAffinityMonkUnarmoredMovementImproved.GuiPresentation.description =
                "Feature/&MonkAlternateUnarmoredMovementImprovedDescription";
            MovementAffinityMonkUnarmoredMovementImproved.GuiPresentation.title =
                "Feature/&MonkAlternateUnarmoredMovementImprovedTitle";
            MovementAffinityMonkUnarmoredMovementImproved.canMoveOnWalls = true;
        }
        else
        {
            MovementAffinityMonkUnarmoredMovementImproved.GuiPresentation.description =
                "Feature/&MonkUnarmoredMovementImprovedDescription";
            MovementAffinityMonkUnarmoredMovementImproved.GuiPresentation.title =
                "Feature/&MonkUnarmoredMovementImprovedTitle";
            MovementAffinityMonkUnarmoredMovementImproved.canMoveOnWalls = true;
        }
    }

    internal static void SwitchMonkWeaponSpecialization()
    {
        var levels = new[] { 2, 11 };

        if (Main.Settings.EnableMonkWeaponSpecialization)
        {
            foreach (var level in levels)
            {
                Monk.FeatureUnlocks.TryAdd(
                    new FeatureUnlockByLevel(InvocationPoolMonkWeaponSpecialization, level));
            }
        }
        else
        {
            foreach (var level in levels)
            {
                Monk.FeatureUnlocks
                    .RemoveAll(x => x.level == level &&
                                    x.FeatureDefinition == InvocationPoolMonkWeaponSpecialization);
            }
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            Monk.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }

    private static void SwitchPathOfTheElementsElementalFuryToUseCustomInvocationPools()
    {
        var elementalFuries = PathOfTheElements.FeatureSetElementalFury.FeatureSet;

        var elementalFuriesSprites = new Dictionary<string, BaseDefinition>
        {
            { "Storm", PowerDomainElementalLightningBlade },
            { "Blizzard", PowerDomainElementalIceLance },
            { "Wildfire", PowerDomainElementalFireBurst }
        };

        foreach (var featureDefinitionAncestry in elementalFuries.OfType<FeatureDefinitionAncestry>())
        {
            var name = featureDefinitionAncestry.Name.Replace("AncestryPathOfTheElements", string.Empty);
            var guiPresentation = featureDefinitionAncestry.guiPresentation;

            _ = CustomInvocationDefinitionBuilder
                .Create($"CustomInvocationPathOfTheElements{name}")
                .SetGuiPresentation(guiPresentation.Title, guiPresentation.Description, elementalFuriesSprites[name])
                .SetPoolType(InvocationPoolTypeCustom.Pools.PathOfTheElementsElementalFuryChoiceChoice)
                .SetGrantedFeature(featureDefinitionAncestry)
                .AddCustomSubFeatures(ModifyInvocationVisibility.Marker)
                .AddToDB();
        }

        // replace the original features with custom invocation pools
        if (!Main.Settings.ImproveLevelUpFeaturesSelection)
        {
            return;
        }

        var subclass = GetDefinition<CharacterSubclassDefinition>(PathOfTheElements.Name);
        var replacedFeatures = subclass.FeatureUnlocks
            .Select(x => x.FeatureDefinition == PathOfTheElements.FeatureSetElementalFury
                ? new FeatureUnlockByLevel(InvocationPoolPathOfTheElementsElementalFuryChoice, x.Level)
                : x)
            .ToList();

        subclass.FeatureUnlocks.SetRange(replacedFeatures);
    }

    internal static void SwitchRangerHumanoidFavoredEnemy()
    {
        if (Main.Settings.AddHumanoidFavoredEnemyToRanger)
        {
            AdditionalDamageRangerFavoredEnemyChoice.featureSet.Add(CommonBuilders
                .AdditionalDamageMarshalFavoredEnemyHumanoid);
        }
        else
        {
            AdditionalDamageRangerFavoredEnemyChoice.featureSet.Remove(CommonBuilders
                .AdditionalDamageMarshalFavoredEnemyHumanoid);
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            AdditionalDamageRangerFavoredEnemyChoice.FeatureSet.Sort((x, y) =>
                String.Compare(x.FormatTitle(), y.FormatTitle(), StringComparison.CurrentCulture));
        }
    }

    internal static void SwitchRangerNatureShroud()
    {
        if (Main.Settings.EnableRangerNatureShroudAt10)
        {
            Ranger.FeatureUnlocks.TryAdd(
                new FeatureUnlockByLevel(FeatureDefinitionPowerNatureShroud, 10));
        }
        else
        {
            Ranger.FeatureUnlocks
                .RemoveAll(x => x.level == 10
                                && x.FeatureDefinition == FeatureDefinitionPowerNatureShroud);
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            Ranger.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }

    private static void SwitchRangerToUseCustomInvocationPools()
    {
        const string Name = "Ranger";

        //
        // Terrain Type Affinity
        //

        var dbFeatureDefinitionTerrainTypeAffinity =
            DatabaseRepository.GetDatabase<FeatureDefinitionTerrainTypeAffinity>();

        var terrainAffinitySprites = new Dictionary<string, byte[]>
        {
            { "Arctic", Resources.TerrainAffinityArctic },
            { "Coast", Resources.TerrainAffinityCoast },
            { "Desert", Resources.TerrainAffinityDesert },
            { "Forest", Resources.TerrainAffinityForest },
            { "Grassland", Resources.TerrainAffinityGrassland },
            { "Mountain", Resources.TerrainAffinityMountain },
            { "Swamp", Resources.TerrainAffinitySwamp }
        };

        foreach (var featureDefinitionTerrainTypeAffinity in dbFeatureDefinitionTerrainTypeAffinity)
        {
            var terrainTypeName = featureDefinitionTerrainTypeAffinity.TerrainType;
            var terrainType = GetDefinition<TerrainTypeDefinition>(terrainTypeName);
            var guiPresentation = terrainType.GuiPresentation;
            var sprite = Sprites.GetSprite(terrainTypeName, terrainAffinitySprites[terrainTypeName], 128);
            var terrainTitle = Gui.Localize($"Environment/&{terrainTypeName}Title");

            _ = CustomInvocationDefinitionBuilder
                .Create($"CustomInvocation{Name}TerrainType{terrainTypeName}")
                .SetGuiPresentation(
                    Gui.Format(guiPresentation.Title, terrainTitle),
                    Gui.Format(guiPresentation.Description, terrainTitle),
                    sprite)
                .SetPoolType(InvocationPoolTypeCustom.Pools.RangerTerrainTypeAffinity)
                .SetGrantedFeature(featureDefinitionTerrainTypeAffinity)
                .AddCustomSubFeatures(ModifyInvocationVisibility.Marker)
                .AddToDB();
        }

        //
        // Preferred Enemy
        //

        var preferredEnemies = AdditionalDamageRangerFavoredEnemyChoice.FeatureSet;

        var preferredEnemySprites = new Dictionary<string, byte[]>
        {
            { "Aberration", Resources.PreferredEnemyAberration },
            { "Beast", Resources.PreferredEnemyBeast },
            { "Celestial", Resources.PreferredEnemyCelestial },
            { "Construct", Resources.PreferredEnemyConstruct },
            { "Dragon", Resources.PreferredEnemyDragon },
            { "Elemental", Resources.PreferredEnemyElemental },
            { "Fey", Resources.PreferredEnemyFey },
            { "Fiend", Resources.PreferredEnemyFiend },
            { "Giant", Resources.PreferredEnemyGiant },
            { "Humanoid", Resources.PreferredEnemyHumanoid },
            { "Monstrosity", Resources.PreferredEnemyMonstrosity },
            { "Ooze", Resources.PreferredEnemyOoze },
            { "Plant", Resources.PreferredEnemyPlant },
            { "Undead", Resources.PreferredEnemyUndead }
        };

        foreach (var featureDefinitionPreferredEnemy in preferredEnemies.OfType<FeatureDefinitionAdditionalDamage>())
        {
            var preferredEnemyName = featureDefinitionPreferredEnemy.RequiredCharacterFamily.Name;
            var guiPresentation = featureDefinitionPreferredEnemy.RequiredCharacterFamily.GuiPresentation;
            var sprite = Sprites.GetSprite(preferredEnemyName, preferredEnemySprites[preferredEnemyName], 128);
            var enemyTitle = Gui.Localize($"CharacterFamily/&{preferredEnemyName}Title");

            _ = CustomInvocationDefinitionBuilder
                .Create($"CustomInvocation{Name}PreferredEnemy{preferredEnemyName}")
                .SetGuiPresentation(
                    Gui.Format(guiPresentation.Title, enemyTitle),
                    Gui.Format(guiPresentation.Description, enemyTitle),
                    sprite)
                .SetPoolType(InvocationPoolTypeCustom.Pools.RangerPreferredEnemy)
                .SetGrantedFeature(featureDefinitionPreferredEnemy)
                .AddCustomSubFeatures(ModifyInvocationVisibility.Marker)
                .AddToDB();
        }

        // replace the original features with custom invocation pools

        if (!Main.Settings.ImproveLevelUpFeaturesSelection)
        {
            return;
        }

        // Ranger

        var replacedFeatures = Ranger.FeatureUnlocks
            .Select(x =>
                x.FeatureDefinition == TerrainTypeAffinityRangerNaturalExplorerChoice
                    ? new FeatureUnlockByLevel(InvocationPoolRangerTerrainType, x.Level)
                    : x.FeatureDefinition == AdditionalDamageRangerFavoredEnemyChoice
                        ? new FeatureUnlockByLevel(InvocationPoolRangerPreferredEnemy, x.Level)
                        : x)
            .ToList();

        Ranger.FeatureUnlocks.SetRange(replacedFeatures);

        // Ranger Survivalist

        var rangerSurvivalist = GetDefinition<CharacterSubclassDefinition>("RangerSurvivalist");

        replacedFeatures = rangerSurvivalist.FeatureUnlocks
            .Select(x =>
                x.FeatureDefinition == AdditionalDamageRangerFavoredEnemyChoice
                    ? new FeatureUnlockByLevel(InvocationPoolRangerPreferredEnemy, x.Level)
                    : x)
            .ToList();

        rangerSurvivalist.FeatureUnlocks.SetRange(replacedFeatures);
    }

    internal static void SwitchScimitarWeaponSpecialization()
    {
        var proficiencies = new List<FeatureDefinitionProficiency> { ProficiencyBardWeapon, ProficiencyRogueWeapon };

        foreach (var proficiency in proficiencies)
        {
            if (Main.Settings.GrantScimitarSpecializationToBardRogue)
            {
                proficiency.Proficiencies.TryAdd(WeaponTypeDefinitions.ScimitarType.Name);
            }
            else
            {
                proficiency.Proficiencies.Remove(WeaponTypeDefinitions.ScimitarType.Name);
            }
        }
    }

    private static void SwitchSubclassAncestriesToUseCustomInvocationPools(
        string name,
        CharacterSubclassDefinition characterSubclassDefinition,
        FeatureDefinitionFeatureSet featureDefinitionFeatureSet,
        FeatureDefinition featureDefinitionCustomInvocationPool,
        InvocationPoolTypeCustom invocationPoolTypeCustom)
    {
        var draconicAncestries = featureDefinitionFeatureSet.FeatureSet;

        var draconicAncestriesSprites = new Dictionary<string, byte[]>
        {
            { $"Ancestry{name}DraconicBlack", Resources.BlackDragon },
            { $"Ancestry{name}DraconicBlue", Resources.BlueDragon },
            { $"Ancestry{name}DraconicGold", Resources.GoldDragon },
            { $"Ancestry{name}DraconicGreen", Resources.GreenDragon },
            { $"Ancestry{name}DraconicSilver", Resources.SilverDragon }
        };

        foreach (var featureDefinitionAncestry in draconicAncestries.OfType<FeatureDefinitionAncestry>())
        {
            var ancestryName = featureDefinitionAncestry.Name;
            var sprite = Sprites.GetSprite(ancestryName, draconicAncestriesSprites[$"{ancestryName}"], 128);

            _ = CustomInvocationDefinitionBuilder
                .Create($"CustomInvocation{ancestryName}")
                .SetGuiPresentation(
                    featureDefinitionAncestry.GuiPresentation.Title,
                    Gui.Format("Feature/&AncestryLevelUpDraconicDescription",
                        Gui.Localize($"Rules/&{featureDefinitionAncestry.damageType}Title")),
                    sprite)
                .SetPoolType(invocationPoolTypeCustom)
                .SetGrantedFeature(featureDefinitionAncestry)
                .AddCustomSubFeatures(ModifyInvocationVisibility.Marker)
                .AddToDB();
        }

        if (!Main.Settings.ImproveLevelUpFeaturesSelection)
        {
            return;
        }

        var replacedFeatures = characterSubclassDefinition.FeatureUnlocks
            .Select(x => x.FeatureDefinition == featureDefinitionFeatureSet
                ? new FeatureUnlockByLevel(featureDefinitionCustomInvocationPool, x.Level)
                : x)
            .ToList();

        characterSubclassDefinition.FeatureUnlocks.SetRange(replacedFeatures);
    }


    private static void BuildFeatureUnlocks(
        int initialFeats,
        bool alternateHuman,
        [CanBeNull] out FeatureUnlockByLevel featureUnlockByLevelNonHuman,
        [CanBeNull] out FeatureUnlockByLevel featureUnlockByLevelHuman)
    {
        string name;

        featureUnlockByLevelNonHuman = null;
        featureUnlockByLevelHuman = null;

        switch (initialFeats)
        {
            case 0:
            {
                if (alternateHuman)
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(PointPoolBonusFeat, 1);
                }

                break;
            }
            case 1:
            {
                featureUnlockByLevelNonHuman = new FeatureUnlockByLevel(PointPoolBonusFeat, 1);

                name = "PointPool2BonusFeats";
                if (alternateHuman && TryGetDefinition<FeatureDefinitionPointPool>(name, out var pointPool2BonusFeats))
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(pointPool2BonusFeats, 1);
                }

                break;
            }
            case > 1:
            {
                name = $"PointPool{initialFeats}BonusFeats";
                if (TryGetDefinition<FeatureDefinitionPointPool>(name, out var featureDefinitionPointPool))
                {
                    featureUnlockByLevelNonHuman = new FeatureUnlockByLevel(featureDefinitionPointPool, 1);
                }

                name = $"PointPool{initialFeats + 1}BonusFeats";
                if (alternateHuman && TryGetDefinition<FeatureDefinitionPointPool>(name, out var pointPoolXBonusFeats))
                {
                    featureUnlockByLevelHuman = new FeatureUnlockByLevel(pointPoolXBonusFeats, 1);
                }

                break;
            }
        }
    }

    private static void LoadRacesLevel1Feats(int initialFeats, bool alternateHuman)
    {
        var human = Human;

        BuildFeatureUnlocks(initialFeats, alternateHuman, out var featureUnlockByLevelNonHuman,
            out var featureUnlockByLevelHuman);

        foreach (var characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>())
        {
            if (IsSubRace(characterRaceDefinition))
            {
                continue;
            }

            if (alternateHuman && characterRaceDefinition == human)
            {
                if (featureUnlockByLevelHuman != null)
                {
                    human.FeatureUnlocks.Add(featureUnlockByLevelHuman);
                }

                var pointPoolAbilityScoreImprovement =
                    new FeatureUnlockByLevel(PointPoolAbilityScoreImprovement, 1);
                human.FeatureUnlocks.Add(pointPoolAbilityScoreImprovement);

                var pointPoolHumanSkillPool = new FeatureUnlockByLevel(PointPoolHumanSkillPool, 1);
                human.FeatureUnlocks.Add(pointPoolHumanSkillPool);

                Remove(human,
                    FeatureDefinitionAttributeModifiers
                        .AttributeModifierHumanAbilityScoreIncrease);
            }
            else
            {
                if (featureUnlockByLevelNonHuman != null)
                {
                    characterRaceDefinition.FeatureUnlocks.Add(featureUnlockByLevelNonHuman);
                }
            }
        }
    }

    private static void UnloadRacesLevel1Feats(int initialFeats, bool alternateHuman)
    {
        var human = Human;

        BuildFeatureUnlocks(initialFeats, alternateHuman,
            out var featureUnlockByLevelNonHuman,
            out var featureUnlockByLevelHuman);

        foreach (var characterRaceDefinition in DatabaseRepository.GetDatabase<CharacterRaceDefinition>())
        {
            if (IsSubRace(characterRaceDefinition))
            {
                continue;
            }

            if (alternateHuman && characterRaceDefinition == human)
            {
                if (featureUnlockByLevelHuman != null)
                {
                    Remove(human, featureUnlockByLevelHuman);
                }

                Remove(human, PointPoolAbilityScoreImprovement);
                Remove(human, PointPoolHumanSkillPool);

                var humanAttributeIncrease = new FeatureUnlockByLevel(
                    FeatureDefinitionAttributeModifiers.AttributeModifierHumanAbilityScoreIncrease, 1);

                human.FeatureUnlocks.Add(humanAttributeIncrease);
            }
            else
            {
                if (featureUnlockByLevelNonHuman != null)
                {
                    Remove(characterRaceDefinition, featureUnlockByLevelNonHuman);
                }
            }
        }
    }

    private static void Remove(
        [NotNull] CharacterRaceDefinition characterRaceDefinition,
        BaseDefinition toRemove)
    {
        var ndx = -1;

        for (var i = 0; i < characterRaceDefinition.FeatureUnlocks.Count; i++)
        {
            if (characterRaceDefinition.FeatureUnlocks[i].Level == 1 &&
                characterRaceDefinition.FeatureUnlocks[i].FeatureDefinition == toRemove)
            {
                ndx = i;
            }
        }

        if (ndx >= 0)
        {
            characterRaceDefinition.FeatureUnlocks.RemoveAt(ndx);
        }
    }

    private static void Remove(
        [NotNull] CharacterRaceDefinition characterRaceDefinition,
        [NotNull] FeatureUnlockByLevel featureUnlockByLevel)
    {
        Remove(characterRaceDefinition, featureUnlockByLevel.FeatureDefinition);
    }

    private static bool IsSubRace(CharacterRaceDefinition raceDefinition)
    {
        return DatabaseRepository.GetDatabase<CharacterRaceDefinition>()
            .Any(crd => crd.SubRaces.Contains(raceDefinition));
    }

    private sealed class FilterTargetingPositionPowerTeleportSummon : IFilterTargetingPosition
    {
        public IEnumerator ComputeValidPositions(CursorLocationSelectPosition cursorLocationSelectPosition)
        {
            cursorLocationSelectPosition.validPositionsCache.Clear();

            var gameLocationPositioningService = ServiceRepository.GetService<IGameLocationPositioningService>();
            var source = cursorLocationSelectPosition.ActionParams.ActingCharacter;
            var summoner = source.RulesetCharacter.GetMySummoner();
            var boxInt = new BoxInt(
                summoner.LocationPosition, new int3(-1, -1, -1), new int3(1, 1, 1));

            foreach (var position in boxInt.EnumerateAllPositionsWithin())
            {
                if (gameLocationPositioningService.CanPlaceCharacter(
                        source, position, CellHelpers.PlacementMode.Station) &&
                    gameLocationPositioningService.CanCharacterStayAtPosition_Floor(
                        source, position, onlyCheckCellsWithRealGround: true))
                {
                    cursorLocationSelectPosition.validPositionsCache.Add(position);
                }

                if (cursorLocationSelectPosition.stopwatch.Elapsed.TotalMilliseconds > 0.5)
                {
                    yield return null;
                }
            }
        }
    }

    private sealed class RollSavingThrowInitiatedIndomitableSaving : IRollSavingThrowInitiated
    {
        public void OnSavingThrowInitiated(
            RulesetCharacter caster,
            RulesetCharacter defender,
            ref int saveBonus,
            ref string abilityScoreName,
            BaseDefinition sourceDefinition,
            List<TrendInfo> modifierTrends,
            List<TrendInfo> advantageTrends,
            ref int rollModifier,
            int saveDC,
            bool hasHitVisual,
            ref RollOutcome outcome,
            ref int outcomeDelta,
            List<EffectForm> effectForms)
        {
            var classLevel = defender!.GetClassLevel(Fighter);

            saveBonus += classLevel;
            modifierTrends.Add(
                new TrendInfo(classLevel, FeatureSourceType.CharacterFeature, "Feature/&IndomitableResistanceTitle",
                    null));
        }
    }

    internal sealed class MonkWeaponSpecialization
    {
        internal WeaponTypeDefinition WeaponType { get; set; }
    }

    private sealed class MonkWeaponSpecializationDiceUpgrade : IValidateContextInsteadOfRestrictedProperty
    {
        private readonly WeaponTypeDefinition _weaponTypeDefinition;

        internal MonkWeaponSpecializationDiceUpgrade(WeaponTypeDefinition weaponTypeDefinition)
        {
            _weaponTypeDefinition = weaponTypeDefinition;
        }

        public (OperationType, bool) ValidateContext(
            BaseDefinition definition,
            IRestrictedContextProvider provider,
            RulesetCharacter character,
            ItemDefinition itemDefinition,
            bool rangedAttack, RulesetAttackMode attackMode,
            RulesetEffect rulesetEffect)
        {
            var attackModeWeaponType =
                (attackMode?.SourceDefinition as ItemDefinition)?.WeaponDescription.WeaponTypeDefinition;

            return (OperationType.Or,
                character.HasMonkShieldExpert() ||
                character.GetSubFeaturesByType<MonkWeaponSpecializationDiceUpgrade>().Exists(
                    x => x._weaponTypeDefinition == attackModeWeaponType));
        }
    }

    #region Rogue Cunning Strike

    internal static readonly ConditionDefinition ConditionReduceSneakDice = ConditionDefinitionBuilder
        .Create("ConditionReduceSneakDice")
        .SetGuiPresentationNoContent(true)
        .SetSilent(Silent.WhenAddedOrRemoved)
        .SetConditionType(ConditionType.Detrimental)
        .SetAmountOrigin(ConditionDefinition.OriginOfAmount.Fixed)
        .AddToDB();

    private static FeatureDefinitionFeatureSet _featureSetRogueCunningStrike;
    private static FeatureDefinitionFeatureSet _featureSetRogueDeviousStrike;
    private static readonly char[] Separator = ['\t'];

    private static void BuildRogueCunningStrike()
    {
        const string Cunning = "RogueCunningStrike";
        const string Devious = "RogueDeviousStrike";

        var powerPool = FeatureDefinitionPowerBuilder
            .Create($"Power{Cunning}")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.Reaction)
            .SetReactionContext(ExtraReactionContext.Custom)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 1)
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 6, TargetType.Individuals)
                    .Build())
            .AddToDB();

        powerPool.AddCustomSubFeatures(IsModifyPowerPool.Marker,
            new PhysicalAttackInitiatedByMeCunningStrike(powerPool));

        // Disarm

        var combatAffinityDisarmed = FeatureDefinitionCombatAffinityBuilder
            .Create($"CombatAffinity{Cunning}Disarmed")
            .SetGuiPresentation($"Condition{Cunning}Disarmed", Category.Condition, Gui.NoLocalization)
            .SetMyAttackAdvantage(AdvantageType.Disadvantage)
            .AddToDB();

        var conditionDisarmed = ConditionDefinitionBuilder
            .Create($"Condition{Cunning}Disarmed")
            .SetGuiPresentation(Category.Condition, Gui.NoLocalization, ConditionDefinitions.ConditionBaned)
            .SetConditionType(ConditionType.Detrimental)
            .AddFeatures(combatAffinityDisarmed)
            .AddToDB();

        var powerDisarm = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Cunning}Disarm")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerPool)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                    .SetDurationData(DurationType.Round, 1)
                    .SetSavingThrowData(false, AttributeDefinitions.Dexterity, false,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency, AttributeDefinitions.Dexterity, 8)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .SetConditionForm(conditionDisarmed, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden, PowerUsesSneakDiceTooltipModifier.Instance)
            .AddToDB();

        // Poison

        var powerPoison = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Cunning}Poison")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerPool)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                    .SetDurationData(DurationType.Minute, 1)
                    .SetSavingThrowData(false, AttributeDefinitions.Constitution, false,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency, AttributeDefinitions.Dexterity, 8)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .HasSavingThrow(EffectSavingThrowType.Negates, TurnOccurenceType.EndOfTurn, true)
                            .SetConditionForm(
                                ConditionDefinitions.ConditionPoisoned, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden, PowerUsesSneakDiceTooltipModifier.Instance)
            .AddToDB();

        // Trip

        var powerTrip = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Cunning}Trip")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerPool)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                    .SetSavingThrowData(false, AttributeDefinitions.Dexterity, false,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency, AttributeDefinitions.Dexterity, 8)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .SetMotionForm(MotionForm.MotionType.FallProne)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden, PowerUsesSneakDiceTooltipModifier.Instance)
            .AddToDB();

        // Withdraw

        _ = ActionDefinitionBuilder
            .Create(StepBack, "Withdraw")
            .SetOrUpdateGuiPresentation(Category.Action)
            .SetActionId(ExtraActionId.Withdraw)
            .SetActionType(ActionDefinitions.ActionType.NoCost)
            .SetAddedConditionName(string.Empty)
            .SetMaxCells(3)
            .RequiresAuthorization()
            .AddToDB();

        var actionAffinityWithdraw = FeatureDefinitionActionAffinityBuilder
            .Create(ActionAffinitySorcererMetamagicToggle, "ActionAffinityWithdraw")
            .SetGuiPresentationNoContent(true)
            .SetAuthorizedActions((ActionDefinitions.Id)ExtraActionId.Withdraw)
            .AddToDB();

        var conditionWithdraw = ConditionDefinitionBuilder
            .Create($"Condition{Cunning}Withdraw")
            .SetGuiPresentation($"Condition/&Condition{Cunning}WithdrawTitle", Gui.NoLocalization,
                ConditionDefinitions.ConditionDisengaging)
            .SetPossessive()
            .SetSilent(Silent.WhenRemoved)
            .AddFeatures(actionAffinityWithdraw)
            .AddToDB();

        var powerWithdraw = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Cunning}Withdraw")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerPool)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(EffectFormBuilder.ConditionForm(conditionWithdraw))
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden, PowerUsesSneakDiceTooltipModifier.Instance)
            .AddToDB();

        //
        // DEVIOUS STRIKES - LEVEL 14
        //

        // Dazed

        var actionAffinityDazedOnlyMovement = FeatureDefinitionActionAffinityBuilder
            .Create($"ActionAffinity{Devious}DazedOnlyMovement")
            .SetGuiPresentationNoContent(true)
            .SetAllowedActionTypes(false, false, freeOnce: false, reaction: false, noCost: false)
            .AddToDB();

        var conditionDazedOnlyMovement = ConditionDefinitionBuilder
            .Create($"Condition{Devious}DazedOnlyMovement")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetConditionType(ConditionType.Detrimental)
            .AddFeatures(actionAffinityDazedOnlyMovement)
            .AddToDB();

        var actionAffinityDazed = FeatureDefinitionActionAffinityBuilder
            .Create($"ActionAffinity{Devious}Dazed")
            .SetGuiPresentationNoContent(true)
            .SetAllowedActionTypes(reaction: false, bonus: false)
            .AddToDB();

        var conditionDazed = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionDazzled, $"Condition{Devious}Dazed")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionDazzled)
            .SetConditionType(ConditionType.Detrimental)
            .SetFeatures(actionAffinityDazed)
            .AddCustomSubFeatures(new ActionFinishedByMeDazed(conditionDazedOnlyMovement))
            .AddToDB();

        var powerDaze = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Devious}Daze")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerPool, 2)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                    .SetDurationData(DurationType.Round, 1)
                    .SetSavingThrowData(false, AttributeDefinitions.Constitution, false,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency, AttributeDefinitions.Dexterity, 8)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .SetConditionForm(conditionDazed, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden, PowerUsesSneakDiceTooltipModifier.Instance)
            .AddToDB();

        // Knock Out

        var conditionKnockOut = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionIncapacitated, $"Condition{Devious}KnockOut")
            .SetGuiPresentation(Category.Condition, Gui.NoLocalization,
                ConditionDefinitions.ConditionAsleep)
            .SetSpecialInterruptions(ConditionInterruption.Damaged)
            .AddToDB();

        var powerKnockOut = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Devious}KnockOut")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerPool, 6)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                    .SetDurationData(DurationType.Minute, 1)
                    .SetSavingThrowData(false, AttributeDefinitions.Constitution, false,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency, AttributeDefinitions.Dexterity, 8)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .HasSavingThrow(EffectSavingThrowType.Negates, TurnOccurenceType.EndOfTurn, true)
                            .SetConditionForm(conditionKnockOut, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden, PowerUsesSneakDiceTooltipModifier.Instance)
            .AddToDB();

        // Obscure

        var powerObscure = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Devious}Obscure")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.NoCost, powerPool, 3)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.Individuals)
                    .SetDurationData(DurationType.Round, 1)
                    .SetSavingThrowData(false, AttributeDefinitions.Dexterity, false,
                        EffectDifficultyClassComputation.AbilityScoreAndProficiency, AttributeDefinitions.Dexterity, 8)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .SetConditionForm(ConditionDefinitions.ConditionBlinded,
                                ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden, PowerUsesSneakDiceTooltipModifier.Instance)
            .AddToDB();

        // MAIN

        PowerBundle.RegisterPowerBundle(powerPool, true,
            powerDisarm, powerPoison, powerTrip, powerWithdraw, powerDaze, powerKnockOut, powerObscure);

        var actionAffinityToggle = FeatureDefinitionActionAffinityBuilder
            .Create(ActionAffinitySorcererMetamagicToggle, "ActionAffinityCunningStrikeToggle")
            .SetGuiPresentationNoContent(true)
            .SetAuthorizedActions((ActionDefinitions.Id)ExtraActionId.CunningStrikeToggle)
            .AddToDB();

        _featureSetRogueCunningStrike = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Cunning}")
            .SetGuiPresentation($"Power{Cunning}", Category.Feature)
            .AddFeatureSet(powerPool, actionAffinityToggle, powerDisarm, powerPoison, powerTrip, powerWithdraw)
            .AddToDB();

        _featureSetRogueDeviousStrike = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Devious}")
            .SetGuiPresentation($"Power{Devious}", Category.Feature)
            .AddFeatureSet(powerDaze, powerKnockOut, powerObscure)
            .AddToDB();
    }

    internal static bool IsSneakAttackValid(
        ActionModifier attackModifier,
        GameLocationCharacter attacker,
        GameLocationCharacter defender)
    {
        // only trigger if haven't used sneak attack yet
        if (!attacker.OncePerTurnIsValid("AdditionalDamageRogueSneakAttack") ||
            !attacker.OncePerTurnIsValid("AdditionalDamageRoguishDuelistDaringDuel") ||
            !attacker.OncePerTurnIsValid("AdditionalDamageRoguishUmbralStalkerDeadlyShadows"))
        {
            return false;
        }

        var advantageType = ComputeAdvantage(attackModifier.attackAdvantageTrends);

        return advantageType switch
        {
            AdvantageType.Advantage => true,
            AdvantageType.Disadvantage => false,
            _ =>
                // it's an attack with a nearby enemy (standard sneak attack)
                ServiceRepository.GetService<IGameLocationBattleService>()
                    .IsConsciousCharacterOfSideNextToCharacter(defender, attacker.Side, attacker) ||
                // it's a Duelist and target is dueling with him
                RoguishDuelist.TargetIsDuelingWithRoguishDuelist(attacker, defender, advantageType) ||
                // it's an Umbral Stalker and source and target are in dim light or darkness
                RoguishUmbralStalker.SourceAndTargetAreNotBrightAndWithin5Ft(attacker, defender, advantageType)
        };
    }

    private sealed class PhysicalAttackInitiatedByMeCunningStrike(FeatureDefinitionPower powerRogueCunningStrike) :
        IAttackBeforeHitConfirmedOnEnemy, IPhysicalAttackFinishedByMe
    {
        private FeatureDefinitionPower _selectedPower;

        public IEnumerator OnAttackBeforeHitConfirmedOnEnemy(
            GameLocationBattleManager battleManager,
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
            _selectedPower = null;

            var rulesetAttacker = attacker.RulesetCharacter;

            if (!rulesetAttacker.IsToggleEnabled((ActionDefinitions.Id)ExtraActionId.CunningStrikeToggle) ||
                !IsSneakAttackValid(actionModifier, attacker, defender))
            {
                yield break;
            }

            var actionManager = ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;

            if (actionManager == null ||
                battleManager is not { IsBattleInProgress: true })
            {
                yield break;
            }

            var implementationManagerService =
                ServiceRepository.GetService<IRulesetImplementationService>() as RulesetImplementationManager;

            var usablePower = PowerProvider.Get(powerRogueCunningStrike, rulesetAttacker);
            var actionParams = new CharacterActionParams(attacker, ActionDefinitions.Id.PowerNoCost)
            {
                ActionModifiers = { actionModifier },
                StringParameter = powerRogueCunningStrike.Name,
                RulesetEffect = implementationManagerService
                    .MyInstantiateEffectPower(rulesetAttacker, usablePower, false),
                UsablePower = usablePower,
                TargetCharacters = { defender }
            };

            var count = actionManager.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestSpendBundlePower(actionParams);

            actionManager.AddInterruptRequest(reactionRequest);

            yield return battleManager.WaitForReactions(attacker, actionManager, count);

            if (!actionParams.ReactionValidated)
            {
                yield break;
            }

            // determine selected power to collect cost
            var option = reactionRequest.SelectedSubOption;
            var subPowers = powerRogueCunningStrike.GetBundle()?.SubPowers;

            if (subPowers == null)
            {
                yield break;
            }

            _selectedPower = subPowers[option];

            // inflict condition passing power cost on amount to be deducted later on from sneak dice
            rulesetAttacker.InflictCondition(
                ConditionReduceSneakDice.Name,
                DurationType.Round,
                0,
                TurnOccurenceType.EndOfTurn,
                AttributeDefinitions.TagEffect,
                rulesetAttacker.guid,
                rulesetAttacker.CurrentFaction.Name,
                1,
                ConditionReduceSneakDice.Name,
                _selectedPower.CostPerUse,
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
            if (_selectedPower == null || _selectedPower.EffectDescription.RangeType != RangeType.MeleeHit)
            {
                yield break;
            }

            var power = _selectedPower;

            _selectedPower = null;

            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false })
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;

            var implementationManagerService =
                ServiceRepository.GetService<IRulesetImplementationService>() as RulesetImplementationManager;

            var usablePower = PowerProvider.Get(power, rulesetAttacker);
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
    }

    private sealed class ActionFinishedByMeDazed(
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        ConditionDefinition conditionDazedOnlyMovement) : IActionFinishedByMe
    {
        public IEnumerator OnActionFinishedByMe(CharacterAction characterAction)
        {
            if (characterAction is not CharacterActionMove)
            {
                yield break;
            }

            var rulesetCharacter = characterAction.ActingCharacter.RulesetCharacter;

            rulesetCharacter.InflictCondition(
                conditionDazedOnlyMovement.Name,
                DurationType.Round,
                0,
                TurnOccurenceType.EndOfTurn,
                AttributeDefinitions.TagEffect,
                rulesetCharacter.guid,
                rulesetCharacter.CurrentFaction.Name,
                1,
                conditionDazedOnlyMovement.Name,
                0,
                0,
                0);
        }
    }

    internal static void SwitchRogueCunningStrike()
    {
        if (Main.Settings.EnableRogueCunningStrike)
        {
            Rogue.FeatureUnlocks.TryAdd(new FeatureUnlockByLevel(_featureSetRogueCunningStrike, 5));
            Rogue.FeatureUnlocks.TryAdd(new FeatureUnlockByLevel(_featureSetRogueDeviousStrike, 14));
        }
        else
        {
            Rogue.FeatureUnlocks.RemoveAll(x => x.level == 5 && x.FeatureDefinition == _featureSetRogueCunningStrike);
            Rogue.FeatureUnlocks.RemoveAll(x => x.level == 14 && x.FeatureDefinition == _featureSetRogueDeviousStrike);
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            Rogue.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }

    internal static void SwitchRogueFightingStyle()
    {
        if (Main.Settings.EnableRogueFightingStyle)
        {
            Rogue.FeatureUnlocks.TryAdd(
                new FeatureUnlockByLevel(FightingStyleChoiceRogue, 2));
        }
        else
        {
            Rogue.FeatureUnlocks.RemoveAll(x => x.level == 2 && x.FeatureDefinition == FightingStyleChoiceRogue);
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            Rogue.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }

    internal static void SwitchRogueSteadyAim()
    {
        if (Main.Settings.EnableRogueSteadyAim)
        {
            Rogue.FeatureUnlocks.TryAdd(new FeatureUnlockByLevel(RangedCombatFeats.PowerFeatSteadyAim, 3));
        }
        else
        {
            Rogue.FeatureUnlocks.RemoveAll(x =>
                x.level == 3 && x.FeatureDefinition == RangedCombatFeats.PowerFeatSteadyAim);
        }

        if (Main.Settings.EnableSortingFutureFeatures)
        {
            Rogue.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }
    }

    private static void SwitchRogueStrSaving()
    {
        var powerNames = new List<string>
        {
            "PowerRogueCunningStrikeDisarm",
            //"PowerRogueCunningStrikePoison",
            "PowerRogueCunningStrikeTrip",
            //"PowerRogueCunningStrikeWithdraw",
            //"PowerRogueDeviousStrikeDaze",
            //"PowerRogueDeviousStrikeKnockOut",
            "PowerRogueDeviousStrikeObscure",
            "PowerRoguishOpportunistDebilitatingStrike",
            "PowerRoguishOpportunistImprovedDebilitatingStrike",
            "PowerRoguishBladeCallerHailOfBlades"
        };

        foreach (var power in DatabaseRepository.GetDatabase<FeatureDefinitionPower>()
                     .Where(x => powerNames.Contains(x.Name)))
        {
            power.AddCustomSubFeatures(new ModifyEffectDescriptionSavingThrowRogue(power));
        }
    }

    #endregion
}
