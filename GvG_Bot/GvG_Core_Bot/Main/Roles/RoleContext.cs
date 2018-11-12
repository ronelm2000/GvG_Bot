using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GvG_Core_Bot.Main.Roles
{
    public class RoleContext
    {
    
    }

	public delegate Task RoleEvent(RoleContext context);

	public enum RoleContextType
    {
        Public,
        Channel,
        Private
    }
}
