﻿using SS14.Client.Input;
using SS14.Client.Interfaces;
using SS14.Client.Interfaces.ResourceManagement;
using SS14.Client.Interfaces.UserInterface;
using SS14.Client.ResourceManagement;
using SS14.Shared.Configuration;
using SS14.Shared.Interfaces.Configuration;
using SS14.Shared.IoC;
using SS14.Shared.Log;
using System.Collections.Generic;

namespace SS14.Client.UserInterface
{
    public sealed class UserInterfaceManager : IUserInterfaceManager, IPostInjectInit
    {
        [Dependency]
        readonly IConfigurationManager _config;
        [Dependency]
        readonly ISceneTreeHolder _sceneTreeHolder;

        public Control RootControl { get; private set; }
        public AcceptDialog PopupControl { get; private set; }

        public void PostInject()
        {
            _config.RegisterCVar("key.keyboard.console", Keyboard.Key.Tilde, CVar.ARCHIVE);
        }

        public void Initialize()
        {
            RootControl = new Control("UIRoot")
            {
                AnchorLeft = 0,
                AnchorRight = 1,
                AnchorTop = 0,
                AnchorBottom = 1,
                MarginBottom = 0,
                MarginTop = 0,
                MarginRight = 0,
                MarginLeft = 0,
            };
            _sceneTreeHolder.SceneTree.GetRoot().AddChild(RootControl.SceneControl);
            PopupControl = new AcceptDialog("RootPopup");
            RootControl.AddChild(PopupControl);
        }

        public void DisposeAllComponents()
        {
            RootControl.DisposeAllChildren();
        }

        public void Popup(string contents, string title="Alert!")
        {
            PopupControl.DialogText = contents;
            PopupControl.Title = title;
            PopupControl.OpenMinimum();
        }
    }
}
