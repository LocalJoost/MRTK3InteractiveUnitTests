using UnityEngine;

namespace MRTKExtensions.Utilities
{
    public static class GameObjectExtensions
    {
        public static void SetHittableStatus(this GameObject gameObject, bool hittable)
        {
            foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = hittable;
            }
        }

        public static Bounds GetEncapsulatingBounds(this GameObject obj)
        {
            Bounds totalBounds = new Bounds();

            foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
            {
                if (totalBounds.size.magnitude == 0f)
                {
                    totalBounds = renderer.bounds;
                }
                else
                {
                    totalBounds.Encapsulate(renderer.bounds);
                }
            }

            return totalBounds;
        }

        public static string GetObjectText(this GameObject textContainingObject)
        {
            var component = textContainingObject.GetComponent<TextMesh>();
            return component != null ? component.text : null;
        }

        public static bool SetObjectText(this GameObject textContainingObject, string newValue)
        {
            var component = textContainingObject.GetComponent<TextMesh>();
            if (component == null)
            {
                return false;
            }

            component.text = newValue;
            return true;
        }

        public static Vector3 GetRenderedSize(this GameObject obj)
        {
            var oldQ = obj.transform.rotation;
            obj.transform.rotation = Quaternion.identity;
            var result = obj.GetComponent<Renderer>().bounds.size;
            obj.transform.rotation = oldQ;
            return result;
        }

        public static Vector3 GetRenderedSizeWithChildren(this GameObject obj)
        {
            var oldQ = obj.transform.rotation;
            obj.transform.rotation = Quaternion.identity;
            var result = obj.GetEncapsulatingBounds();
            obj.transform.rotation = oldQ;
            return result.size;
        }
    }
}
