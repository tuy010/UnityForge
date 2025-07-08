using Tuy.UnityForge.Base;
using UnityEngine;

public class HumanoidBot : HumanoidBase
{
    #region Unity
    void Start()
    {
        Init();
    }

    void Update()
    {
        UpdateInfo();
    }
    #endregion
}
