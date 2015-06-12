package draggridview;


public class ImageAdapter_ViewData
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
	static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("DragGridView.ImageAdapter/ViewData, DragGridView, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ImageAdapter_ViewData.class, __md_methods);
	}


	public ImageAdapter_ViewData () throws java.lang.Throwable
	{
		super ();
		if (getClass () == ImageAdapter_ViewData.class)
			mono.android.TypeManager.Activate ("DragGridView.ImageAdapter/ViewData, DragGridView, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
