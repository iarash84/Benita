namespace Benita
{
    /// <summary>
    /// Represents an instance of a package within the interpreter.
    /// </summary>
    internal class PackageInstance
    {
        private readonly Interpreter _interpreter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstance"/> class.
        /// </summary>
        /// <param name="instanceName">The name of the instance.</param>
        /// <param name="packageNode">The package node that defines the package.</param>
        /// <param name="arguments">The arguments for the constructor, if any.</param>
        public PackageInstance(string instanceName, PackageNode packageNode, List<ExpressionNode?> arguments)
        {
            _interpreter = new Interpreter(instanceName);

            bool hasConstructor = false;
            foreach (var member in packageNode.Members)
            {
                if (member is PackageVariableDeclarationNode field)
                {
                    var packageVariableDeclarationNode =
                        new VariableDeclarationNode(field.Type, field.Name, field.Initializer);
                    _interpreter.Visit(packageVariableDeclarationNode);
                }
                else if (member is PackageFunctionNode method)
                {
                    if (method.Name == packageNode.Name && !hasConstructor)
                        hasConstructor = true;
                    var functionNode = new FunctionNode(method.Name, method.Parameters, method.ReturnType, method.Body, method.ReturnStatement);
                    _interpreter.Visit(functionNode);
                }
            }
            _interpreter.SetGlobalVariable();

            // Execute constructor if it exists
            if (hasConstructor)
            {
                var functionCallNode = new FunctionCallNode(packageNode.Name, arguments);
                _interpreter.Visit(functionCallNode);
            }
        }

        /// <summary>
        /// Visits the specified initializer node within the interpreter's context.
        /// </summary>
        /// <param name="initializer">The initializer node to visit.</param>
        /// <param name="outerScopeVariables">The variables in the outer scope to be used.</param>
        /// <returns>The result of the visit operation.</returns>
        public object Visit(AstNode? initializer, Dictionary<string?, object> outerScopeVariables)
        {
            _interpreter.SetOuterScopeVariables(outerScopeVariables);
            return _interpreter.Visit(initializer);
        }
    }
}
