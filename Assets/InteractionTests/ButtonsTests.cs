using System.Collections;
using System.Collections.Generic;
using MixedReality.Toolkit.Input.Tests;
using MixedReality.Toolkit.UX;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace InteractionTests
{
    public class ButtonsTests : BaseRuntimeHandInputTests
    {
        private const string MenuGuid = "e9ddf3517c4b9c7488c12bdec6a9917f";
        private GameObject testGameObject;
        private List<PressableButton> allButtons;

        [SetUp]
        public void Init()
        {
            testGameObject = InstantiatePrefab(MenuGuid);
            allButtons = FindByName(testGameObject, "Buttons-GridLayout").
                            GetComponentsInChildren<PressableButton>().ToList();
        }

        public override IEnumerator TearDown()
        {
            yield return base.TearDown();
            Object.Destroy(testGameObject);
        }

        [UnityTest]
        public IEnumerator ButtonPressDoesNotEnableOtherButtons()
        {
            var button = allButtons.First();
            var initialHandPosition =
                GetInitialHandPositionBefore(button.gameObject, HandInFrontOfGameObject);
            TestHand hand = null;
            yield return GetHand(initialHandPosition, h => { hand = h; });
            Assert.AreEqual(0, GetToggledButtonCount());
            yield return PokeHand(hand, HandInFrontOfGameObject);
            Assert.AreEqual(1, GetToggledButtonCount());
            yield return PokeHand(hand, HandInFrontOfGameObject);
            Assert.AreEqual(0, GetToggledButtonCount());
        }

        [UnityTest]
        public IEnumerator PressingTwoDifferentButtonsShouldOnlySelectTheLast()
        {
            var pressedButtons = new List<PressableButton>();
            var initialHandPosition = GetInitialHandPosition();
            TestHand hand = null;
            yield return GetHand(initialHandPosition, h => { hand = h; });
            Assert.AreEqual(0, GetToggledButtonCount());
            
            foreach(var button in allButtons)
            {
                var handPosition = 
                    GetInitialHandPositionBefore(button.gameObject, HandInFrontOfGameObject);
                yield return MoveHandTo(hand, handPosition);
                yield return PokeHand(hand, HandInFrontOfGameObject);
                Assert.AreEqual(1, GetToggledButtonCount());
                AddButtonToPressedList(pressedButtons);
            }
            Assert.AreEqual(allButtons.Count, pressedButtons.Count);
        }

        [UnityTest]
        public IEnumerator PressingCloseCloseMenu()
        {
            Assert.IsFalse(testGameObject == null);
            var button = 
                FindByName(testGameObject, "Close");
            var initialHandPosition =
                GetInitialHandPositionBefore(button.gameObject,
                    HandInFrontOfGameObject);
            TestHand hand = null;
            yield return GetHand(initialHandPosition, h => { hand = h; });
            
            yield return PokeHand(hand, HandInFrontOfGameObject);
            Assert.IsTrue(testGameObject == null);
        }

        private int GetToggledButtonCount()
        {
            return allButtons.Count(b => b.IsToggled);
        }

        private void AddButtonToPressedList(List<PressableButton> pressedButtons)
        {
            var button = allButtons.FirstOrDefault(b => b.IsToggled);
            if (!pressedButtons.Contains(button))
            {
                pressedButtons.Add(button);
            }
            else
            {
                Assert.Fail("Button already pressed");
            }
        }
    }
}
