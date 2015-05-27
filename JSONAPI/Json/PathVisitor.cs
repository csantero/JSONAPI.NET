using System.Collections.Generic;
using System.Linq.Expressions;

namespace JSONAPI.Json
{
    internal class PathVisitor : ExpressionVisitor
    {
        private readonly Stack<string> _segments = new Stack<string>();
        public string Path { get { return string.Join(".", _segments.ToArray()); } }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Select")
            {
                Visit(node.Arguments[1]);
                Visit(node.Arguments[0]);
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _segments.Push(node.Member.Name);

            return base.VisitMember(node);
        }
    }

}
