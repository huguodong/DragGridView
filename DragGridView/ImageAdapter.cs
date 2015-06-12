using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DragGridView
{
    public class ImageAdapter : BaseAdapter
    {
        public class ViewData : Java.Lang.Object
        {
            public ImageView imageView { get; set; }
            public TextView tex { get; set; }
        }
        MainActivity context;
        IList<IDictionary<String, Object>> dataSourceList;
        public ImageAdapter(MainActivity c, IList<IDictionary<String, Object>> data)
        {
            context = c;
            dataSourceList = data;
        }
        public override int Count
        {
            get
            {
                return dataSourceList.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                convertView = context.LayoutInflater.Inflate(Resource.Layout.item, null);
                ViewData vd = new ViewData();
                vd.imageView = convertView.FindViewById<ImageView>(Resource.Id.item_image);
                vd.tex = convertView.FindViewById<TextView>(Resource.Id.item_text);
                var Item = dataSourceList[position];
                vd.imageView.SetImageResource(Convert.ToInt32(Item["item_image"]));
                vd.tex.Text = Item["item_text"].ToString();
                convertView.Tag = vd;
            }
            else
            {
                ViewData vd = (ViewData)convertView.Tag;
                var Item = dataSourceList[position];
                vd.imageView.SetImageResource(Convert.ToInt32(Item["item_image"]));
                vd.tex.Text = Item["item_text"].ToString();
            }
            return convertView;

        }
    }
}