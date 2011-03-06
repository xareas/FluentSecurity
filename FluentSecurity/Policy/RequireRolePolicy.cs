using System;
using System.Linq;
using System.Text;

namespace FluentSecurity.Policy
{
	public class RequireRolePolicy : ISecurityPolicy
	{
		private readonly object[] _requiredRoles;

		public RequireRolePolicy(params object[] requiredRoles)
		{
			if (requiredRoles == null)
				throw new ArgumentException("Required roles must not be null");

			if (requiredRoles.Length == 0)
				throw new ArgumentException("Required roles must be specified");

			_requiredRoles = requiredRoles;
		}

		public PolicyResult Enforce(ISecurityContext context)
		{
			if (context.CurrenUserAuthenticated() == false)
				return PolicyResult.CreateFailureResult(this, "Anonymous access denied");

			if (context.CurrenUserRoles() == null || context.CurrenUserRoles().Length == 0)
				return PolicyResult.CreateFailureResult(this, "Access denied");

			foreach (var requiredRole in _requiredRoles)
			{
				foreach (var role in context.CurrenUserRoles())
				{
					if (requiredRole.ToString() == role.ToString())
					{
						return PolicyResult.CreateSuccessResult(this);
					}
				}
			}

			const string message = "Access requires one of the following roles: {0}.";
			var formattedMessage = string.Format(message, GetRoles());
			return PolicyResult.CreateFailureResult(this, formattedMessage);
		}

		public object[] RolesRequired
		{
			get { return _requiredRoles; }
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as RequireRolePolicy);
		}

		public bool Equals(RequireRolePolicy other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (RolesRequired.Count() != other.RolesRequired.Count()) return false;
			return RolesRequired.All(role => other.RolesRequired.Contains(role));
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;
				if (RolesRequired != null)
				{
					hash = RolesRequired.Aggregate(
						hash,
						(current, role) => current * 23 + ((role != null) ? role.GetHashCode() : 0)
						);
				}
				return hash;
			}
		}

		public override string ToString()
		{
			var name = base.ToString();
			var roles = GetRoles();
			return String.IsNullOrEmpty(roles) ? name : String.Concat(name, " (", GetRoles(), ")");
		}

		private string GetRoles()
		{
			var roles = string.Empty;
			if (_requiredRoles != null && _requiredRoles.Length > 0)
			{
				var builder = new StringBuilder();
				foreach (var requiredRole in _requiredRoles)
					builder.AppendFormat("{0} or ", requiredRole);

				roles = builder.ToString(0, builder.Length - 4); ;
			}
			return roles;
		}
	}
}