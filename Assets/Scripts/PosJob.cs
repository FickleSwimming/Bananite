using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct PosJob : IJob
{
    public int NumLods;
    public int NumWidth;
    public int NumHeight;
    public int NumDepth;

    public Vector3 Min;
    public Vector3 Max;
    public float BananaRad;
    [ReadOnly]
    public NativeArray<Vector3> CameraPos;

    [ReadOnly]
    public NativeArray<Plane> FrustrumPlanes;

    [ReadOnly]
    public NativeArray<float> LodDistances;

    [WriteOnly]
    public NativeList<Matrix4x4> MatricesLod0;
    [WriteOnly]
    public NativeList<Matrix4x4> MatricesLod1;
    [WriteOnly]
    public NativeList<Matrix4x4> MatricesLod2;
    [WriteOnly]
    public NativeList<Matrix4x4> MatricesLod3;
    [WriteOnly]
    public NativeList<Matrix4x4> MatricesLod4;
    [WriteOnly]
    public NativeList<Matrix4x4> MatricesLod5;
    [WriteOnly]
    public NativeList<Matrix4x4> MatricesLod6;
    [WriteOnly]
    public NativeList<Matrix4x4> MatricesLod7;
    [WriteOnly]
    public NativeList<Matrix4x4> MatricesLod8;
    [WriteOnly]
    public NativeList<Matrix4x4> MatricesLod9;

    public NativeArray<int> Lod;


    public void Execute()
    {
        MatricesLod0.Clear();
        MatricesLod1.Clear();
        MatricesLod2.Clear();
        MatricesLod3.Clear();
        MatricesLod4.Clear();
        MatricesLod5.Clear();
        MatricesLod6.Clear();
        MatricesLod7.Clear();
        MatricesLod8.Clear();
        MatricesLod9.Clear();

        var totalNum=NumWidth*NumHeight*NumDepth;

        for (int i = 0; i < totalNum; i++)
        {
            var x=i%NumWidth;
            var y=(i/NumWidth)%NumHeight;
            var z=(i/(NumWidth*NumHeight));

            var xL=(float)x/(NumWidth-1);
            var yL=(float)NumHeight==1?1:y/(NumHeight-1);
            var zL=(float)z/(NumDepth-1);

            var pos=Min+new Vector3(xL*(Max.x-Min.x),yL*(Max.y-Min.y),zL*(Max.z-Min.z));

            var inFrustrum=true;
            for (int j = 0; j < 6; j++)
            {
                var frustDst=FrustrumPlanes[j].GetDistanceToPoint(pos);
                if (frustDst < -BananaRad)
                {
                    inFrustrum = false;
                    break;
                }
            }
            if (!inFrustrum)
                continue;

            float dst=Vector3.Distance(pos,CameraPos[0]);
            int lodIndex=Lod[i];
            if (lodIndex < NumLods - 1 && dst > LodDistances[lodIndex])
                lodIndex++;
            if (lodIndex > 0 && dst <= LodDistances[lodIndex - 1])
                lodIndex--;
            Lod[i] = lodIndex;

            switch (lodIndex)
            {
                case 0:
                    MatricesLod0.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    break;
                case 1:
                    MatricesLod1.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    break;
                case 2:
                    MatricesLod2.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    break;
                case 3:
                    MatricesLod3.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    break;
                case 4:
                    MatricesLod4.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    break;
                case 5:
                    MatricesLod5.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    break;
                case 6:
                    MatricesLod6.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    break;
                case 7:
                    MatricesLod7.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    break;
                case 8:
                    MatricesLod8.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    break;
                case 9:
                    MatricesLod9.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));
                    break;
            }
        }
    }
}
