using Morph.Runtime.Functions;
using Morph.Runtime.OOP.Interfaces;

namespace Morph.Runtime.OOP
{
    internal class MethodSet
    {
		private readonly string _name;
		private readonly Dictionary<int, IMorphMethod> _overloads;

		public MethodSet(string name)
		{
			_name = name;
			_overloads = new Dictionary<int, IMorphMethod>();
		}

		public void AddOverload(IMorphMethod method)
		{
			if (_overloads.ContainsKey(method.Arity))
			{
				throw new InvalidOperationException($"Method named {_name} that takes {method.Arity} arguments already exists.");
			}

			_overloads[method.Arity] = method;
		}

		public FunctionSet Bind(MorphInstance instance)
		{
			var set = new FunctionSet(_name);

			foreach (var overload in _overloads.Values)
			{
				set.AddOverload(overload.Bind(instance));
			}

			return set;
		}
	}
}
