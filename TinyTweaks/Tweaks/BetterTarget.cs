using System;
using System.Linq;
using System.Numerics;
using TinyTweaks.Utils;
using Dalamud.Game.Command;
using Dalamud.Game.ClientState.Objects.Types;
using GameObjectStruct = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using System.Collections.Generic;

namespace TinyTweaks.Tweaks
{
    internal class GameObjectList
    {
        public uint objectId;
        public IGameObject? gameObject;
        public float distance;

    }

    internal class BetterTarget : ITweak
    {
        private const string CommandName = "/btarget";

        public void Enable()
        {
            Svc.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Allows targetting closest entity that matches any term of a space-delimited list",
            });
        }

        public void Disable()
        {
            Svc.CommandManager.RemoveHandler(CommandName);
        }

        private unsafe void OnCommand(string command, string args)
        {
            //create gameObjectList and items
            List<GameObjectList> gameObjectList = new List<GameObjectList>();
            GameObjectList gameObjectListItem;

            //Original code
            IGameObject? closestMatch = null;
            var closestDistance = float.MaxValue;
            var player = Svc.ClientState.LocalPlayer;
            var searchTerms = args.Split(" ");


            //parse String args
            int numericValue;
            var tryNumber = searchTerms.Last();
            bool isNumber = int.TryParse(tryNumber, out numericValue);
            if (isNumber)
            {
                searchTerms = searchTerms.SkipLast(1).ToArray();
            }
            else
            {
                numericValue = 1;
            }

            //Original code
            foreach (var actor in Svc.Objects)
            {
                if (actor == null) continue;
                var valueFound = searchTerms.Any(searchName => actor.Name.TextValue.ToLowerInvariant().Contains(searchName.ToLowerInvariant()));
                if (valueFound && ((GameObjectStruct*)actor.Address)->GetIsTargetable())
                {
                    var distance = Vector3.Distance(player.Position, actor.Position);

                    //gameObjectListItem add matched item
                    gameObjectListItem = new GameObjectList();
                    gameObjectListItem.gameObject = actor;
                    gameObjectListItem.distance = distance;
                    gameObjectListItem.objectId = actor.EntityId;
                    gameObjectList = gameObjectList.Append(gameObjectListItem).ToList();

                    //Original code
                    if (closestMatch == null)
                    {
                        closestMatch = actor;
                        closestDistance = distance;
                        continue;
                    }
                    if (closestDistance > distance)
                    {
                        closestMatch = actor;
                        closestDistance = distance;
                    }
                }
            }

            if (closestMatch != null)
            {
                if (numericValue == 1)
                {
                    //Original code
                    Svc.Targets.Target = closestMatch;
                }
                else
                {
                    var sorted = gameObjectList.OrderBy(p => p.distance).ThenBy(p => p.objectId).ToList();
                    Svc.Targets.Target = sorted[numericValue - 1].gameObject;
                }
            }
        }
    }
}
