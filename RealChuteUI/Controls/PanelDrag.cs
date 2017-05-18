using RealChuteUI.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL. */

namespace RealChuteUI.Controls
{
    /// <summary>
    /// Allows dragging the parent transform by dragging this RectTransform
    /// </summary>
    [RequireComponent(typeof(RectTransform)), AddComponentMenu("UI/Drag Panel"), DisallowMultipleComponent]
    public class PanelDrag : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        #region Fields
        private Vector2 originalMousePos, originalPanelPos;     //Mouse/panel positions on mouse hold
        private RectTransform panelTransform, parentTransform;  //Panel/parent transforms
        #endregion

        #region Methods
        /// <summary>
        /// Fires on mouse down over this transform
        /// </summary>
        /// <param name="data">Event data</param>
        public void OnPointerDown(PointerEventData data)
        {
            this.originalPanelPos = this.panelTransform.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.parentTransform, data.position, data.pressEventCamera, out this.originalMousePos);
        }

        /// <summary>
        /// Fires on mouse drag
        /// </summary>
        /// <param name="data">Event data</param>
        public void OnDrag(PointerEventData data)
        {
            //Move window
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.parentTransform, data.position, data.pressEventCamera, out Vector2 mousePos))
            {
                Vector2 offset = mousePos - this.originalMousePos;
                this.panelTransform.localPosition = this.originalPanelPos + offset;
            }

            //Clamp window 
            Vector2 pos = this.panelTransform.localPosition;
            Vector2 minPos = this.parentTransform.rect.min - this.panelTransform.rect.min;
            Vector2 maxPos = this.parentTransform.rect.max - this.panelTransform.rect.max;
            this.panelTransform.localPosition = UIUtils.ClampVector2(pos, minPos, maxPos);
        }
        #endregion

        #region Functions
        private void Awake()
        {
            this.panelTransform = (RectTransform)this.transform.parent;
            this.parentTransform = (RectTransform)this.panelTransform.parent;
        }
        #endregion
    }
}
