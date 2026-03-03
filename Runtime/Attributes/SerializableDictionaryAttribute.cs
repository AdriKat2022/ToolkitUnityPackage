using System;

namespace AdriKat.Toolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializableDictionaryAttribute : Attribute
    {
        public DictionaryDisplay DictionaryDisplay { get; }
        
        public SerializableDictionaryAttribute(DictionaryDisplay dictionaryDisplay)
        {
            DictionaryDisplay = dictionaryDisplay;
        }
    }

    public enum DictionaryDisplay
    {
        OnePropertyPerLine,
        UsePropertyDrawer
    }
}