using NUnit.Framework;
using UnityEngine;

namespace HoloGraffiti
{
    public class HandMenuControllerTests
    {
        [Test]
        public void OnExportPressed_NoExportersAssigned_DoesNotThrow()
        {
            // Create a HandMenuController with no exporters assigned.
            // It should log warnings but not throw an exception.
            var menuObject = new GameObject("HandMenuTest");
            var controller = menuObject.AddComponent<HandMenuController>();

            Assert.DoesNotThrow(() => controller.OnExportPressed());

            Object.DestroyImmediate(menuObject);
        }

        [Test]
        public void HandMenuController_CanBeAddedToGameObject()
        {
            // Verify the component attaches without errors.
            var menuObject = new GameObject("HandMenuTest");
            var controller = menuObject.AddComponent<HandMenuController>();

            Assert.IsNotNull(controller);

            Object.DestroyImmediate(menuObject);
        }
    }
}