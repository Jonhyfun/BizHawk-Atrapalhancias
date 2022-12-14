namespace BizHawk.Bizware.BizwareGL
{
	/// <summary>
	/// Represents an individual (fragment,vertex) shader.
	/// It isn't IDisposable because it'll be lifecycle-managed by the IGL (disposed when all dependent pipelines are disposed)
	/// But if you want to be sure to save it for later, use AddRef
	/// </summary>
	public class Shader
	{
		public Shader(IGL owner, object opaque, bool available)
		{
			Owner = owner;
			Opaque = opaque;
			Available = available;
			Errors = "";
		}

		public IGL Owner { get; }
		public object Opaque { get; }
		public bool Available { get; private set; }
		public string Errors { get; set; }

		private int RefCount;

		public void Release()
		{
			RefCount--;
			if (RefCount <= 0)
			{
				Owner.Internal_FreeShader(this);
				Available = false;
			}
		}

		public void AddRef()
		{
			RefCount++;
		}

	}
}