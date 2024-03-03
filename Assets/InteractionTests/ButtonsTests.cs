using System;
using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Tests;
using MixedReality.Toolkit.UX;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

public class ButtonsTests : BaseRuntimeInputTests
{
    private const string MenuGuid = "e9ddf3517c4b9c7488c12bdec6a9917f";
    private static readonly string MenuGuidPath = AssetDatabase.GUIDToAssetPath(MenuGuid);
    private const int HandMoveSteps = 1;
    private const int UpdateFrames = 2;
    private GameObject testGameObject;
    private Vector3 initialTestGameObjectPosition;
    private List<PressableButton> buttons;

    [SetUp]
    public void Init()
    {
        testGameObject = InstantiatePrefab(MenuGuidPath);
        initialTestGameObjectPosition = testGameObject.transform.position;
        var gridLayout = testGameObject.GetComponentInChildren<GridLayoutGroup>();
        buttons = gridLayout.GetComponentsInChildren<PressableButton>().ToList();
    }
 
    public override IEnumerator TearDown()
    {
        yield return base.TearDown();
        Object.Destroy(testGameObject);
    }
 
    [UnityTest]
    public IEnumerator ButtonPressDoesNotEnableOtherButtons()
    {
        var initialHandPosition = GetInitialHandPosition();
        var handDelta = initialTestGameObjectPosition.z - initialHandPosition.z;
        TestHand hand = null;
        yield return GetHand(initialHandPosition, h =>
        {
            hand = h;
        });
        Assert.AreEqual(0, GetToggledButtonCount());
        yield return PokeHand(hand, handDelta);
        Assert.AreEqual(1, GetToggledButtonCount());
        yield return PokeHand(hand, handDelta);
        Assert.AreEqual(0, GetToggledButtonCount());
    }
    
    [UnityTest]
    public IEnumerator PressingTwoDifferentButtonsShouldOnlySelectTheLast()
    {
        var pressedButtons = new List<PressableButton>();
        var initialHandPosition = GetInitialHandPosition();
        var handDelta = initialTestGameObjectPosition.z - initialHandPosition.z;
        TestHand hand = null;
        yield return GetHand(initialHandPosition, h =>
        {
            hand = h;
        });
        
        yield return PokeHand(hand, handDelta);
        Assert.AreEqual(1, GetToggledButtonCount());
        AddButtonToPressedList(pressedButtons);
        
        yield return MoveHand(hand, -Vector3.right * 0.04f);
        yield return PokeHand(hand, handDelta);
        Assert.AreEqual(1, GetToggledButtonCount());
        AddButtonToPressedList(pressedButtons);
        
        yield return MoveHand(hand, -Vector3.up * 0.04f);
        yield return PokeHand(hand, handDelta);
        Assert.AreEqual(1, GetToggledButtonCount());
        AddButtonToPressedList(pressedButtons);
        
        yield return MoveHand(hand, Vector3.right * 0.04f);
        yield return PokeHand(hand, handDelta);
        Assert.AreEqual(1, GetToggledButtonCount());
        AddButtonToPressedList(pressedButtons);
    }

    [UnityTest]
    public IEnumerator PressingCloseCloseMenu()
    {
        Assert.IsFalse(testGameObject == null);
        var initialHandPosition = GetInitialHandPosition() + Vector3.up * 0.02f;
        var handDelta = initialTestGameObjectPosition.z - initialHandPosition.z;
        TestHand hand = null;
        yield return GetHand(initialHandPosition, h =>
        {
            hand = h;
        });
        yield return hand.Show(initialHandPosition);
        yield return RuntimeTestUtilities.WaitForUpdates(UpdateFrames);
        yield return PokeHand(hand, handDelta);
        Assert.IsTrue(testGameObject == null);
    }
    
    private IEnumerator GetHand(Vector3 initialHandPosition, Action<TestHand>action)
    {
        var hand = new TestHand(Handedness.Right);
        yield return hand.Show(initialHandPosition);
        yield return RuntimeTestUtilities.WaitForUpdates(UpdateFrames);
        action(hand);
    }
    
    private Vector3 GetInitialHandPosition()
    {
        return InputTestUtilities.InFrontOfUser(0.3f) + Vector3.up * 0.015f + Vector3.right * 0.005f;
    }

    private IEnumerator PokeHand(TestHand hand, float handDelta)
    {
        yield return MoveHand(hand, Vector3.forward * handDelta);
        yield return MoveHand(hand, -Vector3.forward * handDelta );
    }

    private IEnumerator MoveHand(TestHand hand, Vector3 handDelta)
    {
        yield return hand.Move(handDelta, HandMoveSteps);
        yield return RuntimeTestUtilities.WaitForUpdates(UpdateFrames);
    }

    private int GetToggledButtonCount()
    {
        return buttons.Count(b=> b.IsToggled);
    }
    
    private void AddButtonToPressedList(List<PressableButton> pressedButtons)
    {
        var button = buttons.FirstOrDefault(b=> b.IsToggled);
        if (!pressedButtons.Contains(button))
        {
            pressedButtons.Add(button);
        }
        else
        {
            Assert.Fail("Button already pressed");
        }
    }
    
    private GameObject InstantiatePrefab(string prefabPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Object));
        return Object.Instantiate(prefab) as GameObject;
    }
}
