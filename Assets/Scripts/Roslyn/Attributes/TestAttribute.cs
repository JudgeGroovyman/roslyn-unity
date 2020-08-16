using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Class)]
public class TestAttribute : Attribute
{
    public TestAttribute(Type type)
    {
        return;
    }
}
