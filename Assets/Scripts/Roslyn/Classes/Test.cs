using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Reflection;
using TMPro;
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

using System.Text.RegularExpressions;

namespace RoslynCSharp
{
    public class Test : MonoBehaviour
    {
        [TextArea(10, 20), SerializeField]
        private string _myScriptNigga;

        [Button(ButtonSizes.Medium, Name = "Compile code")]
        public void Compile()
        {
            Compiler compiler = new Compiler();
            compiler.CompileFile(_myScriptNigga);
        }
    }
}
