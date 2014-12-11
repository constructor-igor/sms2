using System;
using Android.App;
using Android.Appwidget;
using Android.Content;

namespace sms2
{
	[BroadcastReceiver (Label = "@string/widget_name")]
	public class Widget: AppWidgetProvider
	{
		public Widget ()
		{
		}
	}
}

