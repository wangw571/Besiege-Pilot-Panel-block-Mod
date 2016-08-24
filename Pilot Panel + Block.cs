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
        public override Version Version { get { return new Version("0.8"); } }
        public override string Name { get { return "Panel Block Mod"; } }
        public override string DisplayName { get { return "Panel Block Mod"; } }
        public override string BesiegeVersion { get { return "v0.3"; } }
        public override string Author { get { return "Created by bo我是天才od  Maintained by 覅是"; } }

        protected Block panelBlock = new Block()
            .ID(505)
            .BlockName("Panel Block")
            .Obj(new List<Obj> { new Obj("Pilot Panel Block.obj", //Obj
                                         "Pilot Panel Block.png", //贴图
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
            .NeededResources(new List<NeededResource> {
                new NeededResource(ResourceType.Texture, "Indicator.jpg"),
                new NeededResource(ResourceType.Texture, "HUD/Center.png"),
                new NeededResource(ResourceType.Texture, "HUD/Gradienter.png"),
                new NeededResource(ResourceType.Texture, "HUD/Direction Indicator.png"),
                new NeededResource(ResourceType.Texture, "HUD/Zero Zero Front.png"),
                new NeededResource(ResourceType.Texture, "HUD/Zero Zero Back.png"),
                new NeededResource(ResourceType.Texture, "HUD/Ice Floor.png"),
                new NeededResource(ResourceType.Texture, "HUD/Floor Line.png"),
                new NeededResource(ResourceType.Texture, "HUD/Height Line.png"),
                new NeededResource(ResourceType.Texture, "HUD/OverICE Line.png")
            })
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
        private MColourSlider StartColour;
        private MColourSlider EndColour;
        private MKey ActiveHUD;
        private MKey HidePanel;
        public bool HidePanelBool = false;
        public bool HUD_Activated = false;

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

        private Texture 俯仰刻度;
        private Texture 机体准星;
        private Texture 罗盘纹理;
        private Texture 正00纹理;
        private Texture 负00纹理;
        private Texture 冰层纹理;
        private Texture 地面那一条杠杠滴纹理;
        private Texture 现时高度指示纹理;
        private Texture 一千杠杠;

        //private GameObject 冲刺效果 = GameObject.CreatePrimitive(PrimitiveType.Plane);

        public bool 高度计_渐变中 = false;
        public int 高度计状态 = 0; //-1 地底   0 冰层下   1 1000下   2 1000上
        public int 比较用高度计状态 = 0; //-1 地底   0 冰层下   1 1000下   2 1000上
        public Vector2 pos = new Vector2(Screen.width / 2, Screen.height / 2);
        public float 调试函数 = 0;
        private Key LCF1 = new Key(KeyCode.LeftControl, KeyCode.F2);
        private Key RCF1 = new Key(KeyCode.RightControl, KeyCode.F2);
        private float CurrentCameraSpeed;
        private float 速度标记位置 = 0;
        public bool 强制关闭 = false;
        public float 渐变高度计使用的临时函数 = 0;


        private float LastFUveloMag;

        public override void SafeAwake()
        {
            ActivateTargetIndicator = AddKey("Target Indicator", //按键信息
                                 "TI",           //名字
                                 KeyCode.C);       //默认按键

            HideThisPanel = AddToggle("Hide this\n block's panel",   //toggle信息
                                       "Hide",       //名字
                                       false);             //默认状态

            ActiveHUD = AddKey("Toggle HUD", "HUD", KeyCode.Tab);
            HidePanel = AddKey("Hide Panel", "Panel", KeyCode.Backspace);

            AdvTI = AddToggle("ADVANCED \n Target Indicator",   //toggle信息
                                       "AdvTI",       //名字
                                       false);             //默认状态
            AdvTIS = AddSlider("Amount of Lines", "LineAmt", 20f, 2f, 45f);

            StartColour = AddColourSlider("Start Color of the line", "SColor", Color.red);

            EndColour = AddColourSlider("End Color of the line", "EColor", Color.yellow);

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
        public override void OnSave(XDataHolder data)
        {
            SaveMapperValues(data);
        }
        public override void OnLoad(XDataHolder data)
        {
            LoadMapperValues(data);
            if (data.WasSimulationStarted) return;
        }

        protected override void BuildingUpdate()
        {
            AdvTIS.DisplayInMapper = AdvTI.IsActive;
            StartColour.DisplayInMapper = AdvTI.IsActive;
            EndColour.DisplayInMapper = AdvTI.IsActive;
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
                AdvBombDrop.SetColors(StartColour.Value, EndColour.Value);
                AdvBombDrop.SetPosition(0, transform.position);
                AdvBombDrop.SetVertexCount((int)AdvTIS.Value);
                AdvBombDrop.enabled = false;

            }

                机体准星 = resources["HUD/Center.png"].texture;
                俯仰刻度 = resources["HUD/Gradienter.png"].texture;
                罗盘纹理 = resources["HUD/Direction Indicator.png"].texture;
                正00纹理 = resources["HUD/Zero Zero Front.png"].texture;
                负00纹理 = resources["HUD/Zero Zero Back.png"].texture;
                冰层纹理 = resources["HUD/Ice Floor.png"].texture;
                地面那一条杠杠滴纹理 = resources["HUD/Floor Line.png"].texture;
                现时高度指示纹理 = resources["HUD/Height Line.png"].texture;
                一千杠杠 = resources["HUD/OverICE Line.png"].texture;
            
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
            if(ActiveHUD.IsPressed)
            {
                HUD_Activated = !HUD_Activated;
            }
            if (ActiveHUD.IsPressed)
            {
                HidePanelBool = !HidePanelBool;
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
                if (BombDrop.GetComponent<Light>())
                {
                    BombDrop.GetComponent<Light>().enabled = true;
                    BombDrop.GetComponent<Light>().transform.position = bombposition;
                    BombDrop.GetComponent<Light>().intensity = 8;
                    BombDrop.GetComponent<Light>().spotAngle = Math.Abs(displacement.y * 3) + 60;
                    BombDrop.transform.LookAt(new Vector3(bombposition.x, bombposition.y - 10, bombposition.z));
                }

                if (AdvTI.IsActive)
                {
                    for (int i = (int)AdvTIS.Value; i >= 1; --i)
                    {
                        /*for (int i = (int)0; i != AdvTIS.Value; ++i)
                             {
                             T_hitground = ((velocity.y + Mathf.Sqrt(velocity.y * velocity.y + 2 * -Physics.gravity.y * displacement.y)) / -Physics.gravity.y);
                             float gotit = (T_hitground * ((AdvTIS.Value - i) / AdvTIS.Value));
                             bombposition = new Vector3(
                                     displacement.x + gotit * velocity.x,
                                     displacement.y + velocity.y * gotit + (gotit * Physics.gravity.y),
                                     displacement.z + gotit * velocity.z
                                     );*/
                        T_hitground = (velocity.y + Mathf.Sqrt(velocity.y * velocity.y + 2 * 38.5f * displacement.y / (int)AdvTIS.Value * i)) / 38.5f;
                        bombposition = new Vector3(
                            displacement.x + T_hitground * velocity.x,
                            displacement.y / (int)AdvTIS.Value * ((int)AdvTIS.Value - i),
                            displacement.z + T_hitground * velocity.z
                            );
                        //AdvBombDrop.SetPosition((int)Mathf.Clamp(i,0,AdvTIS.Value - 1), bombposition);
                        AdvBombDrop.SetPosition((int)Mathf.Clamp(AdvTIS.Value - i - 1, 0, AdvTIS.Value - 1), bombposition);
                        AdvBombDrop.SetPosition((int)AdvTIS.Value - 1, transform.position);
                        AdvBombDrop.enabled = true;
                        //Debug.Log(bombposition + " abd " + i + " tm " + T_hitground * (i / AdvTIS.Value) + " und " + (T_hitground * (i / AdvTIS.Value) * Physics.gravity.y));
                    }
                    //Debug.Log("Total Time:" + T_hitground);
                }

            }
            else
            {
                if (BombDrop)
                {
                    BombDrop.GetComponent<Light>().enabled = false;
                }
                if (AdvBombDrop)
                {
                    AdvBombDrop.enabled = false;
                }
            }




        }

        public void OnGUI()
        {
            if (disp && StatMaster.isSimulating)
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

                if (HUD_Activated)
                    OnHUDGUI();






            }

            //if (/*按下了reset*/Input.GetKey(s.GetComponent<MyBlockInfo>().key2/*这里是打算使用零件专用的按键*/))
            //{
            //    
            //}

        }

        void OnHUDGUI()
        {
            float 全局屏幕比值W = Screen.width / 1920;
            float 全局屏幕比值H = Screen.height / 1080;
            Matrix4x4 UnRotatedTempMatrix = GUI.matrix;
            Transform MainCameraTransform = GameObject.Find("Main Camera").transform;
            CurrentCameraSpeed = Vector3.Dot(MainCameraTransform.GetComponent<Camera>().velocity, MainCameraTransform.forward);
            Vector3 zerooncamera = MainCameraTransform.GetComponent<Camera>().WorldToScreenPoint(Vector3.zero);
            GUIUtility.RotateAroundPivot(MainCameraTransform.eulerAngles.z, new Vector2(zerooncamera.x - 20, (Screen.height - zerooncamera.y) - 20));

            if (zerooncamera.z > 0)
            {
                GUI.DrawTexture(
                    new Rect(
                        new Vector2(
                            zerooncamera.x - 20,
                            (Screen.height - zerooncamera.y) - 20),
                        new Vector2(40, 40)),
                    正00纹理);
            }
            else if (zerooncamera.z < 0)
            {
                GUI.DrawTexture(
                    new Rect(
                        new Vector2(
                            zerooncamera.x - 20,
                            (Screen.height - zerooncamera.y) - 20),
                        new Vector2(40, 40)),
                    负00纹理);
            }
            GUI.matrix = UnRotatedTempMatrix;
            //Camera.main.gameObject.AddComponent<HUDthingy>();
            //水平球.transform.position = Camera.main.gameObject.transform.position;
            //罗盘球.transform.position = Camera.main.gameObject.transform.position;
            //指示球.transform.position = Camera.main.gameObject.transform.position;
            //GUI.DrawTexture(new Rect(new Vector2(40, 40), new Vector2(40, 40)), 校准纹理, ScaleMode.ScaleAndCrop);
            //GUIUtility.ScaleAroundPivot(new Vector2(Screen.width / 1920, Screen.height / 1080), new Vector2(Screen.width, Screen.height) / 2);
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 130 / 2, Screen.height / 2 - 7), new Vector2(130, 46)), 机体准星);
            GUIUtility.RotateAroundPivot(MainCameraTransform.eulerAngles.z, new Vector2(Screen.width / 2, Screen.height / 2));
            float FOVHeight = 180 / MainCameraTransform.GetComponent<Camera>().fieldOfView;
            float Height = MainCameraTransform.position.y;
            float 视角 = MainCameraTransform.eulerAngles.x;
            float 方向 = MainCameraTransform.eulerAngles.y;

            if (视角 >= 270) 视角 -= 360;
            Matrix4x4 RotatedTempMatrix = GUI.matrix;
            //G俯仰刻度.pixelInset = (new Rect(new Vector2(0 - 453.5f / 2, 0 - (1215 * 1f) * ((视角 + 180 - 10f) / 180)),
            //new Vector2(453.5f, 1215 * 1)));

            GUI.DrawTexture(
                new Rect(new Vector2(Screen.width / 2 - 453.5f / 2, Screen.height - (1215 * 1f) * ((视角 + 180 - (89.175f - 0.1484868421f * Screen.height / 2)) / 180)),
                new Vector2(453.5f, 1215 * 1))
                , 俯仰刻度
                , ScaleMode.ScaleAndCrop);


            绘制罗盘(方向, 罗盘纹理, RotatedTempMatrix);
            绘制罗盘(方向 + 90, 罗盘纹理, RotatedTempMatrix);
            绘制罗盘(方向 - 90, 罗盘纹理, RotatedTempMatrix);
            绘制罗盘(方向 + 180, 罗盘纹理, RotatedTempMatrix);
            绘制罗盘(方向 + 180, 罗盘纹理, RotatedTempMatrix);

            高度计状态 = 判断高度计状态(MainCameraTransform.position.y);

            if (高度计状态 != 比较用高度计状态 && !高度计_渐变中)
            {
                高度计_渐变中 = true;
            }
            if (高度计_渐变中 && (高度计状态 == 1 && 比较用高度计状态 == 0))
            {
                绘制0到1渐变高度计(MainCameraTransform.position.y, 高度计状态);
            }
            else if (高度计_渐变中 && (高度计状态 == 0 && 比较用高度计状态 == 1))
            {
                绘制1到0渐变高度计(MainCameraTransform.position.y, 高度计状态);
            }
            else
            {
                if (高度计状态 == -1) { 绘制天花板高度计(MainCameraTransform.position.y); }
                if (高度计状态 == 0) { 绘制下冰层高度计(MainCameraTransform.position.y); }
                if (高度计状态 == 1) { 绘制下千高度计(MainCameraTransform.position.y); }
                if (高度计状态 == 2) { 绘制随意高度计(MainCameraTransform.position.y); }
            }
            GUI.matrix = UnRotatedTempMatrix;

            //冲刺效果.transform.position = MainCameraTransform.GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, (GameObject.Find("Main Camera").GetComponent<Camera>().nearClipPlane + 0.1f /* MainCameraTransform.GetComponent<Camera>().velocity.magnitude*/)));
            //冲刺效果.transform.eulerAngles = new Vector3(MainCameraTransform.eulerAngles.x + 270, MainCameraTransform.eulerAngles.y, MainCameraTransform.eulerAngles.z);
            //冲刺效果.GetComponent<Renderer>().material.GetTexture("_MainTex").wrapMode = TextureWrapMode.Clamp;
            ////冲刺效果.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.5f);
            //冲刺效果.transform.localScale = new Vector3(0.054f, 1, 0.059f);
            float angel = Mathf.Atan2((Screen.height / 6) * 2, Screen.width / 2) * Mathf.Rad2Deg;
            GUIUtility.RotateAroundPivot(angel, new Vector2(Screen.width / 2, Screen.height / 2));
        }

        void 绘制罗盘(float 输入方向, Texture 纹理, Matrix4x4 正常矩阵)
        {
            GUIUtility.RotateAroundPivot(
            -输入方向,
            new Vector2(
            Screen.width / 2 + Screen.width * (Mathf.Sin(-输入方向 * Mathf.Deg2Rad)) - 2.5f,
            Screen.height / 2 + Screen.height / 4 + (Screen.height / 2) * Math.Abs(Mathf.Sin(输入方向 / 2 * Mathf.Deg2Rad))
            ));
            GUI.DrawTexture(
            new Rect(
            new Vector2(
                Screen.width / 2 + Screen.width * (Mathf.Sin(-输入方向 * Mathf.Deg2Rad)) - 2.5f,
                Screen.height / 2 + Screen.height / 4 + (Screen.height / 2) * Math.Abs(Mathf.Sin(输入方向 / 2 * Mathf.Deg2Rad))),
            new Vector2(5, 40)),
            纹理);
            GUI.matrix = 正常矩阵;
        }
        void 绘制下冰层高度计(float CurrentHeight)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;
            float IF比 = (49.5f / IceFreezehickness);
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比), new Vector2(283.5f * IF比, 100 * IF比)), 冰层纹理);
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比 + (100 * IF比 / IceFreezehickness * IceCenterHeight)), new Vector2(283.5f * IF比, 10)), 地面那一条杠杠滴纹理);
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比 + (100 * IF比 / IceFreezehickness * (IceCenterHeight - CurrentHeight))), new Vector2(283.5f * IF比, 10)), 现时高度指示纹理);
        }
        void 绘制下千高度计(float CurrentHeight)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;
            float IF比 = (49.5f / IceFreezehickness);
            float 千比 = (800 * IF比 - 20 * IF比) / 1000;
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比), new Vector2(283.5f * IF比 * 千比, 5 * IF比)), 地面那一条杠杠滴纹理);//地面
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 20 * IF比), new Vector2(283.5f * IF比 * 千比, 5 * IF比)), 一千杠杠);//一千
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * IceCenterHeight), new Vector2(283.5f * IF比 * 千比, 100 * IF比 * 千比)), 冰层纹理);
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * CurrentHeight), new Vector2(283.5f * IF比 * 千比, 10)), 现时高度指示纹理);
        }
        void 绘制随意高度计(float CurrentHeight)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;
            float IF比 = (49.5f / IceFreezehickness);
            float 自比 = (800 * IF比 - 20 * IF比) / CurrentHeight;
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 自比, Screen.height / 1080 * 20 * IF比), new Vector2(283.5f * IF比 * 自比, 10)), 现时高度指示纹理);
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 自比, Screen.height / 1080 * 800 * IF比), new Vector2(283.5f * IF比 * 自比, 5 * IF比)), 地面那一条杠杠滴纹理);
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 自比, Screen.height / 1080 * 800 * IF比 - 自比 * 1000), new Vector2(283.5f * IF比 * 自比, 5 * IF比)), 一千杠杠);
            GUI.DrawTexture(new Rect(new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 自比, Screen.height / 1080 * 800 * IF比 - 自比 * IceCenterHeight), new Vector2(283.5f * IF比 * 自比, 100 * IF比 * 自比)), 冰层纹理);

        }
        void 绘制天花板高度计(float CurrentHeight)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;

            float IF比 = (49.5f / IceFreezehickness);
            float IF2比 = IF比 * (IceCenterHeight / (IceCenterHeight - CurrentHeight));
            float WidthScale = IF比 * IceCenterHeight / (IceCenterHeight - CurrentHeight);

            GUI.DrawTexture(
                new Rect(
                    new Vector2(
                        Screen.width / 2 - 100 * IF比 - 283.5f * WidthScale,
                        Screen.height / 1080 * 40 * IF比),
                    new Vector2(
                        283.5f * WidthScale,
                        100 * IF2比)),
                冰层纹理);

            GUI.DrawTexture(
                new Rect(
                    new Vector2(
                        Screen.width / 2 - 100 * IF比 - 283.5f * WidthScale,
                        Screen.height / 1080 * 40 * IF比 + (100 * IF比 / IceFreezehickness * IceCenterHeight)),
                    new Vector2(
                        283.5f * WidthScale,
                        10)),
                现时高度指示纹理);




            GUI.DrawTexture(
                new Rect(
                    new Vector2(
                        Screen.width / 2 - 100 * IF比 - 283.5f * WidthScale,
                        Screen.height / 1080 * 40 * IF比 + (IceCenterHeight / (IceCenterHeight - CurrentHeight)) * (100 * IF比 / IceFreezehickness * IceCenterHeight)),
                    new Vector2(
                        283.5f * WidthScale,
                        10)),
                地面那一条杠杠滴纹理);
        }
        void 绘制0到1渐变高度计(float CurrentHeight, int ToSituation)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            if (渐变高度计使用的临时函数 == 0) 渐变高度计使用的临时函数 = Time.time;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;
            float IF比 = (49.5f / IceFreezehickness);
            float 千比 = (800 * IF比 - 20 * IF比) / 1000;
            float zhe = Time.time - 渐变高度计使用的临时函数;

            GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 800 * IF比),
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比), zhe
                        ),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 10),
                        new Vector2(283.5f * IF比 * 千比, 5 * IF比),
                        zhe)
                        ),
                    地面那一条杠杠滴纹理);//地面

            GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比),
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 20 * IF比), zhe
                        ),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 10),
                        new Vector2(283.5f * IF比 * 千比, 5 * IF比),
                        zhe)
                        ),
                    一千杠杠);//一千

            GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比),
                    new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * IceCenterHeight),
                    zhe),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 100 * IF比),
                        new Vector2(283.5f * IF比 * 千比, 100 * IF比 * 千比)
                    , zhe)), 冰层纹理);

            GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比 + (100 * IF比 / IceFreezehickness * (IceCenterHeight - CurrentHeight))),
                    new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * CurrentHeight), zhe),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 10),
                    new Vector2(283.5f * IF比 * 千比, 10), zhe)),
                现时高度指示纹理);
            if (Time.time - 渐变高度计使用的临时函数 >= 1 || (ToSituation != 1 && 比较用高度计状态 == 0))
            {
                高度计_渐变中 = false;
                比较用高度计状态 = 高度计状态;
                渐变高度计使用的临时函数 = 0;
            }
        }
        void 绘制1到0渐变高度计(float CurrentHeight, int ToSituation)
        {
            Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
            if (渐变高度计使用的临时函数 == 0) 渐变高度计使用的临时函数 = Time.time;
            float IceFreezehickness = ICEtrans.localScale.y;
            float IceCenterHeight = ICEtrans.position.y;
            float IF比 = (49.5f / IceFreezehickness);
            float 千比 = (800 * IF比 - 20 * IF比) / 1000;
            float zhe = 1 - (Time.time - 渐变高度计使用的临时函数);
            GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 800 * IF比),
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比), zhe
                        ),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 10),
                        new Vector2(283.5f * IF比 * 千比, 5 * IF比),
                        zhe)
                        ),
                    地面那一条杠杠滴纹理);//地面

            GUI.DrawTexture(
                            new Rect(
                                Vector2.Lerp(
                                    new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比),
                                    new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 20 * IF比), zhe
                                    ),
                                Vector2.Lerp(
                                    new Vector2(283.5f * IF比, 10),
                                    new Vector2(283.5f * IF比 * 千比, 5 * IF比),
                                    zhe)
                                    ),
                                一千杠杠);//一千

            GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比),
                    new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * IceCenterHeight),
                    zhe),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 100 * IF比),
                        new Vector2(283.5f * IF比 * 千比, 100 * IF比 * 千比)
                    , zhe)), 冰层纹理);

            GUI.DrawTexture(
                new Rect(
                    Vector2.Lerp(
                        new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比, Screen.height / 1080 * 40 * IF比 + (100 * IF比 / IceFreezehickness * (IceCenterHeight - CurrentHeight))),
                    new Vector2(Screen.width / 2 - 100 * IF比 - 283.5f * IF比 * 千比, Screen.height / 1080 * 800 * IF比 - 千比 * CurrentHeight), zhe),
                    Vector2.Lerp(
                        new Vector2(283.5f * IF比, 10),
                    new Vector2(283.5f * IF比 * 千比, 10), zhe)),
                现时高度指示纹理);
            if (Time.time - 渐变高度计使用的临时函数 >= 1 || (ToSituation != 0 && 比较用高度计状态 == 1))
            {
                高度计_渐变中 = false;
                比较用高度计状态 = 高度计状态;
                渐变高度计使用的临时函数 = 0;
            }
        }
        int 判断高度计状态(float Height)
        {
            int 最终状态;
            if (GameObject.Find("ICE FREEZE"))
            {
                Transform ICEtrans = GameObject.Find("ICE FREEZE").transform;
                float IceFreezehickness = ICEtrans.localScale.y;
                float IceCenterHeight = ICEtrans.position.y;
                if (Height < 0)
                {
                    最终状态 = -1;
                }
                else if (Height < (IceCenterHeight + IceFreezehickness / 2))
                {
                    最终状态 = 0;
                }
                else if (Height < 1000)
                {
                    最终状态 = 1;
                }
                else
                {
                    最终状态 = 2;
                }
                return 最终状态;
            }
            else { return -2; }
            /*else
            {
                if (Height <= 0)
                {
                    最终状态 = -1;
                }
                else if (Height < 1000)
                {
                    最终状态 = 1;
                }
                else
                {
                    最终状态 = 2;
                }
            }*/

        }

    }




}
