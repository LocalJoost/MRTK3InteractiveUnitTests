using System;
using System.Collections;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Tests;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InteractionTests
{ 
    public abstract class BaseRuntimeHandInputTests : BaseRuntimeInputTests
    {
        protected const int HandMoveSteps = 1;
        protected const int UpdateFrames = 1;
        protected const float HandInFrontOfGameObject = 0.15f;
        protected const float InitialHandInFrontOfUserDistance = 0.2f;
        
        protected GameObject InstantiatePrefab(string guid)
        {
            var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Object));
            return Object.Instantiate(prefab) as GameObject;
        }
        
        protected IEnumerator GetHand(Vector3 initialHandPosition, Action<TestHand> action)
        {
            var hand = new TestHand(Handedness.Right);
            yield return hand.Show(initialHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates(UpdateFrames);
            action(hand);
        }

        protected Vector3 GetInitialHandPositionBefore(
            GameObject testGameObject, 
            float initialDistance = HandInFrontOfGameObject)
        {
            return testGameObject.transform.position - Vector3.forward * initialDistance;
        }
        
        protected Vector3 GetInitialHandPosition(
            float initialDistance = InitialHandInFrontOfUserDistance)
        {
            return InputTestUtilities.InFrontOfUser(Vector3.forward * initialDistance);
        }

        protected IEnumerator PokeHand(TestHand hand, float distance)
        {
            yield return MoveHand(hand, Vector3.forward * distance);
            yield return MoveHand(hand, -Vector3.forward * distance);
        }

        protected IEnumerator MoveHand(TestHand hand, Vector3 distance)
        {
            yield return hand.Move(distance, HandMoveSteps);
            yield return RuntimeTestUtilities.WaitForUpdates(UpdateFrames);
        }
        
        protected IEnumerator MoveHandTo(TestHand hand, Vector3 location)
        {
            yield return hand.MoveTo(location, HandMoveSteps);
            yield return RuntimeTestUtilities.WaitForUpdates(UpdateFrames);
        }
        
        protected GameObject FindByName(GameObject parent, string name)
        {
            if (parent.name == name)
            {
                return parent;
            }
            foreach (Transform child in parent.transform)
            {
                var result = FindByName(child.gameObject, name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}