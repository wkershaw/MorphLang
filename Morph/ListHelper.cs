using System.Diagnostics;

namespace Morph
{
	[DebuggerDisplay("Current = {Current}")]
	internal class ListHelper<T>
	{
		private readonly IReadOnlyList<T> _list;
		private readonly T _emptyUnit;

		private int _currentIndex;
		private int _rangeStartIndex;

		public ListHelper(IReadOnlyList<T> list, T emptyUnit)
		{
			ArgumentNullException.ThrowIfNull(list);

			_list = list;
			_currentIndex = -1;
			_rangeStartIndex = 0;
			_emptyUnit = emptyUnit;
		}

		public int CurrentIndex => _currentIndex;

		public T Current => GetCurrent();

		public T Previous => GetPrevious();

		public bool MoveNext()
		{
			_currentIndex++;

			return !IsAtEnd();
		}

		public bool IsAtEnd()
		{
			return _currentIndex >= _list.Count;
		}

		private T GetCurrent()
		{
			if (_currentIndex < 0)
			{
				throw new InvalidOperationException("MoveNext() must be called at least once before accessing current value");
			}

			if (IsAtEnd())
			{
				return _emptyUnit;
			}

			return _list[_currentIndex];
		}

		private T GetPrevious()
		{
			if (_currentIndex - 1 < 0)
			{
				return _emptyUnit;
			}

			return _list[_currentIndex - 1];
		}

		public T Peek()
		{
			if (_currentIndex + 1 >= _list.Count)
			{
				return _emptyUnit;
			}

			return _list[_currentIndex + 1];
		}

		public T PeekNext()
		{
			if (_currentIndex + 2 >= _list.Count)
			{
				return _emptyUnit;
			}

			return _list[_currentIndex + 2];
		}

		public bool Match<U>(Func<T, U> selector, params IEnumerable<U> matches)
		{
			if (IsAtEnd())
			{
				return false;
			}

			return matches.Contains(selector(Current));
		}

		public T Consume(Predicate<T> predicate, string errorMessage)
		{
			if (!IsAtEnd() && predicate(Current))
			{
				var value = Current;
				MoveNext();
				return value;
			}

			//TODO: Better error type here
			throw new InvalidOperationException(errorMessage);
		}

		public T Consume()
		{ 
			var value = Current;
			MoveNext();
			return value;
		}

		public bool MatchAndConsume<U>(Func<T, U> selector, params IEnumerable<U> matches)
		{
			if (Match(selector, matches))
			{
				MoveNext();
				return true;
			}

			return false;
		}

		public void StartRange()
		{
			if (_currentIndex < 0)
			{
				throw new InvalidOperationException("MoveNext() must be called at least once before starting a range");
			}

			_rangeStartIndex = _currentIndex;
		}

		public T[] GetRange()
		{
			int rangeLength = _currentIndex - _rangeStartIndex + 1;

			if (rangeLength <= 0)
			{
                return Array.Empty<T>();
			}

			T[] range = new T[rangeLength];
			for (int i = 0; i < rangeLength; i++)
			{
				range[i] = _list[_rangeStartIndex + i];
			}

			return range;
		}
	}
}
