using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;

using Android.OS;
using System.Collections.Generic;
using Android.Widget;

namespace DragGridView
{
    [Activity(Label = "DragGridView", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, OnChanageListener
    {
        private IList<IDictionary<String, Object>> dataSourceList = new List<IDictionary<String, Object>>();
        //private Dictionary<String, Object> dataSourceList = new Dictionary<String, Object>();
        private SimpleAdapter mSimpleAdapter;
        ImageAdapter mImageAdapter;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            DragGridView mDragGridView = FindViewById<DragGridView>(Resource.Id.dragGridView);

            for (int i = 0; i < 30; i++)
            {
                Dictionary<String, Object> itemDic = new Dictionary<String, Object>();
                itemDic.Add("item_image", Resource.Drawable.com_tencent_open_notice_msg_icon_big);
                itemDic.Add("item_text", "拖拽 " + Java.Lang.Integer.ToString(i));
                dataSourceList.Add(itemDic);
            }
            mImageAdapter = new ImageAdapter(this, dataSourceList);
            ////mSimpleAdapter = new SimpleAdapter(this, dataSourceList,
            //   Resource.Layout.item, new String[] { "item_image", "item_text" },
            //   new int[] { Resource.Id.item_image, Resource.Id.item_text });


            mDragGridView.Adapter = mImageAdapter;
            mDragGridView.setOnChangeListener(this);

        }

        public void onChange(int form, int to)
        {
            IDictionary<String, Object> temp = dataSourceList[form];
            //直接交互item  
            //              dataSourceList.set(from, dataSourceList.get(to));  
            //              dataSourceList.set(to, temp);  
            //              dataSourceList.set(to, temp);  


            //这里的处理需要注意下  
            if (form < to)
            {
                //for (int i = form; i < to; i++)
                //{
                // swap(dataSourceList, i, i + 1);
                //}
                swap(dataSourceList, form, to);
            }
            else if (form > to)
            {
                swap(dataSourceList, form, to);

            }
            dataSourceList.RemoveAt(to);
            dataSourceList.Insert(to, temp);
            mImageAdapter.NotifyDataSetChanged();
        }
        public static void swap(IList<IDictionary<string, object>> list, int i, int j)
        {

            var a = i;
            var b = j;
            list[i] = list[b];
            list[j] = list[a];
        }
    }


}

