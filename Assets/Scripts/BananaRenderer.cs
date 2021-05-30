using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

public class BananaRenderer : MonoBehaviour
{
    [SerializeField]
    private float _bananaRad;

    [SerializeField]
    private int _numWidth;

    [SerializeField]
    private int _numHeight;

    [SerializeField]
    private int _numDepth;

    [SerializeField]
    private Vector3 _min;

    [SerializeField]
    private Vector3 _max;

    [SerializeField]
    private float _scale;

    [SerializeField]
    private Material _mat;

    [SerializeField]
    private Camera _cam;

    [SerializeField]
    private List<Lod> _lods;

    private List<Matrix4x4[]> _matrices;
    private List<int> _batchNums;
    private int _prevNumWidth=0;
    private int _numPerBatch=1023;

    private PosJob _job;
    private JobHandle _handle;
    private Plane[] _planes;

    private Matrix4x4[] _tempMatrixArr;
    private NativeList<Matrix4x4>[] _tempMatrixBufferArr;



    private void Update()
    {
        for (int i = 0; i < _lods.Count; i++)
        {
            var numBatches=_lods[i].BatchCount;
            var count=0;
            for (int j = 0; j < numBatches; j++)
            {
                var numInBatch=j==numBatches-1?_lods[i].NumInLastBatch:1023;
                Graphics.DrawMeshInstanced(_lods[i].Mesh, 0, _mat, _lods[i].Matrices[j], numInBatch);
                count += numInBatch;
            }
            //Debug.Log("Drew lod: " + i + " num batches: " + numBatches + " num instances: " + count);
        }

    }

    void LateUpdate()
    {
        //_handle.IsCompleted
        if (true)
        {
            _handle.Complete();

            if (_tempMatrixBufferArr == null)
                _tempMatrixBufferArr = new NativeList<Matrix4x4>[10];

            _tempMatrixBufferArr[0] = _job.MatricesLod0;
            _tempMatrixBufferArr[1] = _job.MatricesLod1;
            _tempMatrixBufferArr[2] = _job.MatricesLod2;
            _tempMatrixBufferArr[3] = _job.MatricesLod3;
            _tempMatrixBufferArr[4] = _job.MatricesLod4;
            _tempMatrixBufferArr[5] = _job.MatricesLod5;
            _tempMatrixBufferArr[6] = _job.MatricesLod6;
            _tempMatrixBufferArr[7] = _job.MatricesLod7;
            _tempMatrixBufferArr[8] = _job.MatricesLod8;
            _tempMatrixBufferArr[9] = _job.MatricesLod9;

            if (_tempMatrixArr == null)
                _tempMatrixArr = new Matrix4x4[_numWidth * _numHeight * _numDepth];

            for (int i = 0; i < _lods.Count; i++)
            {
                var currMatrixList=_tempMatrixBufferArr[i];
                if (!currMatrixList.IsCreated)
                {
                    _lods[i].BatchCount = 0;
                    _lods[i].NumInLastBatch = 0;
                    continue;
                }

                if (currMatrixList.Length == 0)
                {
                    _lods[i].BatchCount = 0;
                    _lods[i].NumInLastBatch = 0;
                    continue;
                }


                var num=currMatrixList.Length;
                if (num == 0)
                {
                    _lods[i].BatchCount = 0;
                    _lods[i].NumInLastBatch = 0;
                    continue;
                }

                var numBatches=(int)Mathf.Ceil((float)num/1023);
                _lods[i].BatchCount = numBatches;
                var numInLastBatch=num-((numBatches-1)*1023);
                _lods[i].NumInLastBatch = numInLastBatch;

                if (_lods[i].Matrices == null)
                    _lods[i].Matrices = new List<Matrix4x4[]>();
                var createdBatchCount=_lods[i].Matrices.Count;
                if (createdBatchCount < numBatches)
                {
                    for (int j = 0; j < numBatches - createdBatchCount; j++)
                    {
                        _lods[i].Matrices.Add(new Matrix4x4[1023]);
                    }
                    //Debug.Log("Num batches in lod: " + i + " is: " + _lods[i].Matrices.Count);
                }
                SetNativeMatrixArray(_tempMatrixArr, currMatrixList);
                for (int j = 0; j < numBatches; j++)
                {
                    var numInBatch=j==numBatches-1?numInLastBatch:1023;
                    Array.Copy(_tempMatrixArr, j * 1023, _lods[i].Matrices[j], 0, numInBatch);
                }
            }

            if (_job.NumWidth != _numWidth||_job.NumHeight != _numHeight||_job.NumDepth != _numDepth||_min!=_job.Min||_max!=_job.Max)
            {
                ClearJob();
                Debug.Log("Created new job");
                _job = new PosJob()
                {
                    FrustrumPlanes = new NativeArray<Plane>(6, Allocator.Persistent),
                    CameraPos = new NativeArray<Vector3>(1, Allocator.Persistent),
                    Lod = new NativeArray<int>(_numWidth * _numHeight * _numDepth, Allocator.Persistent),
                    NumLods = _lods.Count,
                    Min = _min,
                    Max = _max,
                    NumWidth = _numWidth,
                    NumHeight = _numHeight,
                    NumDepth = _numDepth,
                    BananaRad = _bananaRad,
                    MatricesLod0 = new NativeList<Matrix4x4>(Allocator.Persistent),
                    MatricesLod1 = new NativeList<Matrix4x4>(Allocator.Persistent),
                    MatricesLod2 = new NativeList<Matrix4x4>(Allocator.Persistent),
                    MatricesLod3 = new NativeList<Matrix4x4>(Allocator.Persistent),
                    MatricesLod4 = new NativeList<Matrix4x4>(Allocator.Persistent),
                    MatricesLod5 = new NativeList<Matrix4x4>(Allocator.Persistent),
                    MatricesLod6 = new NativeList<Matrix4x4>(Allocator.Persistent),
                    MatricesLod7 = new NativeList<Matrix4x4>(Allocator.Persistent),
                    MatricesLod8 = new NativeList<Matrix4x4>(Allocator.Persistent),
                    MatricesLod9 = new NativeList<Matrix4x4>(Allocator.Persistent),
                    LodDistances = new NativeArray<float>(_lods.Count, Allocator.Persistent),
                };
                for (int i = 0; i < _numWidth * _numHeight * _numDepth; i++)
                {
                    _job.Lod[i] = _lods.Count;
                }

            }
            for (int i = 0; i < _lods.Count; i++)
            {
                _job.LodDistances[i] = _lods[i].Distance;
            }
            _job.CameraPos[0] = _cam.transform.position;

            if (_planes == null)
                _planes = new Plane[6];

            var fov=_cam.fieldOfView;
            _cam.fieldOfView = fov * 1.33f;
            GeometryUtility.CalculateFrustumPlanes(_cam, _planes);
            _cam.fieldOfView = fov;

            for (int i = 0; i < 6; i++)
            {
                _job.FrustrumPlanes[i] = _planes[i];
            }

            _handle = _job.Schedule();
        }
    }

    void ClearJob()
    {
        Debug.Log("Disposed job data");
        _handle.Complete();
        if (_job.FrustrumPlanes.IsCreated)
            _job.FrustrumPlanes.Dispose();
        if (_job.CameraPos.IsCreated)
            _job.CameraPos.Dispose();
        if (_job.Lod.IsCreated)
            _job.Lod.Dispose();
        if (_job.MatricesLod0.IsCreated)
            _job.MatricesLod0.Dispose();
        if (_job.MatricesLod1.IsCreated)
            _job.MatricesLod1.Dispose();
        if (_job.MatricesLod2.IsCreated)
            _job.MatricesLod2.Dispose();
        if (_job.MatricesLod3.IsCreated)
            _job.MatricesLod3.Dispose();
        if (_job.MatricesLod4.IsCreated)
            _job.MatricesLod4.Dispose();
        if (_job.MatricesLod5.IsCreated)
            _job.MatricesLod5.Dispose();
        if (_job.MatricesLod6.IsCreated)
            _job.MatricesLod6.Dispose();
        if (_job.MatricesLod7.IsCreated)
            _job.MatricesLod7.Dispose();
        if (_job.MatricesLod8.IsCreated)
            _job.MatricesLod8.Dispose();
        if (_job.MatricesLod9.IsCreated)
            _job.MatricesLod9.Dispose();
        if (_job.LodDistances.IsCreated)
            _job.LodDistances.Dispose();
    }

    private void OnDisable()
    {
        ClearJob();
    }

    unsafe void SetNativeMatrixArray(Matrix4x4[] matArray, NativeList<Matrix4x4> matBuffer)
    {
        fixed (void* vertexArrayPointer = matArray)
        {
            UnsafeUtility.MemCpy(vertexArrayPointer, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(matBuffer.AsArray()), matBuffer.Length * (long)(UnsafeUtility.SizeOf<float>() * 16));
        }
    }
}
