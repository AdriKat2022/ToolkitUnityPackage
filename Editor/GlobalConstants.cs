using AdriKat.Toolkit.Attributes;
using UnityEngine;

namespace AdriKat.Toolkit
{
    internal static class GlobalConstants
    {
        public static Color GetColorFromWarningType(WarningTypeEnum warningType)
        {
            return warningType switch
            {
                WarningTypeEnum.Info => new Color(.75f, 1, 1),
                WarningTypeEnum.Warning => Color.yellow,
                WarningTypeEnum.Error => new Color(1f, .5f, .4f),
                _ => new Color(1, 1, 1),
            };
        }
    }
}