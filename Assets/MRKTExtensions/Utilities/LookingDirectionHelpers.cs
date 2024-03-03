using System.Linq;
using UnityEngine;

namespace MRTKExtensions.Utilities
{
    public static class LookingDirectionHelpers
    {
        /// <summary>
        /// Get a position spatial map right ahead of the camera viewing direction on a maximum distance
        /// and failing that, a position dead ahead
        /// </summary>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static Vector3 GetPositionInLookingDirection(float maxDistance = 2)
        {
            var hitPoint = GetPositionOnSpatialMap(maxDistance);
            return hitPoint ?? CalculatePositionDeadAhead(maxDistance);
        }

        /// <summary>
        /// Get a position on the spatial map right ahead of the camera viewing direction
        /// </summary>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static Vector3? GetPositionOnSpatialMap(float maxDistance = 2)
        {
            var transform = Camera.main.transform;
            var headRay = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(headRay, out var hitInfo, maxDistance, GetSpatialMeshMask()))
            {
                return hitInfo.point;
            }
            return null;
        }

        
        //TODO
        private static int meshPhysicsLayer = 0;

        /// <summary>
        /// Determine the spatial mask(s) from the configuration
        /// </summary>
        /// <returns></returns>
        private static int GetSpatialMeshMask()
        {
            if (meshPhysicsLayer == 0)
            {
                meshPhysicsLayer |= 1 << 31;
            }
            return meshPhysicsLayer;
        }

        /// <summary>
        /// Calculate a position right ahead of the camera viewing direction
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 CalculatePositionDeadAhead(float distance = 2 )
        { 
            return Camera.main.transform.position + Camera.main.transform.forward.normalized * distance;
        }

        /// <summary>
        /// Get a camera position from either a stabilizer or the Camera itself
        /// </summary>
        /// <returns></returns>
        private static Vector3 GetCameraPosition()
        {
            return Camera.main.transform.position;
        }

        /// <summary>
        /// Looks where an object must be placed if it is not to intersect with other objects, or the spatial map
        /// Can be used to place an object on top of another object (or at a distance from it).
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="maxDistance"></param>
        /// <param name="distanceFromObstruction"></param>
        /// <param name="layerMask"></param>
        /// <param name="showDebugLines"></param>
        /// <returns></returns>
        public static Vector3 GetObjectBeforeObstruction(GameObject obj, float maxDistance = 2,
            float distanceFromObstruction = 0.02f, int layerMask = Physics.DefaultRaycastLayers,
            bool showDebugLines = false)
        {
            var totalBounds = obj.GetEncapsulatingBounds();

            var transform = Camera.main.transform;
            var headRay = new Ray(transform.position, transform.forward);


            // Project the object forward, get all hits *except those involving child objects of the main object*
            var hits = Physics.BoxCastAll(GetCameraPosition(),
                                          totalBounds.extents, headRay.direction,
                                          Quaternion.identity, maxDistance, layerMask)
                                          .Where(h => !h.transform.IsChildOf(obj.transform)).ToList();

            // This factor compensates for the fact object center and bounds center for some reason are not always the same
            var centerCorrection = obj.transform.position - totalBounds.center;

            if (showDebugLines)
            {
                BoxCastHelper.DrawBoxCastBox(GetCameraPosition(),
                    totalBounds.extents, headRay.direction,
                    Quaternion.identity, maxDistance, Color.green);
            }

            var orderedHits = hits.OrderBy(p => p.distance).Where(q => q.distance > 0.1f).ToList();

            if (orderedHits.Any())
            {
                var closestHit = orderedHits.First();
                //Find the closest hit - we need to move the object forward to that position

                //We need a vector from the camera to the hit...
                var hitVector = closestHit.point - GetCameraPosition();

                //But the hit needs to be projected on our dead ahead vector, as the first hit may not be right in front of us
                var gazeVector = CalculatePositionDeadAhead(closestHit.distance * 2) - GetCameraPosition();
                var projectedHitVector = Vector3.Project(hitVector, gazeVector);
#if UNITY_EDITOR
                if (showDebugLines)
                {
                    Debug.DrawLine(GetCameraPosition(), closestHit.point, Color.yellow);
                    Debug.DrawRay(GetCameraPosition(), gazeVector, Color.blue);
                    Debug.DrawRay(GetCameraPosition(), projectedHitVector, Color.magenta);
                    BoxCastHelper.DrawBox(totalBounds.center, totalBounds.extents, Quaternion.identity, Color.red);
                }
#endif

                //If we use the projectedHitVector to add to the cameraposition, the CENTER of our object will end up 
                // against the obstruction, so we need to know who much the object is extending from the center in the direction of the hit.
                // So we make a ray from the center that intersects with the object's own bounds.
                var edgeRay = new Ray(totalBounds.center, projectedHitVector);
                float edgeDistance;
                if(totalBounds.IntersectRay(edgeRay,  out edgeDistance))
                {
                    if (showDebugLines)
                    {
                        Debug.DrawRay(totalBounds.center, projectedHitVector.normalized * Mathf.Abs(edgeDistance + distanceFromObstruction),
                            Color.cyan);
                    }
                }

                // The new position is not camera position plus the projected hit vector, minus distance to the edge and a possible extra distance
                // we want to keep.
                return GetCameraPosition() +
                            projectedHitVector - projectedHitVector.normalized * Mathf.Abs(edgeDistance + distanceFromObstruction) +
                            centerCorrection;
            }

            return CalculatePositionDeadAhead(maxDistance) + centerCorrection;
        }
    }
}
