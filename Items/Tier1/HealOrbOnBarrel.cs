using RoR2;
using R2API.Utils;
using UnityEngine;
using UnityEngine.Networking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Linq;
using R2API;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using static MysticsItems.LegacyBalanceConfigManager;

namespace MysticsItems.Items
{
    public class HealOrbOnBarrel : BaseItem
    {
        public static GameObject delayedHealOrbSpawner;

        public static ConfigurableValue<float> flatHealing = new ConfigurableValue<float>(
            "Item: Donut",
            "FlatHealing",
            8f,
            "How much flat HP the healing orbs regenerate",
            new System.Collections.Generic.List<string>()
            {
                "ITEM_MYSTICSITEMS_HEALORBONBARREL_DESC"
            }
        );
        public static ConfigurableValue<float> fractionalHealing = new ConfigurableValue<float>(
            "Item: Donut",
            "FractionalHealing",
            10f,
            "How much HP the healing orbs regenerate (in %)",
            new System.Collections.Generic.List<string>()
            {
                "ITEM_MYSTICSITEMS_HEALORBONBARREL_DESC"
            }
        );
        public static ConfigurableValue<float> fractionalHealingPerStack = new ConfigurableValue<float>(
            "Item: Donut",
            "FractionalHealingPerStack",
            10f,
            "How much HP the healing orbs regenerate (in %)",
            new System.Collections.Generic.List<string>()
            {
                "ITEM_MYSTICSITEMS_HEALORBONBARREL_DESC"
            }
        );

        public override void OnLoad()
        {
            base.OnLoad();
            itemDef.name = "MysticsItems_HealOrbOnBarrel";
            SetItemTierWhenAvailable(ItemTier.Tier1);
            itemDef.tags = new ItemTag[]
            {
                ItemTag.Healing,
                ItemTag.AIBlacklist,
                ItemTag.InteractableRelated
            };
            itemDef.pickupModelPrefab = PrepareModel(Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Donut/Model.prefab"));
            itemDef.pickupIconSprite = Main.AssetBundle.LoadAsset<Sprite>("Assets/Items/Donut/Icon.png");
            foreach (Transform childTransform in itemDef.pickupModelPrefab.transform.Find("Torus.001"))
            {
                GameObject child = childTransform.gameObject;
                Renderer renderer = child.GetComponentInChildren<Renderer>();
                Color.RGBToHSV(renderer.material.color, out float h, out float s, out float v);
                h += Random.value;
                renderer.material.color = Color.HSVToRGB(h % 1, s, v);
            }
            itemDisplayPrefab = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(itemDef.pickupModelPrefab, itemDef.pickupModelPrefab.name + "Display", false));
            onSetupIDRS += () =>
            {
                AddDisplayRule("CommandoBody", "Head", new Vector3(0f, 0.35f, 0f), new Vector3(0f, 180f, 0f), new Vector3(0.15f, 0.15f, 0.15f));
                AddDisplayRule("HuntressBody", "Head", new Vector3(0F, 0.302F, -0.049F), new Vector3(0F, 180F, 0F), new Vector3(0.12F, 0.12F, 0.12F));
                AddDisplayRule("Bandit2Body", "Hat", new Vector3(0F, 0.055F, -0.016F), new Vector3(336.039F, 0F, 0F), new Vector3(0.209F, 0.209F, 0.209F));
                AddDisplayRule("ToolbotBody", "Head", new Vector3(0.053F, 2.57F, 1.265F), new Vector3(55.266F, 359.983F, 0.119F), new Vector3(1.5F, 1.5F, 1.5F));
                AddDisplayRule("EngiBody", "HeadCenter", new Vector3(0F, 0.131F, -0.014F), new Vector3(356.315F, 0.001F, 359.976F), new Vector3(0.175F, 0.175F, 0.175F));
                AddDisplayRule("EngiTurretBody", "Head", new Vector3(0F, 0.548F, 0F), new Vector3(0F, 180F, 0F), new Vector3(1.447F, 1.447F, 1.447F));
                AddDisplayRule("EngiWalkerTurretBody", "Head", new Vector3(0F, 1.13F, -1.52F), new Vector3(351.692F, 180F, 0F), new Vector3(0.684F, 0.399F, 0.478F));
                AddDisplayRule("MageBody", "Head", new Vector3(0F, 0.112F, -0.121F), new Vector3(66.476F, 180F, 0F), new Vector3(0.149F, 0.149F, 0.149F));
                AddDisplayRule("MercBody", "Chest", new Vector3(0.013F, 0.184F, -0.259F), new Vector3(71.925F, 180F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
                AddDisplayRule("TreebotBody", "HandL", new Vector3(0.055F, 0.638F, 0.354F), new Vector3(12.464F, 0.568F, 10.583F), new Vector3(0.315F, 0.315F, 0.315F));
                AddDisplayRule("LoaderBody", "Head", new Vector3(0F, 0.231F, 0F), new Vector3(0F, 180F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
                AddDisplayRule("CrocoBody", "Head", new Vector3(-0.946F, 3.963F, -0.229F), new Vector3(279.836F, 0F, 170.118F), new Vector3(1.602F, 1.602F, 1.602F));
                AddDisplayRule("CaptainBody", "Stomach", new Vector3(0.002F, 0.134F, 0.176F), new Vector3(313.466F, 271.294F, 278.969F), new Vector3(0.086F, 0.086F, 0.086F));
                AddDisplayRule("BrotherBody", "Head", BrotherInfection.white, new Vector3(-0.01F, 0.044F, 0.12F), new Vector3(65.585F, 339.303F, 255.053F), new Vector3(0.107F, 0.107F, 0.107F));
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysSniper)
                {
                    AddDisplayRule("SniperClassicBody", "AntennaL", new Vector3(-0.00991F, 0.78285F, 0.00312F), new Vector3(0F, 0F, 0F), new Vector3(0.11502F, 0.11502F, 0.11502F));
                    AddDisplayRule("SniperClassicBody", "AntennaR", new Vector3(0.01191F, 0.78284F, 0.00313F), new Vector3(0F, 0F, 0F), new Vector3(0.11502F, 0.11502F, 0.11502F));
                }
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysDeputy) AddDisplayRule("DeputyBody", "Hat", new Vector3(0F, -0.00339F, 0.03205F), new Vector3(17.31186F, 0F, 0F), new Vector3(0.46246F, 0.15224F, 0.46246F));
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysChirr) AddDisplayRule("ChirrBody", "Neck", new Vector3(0F, 0.23362F, 0.06386F), new Vector3(332.1304F, 0F, 0F), new Vector3(0.5184F, 0.5184F, 0.5184F));
                AddDisplayRule("RailgunnerBody", "Head", new Vector3(0F, 0.19007F, -0.03365F), new Vector3(0F, 0F, 0F), new Vector3(0.15172F, 0.15172F, 0.15172F));
                AddDisplayRule("VoidSurvivorBody", "Head", new Vector3(0.00001F, 0.16656F, 0.00005F), new Vector3(0F, 0F, 0F), new Vector3(0.17803F, 0.17803F, 0.17803F));
            };
            
            GenericGameEvents.OnInteractionBegin += GenericGameEvents_OnInteractionBegin;

            On.RoR2.GravitatePickup.FixedUpdate += (orig, self) =>
            {
                var ror1style = self.GetComponent<GravitatePickupRoR1Style>();
                if (ror1style && !ror1style.normalBehaviour)
                {
                    var positionDifference = Vector3.Distance(ror1style.targetPosition, self.transform.position);
                    if (positionDifference > ror1style.lastPositionDifference || ror1style.moveTime >= ror1style.moveTimeMax)
                    {
                        ror1style.moveTime = ror1style.moveTimeMax;
                        self.rigidbody.velocity = Vector3.MoveTowards(self.rigidbody.velocity, Vector3.zero, self.acceleration * 0.25f);
                        if (ror1style.floatTime < ror1style.floatTimeMax)
                        {
                            ror1style.floatTime += Time.fixedDeltaTime;
                            if (ror1style.floatTime >= ror1style.floatTimeMax)
                            {
                                ror1style.floatTime = ror1style.floatTimeMax;
                                ror1style.normalBehaviour = true;
                                self.rigidbody.useGravity = true;
                            }
                        }
                    }
                    else
                    {
                        self.rigidbody.velocity = Vector3.MoveTowards(self.rigidbody.velocity, (ror1style.targetPosition - self.transform.position).normalized * self.maxSpeed, self.acceleration);
                        ror1style.moveTime += Time.fixedDeltaTime;
                    }
                    ror1style.lastPositionDifference = positionDifference;
                }
                else
                {
                    orig(self);
                }
            };

            delayedHealOrbSpawner = PrefabAPI.InstantiateClone(new GameObject(), "MysticsItems_DelayedHealOrbSpawner");
            MysticsItemsHealOrbOnBarrelSpawner delayedSpawnerComponent = delayedHealOrbSpawner.AddComponent<MysticsItemsHealOrbOnBarrelSpawner>();
        }

        private void GenericGameEvents_OnInteractionBegin(Interactor interactor, IInteractable interactable, GameObject interactableObject, bool canProc)
        {
            if (NetworkServer.active && canProc)
            {
                CharacterBody characterBody = interactor.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    Inventory inventory = characterBody.inventory;
                    if (inventory)
                    {
                        int itemCount = inventory.GetItemCount(MysticsItemsContent.Items.MysticsItems_HealOrbOnBarrel);
                        if (itemCount > 0)
                        {
                            GameObject spawner = Object.Instantiate(delayedHealOrbSpawner, interactableObject.transform.position, interactableObject.transform.rotation);
                            MysticsItemsHealOrbOnBarrelSpawner component = spawner.GetComponent<MysticsItemsHealOrbOnBarrelSpawner>();
                            spawner.transform.position = interactableObject.transform.position + Vector3.up * 2f;
                            ChildLocator childLocator = interactableObject.GetComponent<ChildLocator>();
                            if (childLocator)
                            {
                                Transform fireworkOrigin = childLocator.FindChild("FireworkOrigin");
                                if (fireworkOrigin) spawner.transform.position = fireworkOrigin.position;
                            }
                            component.interactor = interactor;
                            component.itemCount = itemCount;
                        }
                    }
                }
            }
        }

        public static void SpawnOrb(Vector3 position, Quaternion rotation, TeamIndex teamIndex, int itemCount)
        {
            if (NetworkServer.active)
            {
                var orb = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/HealPack"), position, rotation);
                orb.GetComponent<TeamFilter>().teamIndex = teamIndex;
                orb.GetComponentInChildren<HealthPickup>().flatHealing = flatHealing;
                orb.GetComponentInChildren<HealthPickup>().fractionalHealing = fractionalHealing / 100f + fractionalHealingPerStack / 100f * (float)(itemCount - 1);
                var ror1style = orb.GetComponentInChildren<GravitatePickup>().gameObject.AddComponent<GravitatePickupRoR1Style>();
                ror1style.targetPosition = position + rotation * Vector3.up * 4f;
                orb.GetComponent<Rigidbody>().useGravity = false;
                orb.transform.localScale = Vector3.one * (1f + orb.GetComponentInChildren<HealthPickup>().fractionalHealing);
                NetworkServer.Spawn(orb);
            }
        }

        public class GravitatePickupRoR1Style : MonoBehaviour
        {
            public Vector3 targetPosition;
            public Vector3 targetScale;
            public Vector3 scaleDifference;
            public float lastPositionDifference = Mathf.Infinity;
            public float moveTime = 0f;
            public float moveTimeMax = 2f;
            public float floatTime = 0f;
            public float floatTimeMax = 1f;
            public bool normalBehaviour = false;
        }

        public class MysticsItemsHealOrbOnBarrelSpawner : MonoBehaviour
        {
            public float delay = 0.5f;
            public Interactor interactor;
            public int itemCount;
            public bool consumed = false;

            public void FixedUpdate()
            {
                if (!consumed)
                {
                    delay -= Time.fixedDeltaTime;
                    if (delay <= 0f)
                    {
                        SpawnOrb(transform.position, transform.rotation, TeamComponent.GetObjectTeam(interactor.gameObject), itemCount);
                        consumed = true;
                        Object.Destroy(this);
                    }
                }
            }
        }
    }
}
