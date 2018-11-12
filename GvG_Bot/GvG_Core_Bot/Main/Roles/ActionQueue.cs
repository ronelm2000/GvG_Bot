using System;
using System.Collections.Generic;
using System.Text;

namespace GvG_Core_Bot.Main.Roles
{
    class ActionQueue
    {
		List<Action> list = new List<Action>();

		public ActionQueue () { }

		public void Dequeue ()
		{
			for (int i = 0; i < list.Count; i++ )
			{
				list[i]();
			}
			list.RemoveRange(0, list.Count);
		}

		public static ActionQueue operator + (ActionQueue thisQueue, Action disAction)
		{
			thisQueue.list.Add(disAction);
			return thisQueue;
		}
    }
}
