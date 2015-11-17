//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// A DomNodeAdapter registered on root type like circuit type.
    /// This adapter provides access to list of expression currently defined.
    /// </summary>
    class ExpressionManager : DomNodeAdapter
    {
        protected override void OnNodeSet()
        {
            if (!Schema.circuitType.Type.IsAssignableFrom(DomNode.Type))
                throw new Exception("can attatch only to circuit");

            DomNode.AttributeChanged += (sender, e) =>
            {
                if (!m_running)
                {
                    var key = CreateKey(e.DomNode, e.AttributeInfo);
                    HashSet<string> exprset;
                    if (m_sourceAttributeMap.TryGetValue(key, out exprset))
                    {
                        m_expressionSet.UnionWith(exprset);
                        return;
                    }
                    var expr = e.DomNode.As<Expression>();
                    if (expr != null) m_expressionSet.Add(expr.Id);
                }
            };

            DomNode.ChildInserted += (sender, e) =>
                {
                    var expr = e.Child.As<Expression>();
                    if (e.Parent == DomNode && expr != null) 
                        m_expressionSet.Add(expr.Id);
                };

            Expressions = GetChildList<Expression>(Schema.circuitType.expressionChild);
            foreach (var expression in Expressions)
                m_expressionSet.Add(expression.Id);
        }

        public IList<Expression> Expressions
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets enumerable of all the nodes 
        /// that can be used as source or target of an expression.        
        /// </summary>
        public IEnumerable<DomNode> ExpressionNodes
        {
            get { return DomNode.GetChildList(Schema.circuitType.moduleChild); }
        }


        public Expression CreateExpression()
        {
            Expression exp = new DomNode(Schema.expressionType.Type).Cast<Expression>();
            exp.Id = "Expression";
            exp.Label = "Expression".Localize();
            return exp;
        }

        /// <summary>
        /// Observe the passed attribute on the given node.
        /// if the attribute changed then expression needs to run.
        /// </summary>        
        public void ObserveAttribute(DomNode node, AttributeInfo attrib)
        {
            if (m_running)
            {
                string key = CreateKey(node, attrib);
                HashSet<string> exprSet;
                if (!m_sourceAttributeMap.TryGetValue(key, out exprSet))
                {
                    exprSet = new HashSet<string>();
                    m_sourceAttributeMap.Add(key, exprSet);
                }
                exprSet.Add(m_runningExpressionId);
            }
        }

        private string CreateKey(DomNode node, AttributeInfo attrib)
        {
            return string.Format("{0}.{1}", node.GetId(), attrib.Name);
        }

        /// <summary>
        /// Runs expression if needed.
        /// </summary>
        public void Update(bool runAll = false)
        {
            if ((!runAll && m_expressionSet.Count == 0) || m_running)
                return;

            try
            {
                m_running = true;
                m_sourceAttributeMap.Clear();

                foreach (var expression in Expressions)
                {
                    if (runAll || m_expressionSet.Contains(expression.Id))
                    {
                        m_runningExpressionId = expression.Id;
                        m_pythonService.ExecuteStatements(expression.Script);
                    }
                }
            }
            finally
            {
                m_running = false;
                m_runningExpressionId = null;
                m_expressionSet.Clear();
            }
        }

        private bool m_running;

        // the id of the currently running expression.
        // if the expression is running then it is null.
        private string m_runningExpressionId;


        public void SetPythonScriptService(ScriptingService scriptService)
        {
            if (m_pythonService != null) return; // it is already set.            
            m_pythonService = (BasicPythonService)scriptService;
        }


        // holds set of expression ids that need to be run.
        private HashSet<string> m_expressionSet = new HashSet<string>();

        // maps source attributes to set of expression ids
        // if any of the source attribute changed then all the expressions in the set need to run.
        private Dictionary<string, HashSet<string>> m_sourceAttributeMap = new Dictionary<string, HashSet<string>>();
        private BasicPythonService m_pythonService;
    }
}
