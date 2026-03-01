using UnityEngine;
using UnityEngine.EventSystems;

namespace AdriKat.Toolkit.UIElements
{
    public class FocusOnClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }
    }
}
