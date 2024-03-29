﻿using HarmonyLib;
using ModIO.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
    [EnableReloading]
    public class Main
    {
        public static Settings settings;
        public static Harmony harmonyInstance;
        public static UnityModManager.ModEntry modEntry;
        public static GameObject manager;
        public static Controller controller;
        public static UIController ui;
        public static ChatBubbleTest cbt;
        public static Multiplayer multi;
        public static TrickCustomizer tc;
        public static CameraShake cs;
        public static WalkController wc;
        public static Assembly assembly;

        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            UnityEngine.Object.Destroy(manager);

            try
            {
                harmonyInstance.UnpatchAll(harmonyInstance.Id);
            }
            catch { }

            return true;
        }

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            harmonyInstance = new Harmony(modEntry.Info.Id);
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            manager = new GameObject("FroExperimentalModGO");

            ui = manager.AddComponent<UIController>();
            multi = manager.AddComponent<Multiplayer>();
            cbt = manager.AddComponent<ChatBubbleTest>();
            controller = manager.AddComponent<Controller>();
            tc = manager.AddComponent<TrickCustomizer>();
            cs = manager.AddComponent<CameraShake>();
            wc = manager.AddComponent<WalkController>();

            UnityEngine.Object.DontDestroyOnLoad(manager);
            modEntry.OnUnload = Unload;
            Main.modEntry = modEntry;
            checkLists(modEntry);

            assembly = Assembly.GetExecutingAssembly();

            try
            {
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch(Exception e)
            {
                MessageSystem.QueueMessage(MessageDisplayData.Type.Error, "Experimental mod failed to patch the game, are you using SkaterXL v1.2.2.x?", 5f);
                Utils.Log("Error message: " + e.Message);
            }

            Utils.Log("Loaded " + modEntry.Info.Id);
            return true;
        }

        public static void checkLists(UnityModManager.ModEntry modEntry, bool force = false)
        {
            if (settings.dynamic_feet_states.Count == 0 || force)
            {
                settings.dynamic_feet_states = new List<bool>(new bool[16]);
                settings.dynamic_feet_states[0] = true;
                settings.dynamic_feet_states[1] = true;
                settings.dynamic_feet_states[2] = true;
                settings.dynamic_feet_states[6] = true;
                settings.dynamic_feet_states[7] = true;
                settings.dynamic_feet_states[8] = true;
                settings.dynamic_feet_states[9] = true;
                settings.dynamic_feet_states[10] = true;
                settings.dynamic_feet_states[11] = true;
                settings.Save(modEntry);
            }


            if (settings.look_forward_states.Count == 0 || force)
            {
                settings.look_forward_states = new List<bool>(new bool[16]);
                settings.look_forward_states[1] = true;
                settings.Save(modEntry);
            }

            if (settings.head_rotation_fakie.Count == 0)
            {
                settings.head_rotation_fakie = new List<Vector3>(new Vector3[16]);
                settings.Save(modEntry);
            }

            if (settings.head_rotation_switch.Count == 0)
            {
                settings.head_rotation_switch = new List<Vector3>(new Vector3[16]);
                settings.Save(modEntry);
            }

            if (settings.head_rotation_grinds_fakie.Count == 0)
            {
                settings.head_rotation_grinds_fakie = new List<Vector3>(new Vector3[34]);
                settings.Save(modEntry);
            }

            if (settings.head_rotation_grinds_switch.Count == 0)
            {
                settings.head_rotation_grinds_switch = new List<Vector3>(new Vector3[34]);
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation.Count == 0)
            {
                settings.ollie_customization_rotation = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_length.Count == 0)
            {
                settings.ollie_customization_length = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_backwards.Count == 0)
            {
                settings.ollie_customization_rotation_backwards = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_backwards.Count == 0)
            {
                settings.ollie_customization_length_backwards = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_left_stick.Count == 0)
            {
                settings.ollie_customization_rotation_left_stick = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_left_stick.Count == 0)
            {
                settings.ollie_customization_length_left_stick = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }


            if (settings.ollie_customization_rotation_right_stick.Count == 0)
            {
                settings.ollie_customization_rotation_right_stick = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_right_stick.Count == 0)
            {
                settings.ollie_customization_length_right_stick = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_left_stick_backwards.Count == 0)
            {
                settings.ollie_customization_rotation_left_stick_backwards = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_left_stick_backwards.Count == 0)
            {
                settings.ollie_customization_length_left_stick_backwards = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_right_stick_backwards.Count == 0)
            {
                settings.ollie_customization_rotation_right_stick_backwards = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_right_stick_backwards.Count == 0)
            {
                settings.ollie_customization_length_right_stick_backwards = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_both_outside.Count == 0)
            {
                settings.ollie_customization_rotation_both_outside = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_both_outside.Count == 0)
            {
                settings.ollie_customization_length_both_outside = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_both_inside.Count == 0)
            {
                settings.ollie_customization_rotation_both_inside = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_both_inside.Count == 0)
            {
                settings.ollie_customization_length_both_inside = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_left2left.Count == 0)
            {
                settings.ollie_customization_rotation_left2left = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_left2left.Count == 0)
            {
                settings.ollie_customization_length_left2left = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_left2right.Count == 0)
            {
                settings.ollie_customization_rotation_left2right = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_left2right.Count == 0)
            {
                settings.ollie_customization_length_left2right = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_right2left.Count == 0)
            {
                settings.ollie_customization_rotation_right2left = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_right2left.Count == 0)
            {
                settings.ollie_customization_length_right2left = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_right2right.Count == 0)
            {
                settings.ollie_customization_rotation_right2right = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_right2right.Count == 0)
            {
                settings.ollie_customization_length_right2right = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_both2left.Count == 0)
            {
                settings.ollie_customization_rotation_both2left = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_both2left.Count == 0)
            {
                settings.ollie_customization_length_both2left = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.ollie_customization_rotation_both2right.Count == 0)
            {
                settings.ollie_customization_rotation_both2right = new List<Vector3>(new Vector3[4]);
                settings.Save(modEntry);
            }
            if (settings.ollie_customization_length_both2right.Count == 0)
            {
                settings.ollie_customization_length_both2right = new List<float> { 24, 24, 24, 24 };
                settings.Save(modEntry);
            }

            if (settings.body_rotations.Count == 0)
            {
                settings.body_rotations = new List<List<Vector3>> { };
                for (int i = 0; i < Enums.StatesReal.Length; i++)
                {
                    List<Vector3> temp = new List<Vector3> { };
                    for (int n = 0; n < Enums.BodyParts.Length; n++)
                    {
                        temp.Add(Enums.OriginalRotations[n]);
                    }
                    settings.body_rotations.Add(temp);
                }
                settings.Save(modEntry);
            }
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {

        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Utils.Log("Toggled " + modEntry.Info.Id);

            if (value)
            {
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                UnityEngine.Object.Destroy(manager);
                harmonyInstance.UnpatchAll(harmonyInstance.Id);
            }

            return true;
        }
    }
}
