namespace Morph.Runtime.NativeTypes;

internal class Random : MorphClass
{
	public Random() : base("Random", new Dictionary<string, IMorphInstanceCallable>())
	{
		foreach (var method in GetMethods())
		{
			_methods.Add(method.Key, method.Value);
		}
	}

	private Dictionary<string, IMorphInstanceCallable> GetMethods()
	{
		var methods = new Dictionary<string, IMorphInstanceCallable>();

		methods.Add("init", new Constructor(this));
		methods.Add("between", new Between());

		return methods;
	}

	public class Constructor : IMorphInstanceCallable
	{
		public int Arity => 0;

		private MorphClass _class;

		public Constructor(MorphClass c)
		{
			_class = c;
		}

		IMorphCallable IMorphInstanceCallable.Bind(MorphInstance instance)
		{
			return this;
		}

		object? IMorphCallable.Call(Interpreter interpreter, List<object?> arguments)
		{
			return new RandomInstance(_class);
		}
	}

	public class Between : IMorphInstanceCallable
    {
		private RandomInstance? _instance;

        public int Arity => 2;

		IMorphCallable IMorphInstanceCallable.Bind(MorphInstance instance)
		{
			_instance = instance as RandomInstance;
			return this;
		}

		object? IMorphCallable.Call(Interpreter interpreter, List<object?> arguments)
		{
			if (!int.TryParse(arguments[0]?.ToString(), out int lowerBound))
            {
                throw new RuntimeException(null, "Argument must be an int");
            }

			if (!int.TryParse(arguments[1]?.ToString(), out int upperBound))
            {
                throw new RuntimeException(null, "Argument must be an int");
            }
			
			if (_instance is null)
            {
                throw new RuntimeException(null, "Instance not correctly bound");
            }

			return _instance.Random.Next(lowerBound, upperBound);
		}
    }

	public class RandomInstance : MorphInstance
	{
		public System.Random Random { get; set; }

		public RandomInstance(MorphClass mClass) : base(mClass)
        {
            Random = new System.Random();
        }
	}
}