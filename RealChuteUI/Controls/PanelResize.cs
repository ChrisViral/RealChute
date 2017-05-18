using RealChuteUI.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RealChuteUI.Controls
{
    /// <summary>
    /// Allows resizing of panel when dragging this RectTransform
    /// </summary>
    [RequireComponent(typeof(RectTransform)), AddComponentMenu("UI/Panel Resize"), DisallowMultipleComponent]
    public class PanelResize : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        #region Fields
        [SerializeField]
        private Vector2 min = new Vector2(100, 100);    //Min size
        [SerializeField]
        private Vector2 max = new Vector2(400, 400);    //Max size
        private RectTransform panelTransform, parentTransform;  //Panel/parent transform
        private Vector2 originalMousePos, originalSizeDelta;    //Mouse position/panel size on mouse down
        #endregion

        #region Methods
        /// <summary>
        /// Fires on mouse down over this transform
        /// </summary>
        /// <param name="data">Event data</param>
        public void OnPointerDown(PointerEventData data)
        {
            this.originalSizeDelta = this.panelTransform.sizeDelta;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.panelTransform, data.position, data.pressEventCamera, out this.originalMousePos);
        }

        /// <summary>
        /// Fires on mouse drag
        /// </summary>
        /// <param name="data">Event data</param>
        public void OnDrag(PointerEventData data)
        {
            //Resize
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.panelTransform, data.position, data.pressEventCamera, out Vector2 localMousePos);
            Vector2 offset = localMousePos - this.originalMousePos;
            Vector2 sizeDelta = this.originalSizeDelta + new Vector2(offset.x, -offset.y);
            sizeDelta = UIUtils.ClampVector2(sizeDelta, this.min, this.max);

            //Make sure we're not outside of the draw zone
            Vector2 pos = this.panelTransform.localPosition;
            Vector2 bounds = new Vector2(this.parentTransform.rect.max.x - this.panelTransform.rect.max.x, this.parentTransform.rect.min.y - this.panelTransform.rect.min.y) - offset;
            Vector2 currentDelta = this.panelTransform.sizeDelta;
            this.panelTransform.sizeDelta = new Vector2(pos.x > bounds.x ? currentDelta.x : sizeDelta.x, pos.y < bounds.y ? currentDelta.y : sizeDelta.y);
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
