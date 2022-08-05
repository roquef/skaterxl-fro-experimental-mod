﻿using System;
using UnityEngine;
using UnityModManagerNet;
using SkaterXL.Core;
using HarmonyLib;
using SkaterXL.Multiplayer;
using System.Collections.Generic;
using RootMotion.Dynamics;
using Photon.Realtime;
using UnityEngine.EventSystems;
using ModIO.UI;
using UnityEngine.Rendering.HighDefinition;
using GameManagement;
using UnityEditor;
using UnityEngine.UI;
using ReplayEditor;
using System.Collections;

namespace fro_mod
{
    public class Controller : MonoBehaviour
    {
        private RaycastHit _hit;
        RaycastHit right_hit;
        RaycastHit left_hit;
        GameObject debug_cube, debug_cube_2, debug_cube_3, debug_cube_4;
        Transform left_foot;
        Transform right_foot;
        Transform head, neck, head_replay, spine, left_hand_replay, right_hand_replay;
        Transform center_collider;
        Transform tail_collider;
        Transform nose_collider;
        string last_state;
        int count = 0;
        bool was_leaning = false;
        Cinemachine.CinemachineCollider cinemachine_collider;
        bool bailed_puppet = false;
        System.Random rd = new System.Random();
        Transform replay_skater;

        float random_offset = 0f;

        RealisticEyeMovements.EyeAndHeadAnimator EA;

        public void Start()
        {
            replay_skater = getReplayEditor();

            DisableMultiPopup(Main.settings.disable_popup);
            DisableCameraCollider();
            MultiplayerManager.ROOMSIZE = 20;

            PlayerController.Instance.boardController.boardRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerController.Instance.boardController.boardRigidbody.solverIterations = 20;
            PlayerController.Instance.boardController.backTruckRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerController.Instance.boardController.backTruckRigidbody.solverIterations = 20;
            PlayerController.Instance.boardController.frontTruckRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerController.Instance.boardController.frontTruckRigidbody.solverIterations = 20;
            PlayerController.Instance.skaterController.skaterRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerController.Instance.skaterController.skaterRigidbody.solverIterations = 20;
            PlayerController.Instance.skaterController.leftFootCollider.attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerController.Instance.skaterController.leftFootCollider.attachedRigidbody.solverIterations = 20;
            PlayerController.Instance.skaterController.rightFootCollider.attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerController.Instance.skaterController.rightFootCollider.attachedRigidbody.solverIterations = 20;

            EA = PlayerController.Instance.skaterController.GetComponent<RealisticEyeMovements.EyeAndHeadAnimator>();

            checkDebug();
            getReplayUI();

            PlayerController.Instance.respawn.DoRespawn();
        }

        void AddColliders()
        {

        }

        public string[] getListOfPlayers()
        {
            string[] names = new string[MultiplayerManager.Instance.networkPlayers.Count];
            int i = 0;
            foreach (KeyValuePair<int, NetworkPlayerController> entry in MultiplayerManager.Instance.networkPlayers)
            {
                if (entry.Value)
                {
                    names[i] = entry.Key + ":" + entry.Value.NickName;
                    i++;
                }
            }

            return names;
        }

        Transform getOnlinePlayer()
        {
            if (Main.settings.selected_player != "")
            {
                int index = int.Parse(Main.settings.selected_player.Split(':')[0]);
                var player = MultiplayerManager.Instance.GetNextPlayer(index - 1);

                if (player != null && !player.IsLocal)
                {
                    Transform body = player.GetBody().transform;
                    body.Translate(0, Main.settings.follow_target_offset, 0);
                    return body;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private void LogState()
        {
            if (PlayerController.Instance.currentStateEnum.ToString() != last_state)
            {
                last_state = PlayerController.Instance.currentStateEnum.ToString();
            }
        }

        public bool IsGrabbing() { return (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grabs) || EventManager.Instance.IsGrabbing; }

        LayerMask layerMask = ~(1 << LayerMask.NameToLayer("Skateboard"));
        float head_frame = 0, delay_head = 0;
        Quaternion last_head = Quaternion.identity;
        public void Update()
        {
            if (center_collider == null) getDeck();
            if (left_foot == null) getFeet();

            MultiplayerFilmer();

            if (!PlayerController.Instance.respawn.puppetMaster.isBlending)
            {
                if (Main.settings.camera_feet) CameraFeet();
                if (Main.settings.feet_rotation || Main.settings.feet_offset) DynamicFeet();
            }

            LookForward();
            LetsGoAnimHead();

            if (keyframe_state == true)
            {
                FilmerKeyframes();
            }
        }

        private void setMotionType(Muscle muscle, ConfigurableJointMotion joint)
        {
            muscle.joint.xMotion = joint;
            muscle.joint.yMotion = joint;
            muscle.joint.zMotion = joint;
            SoftJointLimit limit = new UnityEngine.SoftJointLimit();
            limit.bounciness = 1;
            muscle.joint.linearLimit = limit;
        }

        int delay = 0;
        bool freeze_pos = false;
        StickInput left_stick_freezed = new StickInput();
        StickInput right_stick_freezed = new StickInput();
        public void FixedUpdate()
        {
            if (!Main.settings.enabled) return;

            PlayerController.Instance.animationController.ScaleAnimSpeed(1f);

            if (Main.settings.debug)
            {
                center_collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white);
                tail_collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white);
                nose_collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white);
            }
            else
            {
                if (center_collider.GetComponent<MeshRenderer>()) Destroy(center_collider.GetComponent<MeshRenderer>());
                if (tail_collider.GetComponent<MeshRenderer>()) Destroy(tail_collider.GetComponent<MeshRenderer>());
                if (nose_collider.GetComponent<MeshRenderer>()) Destroy(nose_collider.GetComponent<MeshRenderer>());
            }

            if (MultiplayerManager.ROOMSIZE != (byte)Main.settings.multiplayer_lobby_size)
            {
                MultiplayerManager.ROOMSIZE = (byte)Main.settings.multiplayer_lobby_size;
            }

            if (Main.settings.wave_on != "Disabled")
            {
                WaveAnim();
            }

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Powerslide && Main.settings.powerslide_force)
            {
                int multiplier = PlayerController.Instance.GetBoardBackwards() ? -1 : 1;
                multiplier *= PlayerController.Instance.IsSwitch ? -1 : 1;

                PlayerController.Instance.boardController.boardRigidbody.AddRelativeForce(new Vector3(0, 0, .16f * multiplier), ForceMode.Impulse);
            }

            if (Main.settings.celebrate_on != "Disabled" && PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.Bailed)
            {
                CheckLetsgoAnim();
            }

            if (Main.settings.bails && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Bailed && PlayerController.Instance.respawn.bail.bailed)
            {
                if (!bailed_puppet) SetBailedPuppet();
                letsgo_anim_time = Time.unscaledTime - 10f;
            }
            else if (bailed_puppet)
            {
                RestorePuppet();
            }

            if (PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.Bailed)
            {
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass = Main.settings.left_hand_weight;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass = Main.settings.right_hand_weight;
            }

            if (Main.settings.displacement_curve) PlayerController.Instance.ScaleDisplacementCurve(Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.skaterTransform.position - PlayerController.Instance.boardController.boardTransform.position, PlayerController.Instance.skaterController.skaterTransform.forward).magnitude * 1.25f);

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pushing)
            {
                if (Main.settings.sonic_mode)
                {
                    PlayerController.Instance.animationController.ScaleAnimSpeed(.2f + PlayerController.Instance.skaterController.skaterRigidbody.velocity.magnitude);
                }
                if (Main.settings.push_by_velocity)
                {
                    PlayerController.Instance.animationController.ScaleAnimSpeed(1 - PlayerController.Instance.skaterController.skaterRigidbody.velocity.magnitude / 90);
                }
            }

            /*if (last_state != "Grinding" && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grinding)
            {
                random_offset = (-.5f + (float)rd.NextDouble()) / 2f;
            }
            else
            {
                random_offset = 0;
            }*/

            if (!PlayerController.Instance.respawn.puppetMaster.isBlending)
            {
                if (Main.settings.camera_feet) CameraFeet();
                if (Main.settings.feet_rotation || Main.settings.feet_offset) DynamicFeet();
            }

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grinding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.ExitCoping)
            {
                GrindVerticalFlip();
            }

            if (Main.settings.lean)
            {
                Lean();
            }

            if (Main.settings.BetterDecay) PlayerController.Instance.boardController.ApplyFrictionTowardsVelocity(1 - (Main.settings.decay / 1000));

            LetsGoAnim();

            EA.LookAtAreaAround(PlayerController.Instance.boardController.transform.position);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                freeze_pos = !freeze_pos;
            }
        }

        public void PlayLetsGoAnim()
        {
            letsgo_anim_time = 0;
        }

        float letsgo_anim_time = 1;
        float letsgoanim_frame = 0;
        float letsgoanim_length = 30;
        void LetsGoAnim()
        {
            if (letsgo_anim_time == 0) letsgo_anim_time = Time.unscaledTime;

            if (Time.unscaledTime - letsgo_anim_time <= 3f)
            {
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass = 1f;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass = 1f;

                float sin = (float)Math.Sin(Time.unscaledTime * 12f) / 7f;
                float multiplier = 1;
                multiplier = SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular ? multiplier : -multiplier;

                GameObject center_left = new GameObject();
                center_left.transform.position = PlayerController.Instance.skaterController.skaterTransform.position;
                center_left.transform.rotation = PlayerController.Instance.skaterController.skaterTransform.rotation;
                center_left.transform.Translate(new Vector3(.2f * multiplier, .75f + sin, .25f * multiplier), Space.Self);

                GameObject center_right = new GameObject();
                center_right.transform.position = PlayerController.Instance.skaterController.skaterTransform.position;
                center_right.transform.rotation = PlayerController.Instance.skaterController.skaterTransform.rotation;
                center_right.transform.Translate(new Vector3(.2f * multiplier, .75f + sin, -.25f * multiplier), Space.Self);

                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.position = Vector3.Lerp(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.position, center_left.transform.position, map01(letsgoanim_frame, 0, letsgoanim_length));
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.position = Vector3.Lerp(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.position, center_right.transform.position, map01(letsgoanim_frame, 0, letsgoanim_length));
                letsgoanim_frame++;

                Destroy(center_left);
                Destroy(center_right);
            }
            else
            {
                letsgoanim_frame = 0;
            }
        }

        void LookForward()
        {
            if (!Main.settings.look_forward) return;

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
            {
                if (PlayerController.Instance.IsSwitch)
                {
                    float windup_side = PlayerController.Instance.animationController.skaterAnim.GetFloat("WindUp");

                    Vector3 target = nose_collider.transform.position;
                    if (PlayerController.Instance.GetBoardBackwards()) target = tail_collider.transform.position;

                    if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy)
                    {
                        target = tail_collider.transform.position;
                        if (PlayerController.Instance.GetBoardBackwards()) target = nose_collider.transform.position;
                    }

                    GameObject head_copy = new GameObject();
                    head_copy.transform.rotation = head.rotation;
                    head_copy.transform.position = head.position;
                    head_copy.transform.LookAt(target);

                    if (PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                    {
                        Vector3 t = new Vector3(-205, -120, spine.transform.rotation.z);
                        if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy) t = new Vector3(-75, -120, spine.transform.rotation.z);

                        if(windup_side < 0)
                        {
                            if(SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular)
                            {
                                t.z -= 20f * windup_side;
                                t.y -= 60f * windup_side;
                                t.x -= 10f * windup_side;
                            }
                            else
                            {
                                t.z += 20f * windup_side;
                                t.y -= 60f * windup_side;
                                t.x += 100f * windup_side; 
                            }
                        }

                        head_copy.transform.Rotate(t);
                    }
                    else
                    {
                        Vector3 t = new Vector3(-205, -120, spine.transform.rotation.z - 25);
                        if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy) t = new Vector3(-75, -120, spine.transform.rotation.z - 15);
                        head_copy.transform.Rotate(t);
                    }
                    head.rotation = Quaternion.Lerp(head.rotation, head_copy.transform.rotation, Mathf.SmoothStep(0, 1, map01(head_frame, 0, 20)));

                    Destroy(head_copy);
                    if (delay_head >= Main.settings.look_forward_delay)
                    {
                        if (last_head == Quaternion.identity) last_head = head.rotation;
                        head_frame++;
                    }
                    delay_head++;
                }
            }
            else
            {
                /*if (head_frame > 0)
                {
                    head_frame = head_frame > 20 ? 20 : head_frame;
                    head.rotation = Quaternion.Lerp(head.rotation, last_head, Mathf.SmoothStep(0, 1, map01(20 - head_frame, 0, 20)));
                    head_frame--;
                }
                else
                {
                    last_head = Quaternion.identity;
                }*/
                head_frame = 0;
                delay_head = 0;
            }
        }
        void LetsGoAnimHead()
        {
            if (letsgo_anim_time == 0) letsgo_anim_time = Time.unscaledTime;

            if (Time.unscaledTime - letsgo_anim_time <= 3f)
            {
                float multiplier = 0;
                multiplier = SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular ? multiplier : 1;
                float sin = (float)Math.Sin(Time.unscaledTime * 12f) / 7f;
                head.transform.rotation = Quaternion.Lerp(head.transform.rotation, spine.transform.rotation, map01(letsgoanim_frame, 0, letsgoanim_length));
                Vector3 target = Vector3.Lerp(Vector3.zero, new Vector3(sin * 10, sin * 15, (sin * 33.3f)), map01(letsgoanim_frame, 0, letsgoanim_length) / 2);
                head.transform.Rotate(target);
            }
        }

        public static float map01(float value, float min, float max)
        {
            return (value - min) * 1f / (max - min);
        }

        public static bool IsInside(Collider c, Vector3 point)
        {
            Vector3 closest = c.ClosestPoint(point);
            return closest == point;
        }

        void Lean()
        {
            int multiplier = 0;
            if (InAir())
            {
                float sensibility = Main.settings.input_threshold / 200f;
                if (LeaningInputRight(sensibility))
                {
                    multiplier = 1;
                    count++;
                }

                if (LeaningInputLeft(sensibility))
                {
                    multiplier = -1;
                    count++;
                }

                if (multiplier == 0)
                {
                    was_leaning = false;
                    count = 0;
                    return;
                }

                if (count <= Main.settings.wait_threshold)
                {
                    return;
                }
            }

            if (InAir())
            {
                if (IsGrabbing()) return;

                if (Main.settings.swap_lean) multiplier = -multiplier;

                PlayerController.Instance.DisableArmPhysics();

                if (PlayerController.Instance.movementMaster != PlayerController.MovementMaster.Skater)
                {
                    PlayerController.Instance.SetSkaterToMaster();
                }

                MonoBehaviourSingleton<PlayerController>.Instance.boardController.SetBoardControllerUpVector(MonoBehaviourSingleton<PlayerController>.Instance.skaterController.skaterTransform.up);
                PlayerController.Instance.skaterController.skaterTransform.Rotate(0, 0, multiplier * (Main.settings.speed / 1.25f) * Time.deltaTime, Space.Self);

                if (PlayerController.Instance.GetBoardBackwards()) multiplier *= -1;
                PlayerController.Instance.boardController.gameObject.transform.Rotate(0, 0, multiplier * (Main.settings.speed * 2) * Time.deltaTime, Space.Self);
                PlayerController.Instance.boardController.gameObject.transform.Translate(multiplier * .005f, multiplier * .015f, .0f);
                PlayerController.Instance.boardController.boardRigidbody.AddRelativeForce(0, 0f, multiplier * .5f, ForceMode.Impulse);

                PlayerController.Instance.boardController.UpdateReferenceBoardTargetRotation();
                PlayerController.Instance.boardController.UpdateBoardPosition();
                //PlayerController.Instance.comController.UpdateCOM();
                PlayerController.Instance.LerpKneeIkWeight();
                PlayerController.Instance.SnapRotation();
                PlayerController.Instance.SetRotationTarget(true);
                PlayerController.Instance.boardController.currentRotationTarget = PlayerController.Instance.skaterController.skaterTransform.rotation;
                was_leaning = true;
            }
            else
            {
                if (was_leaning)
                {
                    if (IsGrounded())
                    {
                        raycast();
                    }

                    checkAngles();
                }
            }
            /*if (Grinding())
            {
                PlayerController.Instance.boardController.gameObject.transform.Rotate(0, 0, -multiplier * Main.settings.grind_speed * Time.deltaTime, Space.Self);
                MonoBehaviourSingleton<PlayerController>.Instance.boardController.SetBoardControllerUpVector(MonoBehaviourSingleton<PlayerController>.Instance.skaterController.skaterTransform.up);
                PlayerController.Instance.skaterController.skaterTransform.Rotate(0, 0, -multiplier * Main.settings.grind_speed * Time.deltaTime, Space.Self);
                PlayerController.Instance.boardController.UpdateReferenceBoardTargetRotation();
                PlayerController.Instance.boardController.UpdateBoardPosition();
                //PlayerController.Instance.comController.UpdateCOM();
                PlayerController.Instance.boardController.currentRotationTarget = PlayerController.Instance.skaterController.skaterTransform.rotation;
                Traverse.Create(PlayerController.Instance.boardController).Field("_playerGrindRotation").SetValue(PlayerController.Instance.skaterController.skaterTransform.rotation);
                Traverse.Create(PlayerController.Instance.boardController).Field("_boardGrindRotation").SetValue(PlayerController.Instance.boardController.gameObject.transform.rotation);
            }*/
        }

        GameObject LeftLight;
        Light leftLightComp;
        HDAdditionalLightData leftLightAdditionalData;

        GameObject RightLight;
        Light rightLightComp;
        HDAdditionalLightData rightLightAdditionalData;

        Vector3 last_multi_pos = Vector3.zero;

        void MultiplayerFilmer()
        {
            Muscle left_hand = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6];
            Muscle right_hand = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9];

            if (IsPumping())
            {
                if (Main.settings.follow_mode_left || Main.settings.follow_mode_right || Main.settings.follow_mode_head)
                {
                    Transform player = getOnlinePlayer();
                    if (player != null)
                    {
                        if (Main.settings.follow_mode_left)
                        {
                            Vector3 rot = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[4].transform.rotation.eulerAngles;
                            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[4].transform.rotation = Quaternion.Euler(-Main.settings.filmer_arm_angle, rot.y, rot.z);
                            left_hand.transform.LookAt(player);
                            left_hand.transform.Rotate(0, -90, 90);
                        }
                        if (Main.settings.follow_mode_right)
                        {
                            Vector3 rot = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[7].transform.rotation.eulerAngles;
                            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[7].transform.rotation = Quaternion.Euler(Main.settings.filmer_arm_angle, rot.y, rot.z);
                            right_hand.transform.LookAt(player);
                            right_hand.transform.Rotate(0, -90, 90);
                        }

                        if (Main.settings.follow_mode_head)
                        {
                            head.transform.LookAt(player);
                            head.transform.Rotate(0, -90, -90);
                        }
                    }
                }
            }

            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
            {
                if (Main.settings.filmer_light)
                {
                    if (Main.settings.follow_mode_left)
                    {
                        if (LeftLight == null) CreateLeftLight();

                        leftLightAdditionalData.intensity = Main.settings.filmer_light_intensity;
                        leftLightComp.intensity = Main.settings.filmer_light_intensity;
                        leftLightComp.spotAngle = Main.settings.filmer_light_spotangle;
                        leftLightComp.range = Main.settings.filmer_light_range;

                        LeftLight.transform.position = left_hand.transform.position;
                        LeftLight.transform.Translate(new Vector3(-.075f, 0f, .15f), Space.Self);
                        LeftLight.transform.rotation = left_hand.transform.rotation;
                        LeftLight.transform.transform.Rotate(90, 0, 0);
                    }
                    else
                    {
                        if (leftLightComp != null) leftLightComp.intensity = 0;
                    }

                    if (Main.settings.follow_mode_right)
                    {
                        if (RightLight == null) CreateRightLight();

                        rightLightAdditionalData.intensity = Main.settings.filmer_light_intensity;
                        rightLightComp.intensity = Main.settings.filmer_light_intensity;
                        rightLightComp.spotAngle = Main.settings.filmer_light_spotangle;
                        rightLightComp.range = Main.settings.filmer_light_range;

                        RightLight.transform.position = right_hand.transform.position;
                        RightLight.transform.Translate(new Vector3(-.075f, 0f, .15f), Space.Self);
                        RightLight.transform.rotation = right_hand.transform.rotation;
                        RightLight.transform.transform.Rotate(90, 0, 0);
                    }
                    else
                    {
                        if (rightLightComp != null) rightLightComp.intensity = 0;
                    }
                }
            }
        }

        void LerpLookAt(Transform obj, Transform target, Vector3 offset)
        {
            Vector3 relativePos = target.transform.position - obj.transform.position;
            Quaternion toRotation = Quaternion.LookRotation(relativePos);
            obj.transform.rotation = Quaternion.Lerp(obj.transform.rotation, toRotation, 1);
            obj.transform.Rotate(offset.x, offset.y, offset.z);
        }

        void CreateLeftLight()
        {
            LeftLight = new GameObject("LeftLight");
            leftLightComp = LeftLight.AddComponent<Light>();
            leftLightAdditionalData = LeftLight.AddComponent<HDAdditionalLightData>();
            leftLightAdditionalData.lightUnit = LightUnit.Ev100;
            leftLightComp.type = LightType.Spot;
            leftLightAdditionalData.intensity = Main.settings.filmer_light_intensity;
            leftLightComp.intensity = Main.settings.filmer_light_intensity;
            leftLightComp.spotAngle = Main.settings.filmer_light_spotangle;
            leftLightComp.range = Main.settings.filmer_light_range;
            LeftLight.transform.localScale = new Vector3(.1f, 0.1f, 0.1f);
            LeftLight.AddComponent<BoxCollider>().enabled = false;
            LeftLight.AddComponent<Rigidbody>();
            LeftLight.AddComponent<ObjectTracker>();
        }

        void CreateRightLight()
        {
            RightLight = new GameObject("RightLight");
            rightLightComp = RightLight.AddComponent<Light>();
            rightLightAdditionalData = RightLight.AddComponent<HDAdditionalLightData>();
            rightLightAdditionalData.lightUnit = LightUnit.Ev100;
            rightLightComp.type = LightType.Spot;
            rightLightAdditionalData.intensity = Main.settings.filmer_light_intensity;
            rightLightComp.intensity = Main.settings.filmer_light_intensity;
            rightLightComp.spotAngle = Main.settings.filmer_light_spotangle;
            rightLightComp.range = Main.settings.filmer_light_range;
            RightLight.transform.localScale = new Vector3(.1f, 0.1f, 0.1f);
            RightLight.AddComponent<BoxCollider>().enabled = false;
            RightLight.AddComponent<Rigidbody>();
            RightLight.AddComponent<ObjectTracker>();
        }

        public void GrindVerticalFlip()
        {
            int multiplier = -1;

            if (PlayerController.Instance.boardController.IsBoardBackwards)
            {
                PlayerController.Instance.boardController.firstVel = -Main.settings.GrindFlipVerticality * multiplier;
                return;
            }
            PlayerController.Instance.boardController.firstVel = Main.settings.GrindFlipVerticality;
        }

        public bool LeaningInputRight(float sensibility)
        {
            if (PlayerController.Instance.inputController.LeftStick.rawInput.pos.x >= 0.5f && PlayerController.Instance.inputController.RightStick.rawInput.pos.x >= 0.5f)
            {
                bool left_centered = PlayerController.Instance.inputController.LeftStick.rawInput.pos.y <= sensibility && PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= -sensibility;
                bool right_centered = PlayerController.Instance.inputController.RightStick.rawInput.pos.y <= sensibility && PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= -sensibility;
                if (left_centered && right_centered) return true;
            }
            return false;
        }

        public bool LeaningInputLeft(float sensibility)
        {
            if (PlayerController.Instance.inputController.LeftStick.rawInput.pos.x <= -0.5f && PlayerController.Instance.inputController.RightStick.rawInput.pos.x <= -0.5f)
            {
                bool left_centered = PlayerController.Instance.inputController.LeftStick.rawInput.pos.y <= sensibility && PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= -sensibility;
                bool right_centered = PlayerController.Instance.inputController.RightStick.rawInput.pos.y <= sensibility && PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= -sensibility;
                if (left_centered && right_centered) return true;
            }
            return false;
        }

        public void Wobble()
        {
            Vector3 euler = PlayerController.Instance.boardController.boardRigidbody.transform.localRotation.eulerAngles;
            PlayerController.Instance.boardController.boardRigidbody.transform.localRotation = Quaternion.Euler(euler.x, euler.y, euler.z + WobbleValue());

            //PlayerController.Instance.comController.UpdateCOM();
        }

        public float WobbleValue(float multiplier = 1f)
        {
            float mag_multi = PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pushing || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Powerslide ? .15f / 2 : .15f;
            float vel = PlayerController.Instance.boardController.boardRigidbody.velocity.magnitude - Main.settings.wobble_offset;
            vel = vel < 0 ? 0 : vel * multiplier;

            return Mathf.Sin(Time.fixedUnscaledTime * 30 + (vel * mag_multi) + ((float)rd.NextDouble() * 2)) * (mag_multi * vel);
        }

        public void SetBailedPuppet()
        {
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[3], ConfigurableJointMotion.Free);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[4], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[5], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[7], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[8], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[10], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[11], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[12], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[13], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[14], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[15], ConfigurableJointMotion.Limited);

            for (int i = 0; i < PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles.Length; i++)
            {
                if (Main.settings.debug) UnityModManager.Logger.Log(i + " " + PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].name + " " + PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].joint.xMotion);
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].rigidbody.solverIterations = 5;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].rigidbody.mass = 5f;
            }

            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[1].rigidbody.mass = 20f;
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[3].rigidbody.mass = 5f;
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[12].rigidbody.mass = 6.5f;
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[15].rigidbody.mass = 6.5f;

            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass = 1f;
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass = 1f;

            PlayerController.Instance.animationController.CrossFadeAnimation("Falling", .2f);
            PlayerController.Instance.boardController.ApplyFrictionTowardsVelocity(.1f);
            bailed_puppet = true;
        }

        public void RestorePuppet()
        {
            for (int i = 0; i < PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles.Length; i++)
            {
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].rigidbody.solverIterations = 1;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].rigidbody.mass = 1f;
            }

            bailed_puppet = false;
        }

        public void DisableCameraCollider()
        {
            if (!cinemachine_collider)
            {
                cinemachine_collider = PlayerController.Instance.cameraController.gameObject.GetComponentInChildren<Cinemachine.CinemachineCollider>();
            }
            if (cinemachine_collider != null)
            {
                cinemachine_collider.enabled = Main.settings.camera_avoidance;
            }
        }

        public bool keyframe_state = false;
        int keyframe_count = 0;
        float saved_time = 0;
        public void FilmerKeyframes()
        {
            if (saved_time == 0) saved_time = ReplayEditorController.Instance.playbackController.CurrentTime;
            float time = ReplayEditorController.Instance.playbackController.ClipEndTime - saved_time;

            if (Main.settings.keyframe_start_of_clip)
            {
                ReplayEditorController.Instance.playbackController.CurrentTime = ReplayEditorController.Instance.playbackController.ClipStartTime;
                time = ReplayEditorController.Instance.playbackController.ClipEndTime - ReplayEditorController.Instance.playbackController.ClipStartTime;
            }

            float pace = time / Main.settings.keyframe_sample;

            if (keyframe_count < Main.settings.keyframe_sample)
            {
                if(Main.settings.keyframe_start_of_clip)
                {
                    ReplayEditorController.Instance.playbackController.UpdateTimeAndScale(ReplayEditorController.Instance.playbackController.ClipStartTime + (keyframe_count * pace), 1);
                }
                else
                {
                    ReplayEditorController.Instance.playbackController.UpdateTimeAndScale(saved_time + (keyframe_count * pace), 1);
                }

                Transform target = head_replay;
                if (Main.settings.keyframe_target == "Left Hand") target = left_hand_replay;
                if (Main.settings.keyframe_target == "Right Hand") target = right_hand_replay;

                if (Main.settings.keyframe_start_of_clip)
                {
                    AddKeyFrame(ReplayEditorController.Instance.playbackController.ClipStartTime + (keyframe_count * pace), target);
                }
                else
                {
                    AddKeyFrame(saved_time + (keyframe_count * pace), target);
                }

                keyframes.UpdateKeyframes(ReplayEditorController.Instance.cameraController.keyFrames);
                keyframe_count++;
            }
            else
            {
                keyframe_count = 0;
                saved_time = 0;
                keyframe_state = false;
            }
        }

        Transform replay_transform;
        ReplayEditor.KeyframeUIController keyframes;
        void getReplayUI()
        {
            Transform main = PlayerController.Instance.skaterController.transform.parent.transform.parent;
            replay_transform = main.Find("ReplayEditor");
            keyframes = replay_transform.GetComponent<ReplayEditor.ReplayEditorController>().cameraController.keyframeUI;
        }

        void AddKeyFrame(float time, Transform target)
        {
            time = time + Main.settings.time_offset;
            int index = FindKeyFrameInsertIndex(time);
            KeyFrame keyFrame;
            GameObject copy = new GameObject();
            copy.transform.position = target.transform.position;
            copy.transform.rotation = target.transform.rotation;
            if (Main.settings.keyframe_target != "Head")
            {
                copy.transform.Rotate(90, 0, -90);
                copy.transform.Translate(new Vector3(0, -.175f, .25f), Space.Self);
            }
            else
            {
                copy.transform.Rotate(-90, 0, 90);
                copy.transform.Translate(new Vector3(0, 0.125f, 0.125f), Space.Self);
            }
            keyFrame = new FreeCameraKeyFrame(copy.transform, Main.settings.keyframe_fov, time);
            keyFrame.AddKeyframes(ReplayEditorController.Instance.cameraController.cameraCurve);
            ReplayEditorController.Instance.cameraController.keyFrames.Insert(index, keyFrame);
            Destroy(copy);
        }

        int FindKeyFrameInsertIndex(float time)
        {
            if (ReplayEditorController.Instance.cameraController.keyFrames.Count == 0)
            {
                return 0;
            }
            if (time < ReplayEditorController.Instance.cameraController.keyFrames[0].time)
            {
                return 0;
            }
            if (ReplayEditorController.Instance.cameraController.keyFrames.Count == 1)
            {
                return 1;
            }
            for (int i = 0; i < ReplayEditorController.Instance.cameraController.keyFrames.Count - 1; i++)
            {
                if (time > ReplayEditorController.Instance.cameraController.keyFrames[i].time && time < ReplayEditorController.Instance.cameraController.keyFrames[i + 1].time)
                {
                    return i + 1;
                }
            }
            return ReplayEditorController.Instance.cameraController.keyFrames.Count;
        }

        public void LateUpdate()
        {
            if (!Main.settings.enabled) return;

            if (Main.settings.hippie && (InAir() || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop))
            {
                HandleHippieOllie();
            }

            if (MultiplayerManager.Instance.clientState != ClientState.Disconnected && Main.settings.reset_inactive)
            {
                Traverse.Create(MultiplayerManager.Instance).Field("inactiveTime").SetValue(0f);
            }

            if (Main.settings.disable_popup)
            {
                MessageSystem.instance.enabled = false;
            }
            else if (!MessageSystem.instance.enabled)
            {
                MessageSystem.instance.enabled = true;
            }

            if (Main.settings.wobble && IsGrounded())
            {
                Wobble();
            }

            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
            {
                UpdateColliders();
                DoScalingPlaystate();
            }
            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
            {
                if (!replay_skater) replay_skater = getReplayEditor();
                DoScalingReplaystate();
            }

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
            {
                Traverse.Create(PlayerController.Instance.headIk).Field("currentDirTarget").SetValue(head.rotation);
                Traverse.Create(PlayerController.Instance.headIk).Field("currentTargetRot").SetValue(head.rotation);
            }

            LogState();
        }
        void DoScalingPlaystate()
        {
            head.localScale = new Vector3(Main.settings.custom_scale_head, Main.settings.custom_scale_head, Main.settings.custom_scale_head);
            left_hand.localScale = new Vector3(Main.settings.custom_scale_hand_l, Main.settings.custom_scale_hand_l, Main.settings.custom_scale_hand_l);
            right_hand.localScale = new Vector3(Main.settings.custom_scale_hand_r, Main.settings.custom_scale_hand_r, Main.settings.custom_scale_hand_r);
            left_foot.localScale = new Vector3(Main.settings.custom_scale_foot_l, Main.settings.custom_scale_foot_l, Main.settings.custom_scale_foot_l);
            right_foot.localScale = new Vector3(Main.settings.custom_scale_foot_r, Main.settings.custom_scale_foot_r, Main.settings.custom_scale_foot_r);

            neck.localScale = new Vector3(Main.settings.custom_scale_neck, Main.settings.custom_scale_neck, Main.settings.custom_scale_neck);
            pelvis.localScale = new Vector3(Main.settings.custom_scale_pelvis, Main.settings.custom_scale_pelvis, Main.settings.custom_scale_pelvis);
            spine1.localScale = new Vector3(Main.settings.custom_scale_spine, Main.settings.custom_scale_spine, Main.settings.custom_scale_spine);
            spine2.localScale = new Vector3(Main.settings.custom_scale_spine2, Main.settings.custom_scale_spine2, Main.settings.custom_scale_spine2);
            left_arm.localScale = new Vector3(Main.settings.custom_scale_arm_l, Main.settings.custom_scale_arm_l, Main.settings.custom_scale_arm_l);
            left_forearm.localScale = new Vector3(Main.settings.custom_scale_forearm_l, Main.settings.custom_scale_forearm_l, Main.settings.custom_scale_forearm_l);
            right_arm.localScale = new Vector3(Main.settings.custom_scale_arm_r, Main.settings.custom_scale_arm_r, Main.settings.custom_scale_arm_r);
            right_forearm.localScale = new Vector3(Main.settings.custom_scale_forearm_r, Main.settings.custom_scale_forearm_r, Main.settings.custom_scale_forearm_r);
            left_upleg.localScale = new Vector3(Main.settings.custom_scale_upleg_l, Main.settings.custom_scale_upleg_l, Main.settings.custom_scale_upleg_l);
            left_leg.localScale = new Vector3(Main.settings.custom_scale_leg_l, Main.settings.custom_scale_leg_l, Main.settings.custom_scale_leg_l);
            right_upleg.localScale = new Vector3(Main.settings.custom_scale_upleg_r, Main.settings.custom_scale_upleg_r, Main.settings.custom_scale_upleg_r);
            right_leg.localScale = new Vector3(Main.settings.custom_scale_leg_r, Main.settings.custom_scale_leg_r, Main.settings.custom_scale_leg_r);

            PlayerController.Instance.skaterController.gameObject.transform.localScale = Main.settings.custom_scale;
            PlayerController.Instance.playerCollider.transform.localScale = Main.settings.custom_scale;
            //PlayerController.Instance.skaterController.gameObject.transform.parent.Find("CenterOfMassPlayer").localScale = Main.settings.custom_scale;
        }

        void DoScalingReplaystate()
        {
            replay_skater.localScale = Main.settings.custom_scale;
            head_replay.localScale = new Vector3(Main.settings.custom_scale_head, Main.settings.custom_scale_head, Main.settings.custom_scale_head);
            left_hand_replay.localScale = new Vector3(Main.settings.custom_scale_hand_l, Main.settings.custom_scale_hand_l, Main.settings.custom_scale_hand_l);
            right_hand_replay.localScale = new Vector3(Main.settings.custom_scale_hand_r, Main.settings.custom_scale_hand_r, Main.settings.custom_scale_hand_r);
            left_foot_replay.localScale = new Vector3(Main.settings.custom_scale_foot_l, Main.settings.custom_scale_foot_l, Main.settings.custom_scale_foot_l);
            right_foot_replay.localScale = new Vector3(Main.settings.custom_scale_foot_r, Main.settings.custom_scale_foot_r, Main.settings.custom_scale_foot_r);

            neck_replay.localScale = new Vector3(Main.settings.custom_scale_neck, Main.settings.custom_scale_neck, Main.settings.custom_scale_neck);
            pelvis_replay.localScale = new Vector3(Main.settings.custom_scale_pelvis, Main.settings.custom_scale_pelvis, Main.settings.custom_scale_pelvis);
            spine1_replay.localScale = new Vector3(Main.settings.custom_scale_spine, Main.settings.custom_scale_spine, Main.settings.custom_scale_spine);
            spine2_replay.localScale = new Vector3(Main.settings.custom_scale_spine2, Main.settings.custom_scale_spine2, Main.settings.custom_scale_spine2);
            left_arm_replay.localScale = new Vector3(Main.settings.custom_scale_arm_l, Main.settings.custom_scale_arm_l, Main.settings.custom_scale_arm_l);
            left_forearm_replay.localScale = new Vector3(Main.settings.custom_scale_forearm_l, Main.settings.custom_scale_forearm_l, Main.settings.custom_scale_forearm_l);
            right_arm_replay.localScale = new Vector3(Main.settings.custom_scale_arm_r, Main.settings.custom_scale_arm_r, Main.settings.custom_scale_arm_r);
            right_forearm_replay.localScale = new Vector3(Main.settings.custom_scale_forearm_r, Main.settings.custom_scale_forearm_r, Main.settings.custom_scale_forearm_r);
            left_upleg_replay.localScale = new Vector3(Main.settings.custom_scale_upleg_l, Main.settings.custom_scale_upleg_l, Main.settings.custom_scale_upleg_l);
            left_leg_replay.localScale = new Vector3(Main.settings.custom_scale_leg_l, Main.settings.custom_scale_leg_l, Main.settings.custom_scale_leg_l);
            right_upleg_replay.localScale = new Vector3(Main.settings.custom_scale_upleg_r, Main.settings.custom_scale_upleg_r, Main.settings.custom_scale_upleg_r);
            right_leg_replay.localScale = new Vector3(Main.settings.custom_scale_leg_r, Main.settings.custom_scale_leg_r, Main.settings.custom_scale_leg_r);
        }

        float tail_collider_offset = 0;
        float nose_collider_offset = 0;
        void UpdateColliders()
        {
            float nose_tail_size = Main.settings.nose_tail_collider / 100f;
            tail_collider.transform.localScale = new Vector3(.2014f + tail_collider_offset, .116f, nose_tail_size);
            tail_collider.transform.localPosition = new Vector3(0, .307f, -nose_tail_size / 2f);
            nose_collider.transform.localScale = new Vector3(.2014f + nose_collider_offset, .116f, nose_tail_size);
            nose_collider.transform.localPosition = new Vector3(0, -.307f, -nose_tail_size / 2f);
        }

        void WaveAnim()
        {
            if (PlayerController.Instance.currentStateEnum.ToString() == Main.settings.wave_on)
            {
                AnimatorStateInfo currentAnimatorStateInfo = PlayerController.Instance.animationController.skaterAnim.GetCurrentAnimatorStateInfo(1);
                if (!currentAnimatorStateInfo.IsName("Wink"))
                {
                    PlayerController.Instance.animationController.gestureController.PlayAnimation("Wink");
                }
            }
        }

        void CheckLetsgoAnim()
        {
            if (PlayerController.Instance.currentStateEnum.ToString() == Main.settings.celebrate_on && PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.Bailed)
            {
                PlayLetsGoAnim();
            }
        }

        public void DisableMultiPopup(bool disabled)
        {
            if (disabled)
            {
                MultiplayerManager.PopupDuration = 0.00000001f;
                MessageSystem.instance.enabled = false;
            }
            else
            {
                MultiplayerManager.PopupDuration = 3f;
                MessageSystem.instance.enabled = true;
            }
        }

        private void HandleHippieOllie()
        {
            if (PlayerController.Instance.inputController.player.GetButton("B"))
            {
                PlayerController.Instance.SetRightIKLerpTarget(1, 1);
                PlayerController.Instance.SetRightSteezeWeight(0f);
                PlayerController.Instance.SetMaxSteezeRight(0f);
                PlayerController.Instance.SetRightKneeIKTargetWeight(1);
                PlayerController.Instance.SetRightIKWeight(1);
                PlayerController.Instance.SetRightKneeBendWeight(1);
                PlayerController.Instance.SetRightKneeBendWeightManually(1);
                PlayerController.Instance.SetLeftIKLerpTarget(1, 1);
                PlayerController.Instance.SetLeftSteezeWeight(0f);
                PlayerController.Instance.SetMaxSteezeLeft(0f);
                PlayerController.Instance.SetLeftKneeIKTargetWeight(Main.settings.HippieForce);
                PlayerController.Instance.SetLeftIKWeight(Main.settings.HippieForce);
                PlayerController.Instance.SetLeftKneeBendWeight(Main.settings.HippieForce);
                PlayerController.Instance.SetLeftKneeBendWeightManually(Main.settings.HippieForce);
                PlayerController.Instance.animationController.skaterAnim.SetBool("Released", true);
                PlayerController.Instance.CrossFadeAnimation("Extend", Main.settings.HippieTime);
                PlayerController.Instance.boardController.boardRigidbody.AddForce(new Vector3(0, 1, 0) * Physics.gravity.y * (Main.settings.HippieForce / 4), ForceMode.Impulse);
            }
        }

        public void CameraFeet()
        {
            Transform left_pos = (Transform)Traverse.Create(PlayerController.Instance.ikController).Field("ikLeftFootPositionOffset").GetValue();
            left_pos.position = new Vector3(left_pos.position.x, PlayerController.Instance.boardController.boardRigidbody.position.y, left_pos.position.z);
            Transform right_pos = (Transform)Traverse.Create(PlayerController.Instance.ikController).Field("ikRightFootPositionOffset").GetValue();
            right_pos.position = new Vector3(right_pos.position.x, PlayerController.Instance.boardController.boardRigidbody.position.y, right_pos.position.z);
        }

        public void DynamicFeet()
        {
            int multi = -1;
            if (PlayerController.Instance.GetBoardBackwards()) multi = 1;

            if (was_leaning) return;

            int count = 0;
            foreach (var state in Enum.GetValues(typeof(PlayerController.CurrentState)))
            {
                if (state.ToString() == PlayerController.Instance.currentStateEnum.ToString())
                {
                    if (Main.settings.dynamic_feet_states[count] == true)
                    {
                        LeftFootRaycast(multi);
                        RightFootRaycast(multi);
                        return;
                    }
                }
                count++;
            }
        }

        void LeftFootRaycast(int multi)
        {
            GameObject temp_go = new GameObject();
            temp_go.transform.position = left_foot.transform.position;
            temp_go.transform.rotation = PlayerController.Instance.boardController.boardRigidbody.gameObject.transform.rotation;
            float multiplier = PlayerController.Instance.GetBoardBackwards() ? -1 : 1;
            multiplier = SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular ? multiplier : -multiplier;

            temp_go.transform.Translate(new Vector3(0.085f * multiplier, 0f, 0.060f * multiplier), Space.Self);
            Vector3 target = temp_go.transform.position;

            if (Main.settings.debug) setDebugCube(debug_cube_3, temp_go.gameObject, Color.blue);

            if (Physics.Raycast(target, -temp_go.transform.up, out left_hit, .5f))
            {
                if (left_hit.collider.gameObject.layer == LayerMask.NameToLayer("Skateboard"))
                {
                    float offset = (Main.settings.left_foot_offset / 10);
                    if (Main.settings.debug) left_hit.collider.gameObject.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.blue);

                    temp_go.transform.position = left_hit.point;
                    temp_go.transform.rotation = Quaternion.LookRotation(temp_go.transform.forward, left_hit.normal);
                    if (Main.settings.debug) setDebugCube(debug_cube, temp_go.gameObject, Color.blue);

                    if (Main.settings.feet_rotation)
                    {
                        Transform left_rot = (Transform)Traverse.Create(PlayerController.Instance.ikController).Field("ikAnimLeftFootTarget").GetValue();
                        float old_y = left_rot.rotation.eulerAngles.y;
                        left_rot.rotation = Quaternion.LookRotation(left_rot.transform.forward, left_hit.normal);
                        left_rot.transform.Rotate(new Vector3(0, 0, 90f), Space.Self);
                        left_rot.rotation = Quaternion.Euler(left_rot.rotation.eulerAngles.x, old_y, left_rot.rotation.eulerAngles.z);
                    }

                    if (Main.settings.feet_offset && !Main.settings.camera_feet)
                    {
                        left_foot.Translate(new Vector3(offset - left_hit.distance, random_offset, random_offset), Space.Self);
                        PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.position = left_foot.position;

                        Transform left_pos = (Transform)Traverse.Create(PlayerController.Instance.ikController).Field("ikAnimLeftFootTarget").GetValue();
                        left_pos.position = new Vector3(left_pos.position.x + random_offset, left_foot.position.y, left_pos.position.z + random_offset);
                    }
                }
            }

            Destroy(temp_go);
        }

        void RightFootRaycast(int multi)
        {
            GameObject temp_go = new GameObject();
            temp_go.transform.position = right_foot.transform.position;
            temp_go.transform.rotation = PlayerController.Instance.boardController.boardRigidbody.gameObject.transform.rotation;
            float multiplier = PlayerController.Instance.GetBoardBackwards() ? -1 : 1;
            multiplier = SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular ? multiplier : -multiplier;

            temp_go.transform.Translate(new Vector3(0.085f * multiplier, 0f, 0.01f * multiplier), Space.Self);
            Vector3 target = temp_go.transform.position;
            if (Main.settings.debug) setDebugCube(debug_cube_4, temp_go.gameObject, Color.red);

            if (Physics.Raycast(target, -temp_go.transform.up, out right_hit, .5f))
            {
                if (right_hit.collider.gameObject.layer == LayerMask.NameToLayer("Skateboard"))
                {
                    float offset = (Main.settings.right_foot_offset / 10);
                    if (Main.settings.debug) right_hit.collider.gameObject.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.red);

                    temp_go.transform.position = right_hit.point;
                    temp_go.transform.rotation = Quaternion.LookRotation(temp_go.transform.forward, right_hit.normal);
                    if (Main.settings.debug) setDebugCube(debug_cube_2, temp_go.gameObject, Color.red);

                    if (Main.settings.feet_rotation)
                    {
                        Transform right_rot = (Transform)Traverse.Create(PlayerController.Instance.ikController).Field("ikAnimRightFootTarget").GetValue();
                        float old_y = right_rot.rotation.eulerAngles.y;
                        right_rot.rotation = Quaternion.LookRotation(right_rot.transform.forward, right_hit.normal);
                        right_rot.transform.Rotate(new Vector3(0, 0, 90f), Space.Self);
                        right_rot.rotation = Quaternion.Euler(right_rot.rotation.eulerAngles.x, old_y, right_rot.rotation.eulerAngles.z);
                    }

                    if (Main.settings.feet_offset && !Main.settings.camera_feet)
                    {
                        right_foot.Translate(new Vector3(offset - right_hit.distance, random_offset, random_offset), Space.Self);
                        PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.position = right_foot.position;

                        Transform right_pos = (Transform)Traverse.Create(PlayerController.Instance.ikController).Field("ikAnimRightFootTarget").GetValue();
                        right_pos.position = new Vector3(right_pos.position.x + random_offset, right_foot.position.y, right_pos.position.z + random_offset);
                    }
                }
            }

            Destroy(temp_go);
        }

        void setDebugCube(GameObject cube, GameObject temp_go, Color c)
        {
            if (!Main.settings.debug) return;
            try
            {
                if (cube)
                {
                    cube.transform.position = temp_go.transform.position;
                    cube.transform.rotation = temp_go.transform.rotation;
                    cube.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", c);
                }
            }
            catch
            {
                UnityModManager.Logger.Log("error");
            }
        }

        bool InAir()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.InAir || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Release;
        }

        bool Grinding()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grinding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.EnterCoping || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.ExitCoping;
        }

        void raycast()
        {
            if (Physics.Raycast(PlayerController.Instance.boardController.gameObject.transform.position, -PlayerController.Instance.boardController.gameObject.transform.up, out _hit, 2f, layerMask))
            {
                if (_hit.collider.gameObject.name != "Skater_foot_l" && _hit.collider.gameObject.name != "Skater_foot_r" && _hit.collider.gameObject.layer != LayerMask.NameToLayer("Skateboard") && _hit.collider.gameObject.layer != LayerMask.NameToLayer("Character"))
                {
                    MonoBehaviourSingleton<PlayerController>.Instance.currentStateEnum = PlayerController.CurrentState.Pushing;
                    EventManager.Instance.EndTrickCombo(false, false);
                    PlayerController.Instance.skaterController.InitializeSkateRotation();
                    PlayerController.Instance.boardController.DoBoardLean();
                    PlayerController.Instance.movementMaster = PlayerController.MovementMaster.Board;
                    PlayerController.Instance.SetBoardToMaster();
                    PlayerController.Instance.cameraController.NeedToSlowLerpCamera = true;
                    PlayerController.Instance.ScalePlayerCollider();
                    PlayerController.Instance.ToggleFlipColliders(false);
                    PlayerController.Instance.boardController.SetBoardControllerUpVector(_hit.normal);
                    PlayerController.Instance.boardController.SnapRotation();
                    ApplyWeightOnBoard();
                    PlayerController.Instance.comController.UpdateCOM();
                    PlayerController.Instance.skaterController.AddCollisionOffset();
                    Vector3 force = PlayerController.Instance.skaterController.PredictLanding(PlayerController.Instance.skaterController.skaterRigidbody.velocity);
                    PlayerController.Instance.skaterController.skaterRigidbody.AddForce(force, ForceMode.Impulse);
                }
            }
            else
            {
                was_leaning = false;
            }
        }

        void checkAngles()
        {
            Vector3 rot = PlayerController.Instance.boardController.gameObject.transform.rotation.eulerAngles;
            if (rot.z >= 350 || rot.z <= 10)
            {
                was_leaning = false;
                PlayerController.Instance.EnableArmPhysics();
            }
        }

        public void ApplyWeightOnBoard()
        {
            Vector3 velocity = PlayerController.Instance.boardController.boardRigidbody.velocity;
            PlayerController.Instance.boardController.boardRigidbody.AddForce(-PlayerController.Instance.skaterController.skaterTransform.up * Main.settings.wallride_downforce * PlayerController.Instance.impactBoardDownForce, ForceMode.Force);
        }

        public bool IsGrounded()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Impact || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Riding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Manual || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Powerslide || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pushing || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Braking || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grinding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.EnterCoping || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.ExitCoping;
        }

        public bool IsGroundedForFeet()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Impact || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Manual || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grinding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.EnterCoping || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.ExitCoping;
        }

        Transform replay;
        Transform left_foot_replay, right_foot_replay;
        Transform neck_replay, pelvis_replay, spine1_replay, spine2_replay, left_arm_replay, left_forearm_replay, right_arm_replay, right_forearm_replay, left_upleg_replay, left_leg_replay, right_upleg_replay, right_leg_replay;
        Transform getReplayEditor()
        {
            Transform main = PlayerController.Instance.skaterController.transform.parent.transform.parent;
            replay = main.Find("ReplayEditor");
            Transform playback = replay.Find("Playback Skater Root");
            Transform skater = playback.Find("NewSkater");
            Transform joints = skater.Find("Skater_Joints");

            head_replay = joints.FindChildRecursively("Skater_Head");
            left_hand_replay = joints.FindChildRecursively("Skater_hand_l");
            right_hand_replay = joints.FindChildRecursively("Skater_hand_r");
            left_foot_replay = joints.FindChildRecursively("Skater_foot_l");
            right_foot_replay = joints.FindChildRecursively("Skater_foot_r");
            pelvis_replay = joints.FindChildRecursively("Skater_pelvis");
            left_upleg_replay = joints.FindChildRecursively("Skater_UpLeg_l");
            left_leg_replay = joints.FindChildRecursively("Skater_Leg_l");
            right_upleg_replay = joints.FindChildRecursively("Skater_UpLeg_r");
            right_leg_replay = joints.FindChildRecursively("Skater_Leg_r");
            spine1_replay = joints.FindChildRecursively("Skater_Spine1");
            spine2_replay = joints.FindChildRecursively("Skater_Spine2");
            neck_replay = joints.FindChildRecursively("Skater_Neck");
            left_forearm_replay = joints.FindChildRecursively("Skater_ForeArm_l");
            right_forearm_replay = joints.FindChildRecursively("Skater_ForeArm_r");
            left_arm_replay = joints.FindChildRecursively("Skater_Arm_l");
            right_arm_replay = joints.FindChildRecursively("Skater_Arm_r");

            return skater;
        }

        public void getDeck()
        {
            Transform parent = PlayerController.Instance.boardController.gameObject.transform;
            Transform deck = parent.Find("Deck");
            Transform colliders = deck.Find("Colliders");

            center_collider = colliders.Find("Cube (5)");
            center_collider.transform.localScale = new Vector3(.2137f, .5149f, .01f);
            center_collider.transform.localPosition = new Vector3(0, 0, -0.01f);
            if (center_collider.GetComponent<MeshRenderer>() == null)
            {
                center_collider.gameObject.AddComponent<MeshRenderer>();
                center_collider.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
            }

            tail_collider = colliders.Find("Cube (1)");
            if (tail_collider.GetComponent<MeshRenderer>() == null)
            {
                tail_collider.gameObject.AddComponent<MeshRenderer>();
                tail_collider.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
            }

            nose_collider = colliders.Find("Cube (2)");
            if (nose_collider.GetComponent<MeshRenderer>() == null)
            {
                nose_collider.gameObject.AddComponent<MeshRenderer>();
                nose_collider.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
            }

            UnityModManager.Logger.Log("Deck initialized, " + center_collider.name + " " + tail_collider.name + " " + nose_collider.name);
        }

        public void checkDebug()
        {
            try
            {
                if (Main.settings.debug)
                {
                    debug_cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debug_cube.GetComponent<Collider>().enabled = false;
                    debug_cube.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                    debug_cube.transform.localScale = new Vector3(.1f, .1f, .1f);
                    debug_cube.AddComponent<Rigidbody>();
                    // debug_cube.AddComponent<ObjectTracker>();

                    debug_cube_2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debug_cube_2.GetComponent<Collider>().enabled = false;
                    debug_cube_2.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                    debug_cube_2.transform.localScale = new Vector3(.1f, .1f, .1f);
                    debug_cube_2.AddComponent<Rigidbody>();
                    // debug_cube_2.AddComponent<ObjectTracker>();

                    debug_cube_3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debug_cube_3.GetComponent<Collider>().enabled = false;
                    debug_cube_3.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                    debug_cube_3.transform.localScale = new Vector3(.05f, .05f, .05f);
                    debug_cube_3.AddComponent<Rigidbody>();
                    // debug_cube_3.AddComponent<ObjectTracker>();

                    debug_cube_4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debug_cube_4.GetComponent<Collider>().enabled = false;
                    debug_cube_4.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                    debug_cube_4.transform.localScale = new Vector3(.05f, .05f, .05f);
                    debug_cube_4.AddComponent<Rigidbody>();
                    // debug_cube_4.AddComponent<ObjectTracker>();

                    center_collider.gameObject.GetComponent<MeshRenderer>().enabled = Main.settings.debug;
                    tail_collider.gameObject.GetComponent<MeshRenderer>().enabled = Main.settings.debug;
                    nose_collider.gameObject.GetComponent<MeshRenderer>().enabled = Main.settings.debug;
                }
                else
                {
                    if (debug_cube != null) Destroy(debug_cube);
                    if (debug_cube_2 != null) Destroy(debug_cube_2);
                    if (debug_cube_3 != null) Destroy(debug_cube_3);
                    if (debug_cube_4 != null) Destroy(debug_cube_4);
                }
            }
            catch { }
        }

        Transform left_hand, right_hand, pelvis, spine1, spine2, left_arm, left_forearm, right_arm, right_forearm, left_upleg, left_leg, right_upleg, right_leg;
        void getFeet()
        {
            Transform parent = PlayerController.Instance.skaterController.gameObject.transform;
            Transform joints = parent.Find("Skater_Joints");
            pelvis = joints.FindChildRecursively("Skater_pelvis");

            left_upleg = joints.FindChildRecursively("Skater_UpLeg_l");
            left_leg = joints.FindChildRecursively("Skater_Leg_l");
            left_foot = joints.FindChildRecursively("Skater_foot_l");

            right_upleg = joints.FindChildRecursively("Skater_UpLeg_r");
            right_leg = joints.FindChildRecursively("Skater_Leg_r");
            right_foot = joints.FindChildRecursively("Skater_foot_r");

            spine = joints.FindChildRecursively("Skater_Spine");
            spine1 = joints.FindChildRecursively("Skater_Spine1");
            spine2 = joints.FindChildRecursively("Skater_Spine2");
            neck = joints.FindChildRecursively("Skater_Neck");
            head = joints.FindChildRecursively("Skater_Head");

            left_forearm = joints.FindChildRecursively("Skater_ForeArm_l");
            right_forearm = joints.FindChildRecursively("Skater_ForeArm_r");
            left_arm = joints.FindChildRecursively("Skater_Arm_l");
            right_arm = joints.FindChildRecursively("Skater_Arm_r");
            left_hand = joints.FindChildRecursively("Skater_hand_l");
            right_hand = joints.FindChildRecursively("Skater_hand_r");

            UnityModManager.Logger.Log("Body initialized, " + right_foot.name + " " + left_foot.name);
        }

        bool IsPumping()
        {
            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular)
            {
                if (PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= 0.5f || PlayerController.Instance.inputController.LeftStick.rawInput.pos.y <= -0.5f) return true;
            }
            else
            {
                if (PlayerController.Instance.inputController.RightStick.rawInput.pos.y <= -0.5f || PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= 0.5f) return true;
            }

            return false;
        }
    }
}
