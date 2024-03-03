using MixedReality.Toolkit.SpatialManipulation;
using MRTKExtensions.Utilities;
using UnityEngine;

namespace MRTKExtensions.Ux
{
    [RequireComponent(typeof(RadialView))]
    public class AppearInCenterViewController : MonoBehaviour
    {
        private RadialView radialView;

        protected void Awake()
        {
            radialView = GetComponent<RadialView>();
        }

        private void OnEnable()
        {
            transform.position = LookingDirectionHelpers.CalculatePositionDeadAhead(
                (radialView.MinDistance + radialView.MaxDistance) / 2.0f);
        }
    }
}
