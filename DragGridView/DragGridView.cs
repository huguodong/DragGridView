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
using Android.Graphics;
using Android.Util;

namespace DragGridView
{
    public class DragGridView : GridView
    {

        private long dragResponseMS = 1000;// DragGridView的item长按响应的时间， 默认是1000毫秒，也可以自行设置   

        private System.Boolean isDrag = false;//是否可以拖拽，默认不可以 

        private int mDownX;
        private int mDownY;
        private int moveX;
        private int moveY;

        private int mDragPosition; //正在拖拽的position  
        private View mStartDragItemView = null;//刚开始拖拽的item对应的View   

        private ImageView mDragImageView;// 用于拖拽的镜像，这里直接用一个ImageView   

        private Vibrator mVibrator;//震动器   

        private IWindowManager mWindowManager;
        /** 
         * item镜像的布局参数 
         */
        private WindowManagerLayoutParams mWindowLayoutParams;

        private Bitmap mDragBitmap; //我们拖拽的item对应的Bitmap  

        private int mPoint2ItemTop;//按下的点到所在item的上边缘的距离   

        private int mPoint2ItemLeft;  //按下的点到所在item的左边缘的距离 

        private int mOffset2Top;// DragGridView距离屏幕顶部的偏移量   

        /** 
         * DragGridView距离屏幕左边的偏移量 
         */
        private int mOffset2Left;

        private int mStatusHeight; //状态栏的高度   

        private int mDownScrollBorder;//DragGridView自动向下滚动的边界值   

        private int mUpScrollBorder; // DragGridView自动向上滚动的边界值  

        private static int speed = 20;  //DragGridView自动滚动的速度 

        private OnChanageListener onChanageListener; // item发生变化回调的接口
        public Java.Lang.Runnable mLongClickRunnable;
        /// <summary>
        ///  当moveY的值大于向上滚动的边界值，触发GridView自动向上滚动 
        ///当moveY的值小于向下滚动的边界值，触犯GridView自动向下滚动 
        /// 否则不进行滚动 
        /// </summary>
        private Java.Lang.Runnable mScrollRunnable;
        public DragGridView(Context context)
            : base(context)
        {
            mVibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            mWindowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>(); ;
            mStatusHeight = getStatusHeight(context);  //获取状态栏的高度  
            mLongClickRunnable = new Java.Lang.Runnable(mLongClickRunnableRun);
            mScrollRunnable = new Java.Lang.Runnable(mScrollRunnableRun);
        }

        public DragGridView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            mVibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            mWindowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();;
            mStatusHeight = getStatusHeight(context);  //获取状态栏的高度  
            mLongClickRunnable = new Java.Lang.Runnable(mLongClickRunnableRun);
            mScrollRunnable = new Java.Lang.Runnable(mScrollRunnableRun);
        }

        public DragGridView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            mVibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            mWindowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>(); ;
            mStatusHeight = getStatusHeight(context);  //获取状态栏的高度  
            mLongClickRunnable = new Java.Lang.Runnable(mLongClickRunnableRun);
            mScrollRunnable = new Java.Lang.Runnable(mScrollRunnableRun);
        }
        private Handler mHandler = new Handler();

        public void mLongClickRunnableRun()
        {

            isDrag = true; //设置可以拖拽  
            mVibrator.Vibrate(50); //震动一下  
            mStartDragItemView.Visibility = ViewStates.Invisible;//隐藏该item  

            //根据我们按下的点显示item镜像  
            createDragImage(mDragBitmap, mDownX, mDownY);
        }
        public void mScrollRunnableRun()
        {
            int scrollY;
            if (moveY > mUpScrollBorder)
            {
                scrollY = speed;
                mHandler.PostDelayed(mScrollRunnable, 25);
            }
            else if (moveY < mDownScrollBorder)
            {
                scrollY = -speed;
                mHandler.PostDelayed(mScrollRunnable, 25);
            }
            else
            {
                scrollY = 0;
                mHandler.RemoveCallbacks(mScrollRunnable);
            }

            //当我们的手指到达GridView向上或者向下滚动的偏移量的时候，可能我们手指没有移动，但是DragGridView在自动的滚动  
            //所以我们在这里调用下onSwapItem()方法来交换item  
            onSwapItem(moveX, moveY);


            SmoothScrollBy(scrollY, 10);
        }
        private void onSwapItem(int moveX, int moveY)
        {
            //获取我们手指移动到的那个item的position  
            int tempPosition = PointToPosition(moveX, moveY);

            //假如tempPosition 改变了并且tempPosition不等于-1,则进行交换  
            if (tempPosition != mDragPosition && tempPosition != AdapterView.InvalidPosition)
            {
                if (onChanageListener != null)
                {
                    onChanageListener.onChange(mDragPosition, tempPosition);
                }

                GetChildAt(tempPosition - FirstVisiblePosition).Visibility = ViewStates.Invisible;//拖动到了新的item,新的item隐藏掉  
                GetChildAt(mDragPosition - FirstVisiblePosition).Visibility = ViewStates.Visible;//之前的item显示出来  

                mDragPosition = tempPosition;
            }
        }
        /// <summary>
        /// 停止拖拽我们将之前隐藏的item显示出来，并将镜像移除
        /// </summary>
        private void onStopDrag()
        {
            View view = GetChildAt(mDragPosition - FirstVisiblePosition);
            if (view != null)
            {
                view.Visibility = ViewStates.Visible;
            }

            removeDragImage();
        }

        /// <summary>
        /// 设置回调接口 
        /// </summary>
        /// <param name="onChanageListener"></param>
        public void setOnChangeListener(OnChanageListener onChanageListener)
        {
            this.onChanageListener = onChanageListener;
        }
        /** 
      * 创建拖动的镜像 
      * @param bitmap  
      * @param downX 
      *          按下的点相对父控件的X坐标 
      * @param downY 
      *          按下的点相对父控件的X坐标 
      */
        private void createDragImage(Bitmap bitmap, int downX, int downY)
        {
            mWindowLayoutParams = new WindowManagerLayoutParams();
            mWindowLayoutParams.Format = Format.Translucent; //图片之外的其他地方透明  
            mWindowLayoutParams.Gravity = GravityFlags.Top | GravityFlags.Left;
            mWindowLayoutParams.X = downX - mPoint2ItemLeft + mOffset2Left;
            mWindowLayoutParams.Y = downY - mPoint2ItemTop + mOffset2Top - mStatusHeight;
            mWindowLayoutParams.Alpha = 0.55f; //透明度  
            mWindowLayoutParams.Width = WindowManagerLayoutParams.WrapContent;
            mWindowLayoutParams.Height = WindowManagerLayoutParams.WrapContent;
            mWindowLayoutParams.Flags = WindowManagerFlags.NotFocusable | WindowManagerFlags.NotTouchable;

            mDragImageView = new ImageView(Context);
            mDragImageView.SetImageBitmap(bitmap);
            mWindowManager.AddView(mDragImageView, mWindowLayoutParams);
        }
        /// <summary>
        /// 设置响应拖拽的毫秒数，默认是1000毫秒 
        /// </summary>
        /// <param name="dragResponseMS"></param>
        public void setDragResponseMS(long dragResponseMS)
        {
            this.dragResponseMS = dragResponseMS;
        }
        /// <summary>
        /// 从界面上面移动拖动镜像 
        /// </summary>
        private void removeDragImage()
        {
            if (mDragImageView != null)
            {
                mWindowManager.RemoveView(mDragImageView);
                mDragImageView = null;
            }
        }
        public override Boolean DispatchTouchEvent(MotionEvent ev)
        {
            switch (ev.Action)
            {
                case MotionEventActions.Down:
                    mDownX = (int)ev.GetX();
                    mDownY = (int)ev.GetY();

                    //根据按下的X,Y坐标获取所点击item的position  
                    mDragPosition = PointToPosition(mDownX, mDownY);


                    if (mDragPosition == AdapterView.InvalidPosition)
                    {
                        return base.DispatchTouchEvent(ev);
                    }

                    //使用Handler延迟dragResponseMS执行mLongClickRunnable  
                    mHandler.PostDelayed(mLongClickRunnable, dragResponseMS);

                    //根据position获取该item所对应的View  
                    mStartDragItemView = GetChildAt(mDragPosition - FirstVisiblePosition);

                    //下面这几个距离大家可以参考我的博客上面的图来理解下  
                    mPoint2ItemTop = mDownY - mStartDragItemView.Top;
                    mPoint2ItemLeft = mDownX - mStartDragItemView.Left;

                    mOffset2Top = (int)(ev.RawY - mDownY);
                    mOffset2Left = (int)(ev.RawX - mDownX);

                    //获取DragGridView自动向上滚动的偏移量，小于这个值，DragGridView向下滚动  
                    mDownScrollBorder = Height / 4;
                    //获取DragGridView自动向下滚动的偏移量，大于这个值，DragGridView向上滚动  
                    mUpScrollBorder = Height * 3 / 4;



                    //开启mDragItemView绘图缓存  
                    mStartDragItemView.DrawingCacheEnabled = true;
                    //获取mDragItemView在缓存中的Bitmap对象  
                    mDragBitmap = Bitmap.CreateBitmap(mStartDragItemView.DrawingCache);
                    //这一步很关键，释放绘图缓存，避免出现重复的镜像  
                    mStartDragItemView.DestroyDrawingCache();


                    break;
                case MotionEventActions.Move:
                    int moveX = (int)ev.GetX();
                    int moveY = (int)ev.GetY();

                    //如果我们在按下的item上面移动，只要不超过item的边界我们就不移除mRunnable  
                    if (!isTouchInItem(mStartDragItemView, moveX, moveY))
                    {
                        mHandler.RemoveCallbacks(mLongClickRunnable);
                    }
                    break;
                case MotionEventActions.Up:
                    mHandler.RemoveCallbacks(mLongClickRunnable);
                    mHandler.RemoveCallbacks(mScrollRunnable);
                    break;
            }
            return base.DispatchTouchEvent(ev);
        }
        /// 是否点击在GridView的item上面 
        /// </summary>
        /// <param name="dragView"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Boolean isTouchInItem(View dragView, int x, int y)
        {
            if (dragView == null)
            {
                return false;
            }
            int leftOffset = dragView.Left;
            int topOffset = dragView.Top;
            if (x < leftOffset || x > leftOffset + dragView.Width)
            {
                return false;
            }

            if (y < topOffset || y > topOffset + dragView.Height)
            {
                return false;
            }

            return true;
        }
        /// 获取状态栏的高度 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static int getStatusHeight(Context context)
        {
            int statusHeight = 0;
            Rect localRect = new Rect();
            ((Activity)context).Window.DecorView.GetWindowVisibleDisplayFrame(localRect);
            statusHeight = localRect.Top;
            if (0 == statusHeight)
            {

                Java.Lang.Class localClass;
                try
                {
                    localClass = Java.Lang.Class.ForName("com.android.internal.R$dimen");
                    Java.Lang.Object localObject = localClass.NewInstance();
                    int i5 = Java.Lang.Integer.ParseInt(localClass.GetField("status_bar_height").Get(localObject).ToString());
                    statusHeight = context.Resources.GetDimensionPixelSize(i5);
                }
                catch (Exception e)
                {

                }
            }
            return statusHeight;
        }
        public override bool OnTouchEvent(MotionEvent ev)
        {
            if (isDrag && mDragImageView != null)
            {
                switch (ev.Action)
                {
                    case MotionEventActions.Move:
                        moveX = (int)ev.GetX();
                        moveY = (int)ev.GetY();
                        //拖动item  
                        onDragItem(moveX, moveY);
                        break;
                    case MotionEventActions.Up:
                        onStopDrag();
                        isDrag = false;
                        break;
                }
                return true;
            }
            return base.OnTouchEvent(ev);
        }

        /** 
     * 拖动item，在里面实现了item镜像的位置更新，item的相互交换以及GridView的自行滚动 
     * @param x 
     * @param y 
     */
        private void onDragItem(int moveX, int moveY)
        {
            mWindowLayoutParams.X = moveX - mPoint2ItemLeft + mOffset2Left;
            mWindowLayoutParams.Y = moveY - mPoint2ItemTop + mOffset2Top - mStatusHeight;
            mWindowManager.UpdateViewLayout(mDragImageView, mWindowLayoutParams); //更新镜像的位置  
            onSwapItem(moveX, moveY);

            //GridView自动滚动  
            mHandler.Post(mScrollRunnable);
        }


    }

}