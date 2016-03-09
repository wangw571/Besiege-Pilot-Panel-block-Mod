using spaar;
using System;
using spaar.ModLoader;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using TheGuysYouDespise;
using Blocks;


namespace PanelBlock
{
    public class PanelBlock : BlockMod
    {
        public override Version Version { get { return new Version("0.4"); } }
        public override string Name { get { return "Panel Block Mod"; } }
        public override string DisplayName { get { return "Panel Block Mod"; } }
        public override string BesiegeVersion { get { return "v0.25"; } }
        public override string Author { get { return "bo我是天才od 覅是"; } }

        protected Block panelBlock = new Block()
            .ID(505)
            .BlockName("Panel Block")
            .Obj(new List<Obj> { new Obj("k191.obj", //Obj
                                         "outUV1.png", //贴图
                                         new VisualOffset(new Vector3(1f, 1f, 1f), //Scale
                                                          new Vector3(0f, 0f, 0f), //Position
                                                          new Vector3(0f, 0f, 0f)))//Rotation
            })
            ///在UI下方的选模块时的模样
            .IconOffset(new Icon(1f, new Vector3(0f, 0f, 0f), new Vector3(-90f, 45f, 0f))) //Rotation
            .Components(new Type[] { typeof(SensorS), })

            ///给搜索用的关键词
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                                                             "panel",
                                                             "pilot",
                                                             "navigate",
                                                             "data",
                                             }))
            .Mass(0.1f)
            .ShowCollider(false)
            .CompoundCollider(new List<ColliderComposite> { new ColliderComposite(new Vector3(0.7f, 0.7f, 1.3f), new Vector3(0f, 0f, 0.8f), new Vector3(0f, 0f, 0f)) })
            .NeededResources(new List<NeededResource> {new NeededResource(ResourceType.Texture, "Indicator.jpg") })
            .AddingPoints(new List<AddingPoint> {
                               (AddingPoint)new BasePoint(true, true)
                                                .Motionable(false,false,false)
                                                .SetStickyRadius(0.5f),
            });
        public override void OnLoad()
        {
            LoadFancyBlock(panelBlock);//加载该模块
        }
        public override void OnUnload() { }
    }

    public class SensorS : BlockScript
    {
        private MKey ActivateTargetIndicator;
        private MToggle HideThisPanel;
        private MToggle AdvTI;
        private MSlider AdvTIS;

        private Texture IndicatorCookie = BlockScript.resources["Indicator.jpg"].texture;
        private GameObject BombDrop;
        private LineRenderer AdvBombDrop;

        public Vector3 displacement;
        public Vector3 velocity;
        public Vector3 rotation;
        public Vector3 direction;
        public Vector3 horizontal;
        public Vector3 bombposition;
        public float T1;
        public float dt;
        public float T_hitground;
        private float alt = 0;
        private float climbrate = 0;
        private Vector3 vel1;
        private Vector3 vel0;
        private float acce = 0;
        private float overload = 0;
        private float yaw = 0;
        private float pitch = 0;
        private float roll = 0;

        private float row1 = 0;
        private float row2 = 0;
        private float row3 = 0;
        private float tic = 0;
        private float ticc = 0;
        private float toc = 0;



        private float num_alt = 1;
        private float num_vel = 1;
        private float num_time = 1;
        private bool disp = true;
        private bool disp2 = false;
        

        private string label_row2 = "Altitude/m: ";
        private string label_row1 = "Speed/m/s: ";
        private string label_row3 = "Time/s: ";


        public override void SafeAwake()
        {
            ActivateTargetIndicator = AddKey("Target Indicator", //按键信息
                                 "TI",           //名字
                                 KeyCode.C);       //默认按键

            HideThisPanel = AddToggle("Hide this/n block's panel",   //toggle信息
                                       "Hide",       //名字
                                       false);             //默认状态
            AdvTI = AddToggle("ADVANCED /n Target Indicator",   //toggle信息
                                       "AdvTI",       //名字
                                       false);             //默认状态
            AdvTIS = AddSlider("Amount /nof Lines", "LineAmt", 20f, 2f, 45f);
        }

        protected virtual IEnumerator UpdateMapper()
        {
            if (BlockMapper.CurrentInstance == null)
                yield break;
            while (Input.GetMouseButton(0))
                yield return null;
            BlockMapper.CurrentInstance.Copy();
            BlockMapper.CurrentInstance.Paste();
            yield break;
        }
        public override void OnSave(BlockXDataHolder data)
        {
            SaveMapperValues(data);
        }
        public override void OnLoad(BlockXDataHolder data)
        {
            LoadMapperValues(data);
            if (data.WasSimulationStarted) return;
        }
        
        void K()
        {
            AdvTIS.DisplayInMapper = AdvTI.IsActive;
        }   
            

        protected override void OnSimulateStart()
        {
            ticc = Time.time;
            disp = !HideThisPanel.IsActive;

            BombDrop = new GameObject();
            BombDrop.transform.SetParent(transform);
            BombDrop.name = "Bomb Indicator"; 

            BombDrop.AddComponent<Light>();

            BombDrop.GetComponent<Light>().type = LightType.Spot;
            BombDrop.GetComponent<Light>().intensity = 8;
            BombDrop.GetComponent<Light>().range = Camera.main.farClipPlane;
            BombDrop.GetComponent<Light>().color = Color.red;
            BombDrop.GetComponent<Light>().cookie = IndicatorCookie;
            BombDrop.GetComponent<Light>().cookieSize = 100;
            BombDrop.GetComponent<Light>().range = 5;
            BombDrop.transform.LookAt(new Vector3(BombDrop.transform.position.x, BombDrop.transform.position.y - 10, BombDrop.transform.position.z));
            BombDrop.GetComponent<Light>().enabled = false;
            BombDrop.AddComponent<DestroyIfEditMode>();

            if (AdvTI.IsActive)
            {
                    float width = 1f;
                AdvBombDrop =  this.gameObject.AddComponent<LineRenderer>();
                AdvBombDrop.material = new Material(Shader.Find("Particles/Additive"));
                AdvBombDrop.SetWidth(width, width);
                AdvBombDrop.SetColors(Color.red, Color.yellow);
                AdvBombDrop.SetPosition(0, transform.position);
                AdvBombDrop.SetVertexCount((int)AdvTIS.Value);
                AdvBombDrop.enabled = false;

            }
        }

        protected override void OnSimulateFixedUpdate()
        {


            displacement = this.GetComponent<Rigidbody>().position;
            velocity = this.GetComponent<Rigidbody>().velocity;
            rotation = this.GetComponent<Rigidbody>().rotation.eulerAngles;
            direction = this.transform.forward;
            horizontal =new Vector3(-direction.z/direction.x, 0f, 1f).normalized;

            T1 = Time.time;
            dt = Time.fixedDeltaTime;

            if (disp)
            {
                vel0 = vel1;
                vel1 = velocity;
                acce = (vel1.magnitude - vel0.magnitude) / dt;
                overload = (Vector3.Dot((vel1 - vel0), this.transform.up) / dt + (float)38.5 * Vector3.Dot(Vector3.up, this.transform.up)) / (float)38.5;

                alt = displacement.y;
                climbrate = velocity.y;

                pitch = 90 - Mathf.Acos((2 - (direction - Vector3.up).magnitude * (direction - Vector3.up).magnitude) / 2) / Mathf.PI * 180;
                yaw = rotation.y;
                roll = Mathf.Sign(direction.x) * (Mathf.Acos((2 - (horizontal - this.transform.up).magnitude * (horizontal - this.transform.up).magnitude) / 2) / Mathf.PI * 180 - 90);
            }


            

        }
        protected override void OnSimulateUpdate()
        {
            if (ActivateTargetIndicator.IsReleased)
            {
                disp2 = !disp2;
            }
            if (disp2)
            {
                float grav = Physics.gravity.y;
                T_hitground = (velocity.y + Mathf.Sqrt(velocity.y * velocity.y + 2 * 38.5f * displacement.y)) / 38.5f;
                bombposition = new Vector3(
                    displacement.x + T_hitground * velocity.x,
                    4,
                    displacement.z + T_hitground * velocity.z
                    );

                BombDrop.GetComponent<Light>().enabled = true;
                BombDrop.GetComponent<Light>().transform.position = bombposition;
                BombDrop.GetComponent<Light>().spotAngle = Math.Abs(displacement.y * 3) + 60;
                BombDrop.transform.LookAt(new Vector3(bombposition.x, bombposition.y - 10, bombposition.z));

                if (AdvTI.IsActive)
                {
                    for (int i = (int)AdvTIS.Value; i >= 1; --i)
                        {
                            T_hitground = (velocity.y + Mathf.Sqrt(velocity.y * velocity.y + 2 * 38.5f * displacement.y / (int)AdvTIS.Value * i)) / 38.5f;
                            bombposition = new Vector3(
                                displacement.x + T_hitground * velocity.x,
                                displacement.y / (int)AdvTIS.Value * ((int)AdvTIS.Value-i),
                                displacement.z + T_hitground * velocity.z
                                );
                            AdvBombDrop.SetPosition((int)AdvTIS.Value - i - 1, bombposition);
                        AdvBombDrop.SetPosition((int)AdvTIS.Value - 1, transform.position);
                        AdvBombDrop.enabled = true;
                        }                    
                }

            }
            else
            {
               BombDrop.GetComponent<Light>().enabled = false;
                AdvBombDrop.enabled = false;
            }




        }


        public void OnGUI()
        {

                if (disp && AddPiece.isSimulating)
            {

                GUILayout.BeginArea(new Rect(0f, 58f, 200f, 400f));




                if (num_vel == 1)
                {
                    row1 = vel1.magnitude;
                }
                else
                {

                    row1 = acce;
                }


                if (GUILayout.Button(string.Concat(label_row1, ((int)(row1)).ToString())))
                {
                    if (num_vel == 1)
                    {
                        label_row1 = "Accelaration/(m/s^2): ";
                        num_vel = 2;
                    }
                    else 
                    {
                        label_row1 = "Speed/m/s: ";
                        num_vel = 1;
                    }
                }


                if (num_alt == 1)
                {
                    row2 = (int)alt;
                }
                else
                {
                    row2 = (int)climbrate;
                }

                if (GUILayout.Button(string.Concat(label_row2, row2.ToString())))
                {
                    if (num_alt == 1)
                    {
                        label_row2 = "ClimbRate/(m/s): ";
                        num_alt = 2;
                    }
                    else
                    {
                        label_row2 = "Altitude/m: ";
                        num_alt = 1;
                    }


                }


                GUILayout.Button(string.Concat("Overload/G: ", ((int) overload).ToString(),".",(Mathf.Sign(overload)*((int)((overload- ((int)overload)) *10))).ToString() ));

                if (num_time == 1)
                {
                    row3 = (int)(T1 - ticc);
                }
                else if (num_time == 2)
                {
                    row3 = T1 - tic;
                }
                else
                {
                    row3 = toc;
                }

                if (GUILayout.Button(string.Concat(label_row3, row3.ToString())))
                {
                    if (num_time == 1)
                    {
                        tic = T1;
                        label_row3 = "tic... ";
                        num_time = 2;

                    }
                    else if (num_time == 2)
                    {
                        label_row3 = "toc: ";
                        num_time = 3;
                        toc = T1 - tic;
                    }
                    else
                    {
                        label_row3 = "Time/s: ";
                        num_time = 1;
                    }
                }

                GUILayout.BeginHorizontal();

                GUILayout.Button(string.Concat("roll ", ((int) roll).ToString()));
                GUILayout.Button(string.Concat("pitch ", ((int) pitch).ToString()));
                GUILayout.Button(string.Concat("yaw ", ((int) yaw).ToString()));

                GUILayout.EndHorizontal();


                GUILayout.EndArea();








            }

            //if (/*按下了reset*/Input.GetKey(s.GetComponent<MyBlockInfo>().key2/*这里是打算使用零件专用的按键*/))
            //{
            //    
            //}

        }

    }




}
