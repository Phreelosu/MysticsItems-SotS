using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace MysticsItems.Items
{
    public class SpeedGivesDamage : BaseItem
    {
        public static BuffDef buffDef;

        public static int maxDamageBoost = 10;
        public static float maxSpeedMultiplierRequirement = 7f;

        public static float percentPerBuffStack = 10f;
        public static float speedRequirementPerBuffStack = 100f;

        public static GameObject particleSystemPrefab;

        public override void PreLoad()
        {
            itemDef.name = "SpeedGivesDamage";
            itemDef.tier = ItemTier.Tier2;
            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility
            };
        }

        public override void OnLoad()
        {
            base.OnLoad();
            SetAssets("Nuclear Accelerator");
            Main.HopooShaderToMaterial.Standard.Apply(GetModelMaterial());
            Main.HopooShaderToMaterial.Standard.Emission(GetModelMaterial());
            model.AddComponent<MysticsItemsNuclearAcceleratorGlow>();
            CopyModelToFollower();
            onSetupIDRS += () =>
            {
                AddDisplayRule("CommandoBody", "CalfR", new Vector3(0.01F, 0.221F, 0.091F), new Vector3(283.928F, 181.008F, 20.003F), new Vector3(0.039F, 0.039F, 0.039F));
                AddDisplayRule("HuntressBody", "CalfR", new Vector3(0.01F, 0.28F, 0.087F), new Vector3(295.556F, 204.302F, 333.758F), new Vector3(0.033F, 0.033F, 0.033F));
                AddDisplayRule("Bandit2Body", "CalfR", new Vector3(0F, 0.354F, 0.06F), new Vector3(72.024F, 4.881F, 354.627F), new Vector3(0.039F, 0.039F, 0.039F));
                AddDisplayRule("ToolbotBody", "CalfR", new Vector3(0.037F, -0.291F, -0.267F), new Vector3(3.679F, 176.776F, 177.782F), new Vector3(0.495F, 0.495F, 0.495F));
                AddDisplayRule("EngiBody", "CannonHeadR", new Vector3(-0.185F, 0.279F, -0.044F), new Vector3(356.274F, 1.409F, 91.632F), new Vector3(0.052F, 0.093F, 0.052F));
                AddDisplayRule("EngiTurretBody", "LegBar3", new Vector3(0F, -0.013F, 0.071F), new Vector3(270.02F, 180F, 0F), new Vector3(0.211F, 0.211F, 0.211F));
                AddDisplayRule("EngiWalkerTurretBody", "LegBar3", new Vector3(0F, -0.013F, 0.071F), new Vector3(270.02F, 180F, 0F), new Vector3(0.211F, 0.211F, 0.211F));
                AddDisplayRule("MageBody", "Chest", new Vector3(0.115F, 0.315F, -0.182F), new Vector3(351.323F, 180F, 0F), new Vector3(0.047F, 0.047F, 0.047F));
                AddDisplayRule("MageBody", "Chest", new Vector3(-0.111F, 0.315F, -0.182F), new Vector3(351.323F, 180F, 0F), new Vector3(0.047F, 0.047F, 0.047F));
                AddDisplayRule("MercBody", "CalfR", new Vector3(0F, 0.08F, 0.105F), new Vector3(85.174F, 0F, 0F), new Vector3(0.039F, 0.039F, 0.039F));
                AddDisplayRule("TreebotBody", "FootBackR", new Vector3(0.124F, -0.044F, 0.006F), new Vector3(1.536F, 177.979F, 88.624F), new Vector3(0.077F, 0.077F, 0.077F));
                AddDisplayRule("LoaderBody", "MechHandR", new Vector3(0.069F, 0.125F, 0.115F), new Vector3(279.578F, 350.353F, 218.5F), new Vector3(0.043F, 0.043F, 0.043F));
                AddDisplayRule("CrocoBody", "CalfR", new Vector3(0.189F, 1.849F, 0.453F), new Vector3(279.59F, 160.263F, 10.136F), new Vector3(0.387F, 0.387F, 0.387F));
                AddDisplayRule("CaptainBody", "CalfR", new Vector3(0.015F, 0.214F, 0.085F), new Vector3(74.208F, 0F, 0F), new Vector3(0.045F, 0.045F, 0.045F));
                AddDisplayRule("BrotherBody", "CalfR", BrotherInfection.green, new Vector3(0.038F, 0.121F, 0.051F), new Vector3(43.102F, 358.401F, 241.259F), new Vector3(0.078F, 0.078F, 0.078F));
                AddDisplayRule("ScavBody", "CalfR", new Vector3(0.102F, 1.306F, 0.604F), new Vector3(66.818F, 0F, 0F), new Vector3(0.358F, 0.367F, 0.358F));
            };

            model.transform.Find("speedpower_powerspeed").Rotate(new Vector3(60f, 0f, 0f), Space.Self);

            CharacterStats.damageModifiers.Add(new CharacterStats.StatModifier
            {
                multiplier = 1f,
                times = (genericCharacterInfo) =>
                {
                    Inventory inventory = genericCharacterInfo.inventory;
                    if (inventory)
                    {
                        int itemCount = inventory.GetItemCount(itemDef);
                        return itemCount != 0 ? Mathf.Max((0.01f + 0.005f * (float)(itemCount - 1)) * ((genericCharacterInfo.body.moveSpeed / (genericCharacterInfo.body.baseMoveSpeed + genericCharacterInfo.body.levelMoveSpeed * genericCharacterInfo.body.level) - 1f) / 0.025f), 0f) : 0f;
                    }
                    return 0f;
                }
            });

            On.RoR2.CharacterBody.Awake += (orig, self) =>
            {
                orig(self);
                self.onInventoryChanged += delegate ()
                {
                    self.AddItemBehavior<MysticsItemsSpeedGivesDamageBehaviour>(self.inventory.GetItemCount(itemDef));
                };
            };

            particleSystemPrefab = Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Nuclear Accelerator/Particles.prefab");
            Material material = particleSystemPrefab.GetComponent<ParticleSystemRenderer>().sharedMaterial;
            Main.HopooShaderToMaterial.Standard.Apply(material);
            Main.HopooShaderToMaterial.Standard.Emission(material, 3f, new Color32(106, 255, 83, 255));
        }

        public class MysticsItemsNuclearAcceleratorGlow : MonoBehaviour
        {
            public MaterialPropertyBlock materialPropertyBlock;
            public Renderer renderer;
            public float stopwatch;

            public void Awake()
            {
                renderer = GetComponentInChildren<Renderer>();
                materialPropertyBlock = new MaterialPropertyBlock();
            }

            public void Update()
            {
                stopwatch += Time.deltaTime;

                float wave = Mathf.Sin(stopwatch * Mathf.PI * 2f);
                renderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.SetFloat("_EmPower", 1f * (0.75f + wave * 0.25f));
                float rgb = 0.75f + 0.25f * wave;
                materialPropertyBlock.SetColor("_EmColor", new Color(rgb, rgb, rgb, 1f));
                renderer.SetPropertyBlock(materialPropertyBlock);
            }
        }

        public class MysticsItemsSpeedGivesDamageBehaviour : CharacterBody.ItemBehavior
        {
            public ParticleSystem particleSystem;
            public float effectMaxSpeedMult = 10f;
            public float effectMaxMult = 20f;

            public void Start()
            {
                particleSystem = Object.Instantiate(particleSystemPrefab, body.transform).GetComponent<ParticleSystem>();
                particleSystem.transform.localScale *= body.radius;
            }

            public void OnDestroy()
            {
                Object.Destroy(particleSystem.gameObject);
            }

            public void FixedUpdate()
            {
                float mult = (body.moveSpeed / (body.baseMoveSpeed + body.levelMoveSpeed * body.level) - 1f) / effectMaxSpeedMult * effectMaxMult;
                if (particleSystem)
                {
                    ParticleSystem.MainModule main = particleSystem.main;
                    ParticleSystem.EmissionModule emission = particleSystem.emission;
                    emission.rateOverTimeMultiplier = mult;
                }
            }
        }
    }
}
