using System;
using UnityEngine;

namespace AdriKat.Toolkit.Mathematics
{
    /// <summary>
    /// Vector2 wrapper with a "use normalised value" boolean.<br/>
    /// Has a property drawer to nicely show in the editor.
    /// </summary>
    [Serializable]
    public struct Vector2N
    {
        public Vector2 value;
        public bool useNormalisedValue;

        public Vector2N(Vector2 value, bool useNormalisedValue)
        {
            this.value = value;
            this.useNormalisedValue = useNormalisedValue;
        }
        
        public Vector2N(Vector2 value)
        {
            this.value = value;
            this.useNormalisedValue = false;
        }

        public Vector2 Get()
        {
            return useNormalisedValue ? value.normalized : value;
        }
    }
    
    /// <summary>
    /// Vector3 wrapper with a "use normalised value" boolean.<br/>
    /// Has a property drawer to nicely show in the editor.
    /// </summary>
    [Serializable]
    public struct Vector3N
    {
        public Vector3 value;
        public bool useNormalisedValue;

        public Vector3N(Vector3 value, bool useNormalisedValue)
        {
            this.value = value;
            this.useNormalisedValue = useNormalisedValue;
        }
        
        public Vector3N(Vector3 value)
        {
            this.value = value;
            this.useNormalisedValue = false;
        }

        public Vector3 Get()
        {
            return useNormalisedValue ? value.normalized : value;
        }
    }
}