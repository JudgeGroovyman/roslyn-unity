using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace RoslynCSharp
{
    [CreateAssetMenu(fileName ="RoslynSettings", menuName = "RoslynCSharp/Create a RoslynCSharp Setting")]
    public class RoslynSettings : ScriptableObject
    {
        [TabGroup("Compiler")]
        [Title("Compiler settings")]
        public OptimizationLevel optimizationLevel = OptimizationLevel.Release;

        [TabGroup("Compiler")]
        public bool allowUnsafeCode = false;

        [TabGroup("Compiler")]
        public bool allowConcurrentCompile = false;

        [TabGroup("Compiler")]
        public bool generateInMemory = true;

        [Space(20)]
        [TabGroup("Compiler"), LabelText("Assembly References")]
        [PropertyTooltip("Assembly References file name, with extension (.dll)")]
        public List<string> assemblyName = new List<string>() { "UnityEngine.dll", "System.Collections.dll", "System.Collections.Generic.dll"};

        [TabGroup("Security")]
        [LabelText("Security Check Code")]
        [InfoBox("Without security check, any script will be compiled. User's script shouldn't be compiled without security check or the security of your program could be compromised.", InfoMessageType.Warning, VisibleIf = "_")]
        public bool shouldWeCheckSecurity = true;
        
        [HideInInspector]
        public bool _
        {
            get
            {
                return !shouldWeCheckSecurity;
            }
        }

        [TabGroup("Security")]
        [Title("Namespace Reference Restriction")]
        [Space(10)]
        public List<string> prohibitedNamespace = new List<string>() { "System.IO.*", "System.Reflection.*" };

        [TabGroup("Security")]
        [Title("Type Reference Restriction")]
        [Space(0)]
        public List<string> prohibitedType = new List<string>() { "System.AppDomain" };

        [TabGroup("Security")]
        [Title("Member Reference Restriction")]
        [Space(10)]
        public List<string> prohibitedMember = new List<string>() { "UnityEngine.Application.Quit" };
    }
}
