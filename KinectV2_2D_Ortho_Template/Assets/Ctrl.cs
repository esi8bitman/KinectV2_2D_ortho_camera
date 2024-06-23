using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class Ctrl : MonoBehaviour
{
    [SerializeField]
    Transform lHand,rHand;
    [SerializeField]
    Material mat_RGB;
    [SerializeField]
    float z_mul, pos_mul;

    KinectSensor KSence;
    BodyFrameReader body_Reader;
    ColorFrameReader color_Reader;

    Body[] BodyData;
    ColorFrame colorFrame;
    byte[] color_data;
    Texture2D color_Tex;

    Vector3 vec3_tmp,lhand_vec,rhand_vec;
    int i, j;
    // Start is called before the first frame update
    void Start()
    {
        //initial Kinect
        KSence = KinectSensor.GetDefault();
        Application.targetFrameRate = 60;
        if (KSence != null)
        {
            body_Reader = KSence.BodyFrameSource.OpenReader();
            color_Reader = KSence.ColorFrameSource.OpenReader();
            // if (!KSence.IsOpen)
            // {
            KSence.Open();
            BodyData = new Body[KSence.BodyFrameSource.BodyCount];

            var frameDesc = KSence.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);

            color_Tex = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
            color_data = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];
            // }
        }

        mat_RGB.mainTexture = color_Tex;
        mat_RGB.SetTextureScale("_MainTex", new Vector2(1, -1));

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //-------- RGB Image -----
        if (color_Reader != null)
        {
            colorFrame = color_Reader.AcquireLatestFrame();
            if (colorFrame != null)
            {
                colorFrame.CopyConvertedFrameDataToArray(color_data, ColorImageFormat.Rgba);
                color_Tex.LoadRawTextureData(color_data);
                color_Tex.Apply();

                colorFrame.Dispose();
                colorFrame = null;
            }
        }
        //-----------------------body sensor

        if (body_Reader != null)
        {
            var body_frame = body_Reader.AcquireLatestFrame();
            if (body_frame != null)
            {
                if (BodyData == null) BodyData = new Body[KSence.BodyFrameSource.BodyCount];

                body_frame.GetAndRefreshBodyData(BodyData);

                i = 0;
                foreach (var body in BodyData)
                {
                    if (body.IsTracked)
                    {
                        if (body.Joints[JointType.Head].Position.Z < 3f && i < 3)
                        {
                            
                            lHand.position = Joint2Vec3(body.Joints[JointType.HandLeft]);
                            rHand.position = Joint2Vec3(body.Joints[JointType.HandRight]);

                        }
                        i++;
                    }
                }
                //print(i);
                //

                body_frame.GetAndRefreshBodyData(BodyData);
                body_frame.Dispose();
                body_frame = null;
            }
        }

        //************************************************ Game Control


        // if(Input.GetKeyDown(KeyCode.Q)){
        //     print("asas");
        //     heart_pos.position = heart_poses[0];
        // }

    }

        void OnApplicationQuit()
    {
        if (body_Reader != null)
        {
            body_Reader.Dispose();
            body_Reader = null;
        }

        if (color_Reader != null)
        {
            color_Reader.Dispose();
            color_Reader = null;
        }
        if (KSence.IsOpen)
        {
            KSence.Close();
            KSence = null;
        }
    }


    Vector3 Joint2Vec3(Windows.Kinect.Joint joint)
    {
        //pos_mul = (joint.Position.Z*10);
        vec3_tmp.Set(joint.Position.X * (pos_mul /joint.Position.Z), joint.Position.Y * (pos_mul /joint.Position.Z), 24f);
        return vec3_tmp;
    }
}
