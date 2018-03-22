using System;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace Polkovnik.DroidInjector.FodySample
{
    internal class ViewHolder : RecyclerView.ViewHolder
    {
#pragma warning disable 649
        [View] private readonly TextView _titleTextView;
        [View] private readonly TextView _subtitleTextView;
#pragma warning restore 649

        public ViewHolder(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public ViewHolder(View itemView) : base(itemView)
        {
            Injector.InjectViews(itemView);
        }

        public void Bind(string title, string subtitle)
        {
            _titleTextView.Text = title;
            _subtitleTextView.Text = subtitle;
        }
    }
}