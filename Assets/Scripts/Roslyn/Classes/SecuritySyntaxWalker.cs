using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using UnityEditor;
using System.Text.RegularExpressions;

namespace RoslynCSharp
{
    public class CompilerError
    {
        public int line;
        public string context;

    }

    public class SecuritySyntaxWalker : CSharpSyntaxWalker
    {
        private RoslynSettings _roslynSettings;
        private SemanticModel _model;

        private List<string> _namespaceToCheck;
        private List<string> _typeToCheck;
        private List<string> _memberToCheck;

        public SecuritySyntaxWalker(RoslynSettings roslynSettings, SemanticModel model)
        {
            this._roslynSettings = roslynSettings;
            this._model = model;

            _namespaceToCheck = _roslynSettings.prohibitedNamespace;
            _typeToCheck = _roslynSettings.prohibitedType;
            _memberToCheck = _roslynSettings.prohibitedMember;
        }

        // "using UnityEngine;"
        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            string nodeText = node.ToString();
            foreach (string ns in _namespaceToCheck)
            {
                if (Regex.Match(nodeText, ns).Success)
                {

                    return;
                }
            }
        }

        // "System.IO"
        public override void VisitQualifiedName(QualifiedNameSyntax node)
        {
            ITypeSymbol typeSymbol = _model.GetTypeInfo(node).Type;
            if (typeSymbol == null) return;
            string fullName = typeSymbol.ToString();

            foreach (string ns in _namespaceToCheck)
            {
                if (Regex.IsMatch(fullName, ns))
                {

                    return;
                }
            }
        }

        // "int", "string"
        public override void VisitPredefinedType(PredefinedTypeSyntax node)
        {
            string nodeText = node.ToString();
            foreach (string item in _typeToCheck)
            {
                
                return;
            }
        }

        // Custom class
        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            base.VisitIdentifierName(node);
        }

        // Method calls
        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            
        }
    }
}
