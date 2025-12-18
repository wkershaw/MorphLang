using Morph.Runtime.Functions.Interfaces;

namespace Morph.Runtime.Functions
{
    internal class FunctionSet
	{
		private readonly string _name;
		private readonly Dictionary<int, IMorphFunction> _overloads;

		public FunctionSet(string name)
		{
			_name = name;
			_overloads = new Dictionary<int, IMorphFunction>();
		}

		public void AddOverload(IMorphFunction function)
		{
			if (_overloads.ContainsKey(function.Arity))
			{
				throw new InvalidOperationException($"Method named {_name} that takes {function.Arity} arguments already exists.");
			}

			_overloads[function.Arity] = function;
		}

		public IMorphFunction? TryFindOverload(int arity)
		{
			if (_overloads.TryGetValue(arity, out IMorphFunction? function))
			{
				return function;
			}

			return null;
		}

		public IMorphFunction GetOverload(int arity)
		{
			if (_overloads.TryGetValue(arity, out IMorphFunction? function))
			{
				return function;
			}

			throw new InvalidOperationException($"No overload for function {_name} that takes {arity} arguments exists.");
		}
	}
}
