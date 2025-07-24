using UnityEngine;

public static class Vector3Extensions
{
    /// <summary>
    /// 返回一个新的 Vector3，其中 y 分量为 0（即水平分量）
    /// </summary>
    public static Vector3 Horizontal3D(this Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }
}
