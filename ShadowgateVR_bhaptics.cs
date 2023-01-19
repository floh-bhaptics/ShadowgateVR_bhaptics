using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using HarmonyLib;
using MyBhapticsTactsuit;

namespace ShadowgateVR_bhaptics
{
    public class ShadowgateVR_bhaptics : MelonMod
    {
        public static TactsuitVR tactsuitVr;
        public static bool wandIsRight = true;

        public override void OnInitializeMelon()
        {
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }

        [HarmonyPatch(typeof(Player), "Die", new Type[] { typeof(DeathType), typeof(DamageType) })]
        public class bhaptics_PlayerDies
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StopThreads();
            }
        }

        [HarmonyPatch(typeof(Player), "ApplyHealing", new Type[] { typeof(int) })]
        public class bhaptics_PlayerHeal
        {
            [HarmonyPostfix]
            public static void Postfix(Player __instance)
            {
                tactsuitVr.PlaybackHaptics("Healing");
                if (__instance.HP >= 0.3f * (int)__instance.maxHP) tactsuitVr.StopHeartBeat();
            }
        }

        [HarmonyPatch(typeof(Player), "OnDamaged", new Type[] { typeof(int), typeof(DamageType) })]
        public class bhaptics_PlayerDamage
        {
            [HarmonyPostfix]
            public static void Postfix(Player __instance, DamageType type)
            {
                string pattern = "Impact";
                switch (type)
                {
                    case DamageType.None:
                        pattern = "Impact";
                        break;
                    case DamageType.Fire:
                        pattern = "Burning";
                        break;
                    case DamageType.Cold:
                        pattern = "Shiver";
                        break;
                    case DamageType.Lightning:
                        pattern = "Lightning";
                        break;
                    case DamageType.Poison:
                        pattern = "Poison";
                        break;
                    default:
                        pattern = "Impact";
                        break;
                }
                tactsuitVr.PlaybackHaptics(pattern);
                if (__instance.HP < 0.3f * (int)__instance.maxHP) tactsuitVr.StartHeartBeat();
            }
        }

        [HarmonyPatch(typeof(Inventory), "SetDominantHand", new Type[] { typeof(HandMode) })]
        public class bhaptics_SetDominantHand
        {
            [HarmonyPostfix]
            public static void Postfix(Inventory __instance, HandMode hand)
            {
                wandIsRight = (hand == HandMode.Right);
            }
        }

        [HarmonyPatch(typeof(Wand), "GrabWith", new Type[] { typeof(Hand) })]
        public class bhaptics_GrabWand
        {
            [HarmonyPostfix]
            public static void Postfix(Wand __instance, Hand hand)
            {
                tactsuitVr.GetItem(wandIsRight);
            }
        }

        [HarmonyPatch(typeof(Shield), "GrabWith", new Type[] { typeof(Hand) })]
        public class bhaptics_GrabShield
        {
            [HarmonyPostfix]
            public static void Postfix(Shield __instance, Hand hand)
            {
                tactsuitVr.GetItem(!wandIsRight);
            }
        }

        [HarmonyPatch(typeof(InventorySlotBelt), "PlaceItem", new Type[] { typeof(BeltItem), typeof(bool) })]
        public class bhaptics_PlaceInventoryItem
        {
            [HarmonyPostfix]
            public static void Postfix(InventorySlotBelt __instance, BeltItem item)
            {
                if (item.name.Contains("Wand")) tactsuitVr.GetItem(wandIsRight);
                if (item.name.Contains("Shield")) tactsuitVr.GetItem(!wandIsRight);
            }
        }

        /*
        [HarmonyPatch(typeof(InventorySlotBelt), "GrabWith", new Type[] { typeof(Hand) })]
        public class bhaptics_GetInventoryItem
        {
            [HarmonyPostfix]
            public static void Postfix(InventorySlotBelt __instance, BeltItem ____itemInSlot)
            {
                tactsuitVr.LOG("Grabbed: " + ____itemInSlot.name);
                if (____itemInSlot.name == "Wand") tactsuitVr.GetItem(wandIsRight);
                if (____itemInSlot.name == "Shield") tactsuitVr.GetItem(!wandIsRight);
            }
        }

        [HarmonyPatch(typeof(InventorySlotBelt), "RemoveItem", new Type[] { typeof(bool) })]
        public class bhaptics_RemoveInventoryItem
        {
            [HarmonyPostfix]
            public static void Postfix(InventorySlotBelt __instance)
            {
                //tactsuitVr.LOG("Removed: " + ____itemInSlot.name);
                tactsuitVr.LOG("Removed: " + __instance.ItemInSlot().name);
                
                if (__instance.ItemInSlot().name.Contains("Wand")) tactsuitVr.GetItem(wandIsRight);
                if (__instance.ItemInSlot().name.Contains("Shield")) tactsuitVr.GetItem(!wandIsRight);
            }
        }
        */

        [HarmonyPatch(typeof(Wand), "TriggerPress", new Type[] { typeof(bool) })]
        public class bhaptics_WandCast
        {
            [HarmonyPostfix]
            public static void Postfix(Wand __instance, bool pressed, Hand ____hand)
            {
                if ((!pressed)) return;
                bool isRightHand = ____hand.IsRightHand();
                tactsuitVr.CastSpell("Fire", isRightHand);
            }
        }

        [HarmonyPatch(typeof(Shield), "TriggerPress", new Type[] { typeof(bool) })]
        public class bhaptics_ShieldCast
        {
            [HarmonyPostfix]
            public static void Postfix(Shield __instance, bool pressed, Hand ____hand, bool ____isCharged, bool ____isEnabled)
            {
                if ((!pressed) || (!____isCharged) || (!____isEnabled)) return;
                bool isRightHand = ____hand.IsRightHand();
                tactsuitVr.CastSpell("Barrier", isRightHand);
            }
        }

        [HarmonyPatch(typeof(Hand), "Teleported", new Type[] { })]
        public class bhaptics_TeleportHand
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("TeleportThrough");
            }
        }

        /*
        [HarmonyPatch(typeof(LocomotionTeleport), "DoTeleport", new Type[] {  })]
        public class bhaptics_Teleport
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.LOG("Instant");
                tactsuitVr.PlaybackHaptics("TeleportThrough");
            }
        }

        [HarmonyPatch(typeof(TeleportTransitionWarp), "DoWarp", new Type[] { })]
        public class bhaptics_TeleportWarp
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.LOG("Warp");
                tactsuitVr.PlaybackHaptics("TeleportThrough");
            }
        }

        [HarmonyPatch(typeof(TeleportTransitionBlink), "BlinkCoroutine", new Type[] { })]
        public class bhaptics_TeleportBlink
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.LOG("Blink");
                tactsuitVr.PlaybackHaptics("TeleportThrough");
            }
        }
        */

        [HarmonyPatch(typeof(Player), "OnOdinViewEnter", new Type[] { })]
        public class bhaptics_EnterOdinView
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("OdinViewEnter");
            }
        }

        [HarmonyPatch(typeof(OdinView), "EndView", new Type[] { })]
        public class bhaptics_ExitOdinView
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("OdinViewExit");
            }
        }

    }
}
