using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVariable
{
    public static AttachmentTypeList dummyAttachmentTypeList = new();
    public static AttachmentDictionary dummyAttachmentDict = new();

    public static AttachmentTypeList DummyAttachmentTypeList
    {
        get
        {
            dummyAttachmentTypeList.Clear();
            return dummyAttachmentTypeList;
        }
        set { dummyAttachmentTypeList = value; }
    }

    public static AttachmentDictionary DummyAttachmentDict
    {
        get
        {
            dummyAttachmentDict.Clear();
            return dummyAttachmentDict;
        }
        set { dummyAttachmentDict = value; }
    }
}
