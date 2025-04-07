using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoLoveBetweenFollowers
{
    [BepInPlugin("com.snowyegret.nolovebetweenfollowers", "NoLoveBetweenFollowers", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource Log;
        private readonly Harmony harmony = new Harmony("com.snowyegret.nolovebetweenfollowers");

        private void Awake()
        {
            Log = Logger;
            harmony.PatchAll();
            Log.LogInfo($"NoLoveBetweenFollowers 모드가 로드되었습니다!");
        }

        [HarmonyPatch(typeof(Follower), "Update")]
        public class FollowerUpdatePatch
        {
            static void Postfix(Follower __instance)
            {
                if (__instance?.Brain?.Info?.Relationships == null)
                    return;

                foreach (IDAndRelationship relationship in __instance.Brain.Info.Relationships)
                {
                    if (relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Lovers)
                    {
                        // 연인 관계인 경우 친구로 변경
                        relationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Friends;
                        relationship.Relationship = 8;

                        Follower otherFollower = GetFollowerByID(relationship.ID);
                        string otherFollowerName = otherFollower != null ? otherFollower.Brain.Info.Name : "알 수 없음";

                        // 양방향 관계 처리
                        if (otherFollower != null && otherFollower.Brain != null && otherFollower.Brain.Info != null)
                        {
                            foreach (IDAndRelationship otherRelationship in otherFollower.Brain.Info.Relationships)
                            {
                                if (otherRelationship.ID == __instance.Brain.Info.ID)
                                {
                                    otherRelationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Friends;
                                    otherRelationship.Relationship = 8;
                                    break;
                                }
                            }
                        }

                        Log.LogInfo($"관계 변경: {__instance.Brain.Info.Name}(ID: {__instance.Brain.Info.ID})와 " +
                                    $"{otherFollowerName}(ID: {relationship.ID}) 사이의 관계가 연인에서 친구로 변경되었습니다.");
                    }
                    else if (relationship.CurrentRelationshipState == IDAndRelationship.RelationshipState.Friends && relationship.Relationship >= 9)
                    {
                        // 관계 수치가 9 이상이면 8로 제한
                        relationship.Relationship = 8;

                        Follower otherFollower = GetFollowerByID(relationship.ID);
                        string otherFollowerName = otherFollower != null ? otherFollower.Brain.Info.Name : "알 수 없음";

                        Log.LogInfo($"수치 제한: {__instance.Brain.Info.Name}(ID: {__instance.Brain.Info.ID})와 " +
                                    $"{otherFollowerName}(ID: {relationship.ID}) 사이의 수치가 8로 제한되었습니다.");

                        // 양방향 관계 처리
                        if (otherFollower != null && otherFollower.Brain != null && otherFollower.Brain.Info != null)
                        {
                            foreach (IDAndRelationship otherRelationship in otherFollower.Brain.Info.Relationships)
                            {
                                if (otherRelationship.ID == __instance.Brain.Info.ID && otherRelationship.Relationship >= 9)
                                {
                                    otherRelationship.Relationship = 8;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            static Follower GetFollowerByID(int id)
            {
                if (Follower.Followers == null)
                    return null;

                foreach (Follower follower in Follower.Followers)
                {
                    if (follower?.Brain?.Info?.ID == id)
                        return follower;
                }
                return null;
            }
        }
    }
}
