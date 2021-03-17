using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;

namespace MysticsItems.Items
{
    public class AllyDeathRevenge : BaseItem
    {
        public static BuffIndex buffIndex;

        public override void PreAdd()
        {
            itemDef.name = "AllyDeathRevenge";
            itemDef.tier = ItemTier.Tier2;
            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.Utility
            };
            SetUnlockable();
            SetAssets("Ally Death Revenge");
            SetModelPanelDistance(0.75f, 1.5f);
            model.transform.Find("mdlAllyDeathRevenge").Rotate(new Vector3(0f, 0f, 160f), Space.Self);
            model.transform.Find("mdlAllyDeathRevenge").localScale *= 0.8f;
            CopyModelToFollower();
        }

        public override void OnAdd()
        {
            On.RoR2.CharacterMaster.Awake += (orig, self) =>
            {
                orig(self);
                self.gameObject.AddComponent<SurvivedStageCounter>();
            };

            On.RoR2.Stage.Start += (orig, self) =>
            {
                orig(self);
                foreach (CharacterMaster characterMaster in CharacterMaster.readOnlyInstancesList)
                {
                    SurvivedStageCounter component = characterMaster.GetComponent<SurvivedStageCounter>();
                    if (component)
                    {
                        component.count++;
                    }
                }
            };

            On.RoR2.CharacterMaster.OnBodyDeath += (orig, self, body) =>
            {
                orig(self, body);
                GameObject playSoundObject = null;
                if (NetworkServer.active)
                {
                    TeamIndex teamIndex = TeamComponent.GetObjectTeam(body.gameObject);
                    foreach (CharacterBody body2 in CharacterBody.readOnlyInstancesList)
                    {
                        Inventory inventory = body2.inventory;
                        if (inventory && inventory.GetItemCount(itemIndex) > 0 && TeamComponent.GetObjectTeam(body2.gameObject) == teamIndex)
                        {
                            playSoundObject = body2.gameObject;

                            float time = 15f + 5f * (inventory.GetItemCount(itemIndex) - 1);
                            float sameStageDeathTime = 2f + 0.5f * (inventory.GetItemCount(itemIndex) - 1);
                            if (body.master)
                            {
                                SurvivedStageCounter survivedStageCounter = body.master.GetComponent<SurvivedStageCounter>();
                                if (survivedStageCounter && survivedStageCounter.count <= 0)
                                {
                                    time = sameStageDeathTime;
                                    playSoundObject = null;
                                }
                            }
                            else
                            {
                                // Masterless bodies don't get moved to the next stage anyway, so they definitely died on the same stage
                                time = sameStageDeathTime;
                                playSoundObject = null;
                            }
                            body2.AddTimedBuff(buffIndex, time);
                        }
                    }
                }
                if (playSoundObject) Util.PlaySound("Play_item_allydeathrevenge_proc", playSoundObject);
            };

            Main.Overlays.CreateOverlay(Main.AssetBundle.LoadAsset<Material>("Assets/Misc/Materials/matAllyDeathRevengeOverlay.mat"), delegate (CharacterModel model)
            {
                return model.body.HasBuff(buffIndex);
            });
        }

        public class SurvivedStageCounter : MonoBehaviour
        {
            public int count = 0;
        }
    }
}
