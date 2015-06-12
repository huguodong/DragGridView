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

        private long dragResponseMS = 1000;// DragGridView��item������Ӧ��ʱ�䣬 Ĭ����1000���룬Ҳ������������   

        private System.Boolean isDrag = false;//�Ƿ������ק��Ĭ�ϲ����� 

        private int mDownX;
        private int mDownY;
        private int moveX;
        private int moveY;

        private int mDragPosition; //������ק��position  
        private View mStartDragItemView = null;//�տ�ʼ��ק��item��Ӧ��View   

        private ImageView mDragImageView;// ������ק�ľ�������ֱ����һ��ImageView   

        private Vibrator mVibrator;//����   

        private IWindowManager mWindowManager;
        /** 
         * item����Ĳ��ֲ��� 
         */
        private WindowManagerLayoutParams mWindowLayoutParams;

        private Bitmap mDragBitmap; //������ק��item��Ӧ��Bitmap  

        private int mPoint2ItemTop;//���µĵ㵽����item���ϱ�Ե�ľ���   

        private int mPoint2ItemLeft;  //���µĵ㵽����item�����Ե�ľ��� 

        private int mOffset2Top;// DragGridView������Ļ������ƫ����   

        /** 
         * DragGridView������Ļ��ߵ�ƫ���� 
         */
        private int mOffset2Left;

        private int mStatusHeight; //״̬���ĸ߶�   

        private int mDownScrollBorder;//DragGridView�Զ����¹����ı߽�ֵ   

        private int mUpScrollBorder; // DragGridView�Զ����Ϲ����ı߽�ֵ  

        private static int speed = 20;  //DragGridView�Զ��������ٶ� 

        private OnChanageListener onChanageListener; // item�����仯�ص��Ľӿ�
        public Java.Lang.Runnable mLongClickRunnable;
        /// <summary>
        ///  ��moveY��ֵ�������Ϲ����ı߽�ֵ������GridView�Զ����Ϲ��� 
        ///��moveY��ֵС�����¹����ı߽�ֵ������GridView�Զ����¹��� 
        /// ���򲻽��й��� 
        /// </summary>
        private Java.Lang.Runnable mScrollRunnable;
        public DragGridView(Context context)
            : base(context)
        {
            mVibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            mWindowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>(); ;
            mStatusHeight = getStatusHeight(context);  //��ȡ״̬���ĸ߶�  
            mLongClickRunnable = new Java.Lang.Runnable(mLongClickRunnableRun);
            mScrollRunnable = new Java.Lang.Runnable(mScrollRunnableRun);
        }

        public DragGridView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            mVibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            mWindowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();;
            mStatusHeight = getStatusHeight(context);  //��ȡ״̬���ĸ߶�  
            mLongClickRunnable = new Java.Lang.Runnable(mLongClickRunnableRun);
            mScrollRunnable = new Java.Lang.Runnable(mScrollRunnableRun);
        }

        public DragGridView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            mVibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            mWindowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>(); ;
            mStatusHeight = getStatusHeight(context);  //��ȡ״̬���ĸ߶�  
            mLongClickRunnable = new Java.Lang.Runnable(mLongClickRunnableRun);
            mScrollRunnable = new Java.Lang.Runnable(mScrollRunnableRun);
        }
        private Handler mHandler = new Handler();

        public void mLongClickRunnableRun()
        {

            isDrag = true; //���ÿ�����ק  
            mVibrator.Vibrate(50); //��һ��  
            mStartDragItemView.Visibility = ViewStates.Invisible;//���ظ�item  

            //�������ǰ��µĵ���ʾitem����  
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

            //�����ǵ���ָ����GridView���ϻ������¹�����ƫ������ʱ�򣬿���������ָû���ƶ�������DragGridView���Զ��Ĺ���  
            //�������������������onSwapItem()����������item  
            onSwapItem(moveX, moveY);


            SmoothScrollBy(scrollY, 10);
        }
        private void onSwapItem(int moveX, int moveY)
        {
            //��ȡ������ָ�ƶ������Ǹ�item��position  
            int tempPosition = PointToPosition(moveX, moveY);

            //����tempPosition �ı��˲���tempPosition������-1,����н���  
            if (tempPosition != mDragPosition && tempPosition != AdapterView.InvalidPosition)
            {
                if (onChanageListener != null)
                {
                    onChanageListener.onChange(mDragPosition, tempPosition);
                }

                GetChildAt(tempPosition - FirstVisiblePosition).Visibility = ViewStates.Invisible;//�϶������µ�item,�µ�item���ص�  
                GetChildAt(mDragPosition - FirstVisiblePosition).Visibility = ViewStates.Visible;//֮ǰ��item��ʾ����  

                mDragPosition = tempPosition;
            }
        }
        /// <summary>
        /// ֹͣ��ק���ǽ�֮ǰ���ص�item��ʾ���������������Ƴ�
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
        /// ���ûص��ӿ� 
        /// </summary>
        /// <param name="onChanageListener"></param>
        public void setOnChangeListener(OnChanageListener onChanageListener)
        {
            this.onChanageListener = onChanageListener;
        }
        /** 
      * �����϶��ľ��� 
      * @param bitmap  
      * @param downX 
      *          ���µĵ���Ը��ؼ���X���� 
      * @param downY 
      *          ���µĵ���Ը��ؼ���X���� 
      */
        private void createDragImage(Bitmap bitmap, int downX, int downY)
        {
            mWindowLayoutParams = new WindowManagerLayoutParams();
            mWindowLayoutParams.Format = Format.Translucent; //ͼƬ֮��������ط�͸��  
            mWindowLayoutParams.Gravity = GravityFlags.Top | GravityFlags.Left;
            mWindowLayoutParams.X = downX - mPoint2ItemLeft + mOffset2Left;
            mWindowLayoutParams.Y = downY - mPoint2ItemTop + mOffset2Top - mStatusHeight;
            mWindowLayoutParams.Alpha = 0.55f; //͸����  
            mWindowLayoutParams.Width = WindowManagerLayoutParams.WrapContent;
            mWindowLayoutParams.Height = WindowManagerLayoutParams.WrapContent;
            mWindowLayoutParams.Flags = WindowManagerFlags.NotFocusable | WindowManagerFlags.NotTouchable;

            mDragImageView = new ImageView(Context);
            mDragImageView.SetImageBitmap(bitmap);
            mWindowManager.AddView(mDragImageView, mWindowLayoutParams);
        }
        /// <summary>
        /// ������Ӧ��ק�ĺ�������Ĭ����1000���� 
        /// </summary>
        /// <param name="dragResponseMS"></param>
        public void setDragResponseMS(long dragResponseMS)
        {
            this.dragResponseMS = dragResponseMS;
        }
        /// <summary>
        /// �ӽ��������ƶ��϶����� 
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

                    //���ݰ��µ�X,Y�����ȡ�����item��position  
                    mDragPosition = PointToPosition(mDownX, mDownY);


                    if (mDragPosition == AdapterView.InvalidPosition)
                    {
                        return base.DispatchTouchEvent(ev);
                    }

                    //ʹ��Handler�ӳ�dragResponseMSִ��mLongClickRunnable  
                    mHandler.PostDelayed(mLongClickRunnable, dragResponseMS);

                    //����position��ȡ��item����Ӧ��View  
                    mStartDragItemView = GetChildAt(mDragPosition - FirstVisiblePosition);

                    //�����⼸�������ҿ��Բο��ҵĲ��������ͼ�������  
                    mPoint2ItemTop = mDownY - mStartDragItemView.Top;
                    mPoint2ItemLeft = mDownX - mStartDragItemView.Left;

                    mOffset2Top = (int)(ev.RawY - mDownY);
                    mOffset2Left = (int)(ev.RawX - mDownX);

                    //��ȡDragGridView�Զ����Ϲ�����ƫ������С�����ֵ��DragGridView���¹���  
                    mDownScrollBorder = Height / 4;
                    //��ȡDragGridView�Զ����¹�����ƫ�������������ֵ��DragGridView���Ϲ���  
                    mUpScrollBorder = Height * 3 / 4;



                    //����mDragItemView��ͼ����  
                    mStartDragItemView.DrawingCacheEnabled = true;
                    //��ȡmDragItemView�ڻ����е�Bitmap����  
                    mDragBitmap = Bitmap.CreateBitmap(mStartDragItemView.DrawingCache);
                    //��һ���ܹؼ����ͷŻ�ͼ���棬��������ظ��ľ���  
                    mStartDragItemView.DestroyDrawingCache();


                    break;
                case MotionEventActions.Move:
                    int moveX = (int)ev.GetX();
                    int moveY = (int)ev.GetY();

                    //��������ڰ��µ�item�����ƶ���ֻҪ������item�ı߽����ǾͲ��Ƴ�mRunnable  
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
        /// �Ƿ�����GridView��item���� 
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
        /// ��ȡ״̬���ĸ߶� 
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
                        //�϶�item  
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
     * �϶�item��������ʵ����item�����λ�ø��£�item���໥�����Լ�GridView�����й��� 
     * @param x 
     * @param y 
     */
        private void onDragItem(int moveX, int moveY)
        {
            mWindowLayoutParams.X = moveX - mPoint2ItemLeft + mOffset2Left;
            mWindowLayoutParams.Y = moveY - mPoint2ItemTop + mOffset2Top - mStatusHeight;
            mWindowManager.UpdateViewLayout(mDragImageView, mWindowLayoutParams); //���¾����λ��  
            onSwapItem(moveX, moveY);

            //GridView�Զ�����  
            mHandler.Post(mScrollRunnable);
        }


    }

}