﻿using Lidgren.Network;
using SS14.Client.Input;
using SS14.Client.Interfaces.GameObjects;
using SS14.Client.Interfaces.Input;
using SS14.Client.Interfaces.Player;
using SS14.Client.Interfaces.UserInterface;
using SS14.Client.UserInterface;
using SS14.Shared;
using SS14.Shared.Configuration;
using SS14.Shared.Interfaces.Configuration;
using SS14.Shared.Interfaces.GameObjects;
using SS14.Shared.Interfaces.Network;
using SS14.Shared.IoC;
using SS14.Shared.Log;
using SS14.Shared.Network;
using System;

namespace SS14.Client.State.States
{
    // OH GOD.
    // Ok actually it's fine.
    public sealed partial class GameScreen : State
    {
        [Dependency]
        readonly IConfigurationManager _config;
        [Dependency]
        readonly IClientEntityManager _entityManager;
        [Dependency]
        readonly IComponentManager _componentManager;
        [Dependency]
        readonly IKeyBindingManager keyBindingManager;
        [Dependency]
        readonly IClientNetManager networkManager;
        [Dependency]
        readonly IPlayerManager playerManager;
        [Dependency]
        readonly IUserInterfaceManager userInterfaceManager;

        private EscapeMenu escapeMenu;

        public override void Shutdown()
        {
            escapeMenu.Dispose();

            playerManager.LocalPlayer.DetachEntity();

            _entityManager.Shutdown();
            userInterfaceManager.StateRoot.DisposeAllChildren();
            networkManager.MessageArrived -= NetworkManagerMessageArrived;
            GC.Collect();
        }

        public override void Startup()
        {
            IoCManager.InjectDependencies(this);

            escapeMenu = new EscapeMenu
            {
                Visible = false
            };
            escapeMenu.AddToScreen();

            _config.RegisterCVar("player.name", "Joe Genero", CVar.ARCHIVE);

            NetOutgoingMessage message = networkManager.CreateMessage();
            message.Write((byte)NetMessages.RequestMap);
            networkManager.ClientSendMessage(message, NetDeliveryMethod.ReliableUnordered);

            // TODO This should go somewhere else, there should be explicit session setup and teardown at some point.
            var message1 = networkManager.CreateMessage();
            message1.Write((byte)NetMessages.ClientName);
            message1.Write(_config.GetCVar<string>("player.name"));
            networkManager.ClientSendMessage(message1, NetDeliveryMethod.ReliableOrdered);

            networkManager.MessageArrived += NetworkManagerMessageArrived;
        }

        public override void Update(FrameEventArgs e)
        {
            _componentManager.Update(e.Elapsed);
            _entityManager.Update(e.Elapsed);
            //PlacementManager.Update(MousePosScreen);
            playerManager.Update(e.Elapsed);
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (e.Key == Keyboard.Key.Escape)
            {
                if (escapeMenu.Visible)
                {
                    if (escapeMenu.IsAtFront())
                    {
                        escapeMenu.Visible = false;
                    }
                    else
                    {
                        escapeMenu.MoveToFront();
                    }
                }
                else
                {
                    escapeMenu.OpenCentered();
                }

                e.Handle();
                return;
            }

            keyBindingManager.KeyDown(e);
        }

        public override void KeyUp(KeyEventArgs e)
        {
            keyBindingManager.KeyUp(e);
        }

        private void NetworkManagerMessageArrived(object sender, NetMessageArgs args)
        {
            NetIncomingMessage message = args.RawMessage;
            if (message == null)
            {
                return;
            }
            switch (message.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:
                    var statMsg = (NetConnectionStatus)message.ReadByte();
                    if (statMsg == NetConnectionStatus.Disconnected)
                    {
                        string disconnectMessage = message.ReadString();
                        //UserInterfaceManager.AddComponent(new DisconnectedScreenBlocker(StateManager,
                        //                                                                UserInterfaceManager,
                        //                                                                ResourceCache,
                        //                                                                disconnectMessage));
                    }
                    break;
                case NetIncomingMessageType.Data:
                    var messageType = (NetMessages)message.ReadByte();
                    switch (messageType)
                    {
                        case NetMessages.PlacementManagerMessage:
                            //PlacementManager.HandleNetMessage(message);
                            break;
                        case NetMessages.ChatMessage:
                            //HandleChatMessage(message);
                            break;
                    }
                    break;
            }
        }
    }
}
