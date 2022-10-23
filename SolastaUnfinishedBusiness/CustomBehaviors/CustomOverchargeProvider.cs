﻿using System.Linq;
using JetBrains.Annotations;

namespace SolastaUnfinishedBusiness.CustomBehaviors;

internal interface ICustomOverchargeProvider
{
    public (int, int)[] OverchargeSteps(RulesetCharacter character);
}

internal delegate (int, int)[] OverchargeStepsHandler(RulesetCharacter character);

[UsedImplicitly]
internal class CustomOverchargeProvider : ICustomOverchargeProvider
{
    private readonly OverchargeStepsHandler _handler;

    internal CustomOverchargeProvider(OverchargeStepsHandler handler)
    {
        _handler = handler;
    }

    public (int, int)[] OverchargeSteps(RulesetCharacter character)
    {
        return _handler(character);
    }

    internal static int GetAdvancementFromOvercharge(int overcharge, (int, int)[] steps)
    {
        if (steps == null || steps.Length == 0)
        {
            return 0;
        }

        return (from step in steps where step.Item1 == overcharge select step.Item2).FirstOrDefault();
    }
}
