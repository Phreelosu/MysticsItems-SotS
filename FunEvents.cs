using MysticsRisky2Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace MysticsItems
{
    internal static class FunEvents
    {
        public static void Init()
        {
            ConfigOptions.ConfigurableValue<bool> enabledByConfig = ConfigOptions.ConfigurableValue.CreateBool(
                ConfigManager.General.categoryGUID,
                ConfigManager.General.categoryName,
                ConfigManager.General.config,
                "Misc",
                "Fun Events",
                true,
                "Enable fun events that happen on specific dates",
                restartRequired: true
            );

            if (enabledByConfig)
            {
                var today = System.DateTime.Now;
                if (today.Month == 4 && today.Day == 1)
                {
                    BazaarPrank.Init();
                }
                if ((today.Month == 12 && today.Day >= 25) || (today.Month == 1 && today.Day <= 5))
                {
                    ChristmasAndNewYear.Init();
                }
            }
        }

        public static class ChristmasAndNewYear
        {
            public static GameObject festiveEffectsPrefab;

            public static void Init()
            {
                SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            }

            private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (scene.name == "bazaar")
                {
                    if (festiveEffectsPrefab == null)
                    {
                        festiveEffectsPrefab = Main.AssetBundle.LoadAsset<GameObject>("Assets/Mods/Mystic's Items/Effects/FestiveStage.prefab");
                        foreach (var particleSystem in festiveEffectsPrefab.GetComponentsInChildren<ParticleSystem>())
                        {
                            var shape = particleSystem.shape;
                            shape.radius = 100f;
                        }
                    }
                    Object.Instantiate(festiveEffectsPrefab, Vector3.zero, Quaternion.identity);
                }
            }
        }

        public static class BazaarPrank
        {
            public static void Init()
            {
                RoR2Application.onLoad += BazaarPrank_OnGameLoad;
                RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
                On.RoR2.ShopTerminalBehavior.Start += ShopTerminalBehavior_Start;
                On.RoR2.ShopTerminalBehavior.SetPickupIndex += ShopTerminalBehavior_SetPickupIndex;
                PickupDropletController.onDropletHitGroundServer += PickupDropletController_onDropletHitGroundServer;
            }

            public static void Finish()
            {
                finished = true;
                RoR2Application.onLoad -= BazaarPrank_OnGameLoad;
                RoR2Application.onFixedUpdate -= RoR2Application_onFixedUpdate;
                On.RoR2.ShopTerminalBehavior.Start -= ShopTerminalBehavior_Start;
                On.RoR2.ShopTerminalBehavior.SetPickupIndex -= ShopTerminalBehavior_SetPickupIndex;
                PickupDropletController.onDropletHitGroundServer -= PickupDropletController_onDropletHitGroundServer;
            }

            public static PickupIndex gesturePickupIndex;
            public static PickupIndex tonicPickupIndex;
            public static PickupIndex bestItemPickupIndex;
            public static float finishTimer = -1f;
            public static bool finished = false;
            public static int lunarBudIndex = 0;
            
            private static void BazaarPrank_OnGameLoad()
            {
                gesturePickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.Items.AutoCastEquipment.itemIndex);
                tonicPickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.Equipment.Tonic.equipmentIndex);
                bestItemPickupIndex = PickupCatalog.FindPickupIndex(MysticsItemsContent.Equipment.MysticsItems_GateChalice.equipmentIndex);
            }

            private static void RoR2Application_onFixedUpdate()
            {
                if (finishTimer > 0f)
                {
                    finishTimer -= Time.fixedDeltaTime;
                    if (finishTimer <= 0f)
                    {
                        if (NetworkServer.active)
                        {
                            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                            {
                                baseToken = "Happy April Fools' day!"
                            });
                        }
                        Finish();
                    }
                }
            }

            private static void PickupDropletController_onDropletHitGroundServer(ref GenericPickupController.CreatePickupInfo createPickupInfo, ref bool shouldSpawn)
            {
                if ((createPickupInfo.pickupIndex == gesturePickupIndex || createPickupInfo.pickupIndex == tonicPickupIndex) &&
                    Stage.instance.sceneDef && Stage.instance.sceneDef.baseSceneName == "bazaar")
                    createPickupInfo.pickupIndex = bestItemPickupIndex;
            }

            private static void ShopTerminalBehavior_Start(On.RoR2.ShopTerminalBehavior.orig_Start orig, ShopTerminalBehavior self)
            {
                orig(self);
                if (MysticsRisky2Utils.Utils.TrimCloneFromString(self.gameObject.name).Contains("LunarShopTerminal"))
                {
                    self.SetPickupIndex((lunarBudIndex % 5) == 0 ? tonicPickupIndex : gesturePickupIndex);
                    lunarBudIndex++;
                    var purchaseInteraction = self.GetComponent<PurchaseInteraction>();
                    if (purchaseInteraction)
                    {
                        purchaseInteraction.onPurchase.AddListener((interactor) =>
                        {
                            if (!finished)
                            {
                                if (interactor)
                                {
                                    var body = interactor.GetComponent<CharacterBody>();
                                    if (body && body.master && body.master.playerCharacterMasterController)
                                    {
                                        var pcmc = body.master.playerCharacterMasterController;
                                        if (pcmc.networkUser)
                                        {
                                            pcmc.networkUser.AwardLunarCoins((uint)purchaseInteraction.cost);
                                        }
                                    }
                                }
                                finishTimer = 5f;
                            }
                        });
                    }
                }
            }

            private static void ShopTerminalBehavior_SetPickupIndex(On.RoR2.ShopTerminalBehavior.orig_SetPickupIndex orig, ShopTerminalBehavior self, PickupIndex newPickupIndex, bool newHidden)
            {
                if (newPickupIndex != PickupIndex.none && MysticsRisky2Utils.Utils.TrimCloneFromString(self.gameObject.name).Contains("LunarShopTerminal"))
                {
                    if (newPickupIndex != tonicPickupIndex && newPickupIndex != gesturePickupIndex)
                    {
                        newPickupIndex = (lunarBudIndex % 5) == 0 ? tonicPickupIndex : gesturePickupIndex;
                        lunarBudIndex++;
                    }
                }
                orig(self, newPickupIndex, newHidden);
            }
        }
    }
}