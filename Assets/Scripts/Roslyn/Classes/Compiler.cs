using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Text;

namespace RoslynCSharp
{
    public class Compiler : IDisposable
    {
        private const string DefaultDomainName = "Script Domain #";
        private static int _compilerIndex = 0;

        #region AppDomain Fields
        private string _scriptDomainName;
        private AppDomain _scriptDomain;
        private AppDomainProxy _scriptDomainProxy;
        #endregion

        #region Compiler Settings
        private RoslynSettings _roslynSettings;
        private CSharpCompilationOptions _compilationOptions;
        private List<MetadataReference> _metadataReferences = new List<MetadataReference>();
        private List<MetadataReference> GetMetadataReferences
        {
            get
            {
                if (_metadataReferences != null) return _metadataReferences;
                for (int i = 0; i < _roslynSettings.assemblyName.Count; i++)
                {
                    string assemblyLocation = Assembly.Load(_roslynSettings.assemblyName[i]).Location;
                    MetadataReference reference = MetadataReference.CreateFromFile(assemblyLocation);
                    _metadataReferences.Add(reference);
                }
                return _metadataReferences;
            }
        }
        #endregion

        /// <summary>
        /// Compile C# Script at runtime inside a remote AppDomain allowing hot-reloading.
        /// </summary>
        public Compiler()
        {
            InitCompiler(DefaultDomainName + _compilerIndex);
        }

        /// <summary>
        /// Compile C# Script at runtime inside a remote AppDomain allowing hot-reloading.
        /// </summary>
        /// <param name="appDomainName">The name of the remote AppDomain</param>
        public Compiler(string appDomainName)
        {
            InitCompiler(appDomainName);
        }

        /// <summary>
        /// Create an AppDomain and get settings for the compiler.
        /// </summary>
        /// <param name="appDomainName">Name of the created AppDomain.</param>
        private void InitCompiler(string appDomainName)
        {
            //_scriptDomain = AppDomain.CreateDomain(appDomainName);
            _compilerIndex++;
            _scriptDomainName = appDomainName;

            _roslynSettings = Resources.Load<ScriptableObject>("ScriptableObject/RoslynSettings") as RoslynSettings;
            _compilationOptions = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary).WithOptimizationLevel(_roslynSettings.optimizationLevel).WithAllowUnsafe(_roslynSettings.allowUnsafeCode).WithConcurrentBuild(_roslynSettings.allowConcurrentCompile);

            if (_roslynSettings != null)
            {
                Debug.Log("#Compiler# Roslyn Settings found !");
            }
            else
            {
                Debug.LogError("#Compiler# Roslyn Settings not found !");
            }
        }

        /// <summary>
        /// Compile a C# Script and execute it inside a remote AppDomain.
        /// NOTE : The compiled script can't have dependencies to other scripts, otherwise, you should use the following function  CompileFiles or CompileFolder.
        /// </summary>
        /// <param name="code">Script code</param>
        public bool CompileFile(string script)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script);
            IEnumerable<SyntaxNode> syntaxNodes = syntaxTree.GetRoot().DescendantNodes();
            CompilationUnitSyntax compilationUnit = syntaxTree.GetCompilationUnitRoot();
            Compilation compilation = CSharpCompilation.Create(GUID.Generate().ToString(), new[] { syntaxTree }, GetMetadataReferences, _compilationOptions);
            SemanticModel model = compilation.GetSemanticModel(syntaxTree);

           /* foreach (var item in syntaxNodes)
            {
                var type = model.GetTypeInfo(item).Type;
                Debug.Log("Text :" + item.ToString() + " \n Type :" + (type != null ? type.ToString() : "") + " \n SyntaxNode Type :" + item.GetType());
            }*/

            //return false;
            SecuritySyntaxWalker securitySyntaxWalker = new SecuritySyntaxWalker(_roslynSettings, model);
            securitySyntaxWalker.Visit(compilationUnit);
            return false;


            using (MemoryStream ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success) return false;
                return true;
            }
            return false;
        }

        ~Compiler()
        {
            ReleaseUnmanagedResources();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources()
        {
            _scriptDomainProxy?.Dispose();
            if (_scriptDomain == null) return;
            try { AppDomain.Unload(_scriptDomain); }
            catch { }
        }

        /// <summary>
        /// Unload all assemblies inside the domain.
        /// </summary>
        public void ResetDomain()
        {
            if (_scriptDomain != null)
            {
                AppDomain.Unload(_scriptDomain);
            }
            _scriptDomain = AppDomain.CreateDomain(_scriptDomainName);
        }
    }
}
