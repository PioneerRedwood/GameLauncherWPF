// https://docs.microsoft.com/ko-kr/dotnet/standard/events/observer-design-pattern
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroguLauncher.Utils
{
	public abstract class Observer
	{
		public abstract void Update();
	}

	public abstract class Subject
	{
		private List<Observer> observers = new List<Observer>();

		public void Attach(Observer _observer)
		{
			observers.Add(_observer);
		}

		public void Detach(Observer _observer)
		{
			observers.Remove(_observer);
		}

		public void Notify()
		{
			foreach (Observer observer in observers)
			{
				observer.Update();
			}
		}
	}
}
