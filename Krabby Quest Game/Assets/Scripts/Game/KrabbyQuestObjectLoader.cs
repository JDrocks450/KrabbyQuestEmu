using StinkyFile;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KrabbyQuestObjectLoader : MonoBehaviour
{
    public enum KQLoaderMode
    {
        Name,
        GUID
    }
    public string BlockName, BlockGUID;
    public KQLoaderMode Mode;
    public bool AutoLoad = true;
    public bool RemoveAnimation = true;

    // Start is called before the first frame update
    void Start()
    {
        if (AutoLoad)
            Load();
    }

    private void Load()
    {
        GameInitialization.Initialize();
        LevelDataBlock block = default;
        switch (Mode)
        {
            case KQLoaderMode.GUID:
                block = LevelDataBlock.LoadFromDatabase(BlockGUID, out bool success);
                break;
            case KQLoaderMode.Name:
                block = LevelDataBlock.LoadFromDatabase(BlockName);
                break;
        }
        if (block != null)
        {
            var newObject = LevelObjectManager.CreateKrabbyQuestObject(block);
            newObject.SetActive(true);
            var lTransform = newObject.transform;
            var scale = newObject.transform.localScale;
            lTransform.parent = transform;
            lTransform.localPosition = new Vector3(0, 0, 0);
            lTransform.localRotation = new Quaternion();
            lTransform.localScale = scale;
            if (RemoveAnimation)
            {
                var anim = newObject.GetComponentInChildren<Animator>();
                if (anim != null)
                    Destroy(anim);
            }
            newObject.name = block.Name;            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
