using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AdriKat.Toolkit.Attributes
{
    using UnityEditor;
    
    [CustomPropertyDrawer(typeof(VarNameAttribute))]
    public class VarNameDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var varNameAttribute = (VarNameAttribute)attribute;

            var field = new PropertyField(property)
            {
                label = varNameAttribute.DisplayName,
                bindingPath = property.propertyPath
            };

            return field;
        }
    }
}