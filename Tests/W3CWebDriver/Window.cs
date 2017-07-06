﻿//******************************************************************************
//
// Copyright (c) 2016 Microsoft Corporation. All rights reserved.
//
// This code is licensed under the MIT License (MIT).
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//******************************************************************************

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium.Windows;

namespace W3CWebDriver
{
    [TestClass]
    public class Window
    {
        [TestMethod]
        public void CloseWindow()
        {
            WindowsDriver<WindowsElement> singleWindowSession = Utility.CreateNewSession(CommonTestSettings.AlarmClockAppId);
            Assert.IsNotNull(singleWindowSession.SessionId);

            // Close the application window without deleting the session
            singleWindowSession.Close();
            Assert.IsNotNull(singleWindowSession);
            Assert.IsNotNull(singleWindowSession.SessionId);

            // Delete the session
            singleWindowSession.Quit();
        }

        [TestMethod]
        public void CloseWindowErrorNoSuchWindow()
        {
            // Attempt to close the previously closed application window
            try
            {
                Utility.GetOrphanedSession().Close();
                Assert.Fail("Exception should have been thrown");
            }
            catch (System.InvalidOperationException e)
            {
                Assert.AreEqual(ErrorStrings.NoSuchWindow, e.Message);
            }
        }

        [TestMethod]
        public void GetWindowHandle()
        {
            WindowsDriver<WindowsElement> session = Utility.CreateNewSession(CommonTestSettings.CalculatorAppId);
            Assert.IsNotNull(session.SessionId);

            string windowHandle = session.CurrentWindowHandle;
            Assert.IsNotNull(windowHandle);
            session.Quit();
            session = null;
        }

        [TestMethod]
        public void GetWindowHandleErrorNoSuchWindow()
        {
            try
            {
                string windowHandle = Utility.GetOrphanedSession().CurrentWindowHandle;
                Assert.Fail("Exception should have been thrown");
            }
            catch (System.InvalidOperationException e)
            {
                Assert.AreEqual(ErrorStrings.NoSuchWindow, e.Message);
            }
        }

        [TestMethod]
        public void GetWindowHandlesClassicApp()
        {
            WindowsDriver<WindowsElement> session = Utility.CreateNewSession(CommonTestSettings.NotepadAppId);
            Assert.IsNotNull(session);
            Assert.IsNotNull(session.SessionId);

            var handles = session.WindowHandles;
            Assert.IsNotNull(handles);
            Assert.IsTrue(handles.Count > 0);
            session.Quit();
        }

        [TestMethod]
        public void GetWindowHandlesModernApp()
        {
            WindowsDriver<WindowsElement> multiWindowsSession = Utility.CreateNewSession(CommonTestSettings.EdgeAppId);
            Assert.IsNotNull(multiWindowsSession);
            Assert.IsNotNull(multiWindowsSession.SessionId);

            var windowHandlesBefore = multiWindowsSession.WindowHandles;
            Assert.IsNotNull(windowHandlesBefore);
            Assert.IsTrue(windowHandlesBefore.Count > 0);

            // Preserve previously opened Edge window(s) and only manipulate newly opened windows
            List<String> previouslyOpenedEdgeWindows = new List<String>(windowHandlesBefore);
            previouslyOpenedEdgeWindows.Remove(multiWindowsSession.CurrentWindowHandle);

            // Open a new window
            multiWindowsSession.Keyboard.SendKeys(OpenQA.Selenium.Keys.Control + "n" + OpenQA.Selenium.Keys.Control);

            System.Threading.Thread.Sleep(3000); // Sleep for 3 seconds
            var windowHandlesAfter = multiWindowsSession.WindowHandles;
            Assert.IsNotNull(windowHandlesAfter);
            Assert.AreEqual(windowHandlesBefore.Count + 1, windowHandlesAfter.Count);

            // Preserve previously opened Edge Windows by only closing newly opened windows
            List<String> newlyOpenedEdgeWindows = new List<String>(windowHandlesAfter);
            foreach (var previouslyOpenedEdgeWindow in previouslyOpenedEdgeWindows)
            {
                newlyOpenedEdgeWindows.Remove(previouslyOpenedEdgeWindow);
            }

            foreach (var windowHandle in newlyOpenedEdgeWindows)
            {
                multiWindowsSession.SwitchTo().Window(windowHandle);
                multiWindowsSession.Close();
            }

            multiWindowsSession.Quit();
        }

        [TestMethod]
        public void SwitchWindows()
        {
            WindowsDriver<WindowsElement> multiWindowsSession = Utility.CreateNewSession(CommonTestSettings.EdgeAppId);
            Assert.IsNotNull(multiWindowsSession);
            Assert.IsNotNull(multiWindowsSession.SessionId);

            // Preserve previously opened Edge window(s) and only manipulate newly opened windows
            List<String> previouslyOpenedEdgeWindows = new List<String>(multiWindowsSession.WindowHandles);
            previouslyOpenedEdgeWindows.Remove(multiWindowsSession.CurrentWindowHandle);

            // Open a new window
            multiWindowsSession.Keyboard.SendKeys(OpenQA.Selenium.Keys.Control + "n" + OpenQA.Selenium.Keys.Control);
            System.Threading.Thread.Sleep(5000); // Sleep for 5 seconds
            var multipleWindowHandles = multiWindowsSession.WindowHandles;
            Assert.IsTrue(multipleWindowHandles.Count > 1);

            // Preserve previously opened Edge Windows by only operating on newly opened windows
            List<String> newlyOpenedEdgeWindows = new List<String>(multipleWindowHandles);
            foreach (var previouslyOpenedEdgeWindow in previouslyOpenedEdgeWindows)
            {
                newlyOpenedEdgeWindows.Remove(previouslyOpenedEdgeWindow);
            }

            string previousWindowHandle = String.Empty;

            foreach (var windowHandle in newlyOpenedEdgeWindows)
            {
                multiWindowsSession.SwitchTo().Window(windowHandle);
                Assert.AreEqual(multiWindowsSession.CurrentWindowHandle, windowHandle);
                Assert.AreNotEqual(windowHandle, previousWindowHandle);
                previousWindowHandle = windowHandle;
                multiWindowsSession.Close();
            }

            multiWindowsSession.Quit();
        }

        [TestMethod]
        public void SwitchWindowsErrorEmptyValue()
        {
            WindowsDriver<WindowsElement> session = Utility.CreateNewSession(CommonTestSettings.CalculatorAppId);

            try
            {
                session.SwitchTo().Window(string.Empty);
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Missing Command Parameter: name", e.Message);
            }

            session.Quit();
        }

        [TestMethod]
        public void SwitchWindowsErrorForeignWindowHandle()
        {
            WindowsDriver<WindowsElement> session = Utility.CreateNewSession(CommonTestSettings.CalculatorAppId);
            WindowsDriver<WindowsElement> foreignSession = Utility.CreateNewSession(CommonTestSettings.AlarmClockAppId);
            Assert.IsNotNull(session.SessionId);
            Assert.IsNotNull(foreignSession.SessionId);

            // Get a foreign window handle from a different application/process under foreignSession
            var foreignTopLevelWindow = foreignSession.CurrentWindowHandle;
            Assert.IsFalse(string.IsNullOrEmpty(foreignTopLevelWindow));

            try
            {
                session.SwitchTo().Window(foreignTopLevelWindow);
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Window handle does not belong to the same process/application", e.Message);
            }

            foreignSession.Quit();
            session.Quit();
        }

        [TestMethod]
        public void SwitchWindowsErrorInvalidValue()
        {
            WindowsDriver<WindowsElement> session = Utility.CreateNewSession(CommonTestSettings.CalculatorAppId);

            try
            {
                session.SwitchTo().Window("-1");
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception e)
            {
                Assert.AreEqual("String cannot contain a minus sign if the base is not 10.", e.Message);
            }

            session.Quit();
        }

        [TestMethod]
        public void SwitchWindowsErrorNonTopLevelWindowHandle()
        {
            WindowsDriver<WindowsElement> session = Utility.CreateNewSession(CommonTestSettings.CalculatorAppId);
            var nonTopLevelWindowHandle = session.FindElementByClassName("Windows.UI.Core.CoreWindow").GetAttribute("NativeWindowHandle");
            var nonTopLevelWindowHandleHex = Convert.ToInt32(nonTopLevelWindowHandle).ToString("x");

            try
            {
                session.SwitchTo().Window(nonTopLevelWindowHandleHex); // This needs to be in Hex e.g. 0x00880088
                Assert.Fail("Exception should have been thrown");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.EndsWith("is not a top level window handle"));
            }

            session.Quit();
        }

        [TestMethod]
        public void SwitchWindowsErrorNoSuchWindow()
        {
            WindowsDriver<WindowsElement> session = Utility.CreateNewSession(CommonTestSettings.CalculatorAppId);

            // Get an orphaned window handle from a closed application
            var orphanedTopLevelWindow = Utility.GetOrphanedWindowHandle();
            System.Threading.Thread.Sleep(3000);

            try
            {
                session.SwitchTo().Window(orphanedTopLevelWindow);
                Assert.Fail("Exception should have been thrown");
            }
            catch (System.InvalidOperationException e)
            {
                Assert.AreEqual("A request to switch to a window could not be satisfied because the window could not be found.", e.Message);
            }

            session.Quit();
        }
    }

    [TestClass]
    public class WindowTransform
    {
        protected static WindowsDriver<WindowsElement> WindowTransformSession;
        protected static System.Drawing.Size OriginalSize;
        protected static System.Drawing.Point OriginalPosition;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            // Launch the Calculator app
            WindowTransformSession = Utility.CreateNewSession(CommonTestSettings.CalculatorAppId);
            Assert.IsNotNull(WindowTransformSession);
        }

        [ClassCleanup]
        public static void TearDown()
        {
            // Close the application and delete the session
            WindowTransformSession.Quit();
            WindowTransformSession = null;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // Save application window original size and position
            OriginalSize = WindowTransformSession.Manage().Window.Size;
            Assert.IsNotNull(OriginalSize);
            OriginalPosition = WindowTransformSession.Manage().Window.Position;
            Assert.IsNotNull(OriginalPosition);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Restore application window original size and position
            WindowTransformSession.Manage().Window.Size = OriginalSize;
            Assert.AreEqual(OriginalSize, WindowTransformSession.Manage().Window.Size);
            WindowTransformSession.Manage().Window.Position = OriginalPosition;
            Assert.AreEqual(OriginalPosition, WindowTransformSession.Manage().Window.Position);
        }

        [TestMethod]
        public void GetWindowPosition()
        {
            var windowPosition = WindowTransformSession.Manage().Window.Position;
            Assert.IsNotNull(windowPosition);
            Assert.AreEqual(OriginalPosition.X, windowPosition.X);
            Assert.AreEqual(OriginalPosition.Y, windowPosition.Y);
        }

        [TestMethod]
        public void GetWindowSize()
        {
            var windowSize = WindowTransformSession.Manage().Window.Size;
            Assert.IsNotNull(windowSize);
            Assert.AreEqual(OriginalSize.Height, windowSize.Height);
            Assert.AreEqual(OriginalSize.Width, windowSize.Width);
        }

        [TestMethod]
        public void MaximizeWindow()
        {
            WindowTransformSession.Manage().Window.Maximize();
            var windowSize = WindowTransformSession.Manage().Window.Size;
            Assert.IsNotNull(windowSize);
            Assert.IsTrue(OriginalSize.Height <= windowSize.Height);
            Assert.IsTrue(OriginalSize.Width <= windowSize.Width);
        }

        [TestMethod]
        public void SetWindowPosition()
        {
            int offset = 100;
            WindowTransformSession.Manage().Window.Position = new System.Drawing.Point(OriginalPosition.X + offset, OriginalPosition.Y + offset);
            var windowPosition = WindowTransformSession.Manage().Window.Position;
            Assert.IsNotNull(windowPosition);
            Assert.AreEqual(OriginalPosition.X + offset, windowPosition.X);
            Assert.AreEqual(OriginalPosition.Y + offset, windowPosition.Y);
        }

        [TestMethod]
        public void SetWindowPositionToOrigin()
        {
            var origin = new System.Drawing.Point(0, 0);
            WindowTransformSession.Manage().Window.Position = origin;
            var position = WindowTransformSession.Manage().Window.Position;
            Assert.AreEqual(origin.X, position.X);
            Assert.AreEqual(origin.Y, position.Y);
        }

        [TestMethod]
        public void SetWindowSize()
        {
            int offset = 100;
            int newWidth = 300;
            int newHeight = 500;

            WindowTransformSession.Manage().Window.Size = new System.Drawing.Size(newWidth, newHeight);
            var windowSize = WindowTransformSession.Manage().Window.Size;
            Assert.AreEqual(newWidth, windowSize.Width);
            Assert.AreEqual(newHeight, windowSize.Height);

            WindowTransformSession.Manage().Window.Size = new System.Drawing.Size(newWidth + offset, newHeight + offset);
            windowSize = WindowTransformSession.Manage().Window.Size;
            Assert.AreEqual(newWidth + offset, windowSize.Width);
            Assert.AreEqual(newHeight + offset, windowSize.Height);
        }
    }
}
