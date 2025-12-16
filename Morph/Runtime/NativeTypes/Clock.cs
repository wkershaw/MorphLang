namespace Morph.Runtime.NativeTypes;

internal class Clock : MorphClass
{
	public Clock() : base("Clock", new Dictionary<string, IMorphInstanceCallable>())
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
		methods.Add("toFormattedString", new ToFormattedString());

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
			return new ClockInstance(_class);
		}
	}

	public class ToFormattedString : IMorphInstanceCallable
    {
        public int Arity => 1;

		IMorphCallable IMorphInstanceCallable.Bind(MorphInstance instance)
		{
			return this;
		}

		object? IMorphCallable.Call(Interpreter interpreter, List<object?> arguments)
		{
			string? format = arguments[0]?.ToString();

			if (format is null)
            {
                return DateTime.Now.ToString();
            }

			return DateTime.Now.ToString(format);
		}
    }

	public class ClockInstance : MorphInstance
	{
		public ClockInstance(MorphClass mClass) : base(mClass)
		{
		}


		public override string ToString()
		{
			return DateTime.Now.ToString();
		}
	}
}