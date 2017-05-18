using UnityEngine;
using static UnityEngine.Mathf;

namespace RealChuteUI.Utils
{
    /// <summary>
    /// General Utils for working with UI
    /// </summary>
    public static class UIUtils
    {
        #region Static methods
        /// <summary>
        /// Clamps a given Vector2 between a set of minimum and maximum values
        /// </summary>
        /// <param name="v">Vector to clamp</param>
        /// <param name="min">Minimum components vector</param>
        /// <param name="max">Maximum components vector</param>
        /// <returns>A new Vector2, correctly clamped</returns>
        public static Vector2 ClampVector2(Vector2 v, Vector2 min, Vector2 max) => new Vector2(Clamp(v.x, min.x, max.x), Clamp(v.y, min.y, max.y));
        #endregion
    }
}
