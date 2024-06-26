﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidGUI;
using ReplayEditor;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
    class FoldObj
    {
        public bool reference;
        public string text;

        public FoldObj(bool reference, string text)
        {
            this.reference = reference;
            this.text = text;
        }
    }

    public class UIController : MonoBehaviour
    {
        string green = "#b8e994";
        string white = "#ecf0f1";
        string red = "#b71540";
        Texture2D buttonbg;

        public void LoadBG()
        {
            buttonbg = new Texture2D(128, 128);
            var bytes = File.ReadAllBytes(Main.modEntry.Path + "Background.png");
            ImageConversion.LoadImage(buttonbg, bytes, false);
            buttonbg.filterMode = FilterMode.Point;
        }

        bool showMainMenu = false;
        private Rect MainMenuRect = new Rect(20, 20, Screen.width / 6, 20);

        public void updateMainWindow()
        {
            MainMenuRect.height = 0;
            MainMenuRect.width = Screen.width / 6;
        }

        public void Start()
        {
            style.margin = new RectOffset(20, 0, 0, 0);
            boxpadded.padding = new RectOffset(12, 12, 12, 12);
        }

        public void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(Main.settings.Hotkey.keyCode))
            {
                if (showMainMenu)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            }

            if (showMainMenu)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void Open()
        {
            showMainMenu = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            UISounds.Instance.PlayOneShotSelectMajor();
        }

        private void Close()
        {
            showMainMenu = false;
            Cursor.visible = false;
            Main.settings.Save(Main.modEntry);
            UISounds.Instance.PlayOneShotExit();
        }

        bool style_applied = false, loaded = false;
        private void OnGUI()
        {
            Debug.Log(MainMenuRect.width);
            if (showMainMenu)
            {
                GUI.backgroundColor = new Color32(87, 75, 144, 255);
                MainMenuRect = GUILayout.Window(666, MainMenuRect, MainMenu, "<b>Fro's Experimental Mod</b>");

                if (!style_applied)
                {
                    Texture2D flatButtonTex = new Texture2D(1, 1);
                    flatButtonTex.SetPixels(new[] { new Color(1, 1, 1, 1) });
                    flatButtonTex.Apply();
                    RGUIStyle.flatButton.active.background = flatButtonTex;
                    RGUIStyle.flatButton.normal.background = flatButtonTex;
                    RGUIStyle.flatButton.focused.background = flatButtonTex;
                    RGUIStyle.flatButton.hover.background = flatButtonTex;
                    style_applied = true;
                }
            }
        }

        void MainSection()
        {
            //GUILayout.BeginVertical(boxpadded);

            if (RGUI.Button(Main.settings.enabled, "Enabled"))
            {
                Main.settings.enabled = !Main.settings.enabled;
            }

            //GUILayout.EndVertical();
        }

        void Fold(FoldObj obj, string color = "#fad390")
        {
            if (GUILayout.Button($"<b><size=14><color={color}>" + (obj.reference ? "▶" : "▼") + "</color>" + obj.text + "</size></b>", "Label"))
            {
                obj.reference = !obj.reference;

                if (!obj.reference) UISounds.Instance.PlayOneShotSelectionChange();
                else UISounds.Instance.PlayOneShotSelectMinor();

                MainMenuRect.height = 20;
                MainMenuRect.width = Screen.width / 6;
            }
        }

        FoldObj about_fold = new FoldObj(true, "About");
        FoldObj patreons_fold = new FoldObj(true, "Special thanks <3");
        void AboutSection()
        {
            Fold(about_fold, white);
            if (!about_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                GUILayout.Label("<b>fro's experimental mod v1.19.0 for alpha branch XL v1.2.2.X (25/04/2024)</b>");
                GUILayout.Label("Disclaimer: I'm not related to Easy Days Studios and i'm not responsible for any of your actions, use this mod at your own risk.");
                GUILayout.Label("This mod is not intended to harm the game or the respective developer, the online functionality, or the game economy in any purposeful way.");
                GUILayout.Label("I repudiate any type of practice or conduct that involves or promotes racism or any kind of discrimination.");
                GUILayout.Label("This software is distributed 'as is', with no warranty expressed or implied, and no guarantee for accuracy or applicability to any purpose.");

                Fold(patreons_fold);
                if (!patreons_fold.reference)
                {
                    GUILayout.BeginVertical("Box", GUILayout.Width(Screen.width / 2));
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    for (int i = 0; i < Enums.special_patreons.Length; i++)
                    {
                        if (i % 6 == 0)
                        {
                            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                        }
                        GUILayout.Label($"<color=#FFC312>• <b>{Enums.special_patreons[i]}</b></color>");
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj lean_fold = new FoldObj(true, "Leaning / Wallrides");
        void LeanWallrideSection()
        {
            Fold(lean_fold);

            if (!lean_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                if (RGUI.Button(Main.settings.lean, "Lean on double stick to the side"))
                {
                    Main.settings.lean = !Main.settings.lean;
                }

                if (Main.settings.lean)
                {
                    Main.settings.speed = RGUI.SliderFloat(Main.settings.speed, 0f, 1000f, 300f, "In air leaning speed");
                    // Main.settings.grind_speed = RGUI.SliderFloat(Main.settings.grind_speed, 0f, 240f, 40f, "Grinding Speed");
                    Main.settings.wallride_downforce = RGUI.SliderFloat(Main.settings.wallride_downforce, 0f, 200f, 80f, "Wallride downforce");
                    Main.settings.wait_threshold = (int)RGUI.SliderFloat(Main.settings.wait_threshold, 0f, 60f, 10f, "Hold X frames to activate");
                    //Main.settings.input_threshold = RGUI.SliderFloat(Main.settings.input_threshold, 0f, 100f, 20f, "Valid stick vertical area (%)");

                    if (RGUI.Button(Main.settings.swap_lean, "Invert input"))
                    {
                        Main.settings.swap_lean = !Main.settings.swap_lean;
                    }
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj feet_fold = new FoldObj(true, "Dynamic feet");
        FoldObj feet_activation = new FoldObj(true, "Activation states");
        void FeetSection()
        {
            Fold(feet_fold, green);

            if (!feet_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                GUILayout.Label("<b><color=#f9ca24>Dynamically places the skater's feet on the board in the selected states, this feature mainly prevents floaty feet</color></b>", GUILayout.Width(420));
                GUILayout.Label("v1.1.0");
                if (RGUI.Button(Main.settings.feet_rotation, "Follow board rotation"))
                {
                    Main.settings.feet_rotation = !Main.settings.feet_rotation;
                }

                if (RGUI.Button(Main.settings.feet_offset, "Follow board position"))
                {
                    Main.settings.feet_offset = !Main.settings.feet_offset;
                }

                if (Main.settings.feet_offset)
                {
                    Main.settings.left_foot_offset = RGUI.SliderFloat(Main.settings.left_foot_offset, 0.01f, 2f, 1f, "Left shoe height offset");
                    Main.settings.right_foot_offset = RGUI.SliderFloat(Main.settings.right_foot_offset, 0.01f, 2f, 1f, "Right shoe height offset");
                }

                GUILayout.Space(12);

                if (RGUI.Button(Main.settings.jiggle_on_setup, "Jiggle feet on setup"))
                {
                    Main.settings.jiggle_on_setup = !Main.settings.jiggle_on_setup;
                }

                if (Main.settings.jiggle_on_setup)
                {
                    Main.settings.jiggle_delay = RGUI.SliderFloat(Main.settings.jiggle_delay, 0f, 60f, 24f, "Jiggle delay");
                    Main.settings.jiggle_limit = RGUI.SliderFloat(Main.settings.jiggle_limit, 0f, 90f, 40f, "Jiggle angle limit");
                }

                GUILayout.Space(12);

                if (Main.settings.feet_rotation || Main.settings.feet_offset)
                {
                    Fold(feet_activation);

                    if (!feet_activation.reference)
                    {
                        int count = 0;
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        foreach (var state in Enum.GetValues(typeof(PlayerController.CurrentState)))
                        {
                            if (count % 4 == 0 && count != 0)
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                            }

                            if (Main.settings.dynamic_feet_states[count])
                            {
                                GUI.backgroundColor = new Color32(106, 176, 76, 255);
                            }
                            else
                            {
                                GUI.backgroundColor = new Color32(1, 1, 1, 50);
                            }

                            if (GUILayout.Button("<b>" + state.ToString() + "</b>", RGUIStyle.flatButton, GUILayout.Width(92f), GUILayout.Height(26)))
                            {
                                Main.settings.dynamic_feet_states[count] = !Main.settings.dynamic_feet_states[count];
                            }

                            GUI.backgroundColor = Color.black;
                            count++;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.Space(6);
                GUILayout.EndVertical();
            }
        }

        FoldObj hippie_fold = new FoldObj(true, "Hippie jump downforce");
        void HippieSection()
        {
            Fold(hippie_fold, green);

            if (!hippie_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                if (RGUI.Button(Main.settings.hippie, "Hippie jump (B)"))
                {
                    Main.settings.hippie = !Main.settings.hippie;
                }
                if (Main.settings.hippie)
                {
                    Main.settings.HippieForce = RGUI.SliderFloat(Main.settings.HippieForce, 0f, 2f, 1f, "Hippie jump downforce");
                    Main.settings.HippieTime = RGUI.SliderFloat(Main.settings.HippieTime, 0.01f, 4f, 0.3f, "Hippie jump animation time");
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj animpush_fold = new FoldObj(true, "Animations");
        public string[] Animations = new string[] {
            "Waving",
            "Celebrating",
            "Clapping"
        };
        void AnimationAndPushingSection()
        {
            Fold(animpush_fold, green);

            if (!animpush_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                if (RGUI.Button(Main.settings.displacement_curve, "\"Realistic\" animation curve on pop"))
                {
                    Main.settings.displacement_curve = !Main.settings.displacement_curve;
                }
                RGUI.WarningLabel("This feature changes pop time and will affect your pop height");

                GUILayout.Space(12);

                if (RGUI.Button(Main.settings.push_by_velocity, "Velocity based pushing"))
                {
                    Main.settings.push_by_velocity = !Main.settings.push_by_velocity;
                    if (Main.settings.push_by_velocity) Main.settings.sonic_mode = false;
                }
                if (RGUI.Button(Main.settings.sonic_mode, "Sonic© pushing"))
                {
                    Main.settings.sonic_mode = !Main.settings.sonic_mode;
                    if (Main.settings.sonic_mode) Main.settings.push_by_velocity = false;
                }

                if (RGUI.Button(Main.settings.push2push, "Push repeatedly"))
                {
                    Main.settings.push2push = !Main.settings.push2push;
                }
                GUILayout.Space(12);

                if (RGUI.Button(Main.settings.bails, "Alternative bails"))
                {
                    Main.settings.bails = !Main.settings.bails;
                }
                GUILayout.Space(12);


                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Wave on:</b>");
                Main.settings.wave_on = RGUI.SelectionPopup(Main.settings.wave_on, Enums.States);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Celebrate on:</b>");
                Main.settings.celebrate_on = RGUI.SelectionPopup(Main.settings.celebrate_on, Enums.States);
                GUILayout.EndHorizontal();

                GUILayout.Space(12);

                if (RGUI.Button(Main.settings.shuv_fix, "Shuv leg fix"))
                {
                    Main.settings.shuv_fix = !Main.settings.shuv_fix;
                }
                GUILayout.Space(12);

                if (RGUI.Button(Main.settings.bump_anim, "Alternative bumps"))
                {
                    Main.settings.bump_anim = !Main.settings.bump_anim;
                }
                if (Main.settings.bump_anim)
                {
                    if (RGUI.Button(Main.settings.bump_anim_pop, "Play animation"))
                    {
                        Main.settings.bump_anim_pop = !Main.settings.bump_anim_pop;
                    }

                    if (Main.settings.bump_anim_pop) Main.settings.bump_pop_delay = RGUI.SliderFloat(Main.settings.bump_pop_delay, 0f, 1f, .15f, "Animation delay");
                }

                GUILayout.Space(12);

                if (RGUI.Button(Main.settings.catch_acc_enabled, "Realistic catch"))
                {
                    Main.settings.catch_acc_enabled = !Main.settings.catch_acc_enabled;
                }

                if (Main.settings.catch_acc_enabled)
                {
                    //Main.settings.catch_lerp_speed = RGUI.SliderFloat(Main.settings.catch_lerp_speed, 12f, 30f, 30f, "Foot catch speed");

                    Main.settings.catch_left_time = RGUI.SliderFloat(Main.settings.catch_left_time, 0.01f, .1f, .04f, "Left foot catch time");
                    Main.settings.catch_right_time = RGUI.SliderFloat(Main.settings.catch_right_time, 0.01f, .1f, .04f, "Right foot catch time");

                    if (RGUI.Button(Main.settings.catch_acc_onflick, "Flick to catch"))
                    {
                        Main.settings.catch_acc_onflick = !Main.settings.catch_acc_onflick;
                    }

                    if (Main.settings.catch_acc_onflick)
                    {
                        Main.settings.FlickThreshold = RGUI.SliderFloat(Main.settings.FlickThreshold, 0f, 1f, .6f, "Flick threshold");
                    }

                    if(Main.settings.catch_acc_onflick) GUILayout.Label("<b><color=#f9ca24>XXL3 (F7) Catch > Flick to Catch needs to be disabled</color></b>", GUILayout.Width(380));
                }

                GUILayout.EndVertical();
            }
        }

        FoldObj filmer_fold = new FoldObj(true, "Filmer mode");
        FoldObj instructions_fold = new FoldObj(true, "Instructions");
        string[] FilmerActivation = new string[] { "On pump input", "Always on" };
        void FilmerSection()
        {
            Fold(filmer_fold);

            if (!filmer_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);

                if (RGUI.Button(Main.settings.follow_mode_left, "Follow player with left hand"))
                {
                    Main.settings.follow_mode_left = !Main.settings.follow_mode_left;
                }

                if (RGUI.Button(Main.settings.follow_mode_right, "Follow player with right hand"))
                {
                    Main.settings.follow_mode_right = !Main.settings.follow_mode_right;
                }

                if (RGUI.Button(Main.settings.follow_mode_head, "Follow player with head"))
                {
                    Main.settings.follow_mode_head = !Main.settings.follow_mode_head;
                }

                if (Main.settings.follow_mode_left || Main.settings.follow_mode_right || Main.settings.follow_mode_head)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("<b>Activation</b>");
                    Main.settings.multi_filmer_activation = RGUI.SelectionPopup(Main.settings.multi_filmer_activation, FilmerActivation);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("<b>Player username</b>");
                    Main.settings.selected_player = RGUI.SelectionPopup(Main.settings.selected_player, Main.controller.getListOfPlayers());
                    GUILayout.EndHorizontal();

                    Main.settings.follow_target_offset = RGUI.SliderFloat(Main.settings.follow_target_offset, -1f, 1f, -.4f, "Camera target angle");
                    Main.settings.filmer_arm_angle = RGUI.SliderFloat(Main.settings.filmer_arm_angle, 0f, 90f, 37.5f, "Camera arm angle");
                    //Main.settings.lookat_speed = RGUI.SliderFloat(Main.settings.lookat_speed, 0f, 1f, 1f, "Speed");

                    if (RGUI.Button(Main.settings.camera_feet, "Put feet on the ground when pumping"))
                    {
                        Main.settings.camera_feet = !Main.settings.camera_feet;
                    }

                    GUILayout.Space(6);

                    if (RGUI.Button(Main.settings.filmer_light, "Camera Light"))
                    {
                        Main.settings.filmer_light = !Main.settings.filmer_light;
                    }

                    if (Main.settings.filmer_light)
                    {
                        Main.settings.filmer_light_intensity = RGUI.SliderFloat(Main.settings.filmer_light_intensity, 0f, 10000f, 6000f, "Light intensity");
                        Main.settings.filmer_light_spotangle = RGUI.SliderFloat(Main.settings.filmer_light_spotangle, 0f, 360f, 120f, "Light angle");
                        Main.settings.filmer_light_range = RGUI.SliderFloat(Main.settings.filmer_light_range, 0f, 20f, 5f, "Light range");
                    }
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj multi_fold = new FoldObj(true, "Settings");
        void MultiSection()
        {
            Fold(multi_fold, green);

            if (!multi_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                if (RGUI.Button(Main.settings.reset_inactive, "Disable multiplayer AFK timeout"))
                {
                    Main.settings.reset_inactive = !Main.settings.reset_inactive;
                }
                if (RGUI.Button(Main.settings.disable_popup, "Disable multiplayer popup messages"))
                {
                    Main.settings.disable_popup = !Main.settings.disable_popup;
                    Main.controller.DisableMultiPopup(Main.settings.disable_popup);
                }
                if (RGUI.Button(Main.settings.multiplayer_collision, "Enable player collision - WIP"))
                {
                    Main.settings.multiplayer_collision = !Main.settings.multiplayer_collision;
                }
                if (Main.settings.multiplayer_collision)
                {
                    if (RGUI.Button(Main.settings.show_colliders, "Show colliders"))
                    {
                        Main.settings.show_colliders = !Main.settings.show_colliders;
                    }
                }


                Main.settings.multiplayer_lobby_size = (int)RGUI.SliderFloat(Main.settings.multiplayer_lobby_size, 1f, 35f, 20f, "Multiplayer lobby size");
                if (Main.settings.multiplayer_lobby_size > 35) Main.settings.multiplayer_lobby_size = 35;
                if (Main.settings.multiplayer_lobby_size < 1) Main.settings.multiplayer_lobby_size = 1;

                // Main.settings.RoomIDlength = (int)RGUI.SliderFloat(Main.settings.RoomIDlength, 1f, 5f, 5f, "Multiplayer code size");

                if (!MultiplayerManager.Instance.InRoom)
                {
                    if (GUILayout.Button("Create public room", GUILayout.Height(42)))
                    {
                        Main.multi.CreateRoom();
                    }
                }

                GUILayout.EndVertical();
            }
        }

        FoldObj multichat_fold = new FoldObj(true, "Chat");
        void ChatSection()
        {
            Fold(multichat_fold, green);

            if (!multichat_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                if (RGUI.Button(Main.settings.chat_messages, "Enable chat messages"))
                {
                    Main.settings.chat_messages = !Main.settings.chat_messages;
                }
                if (Main.settings.chat_messages)
                {
                    RGUI.WarningLabel("There are in total 11 pages of predefined messages you can use");
                    Main.settings.left_page = (int)RGUI.SliderFloat(Main.settings.left_page, 0f, 10f, 0f, "Left d-pad page");
                    Main.settings.right_page = (int)RGUI.SliderFloat(Main.settings.right_page, 0f, 10f, 1f, "Right d-pad page");
                    Main.settings.up_page = (int)RGUI.SliderFloat(Main.settings.up_page, 0f, 10f, 2f, "Up d-pad page");
                    Main.settings.down_page = (int)RGUI.SliderFloat(Main.settings.down_page, 0f, 10f, 3f, "Down d-pad page");
                }

                GUILayout.EndVertical();
            }
        }

        void ClothColliders()
        {
            Cloth[] dynamic = FindObjectsOfType<Cloth>();
            List<CapsuleCollider> colliders = new List<CapsuleCollider>();

            for (int i = 0; i < PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles.Length; i++)
            {
                if (i != 4 && i != 5 && i != 7 && i != 8)
                {
                    if (PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].transform.gameObject.GetComponent<CapsuleCollider>() != null)
                    {
                        colliders.Add(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].transform.gameObject.GetComponent<CapsuleCollider>());
                    }
                }
            }

            /*colliders.Add(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.gameObject.GetComponent<CapsuleCollider>());
            colliders.Add(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.gameObject.GetComponent<CapsuleCollider>());*/

            foreach (Cloth c in dynamic)
            {
                c.stretchingStiffness = 1;
                c.enableContinuousCollision = true;
                c.stiffnessFrequency = 1;
                c.capsuleColliders = colliders.ToArray();
            }
        }


        public string[] Keyframe_States = new string[] {
            "Head",
            "Left Hand",
            "Right Hand",
            //"Filmer Object"
        };

        FoldObj camera_fold = new FoldObj(true, "Camera");
        FoldObj camera_settings_fold = new FoldObj(true, "Settings");
        FoldObj camera_shake_fold = new FoldObj(true, "Camera shake");
        FoldObj keyframe_fold = new FoldObj(true, "Keyframe creator");
        void CameraSection()
        {
            Fold(camera_fold, green);

            if (!camera_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                Fold(camera_settings_fold, green);
                if (!camera_settings_fold.reference)
                {

                    if (RGUI.Button(Main.settings.camera_avoidance, "v1.2.x.x obstacle avoidance"))
                    {
                        Main.settings.camera_avoidance = !Main.settings.camera_avoidance;
                        Main.controller.DisableCameraCollider(Main.settings.camera_avoidance);
                    }

                    if (RGUI.Button(Main.settings.filmer_object, "Filmer object"))
                    {
                        Main.settings.filmer_object = !Main.settings.filmer_object;
                        Main.controller.DisableCameraCollider(Main.settings.camera_avoidance);
                    }

                    if (Main.settings.filmer_object)
                    {
                        GUILayout.BeginHorizontal();
                        Main.settings.filmer_object_target = GUILayout.TextField(Main.settings.filmer_object_target, 666, GUILayout.Height(21f));

                        if (GUILayout.Button("Scan object", RGUIStyle.button, GUILayout.Width(86)))
                        {
                            Main.controller.scanObject();
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Label("Object " + (Main.controller.object_found != null ? "found" : "not found, check the object name"));
                    }
                }

                Fold(camera_shake_fold, green);
                if (!camera_shake_fold.reference)
                {
                    if (RGUI.Button(Main.settings.camera_shake, "Velocity based shake and field of view"))
                    {
                        Main.settings.camera_shake = !Main.settings.camera_shake;
                    }

                    if (Main.settings.camera_shake)
                    {
                        Main.settings.camera_shake_offset = RGUI.SliderFloat(Main.settings.camera_shake_offset, 0f, 16f, 7f, "Shake minimum velocity");
                        Main.settings.camera_shake_multiplier = RGUI.SliderFloat(Main.settings.camera_shake_multiplier, 0f, 10f, 3f, "Shake multiplier");
                        /*Main.settings.camera_shake_range = RGUI.SliderFloat(Main.settings.camera_shake_range, 0f, 1f, .2f, "Camera shake range");*/
                        Main.settings.camera_shake_length = (int)RGUI.SliderFloat(Main.settings.camera_shake_length, 1f, 10f, 4f, "Shake animation length");
                        GUILayout.Space(8);
                        Main.settings.camera_fov_offset = RGUI.SliderFloat(Main.settings.camera_fov_offset, 0f, 16f, 4f, "FOV minimum velocity");
                        Main.settings.camera_shake_fov_multiplier = RGUI.SliderFloat(Main.settings.camera_shake_fov_multiplier, 0f, 5f, 1.5f, "FOV multiplier");
                    }
                }

                Fold(keyframe_fold);
                if (!keyframe_fold.reference)
                {
                    GUILayout.Label("<b><color=#f9ca24>Use this feature for creating filmer mode or first person keyframes on the replay editor</color></b>", GUILayout.Width(380));

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("<b>Target:</b>");
                    Main.settings.keyframe_target = RGUI.SelectionPopup(Main.settings.keyframe_target, Keyframe_States);
                    GUILayout.EndHorizontal();

                    if (RGUI.Button(Main.settings.keyframe_start_of_clip, "Generate from beginning"))
                    {
                        Main.settings.keyframe_start_of_clip = !Main.settings.keyframe_start_of_clip;
                    }
                    Main.settings.keyframe_sample = (int)RGUI.SliderFloat(Main.settings.keyframe_sample, 2f, 200f, 40f, "Number of keyframes to create");
                    Main.settings.keyframe_fov = (int)RGUI.SliderFloat(Main.settings.keyframe_fov, 1f, 180f, 120f, "Keyframe field of view");
                    Main.settings.time_offset = RGUI.SliderFloat(Main.settings.time_offset, -1f, 1f, 0f, "Time offset");

                    if (Main.controller.keyframe_state == true)
                    {
                        if (GUILayout.Button("Cancel creation", GUILayout.Height(32)))
                        {
                            Main.controller.keyframe_state = false;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Create keyframes", GUILayout.Height(32)))
                        {
                            ReplayEditorController.Instance.cameraController.DeleteAllKeyFrames();
                            Main.controller.keyframe_state = true;
                        }
                    }
                }

                GUILayout.EndVertical();
            }
        }

        FoldObj grinds_fold = new FoldObj(true, "Verticality");
        string selected_grind_vert = "BsFiftyFifty";
        void GrindsSection()
        {
            Fold(grinds_fold, green);

            if (!grinds_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                /*GUILayout.BeginHorizontal();
                GUILayout.Label("Grind:");
                selected_grind_vert = RGUI.SelectionPopup(selected_grind_vert, Enums.GrindType);
                GUILayout.EndHorizontal();*/
                Main.settings.GrindFlipVerticality = RGUI.SliderFloat(Main.settings.GrindFlipVerticality, -1f, 1f, 0f, "Out of grinds");
                GUILayout.EndVertical();

                GUILayout.BeginVertical(boxpadded);
                Main.settings.ManualFlipVerticality = RGUI.SliderFloat(Main.settings.ManualFlipVerticality, -1f, 1f, 0f, "Out of manuals");
                GUILayout.EndVertical();
            }
        }

        FoldObj body_fold = new FoldObj(true, "Body");
        FoldObj muscle_fold = new FoldObj(true, "Body parts scale");
        FoldObj body_rotation_fold = new FoldObj(true, "Body parts rotation");
        int selected_state_bp = 0, selected_body_part = 0;
        void BodySection()
        {
            Fold(body_fold, green);

            if (!body_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);

                Main.settings.custom_scale.x = RGUI.SliderFloat(Main.settings.custom_scale.x, 0.01f, 2f, 1f, "Scale body x");
                Main.settings.custom_scale.y = RGUI.SliderFloat(Main.settings.custom_scale.y, 0.01f, 2f, 1f, "Scale body y");
                Main.settings.custom_scale.z = RGUI.SliderFloat(Main.settings.custom_scale.z, 0.01f, 2f, 1f, "Scale body z");
                GUILayout.Space(12);

                Main.settings.comOffset_y = RGUI.SliderFloat(Main.settings.comOffset_y, -1f, 1f, 0.07f, "Crouch offset");

                GUILayout.Space(12);

                Fold(muscle_fold, green);
                if (!muscle_fold.reference)
                {
                    GUILayout.Space(12);
                    Main.settings.custom_scale_pelvis = RGUI.SliderFloat(Main.settings.custom_scale_pelvis, 0.01f, 4f, 1f, "Pelvis scale");
                    Main.settings.custom_scale_spine = RGUI.SliderFloat(Main.settings.custom_scale_spine, 0.01f, 4f, 1f, "Spine scale");
                    Main.settings.custom_scale_spine2 = RGUI.SliderFloat(Main.settings.custom_scale_spine2, 0.01f, 4f, 1f, "Spine2 scale");

                    Main.settings.custom_scale_head = RGUI.SliderFloat(Main.settings.custom_scale_head, 0.01f, 4f, 1f, "Head scale");
                    Main.settings.custom_scale_neck = RGUI.SliderFloat(Main.settings.custom_scale_neck, 0.01f, 4f, 1f, "Neck scale");

                    Main.settings.custom_scale_arm_l = RGUI.SliderFloat(Main.settings.custom_scale_arm_l, 0.01f, 4f, 1f, "Left arm scale");
                    Main.settings.custom_scale_forearm_l = RGUI.SliderFloat(Main.settings.custom_scale_forearm_l, 0.01f, 4f, 1f, "Left forearm scale");

                    Main.settings.custom_scale_hand_l = RGUI.SliderFloat(Main.settings.custom_scale_hand_l, 0.01f, 4f, 1f, "Left hand scale");

                    Main.settings.custom_scale_arm_r = RGUI.SliderFloat(Main.settings.custom_scale_arm_r, 0.01f, 4f, 1f, "Right arm scale");
                    Main.settings.custom_scale_forearm_r = RGUI.SliderFloat(Main.settings.custom_scale_forearm_r, 0.01f, 4f, 1f, "Right forearm scale");

                    Main.settings.custom_scale_hand_r = RGUI.SliderFloat(Main.settings.custom_scale_hand_r, 0.01f, 4f, 1f, "Right hand scale");

                    Main.settings.custom_scale_upleg_l = RGUI.SliderFloat(Main.settings.custom_scale_upleg_l, 0.01f, 4f, 1f, "Left upleg scale");
                    Main.settings.custom_scale_leg_l = RGUI.SliderFloat(Main.settings.custom_scale_leg_l, 0.01f, 4f, 1f, "Left leg scale");

                    Main.settings.custom_scale_foot_l = RGUI.SliderFloat(Main.settings.custom_scale_foot_l, 0.01f, 4f, 1f, "Left foot scale");

                    Main.settings.custom_scale_upleg_r = RGUI.SliderFloat(Main.settings.custom_scale_upleg_r, 0.01f, 4f, 1f, "Right upleg scale");
                    Main.settings.custom_scale_leg_r = RGUI.SliderFloat(Main.settings.custom_scale_leg_r, 0.01f, 4f, 1f, "Right leg scale");

                    Main.settings.custom_scale_foot_r = RGUI.SliderFloat(Main.settings.custom_scale_foot_r, 0.01f, 4f, 1f, "Right foot scale");
                }

                GUILayout.Space(12);

                Fold(body_rotation_fold, green);
                if (!body_rotation_fold.reference)
                {
                    GUILayout.Space(12);
                    if (RGUI.Button(Main.settings.body_rotation, "Body rotations"))
                    {
                        Main.settings.body_rotation = !Main.settings.body_rotation;
                    }

                    if (Main.settings.body_rotation)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("State:");
                        selected_state_bp = RGUI.SelectionPopup(selected_state_bp, Enums.StatesReal, 0, 0);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Body part:");
                        selected_body_part = RGUI.SelectionPopup(selected_body_part, Enums.BodyParts, 1, 12);
                        GUILayout.EndHorizontal();

                        Vector3 rotation = Main.settings.body_rotations[selected_state_bp][selected_body_part];

                        rotation.x = RGUI.SliderFloat(rotation.x, -360f, 360f, Enums.OriginalRotations[selected_body_part].x, "X");
                        rotation.y = RGUI.SliderFloat(rotation.y, -360f, 360f, Enums.OriginalRotations[selected_body_part].y, "Y");
                        rotation.z = RGUI.SliderFloat(rotation.z, -360f, 360f, Enums.OriginalRotations[selected_body_part].z, "Z");

                        Main.settings.body_rotations[selected_state_bp][selected_body_part] = rotation;
                    }
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical(boxpadded);
                if (RGUI.Button(Main.settings.alternative_arms, "Alternative setup arms"))
                {
                    Main.settings.alternative_arms = !Main.settings.alternative_arms;
                }
                if (Main.settings.alternative_arms)
                {
                    if (RGUI.Button(Main.settings.alternative_arms_damping, "Change setup arms damping"))
                    {
                        Main.settings.alternative_arms_damping = !Main.settings.alternative_arms_damping;
                    }
                }
                Main.settings.left_hand_weight = RGUI.SliderFloat(Main.settings.left_hand_weight, 0.01f, 5f, 1f, "Left hand weight");
                Main.settings.right_hand_weight = RGUI.SliderFloat(Main.settings.right_hand_weight, 0.01f, 5f, 1f, "Right hand weight");
                GUILayout.EndVertical();
            }

        }

        FoldObj head_fold = new FoldObj(true, "Head");
        FoldObj lookforwars_fold = new FoldObj(true, "Activation states");
        FoldObj lookforward_stance_fold = new FoldObj(true, "Neck rotation");
        int selected_state = 1;
        int selected_grind = 0;
        string selected_stance = "Switch";
        void HeadSection()
        {
            Fold(head_fold, green);
            if (!head_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                if (RGUI.Button(Main.settings.look_forward, "Look forward (switch and fakie)"))
                {
                    Main.settings.look_forward = !Main.settings.look_forward;
                }

                if (Main.settings.look_forward)
                {
                    Main.settings.look_forward_delay = (int)RGUI.SliderFloat(Main.settings.look_forward_delay, 0f, 60f, 0f, "Delay (frames)");
                    Main.settings.look_forward_length = (int)RGUI.SliderFloat(Main.settings.look_forward_length, 0f, 60f, 18f, "Animation length (frames)");
                    Fold(lookforwars_fold);
                    if (!lookforwars_fold.reference)
                    {
                        int count = 0;
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        foreach (var state in Enum.GetValues(typeof(PlayerController.CurrentState)))
                        {
                            if (count % 4 == 0 && count != 0)
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                            }

                            if (Main.settings.look_forward_states[count])
                            {
                                GUI.backgroundColor = new Color32(106, 176, 76, 255);
                            }
                            else
                            {
                                GUI.backgroundColor = new Color32(1, 1, 1, 50);
                            }

                            if (GUILayout.Button("<b>" + state.ToString() + "</b>", RGUIStyle.flatButton, GUILayout.Width(92f), GUILayout.Height(26)))
                            {
                                Main.settings.look_forward_states[count] = !Main.settings.look_forward_states[count];
                            }

                            GUI.backgroundColor = Color.black;
                            count++;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }

                    Fold(lookforward_stance_fold, red);
                    if (!lookforward_stance_fold.reference)
                    {
                        if (!Main.settings.look_forward_states[selected_state]) RGUI.WarningLabel($"{Enums.StatesReal[selected_state]} is not enabled");
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("State:");
                        selected_state = RGUI.SelectionPopup(selected_state, Enums.StatesReal);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Stance:");
                        selected_stance = RGUI.SelectionPopup(selected_stance, Enums.Stances);
                        GUILayout.EndHorizontal();

                        if (selected_state == 9)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Grind:");
                            selected_grind = RGUI.SelectionPopup(selected_grind, Enums.GrindType);
                            GUILayout.EndHorizontal();

                            GUILayout.Label("Current detected grind: <b><color=green>" + Main.controller.getStance() + " " + PlayerController.Instance.boardController.triggerManager.grindDetection.grindType + "</color></b>");
                            Vector3 temp_vector_rot = selected_stance == "Fakie" ? Main.settings.head_rotation_grinds_fakie[selected_grind] : Main.settings.head_rotation_grinds_switch[selected_grind];
                            temp_vector_rot.x = RGUI.SliderFloat(temp_vector_rot.x, -120f, 120f, 0f, "X");
                            temp_vector_rot.y = RGUI.SliderFloat(temp_vector_rot.y, -120f, 120f, 0f, "Y");
                            temp_vector_rot.z = RGUI.SliderFloat(temp_vector_rot.z, -120f, 120f, 0f, "Z");
                            if (selected_stance == "Fakie") Main.settings.head_rotation_grinds_fakie[selected_grind] = temp_vector_rot;
                            else Main.settings.head_rotation_grinds_switch[selected_grind] = temp_vector_rot;

                        }
                        else
                        {
                            Vector3 temp_vector_rot = selected_stance == "Fakie" ? Main.settings.head_rotation_fakie[selected_state] : Main.settings.head_rotation_switch[selected_state];
                            temp_vector_rot.x = RGUI.SliderFloat(temp_vector_rot.x, -120f, 120f, 0f, "X");
                            temp_vector_rot.y = RGUI.SliderFloat(temp_vector_rot.y, -120f, 120f, 0f, "Y");
                            temp_vector_rot.z = RGUI.SliderFloat(temp_vector_rot.z, -120f, 120f, 0f, "Z");
                            if (selected_stance == "Fakie") Main.settings.head_rotation_fakie[selected_state] = temp_vector_rot;
                            else Main.settings.head_rotation_switch[selected_state] = temp_vector_rot;
                        }

                        /*Main.settings.reset_head.x = RGUI.SliderFloat(Main.settings.reset_head.x, -120f, 120f, 0f, "X");
                        Main.settings.reset_head.y = RGUI.SliderFloat(Main.settings.reset_head.y, -120f, 120f, 0f, "Y");
                        Main.settings.reset_head.z = RGUI.SliderFloat(Main.settings.reset_head.z, -120f, 120f, 0f, "Z");*/
                    }
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj experimental_fold = new FoldObj(true, "Skater center of mass");

        void ReallyExperimental()
        {
            Fold(experimental_fold, red);

            if (!experimental_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                Main.settings.Kp = RGUI.SliderFloat(Main.settings.Kp, 0f, 10000f, 5000f, "Riding force");
                Main.settings.Kd = RGUI.SliderFloat(Main.settings.Kd, 0f, 2000f, 900f, "Riding smoothness");
                Main.settings.KpImpact = RGUI.SliderFloat(Main.settings.KpImpact, 0f, 10000f, 5000f, "Impact force");
                Main.settings.KdImpact = RGUI.SliderFloat(Main.settings.KdImpact, 0f, 2000f, 1000f, "Impact smoothness");
                Main.settings.KpSetup = RGUI.SliderFloat(Main.settings.KpSetup, 0f, 40000f, 20000f, "Setup force");
                Main.settings.KdSetup = RGUI.SliderFloat(Main.settings.KdSetup, 0f, 3000f, 1500f, "Setup smoothness");
                /*Main.settings.KpGrind = RGUI.SliderFloat(Main.settings.KpGrind, 0f, 4000f, 2000f, "KpGrind");
                Main.settings.KdGrind = RGUI.SliderFloat(Main.settings.KdGrind, 0f, 2000f, 900f, "KdGrind");*/
                Main.settings.comHeightRiding = RGUI.SliderFloat(Main.settings.comHeightRiding, -1f, 2f, 1.06f, "Skater height riding");
                Main.settings.maxLegForce = RGUI.SliderFloat(Main.settings.maxLegForce, 0f, 10000f, 5000f, "Max force");

                /*PlayerController.Instance.boardController.Kp = RGUI.SliderFloat(PlayerController.Instance.boardController.Kp, 0f, 10000f, 5000f, "Board KP");
                PlayerController.Instance.boardController.Ki = RGUI.SliderFloat(PlayerController.Instance.boardController.Ki, 0f, 10000f, 5000f, "Board KI");
                PlayerController.Instance.boardController.Kd = RGUI.SliderFloat(PlayerController.Instance.boardController.Kd, 0f, 10000f, 5000f, "Board KD");*/

                GUILayout.EndVertical();
            }
        }

        FoldObj skate_fold = new FoldObj(true, "Skate");
        FoldObj skate_settings_fold = new FoldObj(true, "Settings");
        UIFold coping_fold = new UIFold("Coping");
        UIFold manual_fold = new UIFold("Manual");
        UIFold map_fold = new UIFold("Map");
        FoldObj customizer_fold = new FoldObj(true, "Trick customizer");
        // string selected_stance_customizer = "Regular";
        int selected_stance_customizer = 0;

        public string[] tricks_customizer = new string[] {
            "Ollie",
            "Kickflip",
            "Heelflip"
        };

        public string[] input_types = new string[] {
            "Both sticks to the front",
            "Both sticks to the back",
            "Left stick to the front",
            "Right stick to the front",
            "Left stick to the back",
            "Right stick to the back",
            "Both sticks to the outside",
            "Both sticks to the inside",
            "Left stick to the left",
            "Left stick to the right",
            "Right stick to the left",
            "Right stick to the right",
            "Both sticks to the left",
            "Both sticks to the right"
        };

        int selected_input = 0;

        void SkateSection()
        {
            Fold(skate_fold, green);

            if (!skate_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                /*Fold(skate_settings_fold, green);

                if (!skate_settings_fold.reference)
                {*/
                if (RGUI.Button(Main.settings.wobble, "Wobble on high speed"))
                {
                    Main.settings.wobble = !Main.settings.wobble;
                }

                if (Main.settings.wobble)
                {
                    Main.settings.wobble_offset = RGUI.SliderFloat(Main.settings.wobble_offset, 0f, 16f, 4f, "Minimum velocity for wobbling");
                }

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.BetterDecay, "Better friction / decay"))
                {
                    Main.settings.BetterDecay = !Main.settings.BetterDecay;
                }

                Main.settings.decay = RGUI.SliderFloat(Main.settings.decay, 0f, 4f, 1.5f, "Friction force");

                GUILayout.Space(6);

                Main.settings.nose_tail_collider = RGUI.SliderFloat(Main.settings.nose_tail_collider, 0f, 2f, 1f, "Nose and tail collider height");

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.powerslide_force, "Add force to powerslides"))
                {
                    Main.settings.powerslide_force = !Main.settings.powerslide_force;
                }

                if (Main.settings.powerslide_force)
                {
                    if (RGUI.Button(Main.settings.powerslide_velocitybased, "Velocity based"))
                    {
                        Main.settings.powerslide_velocitybased = !Main.settings.powerslide_velocitybased;
                    }
                }

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.force_stick_backwards, "Add stomp force on back foot stick to the back"))
                {
                    Main.settings.force_stick_backwards = !Main.settings.force_stick_backwards;
                }
                if (Main.settings.force_stick_backwards)
                {
                    Main.settings.force_stick_backwards_multiplier = RGUI.SliderFloat(Main.settings.force_stick_backwards_multiplier, 0f, .5f, .125f, "Stomp force");
                }

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.forward_force_onpop, "Add forward force on pop"))
                {
                    Main.settings.forward_force_onpop = !Main.settings.forward_force_onpop;
                }
                if (Main.settings.forward_force_onpop)
                {
                    Main.settings.forward_force = RGUI.SliderFloat(Main.settings.forward_force, -1f, 2f, .35f, "Forward force");
                }

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.customGravityWhileRiding, "Custom gravity modifiers"))
                {
                    Main.settings.customGravityWhileRiding = !Main.settings.customGravityWhileRiding;
                }
                if (Main.settings.customGravityWhileRiding)
                {
                    Main.settings.customGravityMultiplier = RGUI.SliderFloat(Main.settings.customGravityMultiplier, 0f, 10f, 1f, "Riding multiplier");
                }

                GUILayout.Space(6);

                /*GUILayout.Space(6);

                PlayerController.Instance.boardController.minManualAngle = RGUI.SliderFloat(PlayerController.Instance.boardController.minManualAngle, 0, 40f, 10f, "Min manual angle");
                PlayerController.Instance.boardController.maxManualAngle = RGUI.SliderFloat(PlayerController.Instance.boardController.maxManualAngle, 0, 40f, 10f, "Max manual angle");*/

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.custom_board_correction, "Custom board correction"))
                {
                    Main.settings.custom_board_correction = !Main.settings.custom_board_correction;
                }

                if (Main.settings.custom_board_correction)
                {
                    Main.settings.board_p = RGUI.SliderFloat(Main.settings.board_p, 0f, 10000f, 5000, "Board P");
                    Main.settings.board_i = RGUI.SliderFloat(Main.settings.board_i, 0f, 2f, 0, "Board I");
                    Main.settings.board_d = RGUI.SliderFloat(Main.settings.board_d, 0f, 2f, 1, "Board D");
                }


                GUILayout.EndVertical();
            }

            TurnSection();

            Fold(customizer_fold, red);

            if (!customizer_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                if (RGUI.Button(Main.settings.trick_customization, "Enabled"))
                {
                    Main.settings.trick_customization = !Main.settings.trick_customization;
                }

                if (Main.settings.trick_customization)
                {
                    if (RGUI.Button(Main.settings.trick_customizer_grinds, "Apply on grinds"))
                    {
                        Main.settings.trick_customizer_grinds = !Main.settings.trick_customizer_grinds;
                    }

                    GUILayout.Space(12);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Stance:");
                    selected_stance_customizer = RGUI.SelectionPopup(selected_stance_customizer, Enums.StancesCustomizer);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Input:");
                    selected_input = RGUI.SelectionPopup(selected_input, input_types);
                    GUILayout.EndHorizontal();

                    // this is not ideal but letsgo
                    Vector3 rotation = Main.settings.ollie_customization_rotation[selected_stance_customizer];
                    if (selected_input == 1) rotation = Main.settings.ollie_customization_rotation_backwards[selected_stance_customizer];
                    if (selected_input == 2) rotation = Main.settings.ollie_customization_rotation_left_stick[selected_stance_customizer];
                    if (selected_input == 3) rotation = Main.settings.ollie_customization_rotation_right_stick[selected_stance_customizer];
                    if (selected_input == 4) rotation = Main.settings.ollie_customization_rotation_left_stick_backwards[selected_stance_customizer];
                    if (selected_input == 5) rotation = Main.settings.ollie_customization_rotation_right_stick_backwards[selected_stance_customizer];
                    if (selected_input == 6) rotation = Main.settings.ollie_customization_rotation_both_outside[selected_stance_customizer];
                    if (selected_input == 7) rotation = Main.settings.ollie_customization_rotation_both_inside[selected_stance_customizer];
                    if (selected_input == 8) rotation = Main.settings.ollie_customization_rotation_left2left[selected_stance_customizer];
                    if (selected_input == 9) rotation = Main.settings.ollie_customization_rotation_left2right[selected_stance_customizer];
                    if (selected_input == 10) rotation = Main.settings.ollie_customization_rotation_right2left[selected_stance_customizer];
                    if (selected_input == 11) rotation = Main.settings.ollie_customization_rotation_right2right[selected_stance_customizer];
                    if (selected_input == 12) rotation = Main.settings.ollie_customization_rotation_both2left[selected_stance_customizer];
                    if (selected_input == 13) rotation = Main.settings.ollie_customization_rotation_both2right[selected_stance_customizer];

                    rotation.z = RGUI.SliderFloat(rotation.z, -180f, 180f, 0f, "Roll");
                    rotation.x = RGUI.SliderFloat(rotation.x, -180f, 180f, 0f, "Pitch");
                    rotation.y = RGUI.SliderFloat(rotation.y, -180f, 180f, 0f, "Yaw");

                    if (selected_input == 0) Main.settings.ollie_customization_rotation[selected_stance_customizer] = rotation;
                    if (selected_input == 1) Main.settings.ollie_customization_rotation_backwards[selected_stance_customizer] = rotation;
                    if (selected_input == 2) Main.settings.ollie_customization_rotation_left_stick[selected_stance_customizer] = rotation;
                    if (selected_input == 3) Main.settings.ollie_customization_rotation_right_stick[selected_stance_customizer] = rotation;
                    if (selected_input == 4) Main.settings.ollie_customization_rotation_left_stick_backwards[selected_stance_customizer] = rotation;
                    if (selected_input == 5) Main.settings.ollie_customization_rotation_right_stick_backwards[selected_stance_customizer] = rotation;
                    if (selected_input == 6) Main.settings.ollie_customization_rotation_both_outside[selected_stance_customizer] = rotation;
                    if (selected_input == 7) Main.settings.ollie_customization_rotation_both_inside[selected_stance_customizer] = rotation;
                    if (selected_input == 8) Main.settings.ollie_customization_rotation_left2left[selected_stance_customizer] = rotation;
                    if (selected_input == 9) Main.settings.ollie_customization_rotation_left2right[selected_stance_customizer] = rotation;
                    if (selected_input == 10) Main.settings.ollie_customization_rotation_right2left[selected_stance_customizer] = rotation;
                    if (selected_input == 11) Main.settings.ollie_customization_rotation_right2right[selected_stance_customizer] = rotation;
                    if (selected_input == 12) Main.settings.ollie_customization_rotation_both2left[selected_stance_customizer] = rotation;
                    if (selected_input == 13) Main.settings.ollie_customization_rotation_both2right[selected_stance_customizer] = rotation;

                    GUILayout.Space(6);

                    if (selected_input == 0) Main.settings.ollie_customization_length[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 1) Main.settings.ollie_customization_length_backwards[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_backwards[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 2) Main.settings.ollie_customization_length_left_stick[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_left_stick[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 3) Main.settings.ollie_customization_length_right_stick[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_right_stick[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 4) Main.settings.ollie_customization_length_left_stick_backwards[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_left_stick_backwards[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 5) Main.settings.ollie_customization_length_right_stick_backwards[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_right_stick_backwards[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 6) Main.settings.ollie_customization_length_both_outside[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_both_outside[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 7) Main.settings.ollie_customization_length_both_inside[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_both_inside[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 8) Main.settings.ollie_customization_length_left2left[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_left2left[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 9) Main.settings.ollie_customization_length_left2right[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_left2right[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 10) Main.settings.ollie_customization_length_right2left[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_right2left[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 11) Main.settings.ollie_customization_length_right2right[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_right2right[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 12) Main.settings.ollie_customization_length_both2left[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_both2left[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 13) Main.settings.ollie_customization_length_both2right[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_both2right[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                }

                GUILayout.EndVertical();
            }
        }

        void CopingSection()
        {
            coping_fold.Fold();

            if (coping_fold.active)
            {
                GUILayout.BeginVertical(boxpadded);
                {
                    if (RGUI.Button(Main.settings.alternative_coping, "Enabled"))
                    {
                        Main.settings.alternative_coping = !Main.settings.alternative_coping;
                    }

                    if (Main.settings.alternative_coping)
                    {
                        Main.settings.coping_detection_distance = RGUI.SliderFloat(Main.settings.coping_detection_distance, 0, 1f, .5f, "Minimum distance to enter coping");
                        Main.settings.coping_max_velocity = RGUI.SliderFloat(Main.settings.coping_max_velocity, 0, 20f, 5f, "Velocity limit to enter coping");

                        GUILayout.Space(6);
                        Main.settings.coping_part_speed = RGUI.SliderFloat(Main.settings.coping_part_speed, 0, 1f, .1f, "Board part transition speed (0 is instant)");
                        Main.settings.coping_part_distance = RGUI.SliderFloat(Main.settings.coping_part_distance, 0, 5f, 5f, "Board part minimum distance (the less, the more precise you need to be)");
                        GUILayout.Label("'Board part' is the piece of the board (front truck, tail, ...) making contact while you move the sticks when grinding coping");
                    }
                }
                GUILayout.EndVertical();
            }
        }

        void BankLeanSection()
        {
            Fold(banklean_fold);
            if (!banklean_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                GUILayout.Label("<b><color=#f9ca24>These options affect normal and alternative banklean</color></b>");
                Main.settings.bankLeanSpeed = RGUI.SliderFloat(Main.settings.bankLeanSpeed, 0f, 20f, 10f, "Banklean body rotation speed");
                Main.settings.bankLeanMultiplier = RGUI.SliderFloat(Main.settings.bankLeanMultiplier, 0f, 2f, 1f, "Banklean speed multiplier");

                GUILayout.Space(12);

                if (RGUI.Button(Main.settings.alternativeBankLean, "Alternative bank lean (experimental)"))
                {
                    Main.settings.alternativeBankLean = !Main.settings.alternativeBankLean;
                    if (Main.settings.alternativeBankLean)
                    {
                        Main.controller.alternativeBankLean = true;
                        NotificationManager.Instance.ShowNotification($"Alternative bank lean { (Main.controller.alternativeBankLean ? "enabled" : "disabled") }", 1f, false, NotificationManager.NotificationType.Normal, TextAlignmentOptions.TopRight, 0.1f);
                    }
                }
                if (Main.settings.alternativeBankLean)
                {
                    GUILayout.Label($"<b><color=#f9ca24>(Hold L1 / LB to { (Main.controller.alternativeBankLean ? "disable" : "enable") } while riding)</color></b>");
                    GUILayout.Space(6);

                    Main.settings.alternativeBankLeanStrength = Mathf.Clamp01(RGUI.SliderFloat(Main.settings.alternativeBankLeanStrength, 0f, 1f, 1f, "Alternative bank lean strength"));
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj turn_fold = new FoldObj(true, "Turning");
        void TurnSection()
        {
            Fold(turn_fold);
            if (!turn_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                if (RGUI.Button(Main.settings.customTurn, "Custom turn"))
                {
                    Main.settings.customTurn = !Main.settings.customTurn;
                }
                if (Main.settings.customTurn)
                {
                    Main.settings.customSkateTurnMultiplier = RGUI.SliderFloat(Main.settings.customSkateTurnMultiplier, 0f, 10f, 1f, "Skate turn multiplier");
                    Main.settings.customSkateTurnSpeed = RGUI.SliderFloat(Main.settings.customSkateTurnSpeed, 0f, 32f, 16f, "Skate turn speed");
                    Main.settings.maxAngleClamp = RGUI.SliderFloat(Main.settings.maxAngleClamp, 0f, 6f, 3f, "Skate max turn angle");

                    GUILayout.Space(6);

                    Main.settings.customTurnMultiplier = RGUI.SliderFloat(Main.settings.customTurnMultiplier, 0f, 10f, 1f, "Body turn multiplier");
                    Main.settings.customTurnSpeed = RGUI.SliderFloat(Main.settings.customTurnSpeed, 0f, 240f, 120f, "Body turn speed");
                }
                GUILayout.EndVertical();
            }
        }

        void ManualSection()
        {
            manual_fold.Fold(green);

            if (manual_fold.active)
            {
                GUILayout.BeginVertical(boxpadded);
                {
                    if (RGUI.Button(Main.settings.nudge_manual, "Nudge manual on stick click"))
                    {
                        Main.settings.nudge_manual = !Main.settings.nudge_manual;
                    }
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj gameplay_fold = new FoldObj(true, "Gameplay");
        FoldObj multi_all_fold = new FoldObj(true, "Multiplayer");
        FoldObj exp_fold = new FoldObj(true, "Experimental");
        FoldObj misc_fold = new FoldObj(true, "Misc");
        FoldObj banklean_fold = new FoldObj(true, "Banklean");
        GUIStyle style = new GUIStyle();
        GUIStyle boxpadded = new GUIStyle("Box");
        public Vector2 scrollPosition = Vector2.zero;
        string kickplayer = "";
        private void MainMenu(int windowID)
        {
            GUI.backgroundColor = Color.red;
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            //GUI.BeginScrollView(new Rect(0, 0, 0, Screen.height), scrollPosition, MainMenuRect, false, true);
            MainSection();
            if (!Main.settings.enabled) return;

            AboutSection();

            AnimationAndPushingSection();

            BodySection();

            HeadSection();

            CameraSection();

            Fold(gameplay_fold);
            if (!gameplay_fold.reference)
            {
                GUILayout.BeginVertical(style);
                FeetSection();
                LeanWallrideSection();
                HippieSection();
                GrindsSection();
                CopingSection();
                //ManualSection();
                BankLeanSection();
                SkateSection();
                ReallyExperimental();
                GUILayout.EndVertical();
            }


            Fold(multi_all_fold, green);
            if (!multi_all_fold.reference)
            {
                GUILayout.BeginVertical(style);
                MultiSection();
                ChatSection();
                FilmerSection();
                GUILayout.EndVertical();
            }

            Fold(misc_fold, green);
            if (!misc_fold.reference)
            {
#if DEBUG
                if (RGUI.Button(Main.settings.debug, "Debug"))
                {
                    Main.settings.debug = !Main.settings.debug;
                    Main.controller.checkDebug();
                    Main.controller.getDeck();

                    for (int i = 0; i < Main.controller.skater_parts.Length; i++)
                    {
                        Utils.Log(Main.controller.skater_parts[i].gameObject.name + " " + Main.controller.skater_parts[i].localRotation.eulerAngles);
                    }
                }
#endif
                GUILayout.BeginVertical(boxpadded);
                {
                    GUILayout.Label("Use this button to reload custom gear textures while the game is open");
                    if (GUILayout.Button("Update gear texture files", GUILayout.Height(28)))
                    {
                        SaveManagerFocusPatch.HandleCustomGearChanges();
                    }
                }

                GUILayout.Space(12);
                GUILayout.Label("Custom resolutions");
                if (GUILayout.Button("720p vertical")) Main.controller.setResolution(720, 1280);
                if (GUILayout.Button("1080p vertical")) Main.controller.setResolution(1080, 1920);
                if (Main.controller.originalResolution != Vector2Int.zero)
                {
                    if (GUILayout.Button("Reset resolution")) Main.controller.resetResolution();
                }
                GUILayout.EndVertical();
            }

            map_fold.Fold(green);
            if (map_fold.active)
            {
                GUILayout.BeginVertical(boxpadded);
                {
                    Main.settings.map_scale.y = RGUI.SliderFloat(Main.settings.map_scale.y, -2f, 2f, 1f, "Map scale height");
                    Main.settings.map_scale.x = RGUI.SliderFloat(Main.settings.map_scale.x, -2f, 2f, 1f, "Map scale X");
                    Main.settings.map_scale.z = RGUI.SliderFloat(Main.settings.map_scale.z, -2f, 2f, 1f, "Map scale Z");


                    if (GUILayout.Button("Scale map", GUILayout.Height(34)))
                    {
                        Main.controller.ScaleMap();
                    }

                    GUILayout.Space(12);

                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    GUILayout.Label("<b>Force maximum detail in all objects (LOD0)</b>");
                    GUILayout.Label("<color=#f9ca24>After toggling this feature your game can freeze for some seconds</color>");
                    GUILayout.Label("<color=#f9ca24>Enabling this feature will <b>for sure</b> cap some frames</color>");
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical(GUILayout.Width(64f));
                    GUILayout.Space(8);
                    if (GUILayout.Button("Enable", RGUIStyle.button, GUILayout.Width(64f)))
                    {
                        Main.controller.ForceLODs();
                    }

                    if (GUILayout.Button("Disable", RGUIStyle.button, GUILayout.Width(64f)))
                    {
                        Main.controller.ResetLODs();
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

            Fold(exp_fold, red);
            if (!exp_fold.reference)
            {
                GUILayout.BeginVertical(boxpadded);
                {
                    if (RGUI.Button(Main.settings.alternative_powerslide, "Alternative powerslides"))
                    {
                        Main.settings.alternative_powerslide = !Main.settings.alternative_powerslide;
                    }

                    if (Main.settings.alternative_powerslide)
                    {
                        Main.settings.powerslide_animation_length = RGUI.SliderFloat(Main.settings.powerslide_animation_length, 0f, 64f, 24f, "Powerslide animation length");
                        Main.settings.powerslide_minimum_velocity = RGUI.SliderFloat(Main.settings.powerslide_minimum_velocity, 0f, 20f, 0f, "Powerslide min velocity");
                        Main.settings.powerslide_max_velocity = RGUI.SliderFloat(Main.settings.powerslide_max_velocity, 0f, 20f, 15f, "Powerslide max velocity");
                        Main.settings.powerslide_maxangle = RGUI.SliderFloat(Main.settings.powerslide_maxangle, 0f, 45f, 20f, "Powerslide max angle");
                    }
                }
                GUILayout.EndVertical();


                /*if (GUILayout.Button("Change Cloth Colliders to legs, torso, and hands", GUILayout.Height(34)))
                {
                    ClothColliders();
                }*/

                /*GUILayout.Space(12);

                if (RGUI.Button(Main.settings.partial_gear, "Multiplayer load partial gear"))
                {
                    Main.settings.partial_gear = !Main.settings.partial_gear;
                }*/

                /*if (RGUI.Button(Main.settings.experimental_dynamic_catch, "Dynamic catch really experimental proof of concept will be weird"))
                {
                    //Main.settings.experimental_dynamic_catch = !Main.settings.experimental_dynamic_catch;
                }*/
            }

            //GUI.EndScrollView();
        }
    }
}
