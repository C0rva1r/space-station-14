﻿using SS14.Client.Interfaces.Input;
using SS14.Client.Interfaces.UserInterface;
using SS14.Client.UserInterface;
using SS14.Shared;
using SS14.Shared.Utility;
using SS14.Shared.IoC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SS14.Shared.ContentPack;
using SS14.Shared.Log;
using SS14.Client.UserInterface.Controls;

namespace SS14.Client.Input
{
    public class KeyBindingManager : IKeyBindingManager
    {
        [Dependency]
        readonly IUserInterfaceManager userInterfaceManager;

        private Dictionary<Keyboard.Key, BoundKeyFunctions> _boundKeys;

        public bool Enabled { get; set; }

        public event EventHandler<BoundKeyEventArgs> BoundKeyDown;
        public event EventHandler<BoundKeyEventArgs> BoundKeyUp;

        public void Initialize()
        {
            Enabled = true;
            LoadKeys();
        }

        public void KeyDown(KeyEventArgs e)
        {
            //If the key is bound, fire the BoundKeyDown event.
            if (Enabled && !UIBlocked() && _boundKeys.Keys.Contains(e.Key))
                BoundKeyDown?.Invoke(this, new BoundKeyEventArgs(BoundKeyState.Down, _boundKeys[e.Key]));
        }

        public void KeyUp(KeyEventArgs e)
        {
            //If the key is bound, fire the BoundKeyUp event.
            if (Enabled && !UIBlocked() && _boundKeys.Keys.Contains(e.Key))
                BoundKeyUp?.Invoke(this, new BoundKeyEventArgs(BoundKeyState.Up, _boundKeys[e.Key]));
        }

        private void LoadKeys()
        {
            var xml = new XmlDocument();
            var kb = new StreamReader(PathHelpers.ExecutableRelativeFile("KeyBindings.xml"));
            xml.Load(kb);
            XmlNodeList resources = xml.SelectNodes("KeyBindings/Binding");
            _boundKeys = new Dictionary<Keyboard.Key, BoundKeyFunctions>();
            if (resources != null)
                foreach (XmlNode node in resources.Cast<XmlNode>().Where(node => node.Attributes != null))
                {
                    _boundKeys.Add(
                        (Keyboard.Key)Enum.Parse(typeof(Keyboard.Key), node.Attributes["Key"].Value, false),
                        (BoundKeyFunctions)
                        Enum.Parse(typeof(BoundKeyFunctions), node.Attributes["Function"].Value, false));
                }
        }

        // Don't take input if we're focused on a LineEdit.
        // LineEdits don't intercept keydowns when typing properly.
        // https://github.com/godotengine/godot/issues/15071
        // So if we didn't do this, the DebugConsole wouldn't block movement (for example).
        private bool UIBlocked()
        {
            return userInterfaceManager.Focused is LineEdit;
        }
    }
}
