using spaar;
using System;
using spaar.ModLoader;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TheGuysYouDespise;
using Blocks;



namespace PanelMod
{
    public class PanelBlock : BlockMod
    {
        public override Version Version { get { return new Version("0.2"); } }
        public override string Name { get { return "Panel Block Mod"; } }
        public override string DisplayName { get { return "Panel Block Mod"; } }
        public override string BesiegeVersion { get { return "v0.11"; } }
        public override string Author { get { return "bo我是天才od 覅是"; } }
        protected Block panelBlock = new Block()
                .ID(505)
                .TextureFile("outUV1.png")
                .BlockName("Panel Block")
                .Obj(new List<Obj> { new Obj("k191.obj", new VisualOffset(Vector3.one, Vector3.zero, Vector3.zero)) })
                .Scripts(new Type[] { typeof(PanelBlockS) })
                .Properties(new BlockProperties().Key1("清除轨迹", "c").Key2("重设相对位置/角度", "x")
                                                 .CanBeDamaged(Mathf.Infinity)
                                                 .ToggleModeEnabled("不标注轨迹", false)
                                                 )
                .Mass(0.01f)
                .IconOffset(new Icon(1f, new Vector3(0f, 0f, 0f), new Vector3(-90f, 45f, 0f)))//第一个float是图标缩放，五六七是我找的比较好的角度
                .ShowCollider(false)
                .AddingPoints(new List<AddingPoint> { new BasePoint(true, false) })
                .CompoundCollider(new List<ColliderComposite> { /*new ColliderComposite (0.5f, 1f, 0, new Vector3(0, 0, 0.7f), new Vector3(0, 0, 0)),*/new ColliderComposite(new Vector3(0.7f, 0.7f, 1.3f), new Vector3(0f, 0f, 0.8f), new Vector3(0f, 0f, 0f)) })
                .NeededResources(new List<NeededResource> { new NeededResource(ResourceType.Audio, "missleLaunch.ogg") }//需要的资源，例如音乐

            );
        public override void OnLoad()
        {
            LoadFancyBlock(panelBlock);//加载该模块
        }
        public override void OnUnload() { }
    }




    public class PanelModCore
    {
        private MouseOrbit m;
        
        private float alt = 0;
        private Vector3 vel3_now = Vector3.zero;
        private Vector3 vel3_past = Vector3.zero;
        private Vector3 vel3_cam = Vector3.zero;
        private float vel1 = 0;
        private Vector3 grav = Vector3.down * (float)38.5;
        private float time0 = 0;
        private float time00 = 0;
        private float time = 0;
        private float T1 = 0;
        private float T2 = 0;
        private Vector3 pos = Vector3.zero;
        private float mileage = 0;
        private float mileage2 = 0;
        private float dt = 0;
        private Vector3 dv = Vector3.zero;
        private Vector3 bar = Vector3.forward;
        private Vector3 p1 = Vector3.zero;
        private Vector3 p2 = Vector3.back;

        private int mode = 0;
        private float num_alt = 1;
        private float num_vel = 1;
        private float num_time = 1;
        private int num_mileage = 1;


        private string label_alt = "Altitude/m: ";
        private string label_vel = "Speed/m/s: ";
        private string label_time = "TotalTime/s: ";
        private string label_mileage = "Mileage/m: ";
        

        
        public PanelModCore()
        {
        }

        public void Parameter(Rect rect)
        {

            //Debug.Log(m.target.name);

            if ((AddPiece.isSimulating))
            {
           
                GUILayout.BeginArea(rect);
                
                if (this.m.target.rigidbody != null)
                {
                    GUILayout.BeginHorizontal("box");
                    if (GUILayout.Button("driver"))
                    {
                        mode = 0;
                    }
                    if (GUILayout.Button("pilot"))
                    {
                        mode = 1;
                    }
                    GUILayout.EndHorizontal();



                    T1 = Time.time;
                    vel3_now = this.m.target.rigidbody.velocity;


                    vel1 = vel3_now.magnitude;
                        if (GUILayout.Button(string.Concat(label_vel, ((int)(vel1* num_vel)).ToString())))
                        {
                            if (num_vel == 1)
                            {
                                label_vel = "Speed/km/h: ";
                                num_vel = (float)3.6;
                            }
                            else if(num_vel==(float)3.6)
                            {
                                label_vel = "Speed/mph: ";
                                num_vel = (float)2.24;
                            }
                            else
                            {
                                label_vel = "Speed/m/s: ";
                                num_vel = 1;
                            }
                        }


                    pos = this.m.target.rigidbody.position;

                    if (mode == 1)
                    {
                        alt = (int)((pos.y) / num_alt);

                        if (GUILayout.Button(string.Concat(label_alt, alt.ToString())))
                        {
                            if (num_alt == 1)
                            {
                                label_alt = "Altitude/ft: ";
                                num_alt = (float)0.3048;
                            }
                            else
                            {
                                label_alt = "Altitude/m: ";
                                num_alt = 1;
                            }


                        }
                    }



                    dv = vel3_now - vel3_past;
                    dt = T1 - T2;

                    if (mode == 0)
                    {

                        
                        mileage = (mileage + Mathf.Sqrt(vel3_now.x * vel3_now.x + vel3_now.z * vel3_now.z) * dt);
                        if (num_mileage == 1)
                        {
                            mileage2 = (int)mileage;
                        }
                        else
                        {
                            mileage2 = mileage/num_mileage;
                        }
                        if (GUILayout.Button(string.Concat(label_mileage, mileage2 .ToString())))
                        {
                            if (num_mileage == 1)
                            {
                                num_mileage = 1000;
                                label_mileage = "Mileage/km: ";
                            }
                            else
                            {
                                num_mileage = 1;
                                label_mileage = "Mileage/m: ";
                            }
                        }
                    }





                    if (num_time == 1)
                    {
                        time = (int)(T1-time00);
                    }
                    else if (num_time == 2)
                    {
                        time = T1 - time0;
                    }

                    if(GUILayout.Button(string.Concat(label_time, time.ToString())))
                    {
                        if (num_time == 1)
                        {
                            time0 = T1;
                            label_time = "tic... ";
                            num_time = 2;
                            
                        }
                        else if (num_time == 2)
                        {
                            label_time = "toc: ";
                            num_time = 3;
                        }
                        else
                        {
                            label_time = "TotalTime/s: ";
                            num_time = 1;
                            time0 = 0;
                        }
                    }


                    

                    vel3_past = vel3_now;
                    T2 = T1;
                    


                }
                GUILayout.EndArea();
                
            }
            else
            {
                mileage = 0;
                time00 = Time.time;
            }
          
            
        }

        public void INPUT()
        {
            if (this.m == null)
            {
                this.m = Camera.main.GetComponent<MouseOrbit>();
                if (this.m == null)
                {
                    return;
                }
            }
            if ((this.m.target == null ? true : !AddPiece.isSimulating))
            {
            }
        }


        
        public void Cam()
        {

            if (AddPiece.isSimulating)
            {
                Camera.main.transform.Rotate(new Vector3(0, 10, 0), Time.fixedDeltaTime * 2);
                vel3_cam.x = vel3_now.x;
                vel3_cam.z = vel3_now.z;
                p1 = pos - vel3_cam.normalized * 10;


            }

            



            
            
        }

    }





    public class PanelModLoader : MonoBehaviour
    {
        public PanelModLoader()
        {
            Debug.Log("Started the PanelMod!");
        }

        public void Start()
        {

            base.gameObject.AddComponent<MMMwo>();

        }
    }

    
    internal class MMMwo : MonoBehaviour
    {
        private PanelModCore MMm = new PanelModCore();

        public MMMwo()
        {
        }

        public void OnGUI()
        {
            this.MMm.Parameter(new Rect(0f, 58f, 200f, 300f));
            this.MMm.Cam();
        }

        public void Update()
        {
            this.MMm.INPUT();
        }
    }




    public class PanelBlockS : BlockScript {
        private string label_alt = "Altitude/m: ";
        private string label_vel = "Speed/m/s: ";
        private string label_time = "TotalTime/s: ";
        private string label_mileage = "Mileage/m: ";
        private Vector3 坐标到归零的差距;
        private Vector3 角度到归零的差距;
        public Vector3 速度;
        public float 平面速度;
        public Vector3 相对位置;
        public float 相对高度;
        public bool 有相对高度;
        public Vector3 绝对位置;
        public Vector3 绝对角度;
        public float 相对角度;
        public float myrts;
        private GameObject Line;
        private int i;

        private RaycastHit hitt;
        private RaycastHit hitt2;


        protected override void OnSimulateStart()
        {
            i = 0;
            坐标到归零的差距 = Vector3.zero;
            角度到归零的差距 = Vector3.zero;
            速度 = Vector3.zero;
            相对位置 = Vector3.zero;
            相对高度 = 0;
            有相对高度 = false;
            相对角度 = 0;
        }

        protected override void OnSimulateFixedUpdate()
        {
            myrts = this.transform.rotation.ToEulerAngles().y * Mathf.Rad2Deg;
            if (myrts >= 180) { myrts = myrts - 360; }
            速度 = this.rigidbody.velocity;
            平面速度 =(float) Math.Sqrt(Math.Pow( this.rigidbody.velocity.x, 2) + Math.Pow(this.rigidbody.velocity.y, 2));
            if (/*按下了reset*/Input.GetKey(this.GetComponent<MyBlockInfo>().key2/*这里是打算使用零件专用的按键*/)) {
                坐标到归零的差距 = Vector3.zero - this.transform.position;
                角度到归零的差距 = Vector3.zero - this.transform.rotation.ToEulerAngles();
                /*ToEulerAngles()能将四元数转化为三轴360° rad格式*/
            }
            相对位置 = 坐标到归零的差距 + Vector3.zero;//我脑子有些糊涂不知道这个算式对不对=。=
            Ray 阻碍检测ray = new Ray(this.transform.position, Vector3.down);
            if (Physics.Raycast(阻碍检测ray, out hitt2, Mathf.Infinity))
            {
                相对高度 = this.transform.position.y - hitt2.point.y;
                有相对高度 = true;
            }
            else { 有相对高度 = false; }
            绝对位置 = this.transform.position;
            相对角度 = (Mathf.Atan2(相对位置.x, 相对位置.z) - 角度到归零的差距.y*Mathf.Rad2Deg - myrts);
            绝对角度 = this.transform.rotation.ToEulerAngles();
            //重设轨迹
            if (Input.GetKey(this.GetComponent<MyBlockInfo>().key1/*这里也是打算使用零件专用的按键*/)) { i = 0; UnityEngine.Object.DestroyImmediate(Line.gameObject); }
            
            //继续轨迹
                if (this.GetComponent<MyBlockInfo>().toggleModeEnabled == false/*这里是打算使用零件专用的toggle*/)
            {
                Line = new GameObject();
                Line.name = "轨迹";
                Line.AddComponent<LineRenderer>();
                Line.renderer.material = new Material(Shader.Find("Particles/Additive"));
                Line.GetComponent<LineRenderer>().SetWidth(0.1f, 0.1f);
                Line.GetComponent<LineRenderer>().SetColors(Color.Lerp(Color.red, Color.white, 0.5f), Color.Lerp(Color.yellow, Color.white, 0.5f));
            }
            Line.GetComponent<LineRenderer>().SetPosition(i, this.transform.position);
            i += 1;
        }




            }



}
