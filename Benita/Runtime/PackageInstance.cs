namespace Benita
{
    /// <summary>
    /// Represents an instance of a package within the interpreter.
    /// </summary>
    internal class PackageInstance
    {
        public PackageNode InstancePackageNode { get; set; }
        private readonly Interpreter _interpreter;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstance"/> class.
        /// </summary>
        /// <param name="instanceName">The name of the instance.</param>
        /// <param name="packageNode">The package node that defines the package.</param>
        /// <param name="arguments">The arguments for the constructor, if any.</param>
        /// <param name="debugMode"></param>
        public PackageInstance(string instanceName, PackageNode packageNode, IReadOnlyList<object?> arguments,
            bool debugMode, RuntimeContext context)
        {
            _interpreter = new Interpreter(debugMode, instanceName, context: context);

            string? initializerName = null;
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
                    if (method.Name == "init")
                        initializerName = "init";
                    else if (method.Name == packageNode.Name && initializerName is null)
                        initializerName = packageNode.Name;
                    var functionNode = new FunctionNode(method.Name, method.Parameters, method.ReturnType, method.Body, method.ReturnStatement);
                    _interpreter.Visit(functionNode);
                }
            }
            _interpreter.MarkCurrentVariablesAsPersistent();
            _interpreter.SetGlobalVariable();

            // Execute constructor if it exists
            if (initializerName is not null)
            {
                var functionCallNode = new FunctionCallNode(initializerName,
                    arguments.Select(argument => (ExpressionNode)new RuntimeValueNode(argument)).ToList());
                _interpreter.Visit(functionCallNode);
            }
        }

        /// <summary>
        /// Visits the specified initializer node within the interpreter's context.
        /// </summary>
        /// <param name="initializer">The initializer node to visit.</param>
        /// <param name="outerScopeVariables">The variables in the outer scope to be used.</param>
        /// <returns>The result of the visit operation.</returns>
        public object Visit(AstNode? initializer, Dictionary<string, object> outerScopeVariables)
        {
            _interpreter.SetOuterScopeVariables(outerScopeVariables);
            return _interpreter.Visit(initializer);
        }

        /// <summary>state داخلی package را در snapshot تراکنش REPL ثبت می‌کند.</summary>
        internal void CaptureCheckpoint(Interpreter.InterpreterState state,
            Interpreter.TransactionCheckpoint checkpoint) => _interpreter.CaptureState(state, checkpoint);

        /// <summary>state داخلی package را پس از شکست submission بازیابی می‌کند.</summary>
        internal void RestoreCheckpoint(Interpreter.InterpreterState state,
            Interpreter.TransactionCheckpoint checkpoint) => _interpreter.RestoreState(state, checkpoint);
    }
}
