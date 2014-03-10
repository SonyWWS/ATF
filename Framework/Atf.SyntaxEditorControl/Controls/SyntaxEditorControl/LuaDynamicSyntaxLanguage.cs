//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Reflection;

using ActiproSoftware.SyntaxEditor;
using ActiproSoftware.SyntaxEditor.Addons.Dynamic;

namespace Sce.Atf.Controls.SyntaxEditorControl
{
    internal class LuaDynamicSyntaxLanguage : DynamicOutliningSyntaxLanguage
    {
        /// <summary>
        /// Constructor</summary>
        /// <remarks>Adds code folding for the Lua language.</remarks>
        /// <param name="assembly">Assembly</param>
        /// <param name="resourceName">Resource name</param>
        public LuaDynamicSyntaxLanguage(Assembly assembly, string resourceName)
            : base(assembly, resourceName, 0)
        {
        }

        public override void GetTokenOutliningAction(TokenStream tokenStream, ref string outliningKey, ref OutliningNodeAction tokenAction)
        {
            // Get the token
            IToken token = tokenStream.Peek();
            if (token == null)
                return;

            if (string.IsNullOrEmpty(token.Key))
                return;

            // See if the token starts or ends an outlining node
            switch (token.Key)
            {
                //case "OpenCurlyBraceToken":
                //    outliningKey = "CodeBlock";
                //    tokenAction = OutliningNodeAction.Start;
                //    break;

                //case "CloseCurlyBraceToken":
                //    outliningKey = "CodeBlock";
                //    tokenAction = OutliningNodeAction.End;
                //    break;

                case ReservedWordToken:
                {
                    string tokenString = token.AutoCaseCorrectText;
                    if (string.IsNullOrEmpty(tokenString))
                        return;

                    switch (tokenString)
                    {
                        case "do":
                        //case "while": // while's also contain "do"
                        //case "for":   // for's also contain "do"
                        case "if":
                        case "repeat":
                        case "function":
                            outliningKey = "CodeBlock";
                            tokenAction = OutliningNodeAction.Start;
                            break;

                        case "until":
                        case "end":
                            outliningKey = "CodeBlock";
                            tokenAction = OutliningNodeAction.End;
                            break;
                    }
                }
                break;

                case MultiLineCommentStartToken:
                    outliningKey = "MultiLineComment";
                    tokenAction = OutliningNodeAction.Start;
                    break;

                case MultiLineCommentEndToken:
                    outliningKey = "MultiLineComment";
                    tokenAction = OutliningNodeAction.End;
                    break;
            }
        }

        private const string ReservedWordToken = "ReservedWordToken";
        private const string MultiLineCommentStartToken = "MultiLineCommentStartToken";
        private const string MultiLineCommentEndToken = "MultiLineCommentEndToken";
    }
}
