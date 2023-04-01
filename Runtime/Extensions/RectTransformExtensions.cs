using UnityEngine;

namespace WindowManager
{
    public static class RectTransformExtensions
    {
        public static void StretchToParentSize(this RectTransform target)
        {
            var parent = target.transform.parent.GetComponent<RectTransform>();
            target.anchoredPosition = parent.position;
            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.pivot = Vector2.one * 0.5f;
            target.sizeDelta = Vector2.zero;
            target.localScale = Vector3.one;
            target.ForceUpdateRectTransforms();
        }

        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            // Get the bottom left corner.
            var position = corners[0];

            var lossyScale = rectTransform.lossyScale;
            var rect = rectTransform.rect;
            var size = new Vector2(
                lossyScale.x * rect.size.x,
                lossyScale.y * rect.size.y);

            return new Rect(position, size);
        }

        public static void SetTopAnchor(this RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            var width = rect.width;
            var height = rect.height;

            rectTransform.anchorMin = new Vector2(0f, 1);
            rectTransform.anchorMax = new Vector2(0f, 1);
            rectTransform.pivot = new Vector2(0.0f, 1);

            rectTransform.sizeDelta = new Vector2(width, height);
        }

        private static void SetAndStretchToParentSize(this RectTransform target, RectTransform parent)
        {
            target.anchoredPosition = parent.position;
            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.pivot = Vector2.one * 0.5f;
            target.sizeDelta = parent.rect.size;
            target.transform.SetParent(parent);
            target.localScale = Vector3.one;
            target.ForceUpdateRectTransforms();
        }
    }
}