﻿using OfflineServer.Servers.Database.Management;
using OfflineServer.Servers.Http.Responses;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;

namespace OfflineServer.Servers.Http.Classes
{
    public static class Event
    {
        public static String arbitration()
        {
            String arbitrationPacket = Access.sHttp.getPostData();

            switch (arbitrationPacket.Substring(1, arbitrationPacket.IndexOf("ArbitrationPacket") - 1))
            {
                case "Drag":
                    {
                        DragArbitrationPacket dragArbitration = arbitrationPacket.DeserializeObject<DragArbitrationPacket>();
                    }
                    break;
                case "Pursuit":
                    {

                    }
                    break;
                case "Route":
                    {
                        RouteArbitrationPacket routeArbitration = arbitrationPacket.DeserializeObject<RouteArbitrationPacket>();

                        RouteEventResult eventResult = new RouteEventResult();
                        eventResult.accolades = getAccolades(routeArbitration);
                        eventResult.durability = Math.Max(0, Access.CurrentSession.ActivePersona.SelectedCar.Durability - 5);
                        eventResult.entrants.Add((RouteEntrantResult)routeArbitration.getEntrantResult());
                        eventResult.personaId = Access.CurrentSession.ActivePersona.Id;

                        return eventResult.SerializeObject();
                    }
                case "TeamEscape":
                    {

                    }
                    break;
            }
            return "";
        }

        public static String launched()
        {
            Access.CurrentSession.ActivePersona.currentEventLaunched = true;
            // write to db here
            return "";
        }

        private static Accolades getAccolades(ArbitrationPacket arbitrationPacket)
        {
            Accolades accolades = new Accolades();
            if (!finishedEvent(arbitrationPacket))
                return accolades;

            Int32 finalExp, finalCash;
            List<RewardPart> rewardParts = arbitrationPacket.calculateRewardParts(out finalExp, out finalCash);

            accolades.finalRewards = new Reward()
            {
                rep = finalExp,
                tokens = finalCash
            };
            accolades.hasLeveledUp = Access.CurrentSession.ActivePersona.ReputationRequiredToPassTheLevel - finalExp <= 0;
            accolades.luckyDrawInfo = new LuckyDrawInfo();
            accolades.originalRewards = arbitrationPacket.calculateBaseReward();
            accolades.rewardInfo = rewardParts;

            Access.CurrentSession.ActivePersona.ReputationInTotal += finalExp;
            Access.CurrentSession.ActivePersona.Cash += finalCash;

            return accolades;
        }

        private static Boolean finishedEvent(ArbitrationPacket arbitrationPacket)
        {
            switch (arbitrationPacket.finishReason)
            {
                case FinishReason.Aborted:
                case FinishReason.Busted:
                case FinishReason.DidNotFinish:
                case FinishReason.FalseStart:
                case FinishReason.TimedOut:
                case FinishReason.TimeLimitExpired:
                case FinishReason.Totalled:
                    return false;
            }

            return true;
        }
    }
}
